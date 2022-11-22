using System;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacter : MonoBehaviour
    {
        private const float TransitionSpeed = 20.0f;
        private const float StickToGroundForce = 10;

        #region Public Properties

        public Vector3 PreviousPosition { get; private set; }
        public CharacterController CharacterController { get; private set; }
        public bool IsTeleporting { get; set; }
        public Camera Camera { get; private set; }

        #endregion

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

        private float _cameraPitch;
        private PlayerInputActions _inputActions;
        private Vector3 _restPosition;
        private float _timer;

        #endregion

        #region Event Functions

        private void Awake()
        {
            CharacterController = GetComponent<CharacterController>();
            Camera = GetComponentInChildren<Camera>();

            // Setup input action bindings.
            _inputActions ??= new PlayerInputActions();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _restPosition = Camera.transform.localPosition;
        }

        private void Update()
        {
            PreviousPosition = transform.position;

            if (!IsTeleporting)
            {
                // Update movement.
                CharacterController.Move(UpdateMovement(
                    _inputActions.OnFoot.CharacterMovement.ReadValue<Vector2>()));
            }
            
            // Update Camera rotation.
            Camera.transform.localEulerAngles = UpdateCameraPitch(
                _inputActions.OnFoot.CameraMovement.ReadValue<Vector2>().y);

            // Update Character rotation.
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
            Physics.SphereCast(tf.position, CharacterController.radius, Vector3.down, out var hitInfo,
                CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            movementDirection = Vector3.ProjectOnPlane(movementDirection, hitInfo.normal).normalized;

            movementDirection.x *= speed;
            movementDirection.z *= speed;

            if (CharacterController.isGrounded)
                movementDirection.y = -StickToGroundForce;
            else
                movementDirection += Physics.gravity * gravityMultiplier;

            UpdateHeadMovement(movementDirection);

            return movementDirection * Time.deltaTime;
        }

        private void UpdateHeadMovement(Vector3 movement)
        {
            if (!CharacterController.isGrounded || !headMovementEnabled) return;

            if (Mathf.Abs(movement.x) > 0.1f || Mathf.Abs(movement.z) > 0.1f)
            {
                _timer += bobFrequency * Time.deltaTime;

                var localPosition = Camera.transform.localPosition;
                localPosition = new Vector3(
                    Mathf.Lerp(localPosition.x, Mathf.Cos(_timer / 2) * bobAmplitude, TransitionSpeed),
                    Mathf.Lerp(localPosition.y, _restPosition.y + Mathf.Sin(_timer) * bobAmplitude, TransitionSpeed),
                    localPosition.z
                );
                Camera.transform.localPosition = localPosition;
            }
            else
            {
                var localPosition = Camera.transform.localPosition;
                localPosition = new Vector3(
                    Mathf.Lerp(localPosition.x, _restPosition.x, TransitionSpeed * Time.deltaTime),
                    Mathf.Lerp(localPosition.y, _restPosition.y, TransitionSpeed * Time.deltaTime),
                    _restPosition.z
                );
                Camera.transform.localPosition = localPosition;
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