/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using TMPro;
using System.Text;
namespace cowsins
{
    public class GetGameInformation : MonoBehaviour
    {
        //FPS
        public bool showFPS;

        public bool showAverageFrameRate, showMaximumFrameRate;

        [SerializeField, Range(.01f, 1f)] private float fpsRefreshRate;

        [SerializeField] private TextMeshProUGUI fpsObject;

        [SerializeField] private Color appropriateValueColor, intermediateValueColor, badValueColor;

        private float fpsTimer;

        private float fps, avgFPS, maxFps;

        private string text = "";

        private int lastFrameIndex;
        private float[] frameDeltaTimeArray;

        private void Start()
        {
            if (showFPS)
                fpsTimer = fpsRefreshRate;
            else
                Destroy(fpsObject);

            frameDeltaTimeArray = new float[50];
        }

        private void Update()
        {
            if (!showFPS) return;

            frameDeltaTimeArray[lastFrameIndex] = Time.deltaTime;
            lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;
            fps = CalculateFramerate();

            fpsTimer -= Time.deltaTime;

            if (fpsTimer <= 0)
            {
                text = "";
                avgFPS = Time.frameCount / Time.time;

                if (fps > maxFps) maxFps = fps;

                fpsTimer = fpsRefreshRate;

                StringBuilder sb = new StringBuilder();

                // Append the current FPS
                sb.Append("Current FPS: ").Append(GetColoredFPSText(fps)).AppendLine();

                // Append the avg FPS if required
                if (showAverageFrameRate)
                    sb.Append("Avg FPS: ").Append(GetColoredFPSText(avgFPS)).AppendLine();

                // Append the maximum FPS if required
                if (showMaximumFrameRate)
                    sb.Append("Max FPS: ").Append(GetColoredFPSText(maxFps));

                // Convert the StringBuilder to a string when needed
                text = sb.ToString();

                fpsObject.text = text;
            }

        }

        private float CalculateFramerate()
        {
            float total = 0;
            foreach (float deltaTime in frameDeltaTimeArray)
            {
                total += deltaTime;
            }
            return frameDeltaTimeArray.Length / total;
        }

        private string GetColoredFPSText(float fps)
        {
            Color fpsColor;

            if (fps < 15f)
            {
                fpsColor = badValueColor;
            }
            else if (fps < 45f)
            {
                fpsColor = intermediateValueColor;
            }
            else
            {
                fpsColor = appropriateValueColor;
            }

            return "<color=#" + ColorUtility.ToHtmlStringRGB(fpsColor) + ">" + fps.ToString("F0") + "</color>";
        }
    }
#if UNITY_EDITOR
    [System.Serializable]
    [CustomEditor(typeof(GetGameInformation))]
    public class GetGameInformatioEditor : Editor
    {

        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            GetGameInformation myScript = target as GetGameInformation;

            EditorGUILayout.LabelField("FPS", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showFPS"));
            if (myScript.showFPS)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fpsRefreshRate"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fpsObject"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("showAverageFrameRate"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("showMaximumFrameRate"));
            }
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("COLOR", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("appropriateValueColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intermediateValueColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("badValueColor"));

            serializedObject.ApplyModifiedProperties();

        }
    }
#endif
}