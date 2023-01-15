using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevionGames
{
    [CustomEditor(typeof(BezierCurve))]
    public class BezierCurveInspector : Editor
    {


        public override void OnInspectorGUI()
        {
            BezierCurve curve = (BezierCurve)target;

            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck()) {
                curve.SendMessage("OnRectTransformDimensionsChange", SendMessageOptions.DontRequireReceiver);
            }

           

            Rect rect = GUILayoutUtility.GetRect(new GUIContent("Approximate"), "Dropdown", GUILayout.ExpandWidth(true));

            if (GUI.Button(rect,"Approximate","Dropdown")) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Circle"), false, delegate () {

                    float c = 55.1915024494f;
                    float a = 100f;

                    BezierPoint p1 = new BezierPoint(curve);
                    p1.localPosition = new Vector3(0f, a);
                    p1.handle1 = new Vector3(c, a);
                    p1.handle2 = new Vector3(c, a);

                    BezierPoint p2 = new BezierPoint(curve);
                    p2.localPosition = new Vector3(a, 0f);
                    p2.handle1 = new Vector3(a, c);
                    p2.handle2 = new Vector3(a, -c);

                    BezierPoint p3 = new BezierPoint(curve);
                    p3.localPosition = new Vector3(0f,-a);
                    p3.handle1 = new Vector3(c,-a);
                    p3.handle2 = new Vector3(-c,-a);

                    BezierPoint p4 = new BezierPoint(curve);
                    p4.localPosition = new Vector3( -a, 0f);
                    p4.handle1 = new Vector3(-a, -c);
                    p4.handle2 = new Vector3(-a, c);

                    BezierPoint p5 = new BezierPoint(curve);
                    p5.localPosition = new Vector3(0f, a);
                    p5.handle1 = new Vector3(-c, a);
                    p5.handle2 = new Vector3(c, a);

                    curve.Clear();
                    curve.Add(p1);
                    curve.Add(p2);
                    curve.Add(p3);
                    curve.Add(p4);
                    curve.Add(p5);
                    EditorUtility.SetDirty(curve);
                    curve.SendMessage("OnRectTransformDimensionsChange", SendMessageOptions.DontRequireReceiver);
                });

                menu.AddItem(new GUIContent("Arc"), false, delegate () {
                    float c = 55.1915024494f;
                    float a = 100f;

                    BezierPoint p1 = new BezierPoint(curve);
                    p1.localPosition = new Vector3(-a, 0f);
                    p1.handle1 = new Vector3(-a, -c);
                    p1.handle2 = new Vector3(-a, c);

                    BezierPoint p2 = new BezierPoint(curve);
                    p2.localPosition = new Vector3(0f, a);
                    p2.handle1 = new Vector3(-c, a);
                    p2.handle2 = new Vector3(c, a);

                    BezierPoint p3 = new BezierPoint(curve);
                    p3.localPosition = new Vector3(a, 0f);
                    p3.handle1 = new Vector3(a, c);
                    p3.handle2 = new Vector3(a, -c);

                    curve.Clear();
                    curve.Add(p1);
                    curve.Add(p2);
                    curve.Add(p3);
                    EditorUtility.SetDirty(curve);
                    curve.SendMessage("OnRectTransformDimensionsChange", SendMessageOptions.DontRequireReceiver);
                });
                menu.DropDown(rect);
            }
        }


        private void OnSceneGUI()
        {
            BezierCurve curve = (BezierCurve)target;
         
            DrawCurve(curve);
            DrawHandles(curve);
            DrawPoints(curve);
            
        }

        private void DrawCurve( BezierCurve curve)
        {
            Color color = Handles.color;
            Handles.color = Color.white;
            //int limit = curve.resolution + 1;
            //float _res = curve.resolution;
            if (curve.count > 1)
            {
                for (int index = 0; index < curve.count - 1; index++)
                {
                    BezierPoint p1 = curve[index];
                    BezierPoint p2 = curve[index + 1];
                    Vector3 lastPoint = p1.localPosition;
                    Vector3 currentPoint = Vector3.zero;
                    Handles.DrawBezier(curve.transform.TransformPoint(p1.localPosition), curve.transform.TransformPoint(p2.localPosition), curve.transform.TransformPoint(p1.handle2), curve.transform.TransformPoint(p2.handle1), Color.white, null, 2f);
                   /* for (int i = 1; i < limit; i++)
                    {
                        currentPoint = BezierCurve.GetPoint(p1, p2, i / _res);
                        Handles.DrawAAPolyLine(curve.transform.TransformPoint(lastPoint), curve.transform.TransformPoint(currentPoint));
                        lastPoint = currentPoint;
                    }*/
                }

        
            }
            Handles.color = color;
        }

        private void DrawPoints(BezierCurve curve)
        {
            Color color = Handles.color;
            Handles.color = Color.white;
            for (int i = 0; i < curve.count; i++)
            {
                BezierPoint point = curve[i];
                Handles.DotHandleCap(0, curve.transform.TransformPoint(point.localPosition), Quaternion.identity, HandleUtility.GetHandleSize(point.localPosition) * 0.05f, EventType.Repaint);
            }
            Handles.color = color;
        }

        private void DrawHandles(BezierCurve curve)
        {
            Color color = Handles.color;
            Handles.color = Color.green;
            for (int i = 0; i < curve.count; i++)
            {
                BezierPoint point = curve[i];
                if(i > 0)
                    Handles.DrawAAPolyLine(curve.transform.TransformPoint(point.localPosition), curve.transform.TransformPoint(point.handle1));
                if(i < curve.count-1)
                    Handles.DrawAAPolyLine(curve.transform.TransformPoint(point.localPosition), curve.transform.TransformPoint(point.handle2));
            }

            Handles.color = Color.cyan;
            for (int i = 0; i < curve.count; i++)
            {
                BezierPoint point = curve[i];
                if (point.handle1 != point.localPosition && i > 0)
                    Handles.SphereHandleCap(0, curve.transform.TransformPoint(point.handle1), Quaternion.identity, HandleUtility.GetHandleSize(point.handle1) * 0.075f, EventType.Repaint);
                if (point.handle2 != point.localPosition && i < curve.count-1)
                    Handles.SphereHandleCap(0, curve.transform.TransformPoint(point.handle2), Quaternion.identity, HandleUtility.GetHandleSize(point.handle2) * 0.075f, EventType.Repaint);
            }
            Handles.color = color;
        }
    }
}