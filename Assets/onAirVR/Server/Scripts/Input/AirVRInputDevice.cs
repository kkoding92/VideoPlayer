/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class AirVRInputDevice : AirVRInputReceiver {
    private abstract class Control {
        public Axis3D AsAxis3D() {
            return this as Axis3D;
        }

        public Axis2D AsAxis2D() {
            return this as Axis2D;
        }

        public Axis AsAxis() {
            return this as Axis;
        }

        public Button AsButton() {
            return this as Button;
        }

        public Xform AsTransform() {
            return this as Xform;
        }

        public Touch AsTouch() {
            return this as Touch;
        }

        public Orientation AsOrientation() {
            return this as Orientation;
        }

        public abstract void PollInput(AirVRInputDevice device, AirVRInputStream inputStream, byte id);
    }

    private class Axis3D : Control {
        public Vector3 value { get; set; }

        public override void PollInput(AirVRInputDevice device, AirVRInputStream inputStream, byte id) {
            value = inputStream.GetVector3D(device, id);
        }
    }

    private class Axis2D : Control {
        public Vector2 value { get; set; }

        public override void PollInput(AirVRInputDevice device, AirVRInputStream inputStream, byte id) {
            value = inputStream.GetVector2D(device, id);
        }
    }

    private class Axis : Control {
        public float value { get; set; }

        public override void PollInput(AirVRInputDevice device, AirVRInputStream inputStream, byte id) {
            value = inputStream.GetAxis(device, id);
        }
    }

    private class Button : Control {
        private float _prev;
        private float _current;

        public float value {
            get {
                return _current;
            }
            set {
                _prev = _current;
                _current = value;
            }
        }

        public bool IsDown() {
            return _prev == 0.0f && _current != 0.0f;
        }

        public bool IsUp() {
            return _prev != 0.0f && _current == 0.0f;
        }

        public override void PollInput(AirVRInputDevice device, AirVRInputStream inputStream, byte id) {
            value = inputStream.GetButton(device, id);
        }
    }

    private class Orientation : Control {
        public Quaternion value { get; set; }

        public override void PollInput(AirVRInputDevice device, AirVRInputStream inputStream, byte id) {
            value = inputStream.GetQuaternion(device, id);
        }
    }

    private class Xform : Control {
        private Vector3 _position;
        private Quaternion _orientation;

        public Vector3 position {
            get {
                return _position;
            }
        }

        public Quaternion orientation {
            get {
                return _orientation;
            }
        }

        public override void PollInput(AirVRInputDevice device, AirVRInputStream inputStream, byte id) {
            inputStream.GetTransform(device, id, out _position, out _orientation);
        }
    }

    private class Touch : Control {
        private Vector2 _position;
        private bool _touch;

        public Vector2 position {
            get {
                return _position;
            }
        }

        public bool touch {
            get {
                return _touch;
            }
        }

        public override void PollInput(AirVRInputDevice device, AirVRInputStream inputStream, byte id) {
            inputStream.GetTouch(device, id, out _position, out _touch);
        }
    }

    public AirVRInputDevice() {
        _controls = new Dictionary<byte, Control>();
        _extControls = new Dictionary<byte, Control>();

        MakeControlList();
    }

    private Dictionary<byte, Control> _controls;
    private Dictionary<byte, Control> _extControls;

    private Control findControl(byte controlID) {
        return _controls.ContainsKey(controlID) ? _controls[controlID] :
               _extControls.ContainsKey(controlID) ? _extControls[controlID] : 
                                                     null;
    }

    protected abstract string deviceName { get; }
    protected abstract void MakeControlList();

    protected virtual void UpdateExtendedControls() {}

    protected void AddControlTouch(byte controlID) {
        _controls.Add(controlID, new Touch());
    }

    protected void AddControlTransform(byte controlID) {
        _controls.Add(controlID, new Xform());
    }

    protected void AddControlOrientation(byte controlID) {
        _controls.Add(controlID, new Orientation());
    }

    protected void AddControlAxis3D(byte controlID) {
        _controls.Add(controlID, new Axis3D());
    }

    protected void AddControlAxis2D(byte controlID) {
        _controls.Add(controlID, new Axis2D());
    }

    protected void AddControlAxis(byte controlID) {
        _controls.Add(controlID, new Axis());
    }

    protected void AddControlButton(byte controlID) {
        _controls.Add(controlID, new Button());
    }

    protected void AddExtControlAxis3D(byte controlID) {
        _extControls.Add(controlID, new Axis3D());
    }

    protected void AddExtControlAxis2D(byte controlID) {
        _extControls.Add(controlID, new Axis2D());
    }

    protected void AddExtControlAxis(byte controlID) {
        _extControls.Add(controlID, new Axis());
    }

    protected void AddExtControlButton(byte controlID) {
        _extControls.Add(controlID, new Button());
    }

    protected void SetExtControlAxis3D(byte controlID, Vector3 value) {
        Assert.IsTrue(_extControls.ContainsKey(controlID));
        _extControls[controlID].AsAxis3D().value = value;
    }

    protected void SetExtControlAxis2D(byte controlID, Vector2 value) {
        Assert.IsTrue(_extControls.ContainsKey(controlID));
        _extControls[controlID].AsAxis2D().value = value;
    }

    protected void SetExtControlAxis(byte controlID, float value) {
        Assert.IsTrue(_extControls.ContainsKey(controlID));
        _extControls[controlID].AsAxis().value = value;
    }

    protected void SetExtControlButton(byte controlID, float value) {
        Assert.IsTrue(_extControls.ContainsKey(controlID));
        _extControls[controlID].AsButton().value = value;
    }

    public bool GetTouch(byte controlID, ref Vector2 position, ref bool touch) {
        Control control = findControl(controlID);
        if (control != null) {
            position = control.AsTouch().position;
            touch = control.AsTouch().touch;
            return true;
        }
        return false;
    }

    public bool GetTransform(byte controlID, ref Vector3 position, ref Quaternion orientation) {
        Control control = findControl(controlID);
        if (control != null) {
            position = control.AsTransform().position;
            orientation = control.AsTransform().orientation;
            return true;
        }
        return false;
    }

    public Quaternion GetOrientation(byte controlID) {
        Control control = findControl(controlID);
        return control != null ? control.AsOrientation().value : Quaternion.identity;
    }

    public Vector3 GetAxis3D(byte controlID) {
        Control control = findControl(controlID);
        return control != null ? control.AsAxis3D().value : Vector3.zero;
    }

    public Vector2 GetAxis2D(byte controlID) {
        Control control = findControl(controlID);
        return control != null ? control.AsAxis2D().value : Vector2.zero;
    }

    public float GetAxis(byte controlID) {
        Control control = findControl(controlID);
        return control != null ? control.AsAxis().value : 0.0f;
    }

    public float GetButtonRaw(byte controlID) {
        Control control = findControl(controlID);
        return control != null ? control.AsButton().value : 0.0f;
    }

    public bool GetButton(byte controlID) {
        return GetButtonRaw(controlID) != 0.0f;
    }

    public bool GetButtonDown(byte controlID) {
        Control control = findControl(controlID);
        return control != null ? control.AsButton().IsDown() : false;
    }

    public bool GetButtonUp(byte controlID) {
        Control control = findControl(controlID);
        return control != null ? control.AsButton().IsUp() : false;
    }

    // implements IAirVRInputReceiver
    public override string name {
        get {
            return deviceName;
        }
    }
    
    public override void PollInputsPerFrame(AirVRInputStream inputStream) {
        foreach (var controlID in _controls.Keys) {
            _controls[controlID].PollInput(this, inputStream, controlID);
        }
        UpdateExtendedControls();
    }
}
