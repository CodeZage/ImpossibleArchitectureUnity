using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacter : MonoBehaviour
    {
        private const float TransitionSpeed = 20.0f;
        private const float StickToGroundForce = 10;

        #region Serialized Fields

        // Movement Settings
        [Header("Movement")] [SerializeField] [Range(0.0f, 10.0f)]
        private float speed = 6.0f;

        [SerializeField] [Range(0.0f, 5.0f)] private float gravityMultiplier = 2.0f;

        // Camera Settings
        [Header("Camera")] [Range(0f, 1f)] [SerializeField]
        private float cameraSensitivity = 5f;

        [SerializeField] private bool headMovementEnabled;

        // Head Movement
        [SerializeField] private float bobAmplitude = 0.05f;
        [SerializeField] private float bobFrequency = 12f;

        #endregion

        #region Non-serialized Fields

        private Camera _camera;
        private float _cameraPitch;
        private CharacterController _characterController;
        private PlayerInputActions _inputActions;
        private Vector3 _restPosition;
        private float _timer;

        #endregion

        #region Event Functions

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _camera = GetComponentInChildren<Camera>();

            // Setup input action bindings.
            _inputActions ??= new PlayerInputActions();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _restPosition = _camera.transform.localPosition;
        }

        private void Update()
        {
            _characterController.Move(UpdateMovement(
                _inputActions.OnFoot.CharacterMovement.ReadValue<Vector2>()));
            _camera.transform.localEulerAngles = UpdateCameraPitch(
                _inputActions.OnFoot.CameraMovement.ReadValue<Vector2>().y);
            transform.Rotate(UpdateCharacterRotation(
                _inputActions.OnFoot.CameraMovement.ReadValue<Vector2>().x));
        }

        private void OnEnable()
        {
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        #endregion

        #region Private Functions

        private Vector3 UpdateMovement(Vector2 value)
        {
            var tf = transform;
            var movementDirection = tf.forward * value.y + tf.right * value.x;
            movementDirection.Normalize();

            // Get a normal for the surface that is being touched to move along it
            Physics.SphereCast(tf.position, _characterController.radius, Vector3.down, out var hitInfo,
                _characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            movementDirection = Vector3.ProjectOnPlane(movementDirection, hitInfo.normal).normalized;

            movementDirection.x *= speed;
            movementDirection.z *= speed;

            if (_characterController.isGrounded)
                movementDirection.y = -StickToGroundForce;
            else
                movementDirection += Physics.gravity * gravityMultiplier;

            UpdateHeadMovement(movementDirection);

            return movementDirection * Time.deltaTime;
        }

        private void UpdateHeadMovement(Vector3 movement)
        {
            if (!_characterController.isGrounded || !headMovementEnabled) return;

            if (Mathf.Abs(movement.x) > 0.1f || Mathf.Abs(movement.z) > 0.1f)
            {
                _timer += bobFrequency * Time.deltaTime;

                var localPosition = _camera.transform.localPosition;
                localPosition = new Vector3(
                    Mathf.Lerp(localPosition.x, Mathf.Cos(_timer / 2) * bobAmplitude, TransitionSpeed),
                    Mathf.Lerp(localPosition.y, _restPosition.y + Mathf.Sin(_timer) * bobAmplitude, TransitionSpeed),
                    localPosition.z
                );
                _camera.transform.localPosition = localPosition;
            }
            else
            {
                var localPosition = _camera.transform.localPosition;
                localPosition = new Vector3(
                    Mathf.Lerp(localPosition.x, _restPosition.x, TransitionSpeed * Time.deltaTime),
                    Mathf.Lerp(localPosition.y, _restPosition.y, TransitionSpeed * Time.deltaTime),
                    _restPosition.z
                );
                _camera.transform.localPosition = localPosition;
            }
        }

        private Vector3 UpdateCameraPitch(float value)
        {
            _cameraPitch -= value * cameraSensitivity;
            _cameraPitch = Mathf.Clamp(_cameraPitch, -90.0f, 90.0f);

            return Vector3.right * _cameraPitch;
        }

        private Vector3 UpdateCharacterRotation(float value)
        {
            return Vector3.up * (value * cameraSensitivity);
        }

        #endregion
    }
}