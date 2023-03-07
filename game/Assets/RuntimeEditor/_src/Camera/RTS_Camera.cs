using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace RTS_Cam
{
    public class RTS_Camera : MonoBehaviour
    {
        [SerializeField]
        private Transform m_RigTransform;
        [SerializeField]
        private Transform m_CameraTransform;
        [SerializeField]
        private InputActionReference m_MoveAction;
        [SerializeField]
        private InputActionReference m_RotateAction;
        [SerializeField]
        private InputActionReference m_LookAction;
        [SerializeField]
        private InputActionReference m_ZoomAction;
        [SerializeField]
        private InputActionReference m_LookEnableAction;
        [SerializeField]
        private InputActionReference m_MoveEnableAction;

        public float MovementSpeed = 1f;    
        public float MovementVelocity = 0.2f;

        Vector3 m_Move = Vector3.zero;
        Vector3 m_CurrentMove = Vector3.zero;
        Vector3 m_VelocityMove = Vector3.zero;
        public AnimationCurve MoveCurve;

        public float RotationSpeed = 1;
        public float RotationVelocity = 0.2f;

        Vector3 m_Rotate = Vector3.zero;
        Vector3 m_CurrentRotate = Vector3.zero;
        Vector3 m_VelocityRotate = Vector3.zero;
        public AnimationCurve RotateCurve;

        public float ZoomingSensitivity = 1f;
        public float zoomVelocity = 0.2f;
        public AnimationCurve ZoomCurve;

        float m_Zoom = 0;
        Vector3 m_CurrentZoom = Vector3.zero;
        Vector3 m_VelocityZoom = Vector3.zero;

        public bool useFixedUpdate = false; //use FixedUpdate() or Update()

        #region Movement
        [Header("Movement")]
        public float screenEdgeMovementSpeed = 3f;  //
        public float followingSpeed = 5f;
        #endregion
        #region Height
        [Header("Height")]
        public bool autoHeight = true;
        public LayerMask groundMask = -1; //layermask of ground or other objects that affect height
        [Range(0, 1)]
        public float zoomPos = 0; //value in range (0, 1) used as t in Matf.Lerp
        #endregion
        #region MapLimits
        [Header("Limits")]
        public bool limitMap = true;
        public float limitX = 50f; //x limit of map
        public float limitY = 50f; //z limit of map

        public float maxHeight = 10f; //maximal height
        public float minHeight = 0f; //minimnal height

        public bool limitRotationX = true;
        public float maxRotationX = 90f;
        public float minRotationX = 0f;
        public bool limitRotationY = false;
        public float maxRotationY = 0f;
        public float minRotationY = 0f;

        #endregion
        #region Targeting
        [Header("Targeting")]
        public Transform targetFollow; //target to follow
        public Vector3 targetOffset;
        public bool FollowingTarget
        {
            get {
                return targetFollow != null;
            }
        }
        #endregion
        #region Input
        [Header("Input")]
        public bool useScreenEdgeInput = true;
        public float screenEdgeBorder = 25f;

        public bool invertRotationX = false;
        public bool invertRotationY = true;
        public bool invertZooming = true;
        #endregion
        #region Unity_Methods
        private void Awake()
        {
            InputAction action = m_MoveAction;

            action = m_ZoomAction;
            action.performed += OnZoomChange;
            action.started += OnZoomChange;
            action.canceled += OnZoomChange;

            action = m_LookEnableAction;
            action.started += context => m_VelocityRotate = Vector3.zero;
            action.canceled += context => m_VelocityRotate = Vector3.zero;

            action = m_MoveEnableAction;
            action.started += context => m_VelocityMove = Vector3.zero;
            action.canceled += context => m_VelocityMove = Vector3.zero;

            m_LookAction.action.ApplyBindingOverride(new InputBinding { overrideProcessors = $"invertVector2(invertX={invertRotationX},invertY={invertRotationY})" });
        }

        private void OnZoomChange(InputAction.CallbackContext context)
        {
            var input = context.ReadValue<Vector2>();
            m_Zoom = input.y;
        }

        private void Update()
        {
            if (!useFixedUpdate)
                CameraUpdate();
        }

        private void FixedUpdate()
        {
            if (useFixedUpdate)
                CameraUpdate();
        }

        #endregion
        #region RTSCamera_Methods
        private void CameraUpdate()
        {
            Move();
            HeightCalculation();
            Rotation();
            LimitPosition();
            if (FollowingTarget)
                FollowTarget();
        }

        private void Move()
        {
            if (m_MoveEnableAction.action.IsPressed())
            {
                var input = m_LookAction.action.ReadValue<Vector2>() * 50;
                m_Move = new Vector3(-input.x, 0, input.y);
                m_CurrentMove = m_Rotate;
            }
            else
            {
                var input = m_MoveAction.action.ReadValue<Vector2>();
                m_Move = new Vector3(input.x, 0, input.y);
            }

            m_CurrentMove = Vector3.SmoothDamp(m_CurrentMove, m_Move, ref m_VelocityMove, MovementVelocity);
            if (m_CurrentMove.magnitude == 0)
                return;

            Vector3 desiredMove = m_CurrentMove;
            desiredMove *= MovementSpeed;
            desiredMove *= Time.deltaTime;
            desiredMove *= MoveCurve.Evaluate(zoomPos);
            desiredMove = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) * desiredMove;
            desiredMove = m_RigTransform.InverseTransformDirection(desiredMove);
            if (desiredMove != Vector3.zero)
                ResetTarget();
            m_RigTransform.Translate(desiredMove, Space.Self);

            /*
            if (useScreenEdgeInput)
            {
                Vector3 desiredMove = new Vector3();

                Rect leftRect = new Rect(0, 0, screenEdgeBorder, Screen.height);
                Rect rightRect = new Rect(Screen.width - screenEdgeBorder, 0, screenEdgeBorder, Screen.height);
                Rect upRect = new Rect(0, Screen.height - screenEdgeBorder, Screen.width, screenEdgeBorder);
                Rect downRect = new Rect(0, 0, Screen.width, screenEdgeBorder);

                desiredMove.x = leftRect.Contains(MouseInput) ? -1 : rightRect.Contains(MouseInput) ? 1 : 0;
                desiredMove.z = upRect.Contains(MouseInput) ? 1 : downRect.Contains(MouseInput) ? -1 : 0;

                desiredMove *= screenEdgeMovementSpeed;
                desiredMove *= Time.deltaTime;
                desiredMove = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) * desiredMove;
                desiredMove = m_RigTransform.InverseTransformDirection(desiredMove);
                //if (desiredMove != Vector3.zero)
                //    ResetTarget();
                m_RigTransform.Translate(desiredMove, Space.Self);
            }
            */
        }

        private void HeightCalculation()
        {
            zoomPos -= m_Zoom * Time.deltaTime * ZoomingSensitivity * ZoomCurve.Evaluate(zoomPos);
            zoomPos = Mathf.Clamp01(zoomPos);
            float targetHeight = Mathf.Lerp(minHeight, maxHeight, zoomPos);
            float difference = 0;
            /*
            float distanceToGround = DistanceToGround(targetHeight);// - m_Transform.position.y
            if (distanceToGround != targetHeight)// && distanceToGround > 0
                difference = targetHeight - distanceToGround;
            */
            var zoom = new Vector3(m_CameraTransform.localPosition.x, m_CameraTransform.localPosition.y, -(targetHeight + difference));
            m_CurrentZoom = Vector3.SmoothDamp(m_CurrentZoom, zoom, ref m_VelocityZoom, zoomVelocity);
            m_CameraTransform.localPosition = m_CurrentZoom;
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }

        private void Rotation()
        {
            if (m_LookEnableAction.action.IsPressed())
            {
                var input = m_LookAction.action.ReadValue<Vector2>();
                m_Rotate = new Vector3(input.y, input.x, 0);
                m_CurrentRotate = m_Rotate;
            }
            else
            {
                var input = m_RotateAction.action.ReadValue<Vector2>();
                m_Rotate = new Vector3(input.y, input.x, 0);
            }

            m_CurrentRotate = Vector3.SmoothDamp(m_CurrentRotate, m_Rotate, ref m_VelocityRotate, RotationVelocity);
            var rotation = m_CurrentRotate;

            if (rotation.magnitude == 0)
                return;
            rotation *= Time.deltaTime * RotationSpeed;
            rotation *= RotateCurve.Evaluate(zoomPos);
            rotation += transform.eulerAngles;
            if (limitRotationX)
                rotation.x = ClampAngle(rotation.x, minRotationX, maxRotationX);
            if (limitRotationY)
                rotation.y = ClampAngle(rotation.y, minRotationY, maxRotationY);
            transform.rotation = Quaternion.Euler(rotation);
        }

        private void FollowTarget()
        {
            Vector3 targetPos = new Vector3(targetFollow.position.x, m_RigTransform.position.y, targetFollow.position.z) + targetOffset;
            m_RigTransform.position = Vector3.MoveTowards(m_RigTransform.position, targetPos, Time.deltaTime * followingSpeed);
        }

        private void LimitPosition()
        {
            if (!limitMap)
                return;

            m_RigTransform.position = new Vector3(Mathf.Clamp(m_RigTransform.position.x, -limitX, limitX),
                m_RigTransform.position.y,
                Mathf.Clamp(m_RigTransform.position.z, -limitY, limitY));
        }

        public void SetTarget(Transform target)
        {
            targetFollow = target;
        }

        public void ResetTarget()
        {
            targetFollow = null;
        }

        private float DistanceToGround(float height)
        {
            //TODO: сделать переключаьель
            /*
            if (false)
            {
                var position = m_RigTransform.position;
                position.y = height;

                Ray ray = new Ray(position, Vector3.down);
                return Physics.Raycast(ray, out RaycastHit hit, 9999f, groundMask.value)
                    ? (hit.point - m_RigTransform.position).magnitude
                    : 0f;
            }
            */
            return 0f;
        }
        #endregion
    }
}