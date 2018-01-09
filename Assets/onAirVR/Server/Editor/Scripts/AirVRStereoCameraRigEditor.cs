/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEditor;

[CustomEditor(typeof(AirVRStereoCameraRig))]

public class AirVRStereoCameraRigEditor : Editor {
    private SerializedProperty propTrackingModel;
    private SerializedProperty propExternalTrackingOrigin;
    private SerializedProperty propExternalTracker;

    private void OnEnable() {
        propTrackingModel = serializedObject.FindProperty("trackingModel");
        propExternalTrackingOrigin = serializedObject.FindProperty("externalTrackingOrigin");
        propExternalTracker = serializedObject.FindProperty("externalTracker");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(propTrackingModel);
        if (propTrackingModel.enumValueIndex == (int)AirVRStereoCameraRig.TrackingModel.ExternalTracker) {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(propExternalTrackingOrigin);
            EditorGUILayout.PropertyField(propExternalTracker);
            EditorGUILayout.EndVertical();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
