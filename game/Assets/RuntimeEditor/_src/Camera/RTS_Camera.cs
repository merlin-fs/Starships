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

        public float MovementSpeed = 1f;    
        public float MovementVelocity = 0.2f;

        Vector3 m_Move = Vector3.zero;
        Vector3 m_CurrentMove = Vector3.zero;
        Vector3 m_VelocityMove = Vector3.zero;
        public AnimationCurve MoveCurve;

        public float RotationSpeed = 1;
        public float RotationVelocity = 0.2f;

        bool m_Look = false;
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
        /// <summary>
        /// Speed with keyboard movement
        /// </summary>
        /// <summary>
        /// speed with screen edge movement
        /// </summary>
        public float screenEdgeMovementSpeed = 3f;  //
        /// <summary>
        /// speed when following a target
        /// </summary>
        public float followingSpeed = 5f;
        /// <summary>
        /// speed with keyboard rotation
        /// </summary>
        /// <summary>
        /// speed with mouse rotation
        /// </summary>
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
        /// <summary>
        /// are we following target
        /// </summary>
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

        public bool useKeyboardInput = true;
        public string horizontalAxis = "Horizontal";
        public string verticalAxis = "Vertical";
        public bool invertRotationX = false;
        public bool invertRotationY = true;
        public bool invertZooming = true;

        public bool usePanning = true;
        public KeyCode panningKey = KeyCode.Mouse2;

        public bool useKeyboardZooming = true;
        public KeyCode zoomInKey = KeyCode.Z;
        public KeyCode zoomOutKey = KeyCode.X;

        public bool useScrollwheelZooming = true;
        public string zoomingAxis = "Mouse ScrollWheel";

        public bool useKeyboardRotation = true;
        public KeyCode rotateRightKey = KeyCode.E;
        public KeyCode rotateLeftKey = KeyCode.Q;

        public bool useMouseRotation = true;
        public KeyCode mouseRotationKey = KeyCode.Mouse1;

        private float ScrollWheel
        {
            get 
            {
                return Input.GetAxis(zoomingAxis); 
            }
        }

        private int ZoomDirection
        {
            get {
                bool zoomIn = Input.GetKey(zoomInKey);
                bool zoomOut = Input.GetKey(zoomOutKey);
                if (zoomIn && zoomOut)
                    return 0;
                else if (!zoomIn && zoomOut)
                    return 1;
                else if (zoomIn && !zoomOut)
                    return -1;
                else
                    return 0;
            }
        }
        #endregion
        #region Unity_Methods
        private void Awake()
        {
            var action = m_MoveAction.action;
            action.performed += OnMoveChange;
            action.started += OnMoveChange;
            action.canceled += OnMoveChange;

            action = m_RotateAction.action;
            action.performed += OnRotateChange;
            action.started += OnRotateChange;
            action.canceled += OnRotateChange;

            action = m_ZoomAction.action;
            action.performed += OnZoomChange;
            action.started += OnZoomChange;
            action.canceled += OnZoomChange;

            action = m_LookEnableAction.action;
            action.started += context => 
            {
                m_VelocityRotate = Vector3.zero;
                m_Look = true; 
            };
            action.canceled += context => m_Look = false;

            m_LookAction.action.ApplyBindingOverride(new InputBinding { overrideProcessors = $"invertVector2(invertX={invertRotationX},invertY={invertRotationY})" });
        }

        private void OnMoveChange(InputAction.CallbackContext context)
        {
            var input = context.ReadValue<Vector2>();
            m_Move = new Vector3(input.x, 0, input.y); 
        }

        private void OnRotateChange(InputAction.CallbackContext context)
        {
            var input = context.ReadValue<Vector2>();
            m_Rotate = new Vector3(input.y, input.x, 0);
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
        /// <summary>
        /// update camera movement and rotation
        /// </summary>
        private void CameraUpdate()
        {
            Move();
            HeightCalculation();
            Rotation();
            LimitPosition();
            if (FollowingTarget)
                FollowTarget();
        }
        /// <summary>
        /// move camera with keyboard or with screen edge
        /// </summary>
        private void Move()
        {
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
            if (usePanning && Input.GetKey(panningKey) && MouseAxis != Vector2.zero)
            {
                Vector3 desiredMove = new Vector3(-MouseAxis.x, 0, -MouseAxis.y);
                desiredMove *= panningSpeed;
                desiredMove *= Time.deltaTime;
                desiredMove = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) * desiredMove;
                desiredMove = m_RigTransform.InverseTransformDirection(desiredMove);
                if (desiredMove != Vector3.zero)
                    ResetTarget();
                m_RigTransform.Translate(desiredMove, Space.Self);
            }
            */


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

            /*
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
            Vector3 rotation;
            if (m_Look)
            {
                var input = m_LookAction.action.ReadValue<Vector2>();
                m_CurrentRotate = new Vector3(input.y, input.x, 0);
                rotation = m_CurrentRotate;
            }
            else
            {
                m_CurrentRotate = Vector3.SmoothDamp(m_CurrentRotate, m_Rotate, ref m_VelocityRotate, RotationVelocity);
                rotation = m_CurrentRotate;
            }
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

        /// <summary>
        /// limit camera position
        /// </summary>
        private void LimitPosition()
        {
            if (!limitMap)
                return;

            m_RigTransform.position = new Vector3(Mathf.Clamp(m_RigTransform.position.x, -limitX, limitX),
                m_RigTransform.position.y,
                Mathf.Clamp(m_RigTransform.position.z, -limitY, limitY));
        }

        /// <summary>
        /// set the target
        /// </summary>
        /// <param name="target"></param>
        public void SetTarget(Transform target)
        {
            targetFollow = target;
        }

        /// <summary>
        /// reset the target (target is set to null)
        /// </summary>
        public void ResetTarget()
        {
            targetFollow = null;
        }

        /// <summary>
        /// calculate distance to ground
        /// </summary>
        /// <returns></returns>
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