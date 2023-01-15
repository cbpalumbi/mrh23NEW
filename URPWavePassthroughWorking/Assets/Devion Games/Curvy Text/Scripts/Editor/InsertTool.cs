using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace DevionGames
{
    [EditorTool("Insert Point", typeof(BezierCurve))]
    public class InsertTool : TransformTool
    {

        public override GUIContent toolbarIcon
        {
            get
            {
                if (m_IconContent == null)
                {
                    m_IconContent = new GUIContent()
                    {
                        image = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Devion Games/Curvy Text/Scripts/Editor/Icons/Insert Point.png"),
                        text = "Insert Point",
                        tooltip = "Insert Point"
                    };
                }
                return m_IconContent;
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            Event currentEvent = Event.current;

            if (currentEvent.control || !GetInsertPointAtMousePosition())
            {
                DoTransformTool();
            }
            else
            {
                DoInsertTool();
            }
        }
    }
}