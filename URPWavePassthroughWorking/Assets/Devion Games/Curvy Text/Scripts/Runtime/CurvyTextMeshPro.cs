using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DevionGames;

namespace TMPro
{
    [ExecuteInEditMode]
    [AddComponentMenu("UI/Curvy Text", 0)]
    [RequireComponent(typeof(BezierCurve))]
    public partial class CurvyTextMeshPro : TextMeshProUGUI, ILayoutSelfController
    {

        [SerializeField]
        private int m_Resolution = 80;
        [SerializeField]
        private Vector3 m_Offset = Vector3.zero;

        private BezierCurve m_Curve;
        public BezierCurve curve
        {
            get
            {
                if (this.m_Curve == null)
                {
                    this.m_Curve = GetComponent<BezierCurve>();
                }
                return this.m_Curve;
            }
        }
        public void Update()
        {
            base.ForceMeshUpdate();
            Modify();
            Debug.Log("Test");
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {

           
            Modify();
    
            base.OnPopulateMesh(toFill);
            List<UIVertex> list = new List<UIVertex>();
            toFill.GetUIVertexStream(list);
           

            toFill.Clear();
            toFill.AddUIVertexTriangleStream(list);
        }


        private void Modify()
        {
            
            List<Vector3> verts  = new List<Vector3>();
            mesh.GetVertices(verts);
            Vector3 size = new Vector3(rectTransform.rect.width, rectTransform.rect.height);
            Vector3 pivotOffset = new Vector3(rectTransform.pivot.x * size.x, rectTransform.pivot.y * size.y, 0);

            float curveLength = curve.ApproximateLength(m_Resolution);

            float offset = 0f;
            if (alignment == TextAlignmentOptions.BottomJustified || alignment == TextAlignmentOptions.MidlineJustified || alignment == TextAlignmentOptions.TopJustified)
            {
                offset = (curveLength - size.x) * 0.5f;
            }
            // else if (alignment == TextAnchor.LowerRight || alignment == TextAnchor.MiddleRight || alignment == TextAnchor.UpperRight)
            else if (alignment == TextAlignmentOptions.BottomJustified|| alignment == TextAlignmentOptions.MidlineRight|| alignment == TextAlignmentOptions.TopRight)
            {
                offset = curveLength - size.x - 5f;
            }

            for (int i = 0; i < verts.Count; i += 6)
            {
                var topLeft = verts[i]; // Top Left
                var topRight = verts[i + 1]; // Top Right
                var bottomRight = verts[i + 2]; // Bottom Right
                var bottomLeft = verts[i + 4]; // Bottom Left

                float width = (bottomRight - bottomLeft).magnitude;
                float height = (topLeft - bottomLeft).magnitude;

                Vector3 dir = (curve.GetPointAtDistance(bottomRight.x + pivotOffset.x + m_Offset.x + offset, m_Resolution) - curve.GetPointAtDistance(bottomLeft.x + pivotOffset.x + m_Offset.x + offset, m_Resolution)).normalized;
                Vector3 normal = new Vector3(-dir.y, dir.x, 0);
                Vector3 position = curve.GetPointAtDistance(bottomLeft.x + pivotOffset.x + m_Offset.x + offset, m_Resolution) + (bottomLeft.y + m_Offset.y + pivotOffset.y) * normal;

                topLeft = position + normal * height;
                topRight = position + dir * width + normal * height;
                bottomRight = position + dir * width;
                bottomLeft = position;

                verts[i] = topLeft;
                verts[i + 1] = topRight;
                verts[i + 2] = bottomRight;
                verts[i + 3] = bottomRight;
                verts[i + 4] = bottomLeft;
                verts[i + 5] = topLeft;
            }
            m_minWidth = curve.ApproximateLength(m_Resolution);
            mesh.SetVertices(verts);
        }

        public void SetLayoutHorizontal()
        {
            m_Tracker.Clear();
            HandleSelfFittingAlongAxis(0);
        }

        public void SetLayoutVertical()
        {

        }

        private DrivenRectTransformTracker m_Tracker;
        private void HandleSelfFittingAlongAxis(int axis)
        {

            m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
            rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetMinSize(rectTransform, axis));
        }

    }
}