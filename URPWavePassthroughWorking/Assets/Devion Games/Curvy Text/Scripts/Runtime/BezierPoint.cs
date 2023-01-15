using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevionGames
{
    [System.Serializable]
    public class BezierPoint
    {
        [SerializeField]
        private Vector3 m_LocalPosition;
        public Vector3 localPosition {
            get { return m_LocalPosition*m_Curve.scale; }
            set { m_LocalPosition = value/m_Curve.scale; }
        }

        [SerializeField]
        private Vector3 m_Handle1;
        public Vector3 handle1 {
            get { return m_Handle1*m_Curve.scale; }
            set { m_Handle1 = value/m_Curve.scale; }
        }

        [SerializeField]
        private Vector3 m_Handle2;
        public Vector3 handle2
        {
            get { return m_Handle2*m_Curve.scale; }
            set { m_Handle2 = value/m_Curve.scale; }
        }

        [SerializeField]
        private BezierCurve m_Curve;

        public BezierPoint(BezierCurve curve) {
            this.m_Curve = curve;
        }
    }
}