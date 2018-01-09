/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class AirVRGraphicRaycaster : GraphicRaycaster {
    private static readonly List<AirVRGraphicRaycaster> _allRaycasters = new List<AirVRGraphicRaycaster>();

    public static List<AirVRGraphicRaycaster> GetAllRaycasters() {
        return _allRaycasters;
    }

    private struct RaycastHit {
        public Graphic graphic;
        public Vector3 worldPos;
        public Vector3 worldNormal;
        public bool fromMouse;
    };

    private static readonly List<RaycastHit> _sortedGraphics = new List<RaycastHit>();

    private static bool RayIntersectsRectTransform(RectTransform rectTransform, Ray ray, out Vector3 worldPos, out Vector3 worldNormal) {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        Plane plane = new Plane(corners[0], corners[1], corners[2]);

        float enter;
        if (plane.Raycast(ray, out enter) == false) {
            worldPos = Vector3.zero;
            worldNormal = Vector3.zero;
            return false;
        }

        Vector3 intersection = ray.GetPoint(enter);
        Vector3 bottomEdge = corners[3] - corners[0];
        Vector3 leftEdge = corners[1] - corners[0];
        float bottomDot = Vector3.Dot(intersection - corners[0], bottomEdge);
        float leftDot = Vector3.Dot(intersection - corners[0], leftEdge);
        if (bottomDot < bottomEdge.sqrMagnitude &&
            leftDot < leftEdge.sqrMagnitude &&
            bottomDot >= 0.0f &&
            leftDot >= 0.0f) {
            worldPos = corners[0] + leftDot * leftEdge / leftEdge.sqrMagnitude + bottomDot * bottomEdge / bottomEdge.sqrMagnitude;
            worldNormal = plane.normal;
            return true;
        }
        else {
            worldPos = Vector3.zero;
            worldNormal = Vector3.zero;
            return false;
        }
    }

    private List<RaycastHit> _raycastResults = new List<RaycastHit>();

    [System.NonSerialized] private Canvas _canvas;
    [SerializeField] private AirVRPointer _pointer;

    private Canvas canvas {
        get {
            if (_canvas != null) {
                return _canvas;
            }
            _canvas = GetComponent<Canvas>();
            return _canvas;
        }
    }

    public AirVRPointer pointer {
        get {
            return _pointer;
        }
    }

    private void graphicRaycast(Canvas canvas, Ray ray, List<RaycastHit> results) {
        var foundGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
        _sortedGraphics.Clear();

        for (int i = 0; i < foundGraphics.Count; i++) {
            Graphic graphic = foundGraphics[i];
            if (graphic.depth == -1) {
                continue;
            }

            Vector3 worldPos, worldNormal;
            if (RayIntersectsRectTransform(graphic.rectTransform, ray, out worldPos, out worldNormal)) {
                Vector2 screenPos = eventCamera.WorldToScreenPoint(worldPos);
                if (graphic.Raycast(screenPos, eventCamera)) {  // check for mask/image intersection (eventAlphaThreshold)
                    RaycastHit hit;
                    hit.graphic = graphic;
                    hit.worldPos = worldPos;
                    hit.worldNormal = worldNormal;
                    hit.fromMouse = false;
                    _sortedGraphics.Add(hit);
                }
            }
        }

        _sortedGraphics.Sort((g1, g2) => g2.graphic.depth.CompareTo(g1.graphic.depth));
        for (int i = 0; i < _sortedGraphics.Count; i++) {
            results.Add(_sortedGraphics[i]);
        }
    }

    private void raycast(PointerEventData eventData, List<RaycastResult> resultAppendList, Ray ray, bool checkForBlocking) {
        if (canvas == null) {
            return;
        }

        float hitDistance = float.MaxValue;
        if (checkForBlocking && blockingObjects != BlockingObjects.None) {
            float dist = eventCamera.farClipPlane;
            if (blockingObjects == BlockingObjects.ThreeD || blockingObjects == BlockingObjects.All) {
                var hits = Physics.RaycastAll(ray, dist, m_BlockingMask);
                if (hits.Length > 0 && hits[0].distance < hitDistance) {
                    hitDistance = hits[0].distance;
                }
            }

            if (blockingObjects == BlockingObjects.TwoD || blockingObjects == BlockingObjects.All) {
                var hits = Physics2D.GetRayIntersectionAll(ray, dist, m_BlockingMask);
                if (hits.Length > 0 && hits[0].fraction * dist < hitDistance) {
                    hitDistance = hits[0].fraction * dist;
                }
            }
        }

        _raycastResults.Clear();
        graphicRaycast(canvas, ray, _raycastResults);

        for (int i = 0; i < _raycastResults.Count; i++) {
            var go = _raycastResults[i].graphic.gameObject;
            bool appendGraphic = true;

            if (ignoreReversedGraphics) {
                var cameraForward = ray.direction;
                var dir = go.transform.rotation * Vector3.forward;
                appendGraphic = Vector3.Dot(cameraForward, dir) > 0;
            }

            if (eventCamera.transform.InverseTransformPoint(_raycastResults[i].worldPos).z <= 0) {
                appendGraphic = false;
            }

            if (appendGraphic) {
                float distance = Vector3.Distance(ray.origin, _raycastResults[i].worldPos);
                if (distance >= hitDistance) {
                    continue;
                }

                var castResult = new RaycastResult {
                    gameObject = go,
                    module = this,
                    distance = distance,
                    index = resultAppendList.Count,
                    depth = _raycastResults[i].graphic.depth,
                    worldPosition = _raycastResults[i].worldPos,
                    worldNormal = _raycastResults[i].worldNormal
                };
                resultAppendList.Add(castResult);
            }
        }
    }

    protected override void OnEnable() {
        base.OnEnable();
        _allRaycasters.Add(this);
    }

    protected override void OnDisable() {
        base.OnDisable();
        _allRaycasters.Remove(this);
    }

    public Vector2 GetScreenPosition(RaycastResult raycastResult) {
        return eventCamera.WorldToScreenPoint(raycastResult.worldPosition);
    }

    // overrides GraphicRaycaster
    public override Camera eventCamera {
        get {
            return _pointer.cameraRig.cameras[0];
        }
    }

    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList) {
        if (eventData.IsVRPointer()) {
            if (_pointer.interactable) {
                raycast(eventData, resultAppendList, eventData.GetRay(), true);
            }
        }
        else {
            base.Raycast(eventData, resultAppendList);
        }
    }
}
