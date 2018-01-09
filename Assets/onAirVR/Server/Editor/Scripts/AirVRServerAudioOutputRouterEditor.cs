/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEditor;

[CustomEditor(typeof(AirVRServerAudioOutputRouter))]
public class AirVRServerAudioOutputRouterEditor : Editor {
    private SerializedProperty propInput;
    private SerializedProperty propOutput;
    private SerializedProperty propTargetAudioMixer;
    private SerializedProperty propExposedRendererIDParameterName;
    private SerializedProperty propTargetCameraRig;

    private void OnEnable() {
        propInput = serializedObject.FindProperty("input");
        propOutput = serializedObject.FindProperty("output");
        propTargetAudioMixer = serializedObject.FindProperty("targetAudioMixer");
        propExposedRendererIDParameterName = serializedObject.FindProperty("exposedRendererIDParameterName");
        propTargetCameraRig = serializedObject.FindProperty("targetCameraRig");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(propInput);
        if (propInput.enumValueIndex == (int)AirVRServerAudioOutputRouter.Input.AudioPlugin) {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.PropertyField(propTargetAudioMixer);
            EditorGUILayout.PropertyField(propExposedRendererIDParameterName);
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.PropertyField(propOutput);
        if (propOutput.enumValueIndex == (int)AirVRServerAudioOutputRouter.Output.One) {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.PropertyField(propTargetCameraRig);
            EditorGUILayout.EndVertical();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
