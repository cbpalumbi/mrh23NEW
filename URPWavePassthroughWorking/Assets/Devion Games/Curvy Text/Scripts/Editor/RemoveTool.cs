using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace DevionGames
{
    [EditorTool("Remove Point", typeof(BezierCurve))]
    public class RemoveTool : TransformTool
    {
        public override GUIContent toolbarIcon
        {
            get
            {
                if (m_IconContent == null)
                {
                    m_IconContent = new GUIContent()
                    {
                        image = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Devion Games/Curvy Text/Scripts/Editor/Icons/Remove Point.png"),
                        text = "Remove Point",
                        tooltip = "Remove Point"
                    };
                }
                return m_IconContent;
            }
        }


        public override void OnToolGUI(EditorWindow window)
        {
            Event currentEvent = Event.current;
            if (currentEvent.control || GetPointAtMousePosition() == null)
            {
                DoTransformTool();
            }
            else
            {
                DoRemoveTool();
            }
        }
    }
}