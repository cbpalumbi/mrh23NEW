using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DevionGames
{
    [AddComponentMenu("UI/Curvy Text",0)]
    [RequireComponent(typeof(BezierCurve))]
    public class CurvyText :Text, ILayoutSelfController
    {

        [SerializeField]
        private int m_Resolution = 80;
        [SerializeField]
        private Vector3 m_Offset= Vector3.zero;
   
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

       
       protected override void OnPopulateMesh(VertexHelper toFill)
        {
            base.OnPopulateMesh(toFill);
            List<UIVertex> list = new List<UIVertex>();
            toFill.GetUIVertexStream(list);
            Modify(list);

            toFill.Clear();
            toFill.AddUIVertexTriangleStream(list);
        }

        public override float minWidth {
            get {
               
                return curve.ApproximateLength(m_Resolution);
            }
        }

        private void Modify(List<UIVertex> verts)
        {
           

            Vector3 size = new Vector3(rectTransform.rect.width, rectTransform.rect.height);
            Vector3 pivotOffset = new Vector3(rectTransform.pivot.x * size.x, rectTransform.pivot.y * size.y, 0);
           
            float curveLength = curve.ApproximateLength(m_Resolution);

            float offset = 0f;
            if (alignment == TextAnchor.LowerCenter || alignment == TextAnchor.MiddleCenter || alignment == TextAnchor.UpperCenter)
            {
                offset = (curveLength - size.x) * 0.5f;
            }
            else if (alignment == TextAnchor.LowerRight || alignment == TextAnchor.MiddleRight || alignment == TextAnchor.UpperRight) {
                offset = curveLength - size.x-5f;
            }

            for (int i = 0; i < verts.Count; i += 6)
            {
                var topLeft = verts[i]; // Top Left
                var topRight = verts[i + 1]; // Top Right
                var bottomRight = verts[i + 2]; // Bottom Right
                var bottomLeft = verts[i + 4]; // Bottom Left

                float width = (bottomRight.position - bottomLeft.position).magnitude;
                float height = (topLeft.position - bottomLeft.position).magnitude;

                 Vector3 dir = (curve.GetPointAtDistance(bottomRight.position.x + pivotOffset.x + m_Offset.x+offset,m_Resolution) - curve.GetPointAtDistance(bottomLeft.position.x + pivotOffset.x + m_Offset.x+offset,m_Resolution)).normalized;
                 Vector3 normal = new Vector3(-dir.y, dir.x, 0);
                 Vector3 position = curve.GetPointAtDistance(bottomLeft.position.x + pivotOffset.x + m_Offset.x+offset,m_Resolution) + (bottomLeft.position.y + m_Offset.y + pivotOffset.y) * normal;

                topLeft.position = position + normal * height;
                topRight.position = position + dir * width + normal * height;
                bottomRight.position = position + dir * width;
                bottomLeft.position = position;

                verts[i] = topLeft;
                verts[i + 1] = topRight;
                verts[i + 2] = bottomRight;
                verts[i + 3] = bottomRight;
                verts[i + 4] = bottomLeft;
                verts[i + 5] = topLeft;
            }
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