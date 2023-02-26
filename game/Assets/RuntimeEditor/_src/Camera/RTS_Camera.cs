using UnityEngine;
using UnityEngine.InputSystem;

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
        private InputActionReference m_LookEnableAction;
        [SerializeField]
        private InputActionReference m_LookAction;


        public bool useFixedUpdate = false; //use FixedUpdate() or Update()

        #region Movement
        [Header("Movement")]
        /// <summary>
        /// Speed with keyboard movement
        /// </summary>
        public float keyboardMovementSpeed = 5f;    //speed with keyboard movement
        public float keyboardMovementVelocity = 0.2f; //speed with keyboard movement
        public float keyboardMovementZoom = 5f;
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
        public float rotationSped = 125f;
        public float panningSpeed = 10f;
        /// <summary>
        /// speed with mouse rotation
        /// </summary>
        public float mouseRotationSpeed = 150f;
        #endregion
        #region Height
        [Header("Height")]
        public bool autoHeight = true;
        public LayerMask groundMask = -1; //layermask of ground or other objects that affect height
        public float heightDampening = 5f;
        public float keyboardZoomingSensitivity = 2f;
        public float scrollWheelZoomingSensitivity = 25f;
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


        Vector3 m_Move = Vector3.zero;
        Vector3 m_CurrentMove = Vector3.zero;
        Vector3 m_VelocityMove = Vector3.zero;

        Vector3 m_Rotate = Vector3.zero;
        Vector3 m_CurrentRotate = Vector3.zero;
        Vector3 m_VelocityRotate = Vector3.zero;

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

        private int RotationDirection
        {
            get {
                bool rotateRight = Input.GetKey(rotateRightKey);
                bool rotateLeft = Input.GetKey(rotateLeftKey);
                if (rotateLeft && rotateRight)
                    return 0;
                else if (rotateLeft && !rotateRight)
                    return -1;
                else if (!rotateLeft && rotateRight)
                    return 1;
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

            action = m_LookAction.action;
            action.performed += OnRotateChange;
            action.started += OnRotateChange;
            action.canceled += OnRotateChange;
            m_LookAction.action.Disable();

            action = m_LookEnableAction.action;
            action.started += OnRotateChange;
            action.canceled += OnRotateChange;

            m_LookAction.action.ApplyBindingOverride(new InputBinding { overrideProcessors = $"invertVector2(invertX={invertRotationX},invertY={invertRotationY})" });
        }

        private void OnMoveChange(InputAction.CallbackContext context)
        {
            var input = context.ReadValue<Vector2>();
            m_Move = new Vector3(input.x, 0, input.y); 
        }

        private void OnRotateChange(InputAction.CallbackContext context)
        {
            if (context.action.name == "LookActive")
            {
                if (context.started)
                    m_LookAction.action.Enable();
                else if (context.canceled)
                    m_LookAction.action.Disable();
                return;
            }
            var input = context.ReadValue<Vector2>();
            m_Rotate = new Vector3(input.y, input.x, 0);
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

            m_CurrentMove = Vector3.SmoothDamp(m_CurrentMove, m_Move, ref m_VelocityMove, keyboardMovementVelocity);

            Vector3 desiredMove = m_CurrentMove;
            desiredMove *= keyboardMovementSpeed;
            desiredMove *= Time.deltaTime;
            desiredMove *= Mathf.Lerp(.5f, keyboardMovementZoom, zoomPos);
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

        /// <summary>
        /// calcualte height
        /// </summary>
        private void HeightCalculation()
        {
            if (useScrollwheelZooming)
            {
                zoomPos = (invertZooming)
                    ? zoomPos -= ScrollWheel * Time.deltaTime * scrollWheelZoomingSensitivity
                    : zoomPos += ScrollWheel * Time.deltaTime * scrollWheelZoomingSensitivity;
            }

            if (useKeyboardZooming)
                zoomPos += ZoomDirection * Time.deltaTime * keyboardZoomingSensitivity;

            zoomPos = Mathf.Clamp01(zoomPos);

            float targetHeight = Mathf.Lerp(minHeight, maxHeight, zoomPos);
            float difference = 0;
            float distanceToGround = DistanceToGround(targetHeight);// - m_Transform.position.y

            if (distanceToGround != targetHeight)// && distanceToGround > 0
                difference = targetHeight - distanceToGround;

            m_CameraTransform.localPosition = Vector3.Lerp(m_CameraTransform.localPosition,
                new Vector3(m_CameraTransform.localPosition.x, m_CameraTransform.localPosition.y, -(targetHeight + difference)),
                Time.deltaTime * heightDampening);
            /*
            m_CameraTransform.localPosition = Vector3.Lerp(m_CameraTransform.localPosition,
                new Vector3(m_CameraTransform.localPosition.x, targetHeight + difference, m_CameraTransform.localPosition.z), 
                Time.deltaTime * heightDampening);
            */
        }
        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }
        /// <summary>
        /// rotate camera
        /// </summary>
        private void Rotation()
        {
            float angleX = 0f;
            float angleY = 0f;

            /*
            if (useKeyboardRotation)
            {
                angleY = RotationDirection * Time.deltaTime * rotationSped;
            }
            */

            /*
            if (useMouseRotation && Input.GetKey(mouseRotationKey))
            {
                var x = (invertRotationX)
                    ? MouseAxis.x
                    : -MouseAxis.x;
                var y = (invertRotationY)
                    ? -MouseAxis.y
                    : MouseAxis.y;
                angleY = x * Time.deltaTime * mouseRotationSpeed;
                angleX = y * Time.deltaTime * mouseRotationSpeed;
            }
            */
            m_Rotate *= Time.deltaTime * mouseRotationSpeed;

            var rotation = transform.eulerAngles;
            //if (angleX != 0 || angleY != 0)
            //    ResetTarget();
            rotation.x += angleX;
            rotation.y += angleY;
            rotation += m_Rotate;

            if (limitRotationX)
                rotation.x = ClampAngle(rotation.x, minRotationX, maxRotationX);
            if (limitRotationY)
                rotation.y = ClampAngle(rotation.y, minRotationY, maxRotationY);

            transform.rotation = Quaternion.Euler(rotation);
        }
        /// <summary>
        /// follow targetif target != null
        /// </summary>
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
            if (false)
            {
                var position = m_RigTransform.position;
                position.y = height;

                Ray ray = new Ray(position, Vector3.down);
                return Physics.Raycast(ray, out RaycastHit hit, 9999f, groundMask.value)
                    ? (hit.point - m_RigTransform.position).magnitude
                    : 0f;
            }
            return 0f;
        }

        #endregion
    }
}