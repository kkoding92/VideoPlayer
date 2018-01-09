/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class AirVRInputModule : PointerInputModule {
    private float _prevActionTime;
    private Vector2 _lastMoveVector;
    private int _consecutiveMoveCount = 0;

    private readonly MouseState _mouseState = new MouseState();
    private List<RaycastResult> _raycastResultCache = new List<RaycastResult>();

    private Dictionary<int, Dictionary<int, AirVRPointerEventData>> _pointerData = new Dictionary<int, Dictionary<int, AirVRPointerEventData>>();

    [SerializeField]
    private string _horizontalAxis = "Horizontal";
    [SerializeField]
    private string _verticalAxis = "Vertical";
    [SerializeField]
    private string _submitButton = "Submit";
    [SerializeField]
    private string _cancelButton = "Cancel";
    [SerializeField]
    private float _inputActionsPerSecond = 10;
    [SerializeField]
    private float _repeatDelay = 0.5f;

    public string horizontalAxis {
        get { return _horizontalAxis; }
        set { _horizontalAxis = value; }
    }

    public string verticalAxis {
        get { return _verticalAxis; }
        set { _verticalAxis = value; }
    }

    public string submitButton {
        get { return _submitButton; }
        set { _submitButton = value; }
    }

    public string cancelButton {
        get { return _cancelButton; }
        set { _cancelButton = value; }
    }

    public float inputActionsPerSecond {
        get { return _inputActionsPerSecond; }
        set { _inputActionsPerSecond = value; }
    }

    public float repeatDelay {
        get { return _repeatDelay; }
        set { _repeatDelay = value; }
    }

    private Vector2 getRawMoveVector() {
        Vector2 move = Vector2.zero;
        move.x = Input.GetAxisRaw(_horizontalAxis);
        move.y = Input.GetAxisRaw(_verticalAxis);

        if (Input.GetButtonDown(_horizontalAxis)) {
            move.x = move.x < 0 ? -1.0f :
                     move.x > 0 ? 1.0f : 0.0f;
        }
        if (Input.GetButtonDown(_verticalAxis)) {
            move.y = move.y < 0 ? -1.0f :
                     move.y > 0 ? 1.0f : 0.0f;
        }
        return move;
    }

    private bool sendUpdateEventToSelectedObject() {
        if (eventSystem.currentSelectedGameObject == null) {
            return false;
        }

        var data = GetBaseEventData();
        ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
        return data.used;
    }

    private bool sendMoveEventToSelectedObject() {
        float time = Time.unscaledTime;

        Vector2 movement = getRawMoveVector();
        if (Mathf.Approximately(movement.x, 0.0f) && Mathf.Approximately(movement.y, 0.0f)) {
            _consecutiveMoveCount = 0;
            return false;
        }

        bool allow = Input.GetButtonDown(_horizontalAxis) || Input.GetButtonDown(_verticalAxis);
        bool similarDir = (Vector2.Dot(movement, _lastMoveVector) > 0);
        if (allow == false) {
            if (similarDir && _consecutiveMoveCount == 1) {
                allow = (time > _prevActionTime + _repeatDelay);
            }
            else {
                allow = (time > _prevActionTime + 1.0f / _inputActionsPerSecond);
            }
        }
        if (allow == false) {
            return false;
        }

        var axisEventData = GetAxisEventData(movement.x, movement.y, 0.6f);
        ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
        if (similarDir == false) {
            _consecutiveMoveCount = 0;
        }
        _consecutiveMoveCount++;
        _prevActionTime = time;
        _lastMoveVector = movement;
        return axisEventData.used;
    }

    private bool sendSubmitEventToSelectedObject() {
        if (eventSystem.currentSelectedGameObject == null) {
            return false;
        }

        var data = GetBaseEventData();
        if (Input.GetButtonDown(_submitButton)) {
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);
        }
        if (Input.GetButtonDown(_cancelButton)) {
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
        }
        return data.used;
    }

    private void processMousePress(MouseButtonEventData data) {
        var pointerEvent = data.buttonData;
        var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

        if (data.PressedThisFrame()) {
            pointerEvent.eligibleForClick = true;
            pointerEvent.delta = Vector2.zero;
            pointerEvent.dragging = false;
            pointerEvent.useDragThreshold = true;
            pointerEvent.pressPosition = pointerEvent.position;
            pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

            DeselectIfSelectionChanged(currentOverGo, pointerEvent);

            var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);
            if (newPressed == null) {
                newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
            }

            float time = Time.unscaledTime;
            if (newPressed == pointerEvent.lastPress) {
                var diffTime = time - pointerEvent.clickTime;
                if (diffTime < 0.3f) {
                    pointerEvent.clickCount++;
                }
                else {
                    pointerEvent.clickCount = 1;
                }
            }
            else {
                pointerEvent.clickCount = 1;
            }

            pointerEvent.pointerPress = newPressed;
            pointerEvent.rawPointerPress = currentOverGo;
            pointerEvent.clickTime = time;

            pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);
            if (pointerEvent.pointerDrag != null) {
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
            }
        }

        if (data.ReleasedThisFrame()) {
            ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

            var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
            if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick) {
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
            }
            else if (pointerEvent.pointerDrag != null && pointerEvent.dragging) {
                ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
            }

            pointerEvent.eligibleForClick = false;
            pointerEvent.pointerPress = null;
            pointerEvent.rawPointerPress = null;

            if (pointerEvent.pointerDrag != null && pointerEvent.dragging) {
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
            }

            pointerEvent.dragging = false;
            pointerEvent.pointerDrag = null;

            if (currentOverGo != pointerEvent.pointerEnter) {
                HandlePointerExitAndEnter(pointerEvent, null);
                HandlePointerExitAndEnter(pointerEvent, currentOverGo);
            }
        }
    }

    private MouseState getMousePointerEventData(int id) {
        PointerEventData leftData;
        var created = GetPointerData(kMouseLeftId, out leftData, true);

        leftData.Reset();
        if (created) {
            leftData.position = Input.mousePosition;
        }

        Vector2 pos = Input.mousePosition;
        leftData.delta = pos - leftData.position;
        leftData.position = pos;
        leftData.scrollDelta = Input.mouseScrollDelta;
        leftData.button = PointerEventData.InputButton.Left;
        eventSystem.RaycastAll(leftData, _raycastResultCache);
        var raycast = FindFirstRaycast(_raycastResultCache);
        leftData.pointerCurrentRaycast = raycast;
        _raycastResultCache.Clear();

        PointerEventData rightData;
        GetPointerData(kMouseRightId, out rightData, true);
        CopyFromTo(leftData, rightData);
        rightData.button = PointerEventData.InputButton.Right;

        PointerEventData middleData;
        GetPointerData(kMouseMiddleId, out middleData, true);
        CopyFromTo(leftData, middleData);
        middleData.button = PointerEventData.InputButton.Middle;

        _mouseState.SetButtonState(PointerEventData.InputButton.Left, StateForMouseButton(0), leftData);
        _mouseState.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), rightData);
        _mouseState.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), middleData);

        return _mouseState;
    }

    private void processMouseEvent(MouseState mouseData) {
        var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

        processMousePress(leftButtonData);
        ProcessMove(leftButtonData.buttonData);
        ProcessDrag(leftButtonData.buttonData);

        processMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
        ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
        processMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
        ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);

        if (Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f) == false) {
            var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
            ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
        }
    }

    private static int raycastComparer(RaycastResult lhs, RaycastResult rhs) {
        if (lhs.module != rhs.module) {
            if (lhs.module.eventCamera != null && rhs.module.eventCamera != null &&
                lhs.module.eventCamera.depth != rhs.module.eventCamera.depth) {
                return lhs.module.eventCamera.depth < rhs.module.eventCamera.depth ? 1 :
                       lhs.module.eventCamera.depth == rhs.module.eventCamera.depth ? 0 : -1;
            }

            if (lhs.module.sortOrderPriority != rhs.module.sortOrderPriority) {
                return rhs.module.sortOrderPriority.CompareTo(lhs.module.sortOrderPriority);
            }
            if (lhs.module.renderOrderPriority != rhs.module.renderOrderPriority) {
                return rhs.module.renderOrderPriority.CompareTo(lhs.module.renderOrderPriority);
            }
        }

        if (lhs.sortingLayer != rhs.sortingLayer) {
            var rid = SortingLayer.GetLayerValueFromID(rhs.sortingLayer);
            var lid = SortingLayer.GetLayerValueFromID(lhs.sortingLayer);
            return rid.CompareTo(lid);
        }

        if (lhs.sortingOrder != rhs.sortingOrder) {
            return rhs.sortingOrder.CompareTo(lhs.sortingOrder);
        }
        if (lhs.depth != rhs.depth) {
            return rhs.depth.CompareTo(lhs.depth);
        }
        if (lhs.distance != rhs.distance) {
            return lhs.distance.CompareTo(rhs.distance);
        }
        return lhs.index.CompareTo(rhs.index);
    }

    private static readonly Comparison<RaycastResult> _raycastComparer = raycastComparer;

    private void raycastForAirVRPointer(AirVRPointer pointer, PointerEventData eventData, List<RaycastResult> raycastResults) {
        raycastResults.Clear();
        foreach (var raycaster in AirVRPhysicsRaycaster.GetAllRaycasters()) {
            if (raycaster.pointer == pointer) {
                raycaster.Raycast(eventData, raycastResults);
            }
        }
        foreach (var raycaster in AirVRGraphicRaycaster.GetAllRaycasters()) {
            if (raycaster.pointer == pointer) {
                raycaster.Raycast(eventData, raycastResults);
            }
        }

        raycastResults.Sort(_raycastComparer);
    }

    private bool getAirVRPointerData(AirVRPointer pointer, int pointerId, out AirVRPointerEventData data, bool create) {
        if (_pointerData.ContainsKey(pointer.GetHashCode()) == false || _pointerData[pointer.GetHashCode()].TryGetValue(pointerId, out data) == false) {
            if (create) {
                if (_pointerData.ContainsKey(pointer.GetHashCode()) == false) {
                    _pointerData.Add(pointer.GetHashCode(), new Dictionary<int, AirVRPointerEventData>());
                }
                data = new AirVRPointerEventData(eventSystem) {
                    pointerId = pointer.GetHashCode()
                };

                _pointerData[pointer.GetHashCode()].Add(pointerId, data);
                return true;
            }
            else {
                data = null;
            }
        }
        return false;
    }

    private void copyAirVRPointerEventData(AirVRPointerEventData from, AirVRPointerEventData to) {
        to.position = from.position;
        to.delta = from.delta;
        to.scrollDelta = from.scrollDelta;
        to.pointerCurrentRaycast = from.pointerCurrentRaycast;
        to.pointerEnter = from.pointerEnter;
        to.worldSpaceRay = from.worldSpaceRay;
    }

    private PointerEventData.FramePressState getAirVRPointerButtonState(AirVRPointer pointer) {
        bool pressed = pointer.primaryButtonPressed;
        bool released = pointer.primaryButtonReleased;

        return (pressed && released) ? PointerEventData.FramePressState.PressedAndReleased :
               pressed ?               PointerEventData.FramePressState.Pressed :
               released ?              PointerEventData.FramePressState.Released :
                                       PointerEventData.FramePressState.NotChanged;
    }

    private MouseState getAirVRPointerEventData(AirVRPointer pointer) {
        AirVRPointerEventData leftData;
        var created = getAirVRPointerData(pointer, kMouseLeftId, out leftData, true);
        leftData.Reset();

        leftData.worldSpaceRay = pointer.GetWorldRay();
        leftData.button = PointerEventData.InputButton.Left;
        leftData.scrollDelta = Vector2.zero;
        leftData.useDragThreshold = false;

        raycastForAirVRPointer(pointer, leftData, _raycastResultCache);
        var raycast = FindFirstRaycast(_raycastResultCache);
        leftData.pointerCurrentRaycast = raycast;
        _raycastResultCache.Clear();

        Vector2 position = Vector2.zero;
        AirVRGraphicRaycaster airvrRaycaster = raycast.module as AirVRGraphicRaycaster;
        if (airvrRaycaster) {
            position = airvrRaycaster.GetScreenPosition(raycast);
        }

        AirVRPhysicsRaycaster airvrPhysicsRaycaster = raycast.module as AirVRPhysicsRaycaster;
        if (airvrPhysicsRaycaster) {
            position = airvrPhysicsRaycaster.GetScreenPosition(raycast.worldPosition);
        }

        if (created) {
            leftData.position = position;
        }
        leftData.delta = position - leftData.position;
        leftData.position = position;

        AirVRPointerEventData rightData;
        getAirVRPointerData(pointer, kMouseRightId, out rightData, true);
        copyAirVRPointerEventData(leftData, rightData);
        rightData.button = PointerEventData.InputButton.Right;

        AirVRPointerEventData middleData;
        getAirVRPointerData(pointer, kMouseMiddleId, out middleData, true);
        copyAirVRPointerEventData(leftData, middleData);
        middleData.button = PointerEventData.InputButton.Middle;

        _mouseState.SetButtonState(PointerEventData.InputButton.Left, getAirVRPointerButtonState(pointer), leftData);
        _mouseState.SetButtonState(PointerEventData.InputButton.Right, PointerEventData.FramePressState.NotChanged, rightData);
        _mouseState.SetButtonState(PointerEventData.InputButton.Middle, PointerEventData.FramePressState.NotChanged, middleData);

        return _mouseState;
    }

    private void processAirVRInputEvents() {
        foreach (AirVRPointer pointer in AirVRPointer.pointers) {
            if (pointer.interactable) {
                var eventData = getAirVRPointerEventData(pointer);
                processMouseEvent(eventData);

                AirVRPointerEventData leftButtonData = eventData.GetButtonState(PointerEventData.InputButton.Left).eventData.buttonData as AirVRPointerEventData;
                pointer.UpdateRaycastResult(leftButtonData.worldSpaceRay, leftButtonData.pointerCurrentRaycast);
            }
        }
    }

    public override void Process() {
        bool usedEvent = sendUpdateEventToSelectedObject();

        if (eventSystem.sendNavigationEvents) {
            if (usedEvent == false) {
                usedEvent |= sendMoveEventToSelectedObject();
            }
            if (usedEvent != false) {
                sendSubmitEventToSelectedObject();
            }
        }

        processMouseEvent(getMousePointerEventData(0));
        processAirVRInputEvents();
    }
}
