using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Editor.Grids
{
    public class Grid : MonoBehaviour
    {
        [SerializeField]
        private Camera m_Camera;
        [SerializeField]
        private bool m_GridIsOrthographic;
        [SerializeField]
        private bool m_DrawGrid;
        [SerializeField]
        private SnapSettings m_SnapSettings;
        [SerializeField]
        private bool m_DoGridRepaint;

        private float m_AngleValue = 45f;
        private bool m_DrawAngles = false;
        private Vector3 m_CameraDirection = Vector3.zero;
        private Vector3 m_PreviousCameraDirection = Vector3.zero;

        private float m_PlaneGridDrawDistance = 0f;

        private Vector3 m_Pivot = Vector3.zero;
        private Axis m_RenderPlane = Axis.Y;
        float m_LastDistanceCameraToPivot = 0f;

        bool menuIsOrtho = false;
        Axis m_savedAxis;
        bool m_savedFullGrid;
        bool m_GridIsLocked = false;

        public bool GridIsOrthographic => m_GridIsOrthographic;

        public float GridRenderOffset { get; set; }
        internal bool FullGridEnabled { get; private set; }

        internal float SnapValueInUnityUnits
        {
            get { return m_SnapSettings.SnapValueInUnityUnits(); }
        }

        private void OnGUI()
        {
            var currentEvent = Event.current;

            //if (m_DrawGrid && (currentEvent.type == EventType.Repaint || m_DoGridRepaint))
            {
                Vector3 previousPivot = m_Pivot;
                CalculateGridPlacement();

                if (GridIsOrthographic)
                {
                    Axis camAxis = EnumExtension.AxisWithVector(m_Camera.transform.TransformDirection(Vector3.forward).normalized);
                    if (m_RenderPlane != camAxis)
                        SetRenderPlane(camAxis);
                    GridRenderer.DrawOrthographic(m_Camera, camAxis, SnapValueInUnityUnits, m_DrawAngles ? m_AngleValue : -1f);
                }
                else
                {
                    float camDistance = Vector3.Distance(m_Camera.transform.position, previousPivot);

                    if (m_DoGridRepaint
                        || m_Pivot != previousPivot
                        || Mathf.Abs(camDistance - m_LastDistanceCameraToPivot) > m_LastDistanceCameraToPivot / 2
                        || m_CameraDirection != m_PreviousCameraDirection)
                    {
                        m_PreviousCameraDirection = m_CameraDirection;
                        m_DoGridRepaint = false;
                        m_LastDistanceCameraToPivot = camDistance;

                        if (FullGridEnabled)
                            GridRenderer.SetPerspective3D(m_Camera, m_Pivot, m_SnapSettings.SnapValueInUnityUnits());
                        else
                            m_PlaneGridDrawDistance = GridRenderer.SetPerspective(m_Camera, m_RenderPlane, m_SnapSettings.SnapValueInUnityUnits(), m_Pivot, GridRenderOffset);
                    }

                    GridRenderer.Repaint();
                }
            }
        }

        void SetRenderPlane(Axis axis)
        {
            GridRenderOffset = 0f;
            FullGridEnabled = false;
            m_RenderPlane = axis;
            //EditorPrefs.SetBool(PreferenceKeys.PerspGrid, FullGridEnabled);
            //EditorPrefs.SetInt(PreferenceKeys.GridAxis, (int)m_RenderPlane);
            DoGridRepaint();
        }

        internal void DoGridRepaint()
        {
            m_DoGridRepaint = true;
            //SceneView.RepaintAll();
        }

        public bool InFrustum(Camera cam, Vector3 point)
        {
            Vector3 p = cam.WorldToViewportPoint(point);
            return (p.x >= 0f && p.x <= 1f) &&
                    (p.y >= 0f && p.y <= 1f) &&
                    p.z >= 0f;
        }

        void CalculateGridPlacement()
        {
            var cam = m_Camera;

            bool wasOrtho = GridIsOrthographic;

            m_GridIsOrthographic = cam.orthographic;// && Snapping.IsRounded(view.rotation.eulerAngles.normalized);

            m_CameraDirection = Snapping.Sign(m_Pivot - cam.transform.position);

            if (wasOrtho != GridIsOrthographic)
            {
                m_DoGridRepaint = true;

                if (GridIsOrthographic != menuIsOrtho)//view == SceneView.lastActiveSceneView && 
                {
                    if (GridIsOrthographic)
                    {
                        //m_savedAxis = (Axis)EditorPrefs.GetInt(PreferenceKeys.GridAxis);
                        //m_savedFullGrid = EditorPrefs.GetBool(PreferenceKeys.PerspGrid);
                    }
                    else
                    {
                        SetRenderPlane(m_savedAxis);
                        FullGridEnabled = m_savedFullGrid;
                    }
                    //SetMenuIsExtended(menuOpen);
                }
            }

            if (GridIsOrthographic)
                return;

            if (FullGridEnabled)
            {
                //m_Pivot = m_GridIsLocked || Selection.activeTransform == null ? m_Pivot : Selection.activeTransform.position;
            }
            else
            {
                Vector3 sceneViewPlanePivot = m_Pivot;

                Ray ray = new Ray(cam.transform.position, cam.transform.forward);
                Plane plane = new Plane(Vector3.up, m_Pivot);

                // the only time a locked grid should ever move is if it's m_Pivot is out
                // of the camera's frustum.
                if ((m_GridIsLocked && !InFrustum(cam, m_Pivot)) || !m_GridIsLocked)//|| view != SceneView.lastActiveSceneView
                {
                    float dist;

                    if (plane.Raycast(ray, out dist))
                        sceneViewPlanePivot = ray.GetPoint(Mathf.Min(dist, m_PlaneGridDrawDistance / 2f));
                    else
                        sceneViewPlanePivot = ray.GetPoint(Mathf.Min(cam.farClipPlane / 2f, m_PlaneGridDrawDistance / 2f));
                }

                if (m_GridIsLocked)
                {
                    m_Pivot = EnumExtension.InverseAxisMask(sceneViewPlanePivot, m_RenderPlane) + EnumExtension.AxisMask(m_Pivot, m_RenderPlane);
                }
                else
                {
                    //m_Pivot = Selection.activeTransform == null ? m_Pivot : Selection.activeTransform.position;

                    if (!InFrustum(cam, m_Pivot))//Selection.activeTransform == null || 
                    {
                        //m_Pivot = EnumExtension.InverseAxisMask(sceneViewPlanePivot, m_RenderPlane) + EnumExtension.AxisMask(Selection.activeTransform == null ? m_Pivot : Selection.activeTransform.position, m_RenderPlane);
                        m_Pivot = EnumExtension.InverseAxisMask(sceneViewPlanePivot, m_RenderPlane)
                            + EnumExtension.AxisMask(m_Pivot, m_RenderPlane);
                    }
                }
            }
        }

    }
}