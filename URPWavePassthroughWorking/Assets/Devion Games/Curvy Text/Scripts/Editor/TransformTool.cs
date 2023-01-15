using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;


namespace DevionGames
{
    [EditorTool("Convert Point", typeof(BezierCurve))]
    public class TransformTool : FreeformTool
    {
      
        public override GUIContent toolbarIcon
        {
            get
            {
                if (m_IconContent == null)
                {
                    m_IconContent = new GUIContent()
                    {
                        image = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Devion Games/Curvy Text/Scripts/Editor/Icons/Transform Point.png"),
                        text = "Convert Point",
                        tooltip = "Convert Point"
                    };
                }
                return m_IconContent;
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            DoTransformTool();
        }
    }
}