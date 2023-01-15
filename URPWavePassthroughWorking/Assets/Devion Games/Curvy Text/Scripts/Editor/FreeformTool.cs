using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace DevionGames
{
    [EditorTool("Freeform", typeof(BezierCurve))]
    public class FreeformTool : EditorTool
    {
        protected GUIContent m_IconContent;

        public override GUIContent toolbarIcon
        {
            get
            {
                if (m_IconContent == null)
                {
                    m_IconContent = new GUIContent()
                    {
                        image = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Devion Games/Curvy Text/Scripts/Editor/Icons/Freeform.png"),
                        text = "Freeform",
                        tooltip = "Freeform"
                    };
                }
                return m_IconContent;
            }
        }

        protected Texture2D m_FreeformCursor;

        public Texture2D freeformCursor
        {
            get
            {
                if (m_FreeformCursor == null)
                {
                    m_FreeformCursor = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Devion Games/Curvy Text/Scripts/Editor/Icons/Cursor Freeform.png");
                }
                return m_FreeformCursor;
            }
        }

        protected Texture2D m_RemoveCursor;

        public Texture2D removeCursor
        {
            get
            {
                if (m_RemoveCursor == null)
                {
                    m_RemoveCursor = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Devion Games/Curvy Text/Scripts/Editor/Icons/Cursor Remove.png");
                }
                return m_RemoveCursor;
            }
        }

        protected Texture2D m_InsertCursor;

        public Texture2D insertCursor
        {
            get
            {
                if (m_InsertCursor == null)
                {
                    m_InsertCursor = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Devion Games/Curvy Text/Scripts/Editor/Icons/Cursor Insert.png");
                }
                return m_InsertCursor;
            }
        }

        protected Texture2D m_TransformCursor;

        public Texture2D transformCursor
        {
            get
            {
                if (m_TransformCursor == null)
                {
                    m_TransformCursor = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Devion Games/Curvy Text/Scripts/Editor/Icons/Cursor Transform.png");
                }
                return m_TransformCursor;
            }
        }

        protected BezierCurve bezierCurve
        {
            get { return (BezierCurve)target; }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            Event currentEvent = Event.current;
            if (currentEvent.control){
                DoTransformTool();
            }  else {
                SelectTool();
                if (m_Tool == Tool.Remove){
                    DoRemoveTool();
                } else if (m_Tool == Tool.Insert) {
                    DoInsertTool();
                }else{
                    DoAddTool();
                }
            }
        }

        protected BezierPoint m_Point;
        protected int m_Handle;
        protected Tool m_Tool;

        private void SelectTool() {
            Event currentEvent = Event.current;

            if (currentEvent.type== EventType.MouseMove)
            {
                if (GetInsertPointAtMousePosition())
                {
                    m_Tool = Tool.Insert;
                }
                else if (GetPointAtMousePosition() != null)
                {
                    m_Tool = Tool.Remove;
                }
                else {
                    m_Tool = Tool.Add; 
                }
            }
        }

        protected void DoAddTool() {
            Event currentEvent = Event.current;
            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    GUIUtility.hotControl = 0;
                    Plane plane = new Plane(Camera.current.transform.forward, Vector3.zero);
                    float dist = 0f;
                    Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
                    if (plane.Raycast(ray, out dist))
                    {
                        Vector3 newPosition = ray.GetPoint(dist);
                        bezierCurve.Add(bezierCurve.transform.InverseTransformPoint(newPosition));
                        m_Point = bezierCurve[bezierCurve.count - 1];
                        m_Handle = 0;
                    }
                    bezierCurve.SendMessage("OnRectTransformDimensionsChange", SendMessageOptions.DontRequireReceiver);
                    SceneView.RepaintAll();
                    EditorUtility.SetDirty(bezierCurve);
                    currentEvent.Use();
                    break;
                case EventType.MouseDrag:
                    DoTransformTool();
                    break;
                case EventType.MouseMove:
                    SceneView.RepaintAll();
                    break;
            }
            Cursor.SetCursor(freeformCursor, new Vector2(9, 9), CursorMode.Auto);
            EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.CustomCursor);
        }

        protected void DoRemoveTool() {
            Event currentEvent = Event.current;
            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    GUIUtility.hotControl = 0;
                    bezierCurve.Remove(GetPointAtMousePosition());
                    bezierCurve.SendMessage("OnRectTransformDimensionsChange", SendMessageOptions.DontRequireReceiver);
                    EditorUtility.SetDirty(bezierCurve);
                    SceneView.RepaintAll();
                    currentEvent.Use();
                    break;
                case EventType.MouseMove:
                    SceneView.RepaintAll();
                    break;
            }
            Cursor.SetCursor(removeCursor, new Vector2(3, 6), CursorMode.Auto);
            EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.CustomCursor);
        }

        protected void DoInsertTool() {
            Event currentEvent = Event.current;
            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    GUIUtility.hotControl = 0;
                    int index;
                    Vector3 position;
                    GetInsertPointAtMousePosition(out index, out position);
                    m_Point = bezierCurve.Insert(index, position);
                    m_Handle = 0;
                    bezierCurve.SendMessage("OnRectTransformDimensionsChange", SendMessageOptions.DontRequireReceiver);
                    EditorUtility.SetDirty(bezierCurve);
                    SceneView.RepaintAll();
                    currentEvent.Use();
                    break;
                case EventType.MouseDrag:
                    DoTransformTool();
                    break;
                case EventType.MouseMove:
                    SceneView.RepaintAll();
                    break;
            }
            Cursor.SetCursor(insertCursor, new Vector2(3, 6), CursorMode.Auto);
            EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.CustomCursor);
        }

        protected void DoTransformTool() {
            Event currentEvent = Event.current;

            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    GUIUtility.hotControl = 0;
                    for (int index = 0; index < bezierCurve.count; index++)
                    {
                        BezierPoint p = bezierCurve[index];
                        if (ContainsMousePosition(p.localPosition))
                        {
                            if (currentEvent.control)
                            {
                                m_Handle = -1;
                            }
                            else
                            {
                                p.handle1 = p.localPosition;
                                p.handle2 = p.localPosition;
                                m_Handle = 0;
                                bezierCurve.SendMessage("OnRectTransformDimensionsChange", SendMessageOptions.DontRequireReceiver);
                            }
                            m_Point = p;
                        }
                        else if (ContainsMousePosition(p.handle1))
                        {
                            m_Handle = 1;
                            m_Point = p;
                        }
                        else if (ContainsMousePosition(p.handle2))
                        {
                            m_Handle = 2;
                            m_Point = p;
                        }
                    }
                    currentEvent.Use();
                    break;
                case EventType.MouseUp:
                    m_Point = null;
                    break;
                case EventType.MouseDrag:
                    if (m_Point != null)
                    {
                        Plane plane = new Plane(Camera.current.transform.forward, Vector3.zero);
                        float dist = 0f;
                        Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
                        if (plane.Raycast(ray, out dist))
                        {
                            Vector3 newPosition = bezierCurve.transform.InverseTransformPoint(ray.GetPoint(dist));
                            switch (m_Handle)
                            {
                                case -1:
                                    Vector3 diff = (newPosition - m_Point.localPosition);
                                    m_Point.localPosition = newPosition;
                                    m_Point.handle1 += diff;
                                    m_Point.handle2 += diff;
                                    break;
                                case 0:
                                    if (bezierCurve[0] == m_Point)
                                    {
                                        m_Point.handle2 = newPosition;
                                        m_Point.handle1 = -(m_Point.handle2 - m_Point.localPosition) + m_Point.localPosition;
                                    }
                                    else
                                    {
                                        m_Point.handle1 = newPosition;
                                        m_Point.handle2 = -(m_Point.handle1 - m_Point.localPosition) + m_Point.localPosition;
                                    }
                                    break;
                                case 1:
                                    m_Point.handle1 = newPosition;
                                    if (currentEvent.control)
                                    {
                                        m_Point.handle2 = -(m_Point.handle1 - m_Point.localPosition) + m_Point.localPosition;
                                    }
                                    break;
                                case 2:
                                    m_Point.handle2 = newPosition;
                                    if (currentEvent.control)
                                    {
                                        m_Point.handle1 = -(m_Point.handle2 - m_Point.localPosition) + m_Point.localPosition;
                                    }
                                    break;
                            }
                            bezierCurve.SendMessage("OnRectTransformDimensionsChange", SendMessageOptions.DontRequireReceiver);
                            EditorUtility.SetDirty(bezierCurve);
                            SceneView.RepaintAll();
                        }
                    }
                    currentEvent.Use();
                    break;
                case EventType.MouseMove:
                    SceneView.RepaintAll();
                    break;
            }

            for (int index = 0; index < bezierCurve.count; index++)
            {
                BezierPoint p = bezierCurve[index];
                if (ContainsMousePosition(p.localPosition) || ContainsMousePosition(p.handle1) || ContainsMousePosition(p.handle2))
                {
                    Cursor.SetCursor(transformCursor, new Vector2(13, 8), CursorMode.Auto);
                    EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.CustomCursor);

                }
            }

        }

        protected bool GetInsertPointAtMousePosition()
        {
            int index = -1;
            Vector3 position = Vector3.zero;
            return GetInsertPointAtMousePosition(out index, out position);
        }

        protected bool GetInsertPointAtMousePosition(out int index, out Vector3 position)
        {
            int limit = 31;
            float _res = 30;
            index = -1;
            position = Vector3.zero;

            if (bezierCurve.count > 1)
            {
                for (int j = 0; j < bezierCurve.count - 1; j++)
                {
                    BezierPoint p1 = bezierCurve[j];
                    BezierPoint p2 = bezierCurve[j + 1];
                    Vector3 lastPoint = p1.localPosition;
                    Vector3 currentPoint = Vector3.zero;

                    for (int i = 1; i < limit; i++)
                    {
                        currentPoint = BezierCurve.GetPoint(p1, p2, i / _res);
                        if (HandleUtility.DistanceToLine(bezierCurve.transform.TransformPoint(currentPoint), bezierCurve.transform.TransformPoint(lastPoint)) < 2f && Vector2.Distance(Event.current.mousePosition, HandleUtility.WorldToGUIPoint(bezierCurve.transform.TransformPoint(p1.localPosition))) > 5f && Vector2.Distance(Event.current.mousePosition, HandleUtility.WorldToGUIPoint(bezierCurve.transform.TransformPoint(p2.localPosition))) > 5f)
                        {
                            index = j + 1;
                            position = HandleUtility.ClosestPointToPolyLine(lastPoint, currentPoint);
                            return true;
                        }
                        lastPoint = currentPoint;
                    }
                }
            }
            

            return false;
        }

        /// <summary>
        /// Get a BezierPoint at mouse position
        /// </summary>
        /// <param name="maxDistance"></param>
        /// <returns>BezierPoint at mouse position</returns>
        protected BezierPoint GetPointAtMousePosition(bool checkHandle = false)
        {
            for (int index = 0; index < bezierCurve.count; index++)
            {
                BezierPoint p = bezierCurve[index];
                if (checkHandle && (ContainsMousePosition(p.handle1) || ContainsMousePosition(p.handle2)) || ContainsMousePosition(p.localPosition))
                {
                    return p;
                }
            }
            return null;
        }

        protected bool ContainsMousePosition(Vector3 position)
        {
            Vector2 guiPoint = HandleUtility.WorldToGUIPoint(bezierCurve.transform.TransformPoint(position));
            if (Vector2.Distance(Event.current.mousePosition, guiPoint) < 5f)
            {
                return true;
            }
            return false;
        }

        public enum Tool
        {
            None,
            Add,
            Remove,
            Insert
        }

        /* private Tool m_Tool = Tool.Freeform;
             protected BezierPoint m_TransformPoint=null;

             public override void OnToolGUI(EditorWindow window)
             {
                 DrawCursor(m_Tool);

                 int controlID = GUIUtility.GetControlID(FocusType.Passive);
                 Event currentEvent = Event.current;

                 if (currentEvent.control) {
                     m_Tool = Tool.Transform;
                 }else if (m_Tool == Tool.Transform && !currentEvent.control) {
                     m_Tool = Tool.Freeform;
                     m_TransformPoint = null;
                 }

                 switch (currentEvent.GetTypeForControl(controlID))
                 {
                     case EventType.Layout:
                         HandleUtility.AddDefaultControl(controlID);
                         break;
                     case EventType.MouseDown:
                         if (currentEvent.button == 0)
                         {
                             GUIUtility.hotControl = controlID;

                             if (m_Tool == Tool.Freeform)
                             {
                                 Plane plane = new Plane(Camera.current.transform.forward, Vector3.zero);
                                 float dist = 0f;
                                 Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
                                 if (plane.Raycast(ray, out dist))
                                 {
                                     Vector3 newPosition = ray.GetPoint(dist);
                                     bezierCurve.Add(bezierCurve.transform.InverseTransformPoint(newPosition));
                                     m_TransformPoint = bezierCurve[bezierCurve.count - 1];
                                     m_Tool = Tool.TransformHandle;
                                 }

                             }
                             else if (m_Tool == Tool.Insert)
                             {
                                 int index;
                                 Vector3 position;
                                 GetInsertPointAtMousePosition(out index, out position);
                                 bezierCurve.Insert(index, position);
                                 m_TransformPoint = bezierCurve[index];
                                 m_Tool = Tool.TransformHandle;
                             }
                             else if (m_Tool == Tool.Remove)
                             {
                                 bezierCurve.Remove(GetPointAtMousePosition());
                             }else if (m_Tool == Tool.Transform) {
                                 m_TransformPoint = GetPointAtMousePosition();
                             }

                             EditorUtility.SetDirty(bezierCurve);
                             currentEvent.Use();
                         }
                         break;
                     case EventType.MouseUp:
                         GUIUtility.hotControl = 0;
                         m_Tool = Tool.Freeform;
                         m_TransformPoint = null;
                         currentEvent.Use();
                         break;   
                     case EventType.MouseDrag:
                         if (GUIUtility.hotControl == controlID)
                         {
                             Plane plane = new Plane(Camera.current.transform.forward, Vector3.zero);
                             float dist = 0f;
                             Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
                             if (plane.Raycast(ray, out dist))
                             {
                                 Vector3 newPosition = bezierCurve.transform.InverseTransformPoint(ray.GetPoint(dist));
                                 if (m_Tool == Tool.TransformHandle)
                                 {
                                     m_TransformPoint.handle1 = newPosition;
                                     if (!currentEvent.alt)
                                         m_TransformPoint.handle2 = -(m_TransformPoint.handle1 - m_TransformPoint.localPosition) + m_TransformPoint.localPosition;
                                 }
                                 else if (m_Tool == Tool.Transform && m_TransformPoint != null) {
                                     Vector3 diff = newPosition - m_TransformPoint.localPosition;
                                     m_TransformPoint.localPosition = newPosition;
                                     m_TransformPoint.handle1 += diff;
                                     m_TransformPoint.handle2 += diff;
                                 }
                                 EditorUtility.SetDirty(bezierCurve);
                                 SceneView.RepaintAll();
                             }
                             currentEvent.Use();
                         }
                         break;
                     case EventType.MouseMove:
                         if (!currentEvent.control)
                         {
                             if (GetInsertPointAtMousePosition())
                             {
                                 m_Tool = Tool.Insert;
                             }
                             else if (GetPointAtMousePosition() != null)
                             {
                                 m_Tool = Tool.Remove;
                             }
                             else if (m_Tool != Tool.TransformHandle)
                             {
                                 m_Tool = Tool.Freeform;
                             }
                         }
                         break;
                 }
             }

             protected void DrawCursor(Tool tool) {
                 switch (tool)
                 {
                     case Tool.Freeform:
                         Cursor.SetCursor(freeformCursor, new Vector2(9, 9), CursorMode.Auto);
                         EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.CustomCursor);
                         break;
                     case Tool.Insert:
                         Cursor.SetCursor(insertCursor, new Vector2(3, 6), CursorMode.Auto);
                         EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.CustomCursor);
                         break;
                     case Tool.Remove:
                         Cursor.SetCursor(removeCursor, new Vector2(3, 6), CursorMode.Auto);
                         EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.CustomCursor);
                         break;
                     case Tool.TransformHandle:

                         Cursor.SetCursor(transformCursor, new Vector2(13, 8), CursorMode.Auto);
                         EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.CustomCursor);
                         break;

                 }
             }*/



        /* public enum Tool {
             None,
             Freeform, 
             Remove,
             Insert,
             Transform,
             TransformHandle
         }*/
    }
}