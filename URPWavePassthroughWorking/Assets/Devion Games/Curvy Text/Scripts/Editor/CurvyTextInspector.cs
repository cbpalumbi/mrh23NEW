using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DevionGames
{
    [CustomEditor(typeof(CurvyText))]
    public class CurvyTextInspector : UnityEditor.UI.TextEditor
    {
        private SerializedProperty m_Resolution;
        private SerializedProperty m_Offset;

        protected override void OnEnable()
        {
            base.OnEnable();
            this.m_Resolution = serializedObject.FindProperty("m_Resolution");
            this.m_Offset = serializedObject.FindProperty("m_Offset");

        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Curve", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(m_Resolution);
            EditorGUILayout.PropertyField(m_Offset);
            EditorGUI.indentLevel -= 1;
            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("GameObject/UI/Curvy Text", false, 10)]
        static void CreateCurvyText(MenuCommand menuCommand)
        {
            GameObject selected = Selection.activeGameObject;
            Canvas canvas;
            if (selected != null){
                canvas = selected.GetComponentInParent<Canvas>();
                if (canvas != null) {
                    CreateCurvyText(selected);
                    return;
                }
            }else {
                canvas = FindObjectOfType<Canvas>();
            }

            if (canvas == null) {
                canvas = CreateCanvas();
            }

            CreateCurvyText(canvas.gameObject);
           
        }


        private static void CreateCurvyText(GameObject canvas)
        {
            GameObject go = CreateUIElementRoot("Text", new Vector2(160f, 30f));
            BezierCurve curve = go.AddComponent<BezierCurve>();
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            Vector3 size = new Vector3(rectTransform.rect.width * 160, rectTransform.rect.height);
            Vector3 pivotOffset = new Vector3(rectTransform.pivot.x * size.x, rectTransform.pivot.y * size.y, 0);
            curve.Add(new Vector3(-80f,-15f,0f));
            curve.Add(new Vector3(80f,-15f,0f));

            go.transform.SetParent(canvas.transform, false);
           


            CurvyText lbl = go.AddComponent<CurvyText>();
            lbl.text = "New Text";
          

        }

        private static Canvas CreateCanvas() {
            GameObject go = new GameObject("Canvas");
           Canvas canvas = go.AddComponent<Canvas>();
            canvas.pixelPerfect = true;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static GameObject CreateUIElementRoot(string name, Vector2 size)
        {
            GameObject child = new GameObject(name);
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            return child;
        }
    }
}