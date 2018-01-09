/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(AirVRPointer))]

public class AirVRPhysicsRaycaster : BaseRaycaster {
    private static List<AirVRPhysicsRaycaster> _allRaycasters = new List<AirVRPhysicsRaycaster>();

    public static List<AirVRPhysicsRaycaster> GetAllRaycasters() {
        return _allRaycasters;
    }

    protected AirVRPhysicsRaycaster() { }

    private AirVRPointer _pointer;

    [SerializeField]
    protected LayerMask _eventMask = -1;

    public AirVRPointer pointer {
        get {
            return _pointer;
        }
    }

    public override Camera eventCamera {
        get {
            return _pointer.cameraRig.cameras[0];
        }
    }

    public virtual int depth {
        get {
            return (int)eventCamera.depth;
        }
    }

    public int finalEventMask {
        get {
            return eventCamera.cullingMask & _eventMask;
        }
    }

    public LayerMask eventMask {
        get {
            return _eventMask;
        }
        set {
            _eventMask = value;
        }
    }

    protected override void OnEnable() {
        _pointer = GetComponent<AirVRPointer>();

        base.OnEnable();
        _allRaycasters.Add(this);
    }

    protected override void OnDisable() {
        base.OnDisable();
        _allRaycasters.Remove(this);
    }

    public Vector2 GetScreenPosition(Vector3 worldPosition) {
        return eventCamera.WorldToScreenPoint(worldPosition);
    }

    // overrides BaseRaycaster
    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList) {
        if (eventData.IsVRPointer() == false || _pointer.interactable == false) {
            return;
        }

        var ray = _pointer.GetWorldRay();
        float dist = eventCamera.farClipPlane - eventCamera.nearClipPlane;
        var hits = Physics.RaycastAll(ray, dist, finalEventMask);

        if (hits.Length > 0) {
            if (hits.Length > 1) {
                System.Array.Sort(hits, (r1, r2) => r1.distance.CompareTo(r2.distance));
            }

            for (int i = 0; i < hits.Length; i++) {
                var result = new RaycastResult {
                    gameObject = hits[i].collider.gameObject,
                    module = this,
                    distance = hits[i].distance,
                    index = resultAppendList.Count,
                    worldPosition = hits[0].point,
                    worldNormal = hits[0].normal
                };
                resultAppendList.Add(result);
            }
        }
    }
}
