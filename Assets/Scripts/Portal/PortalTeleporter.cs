using System.Collections;
using Player;
using UnityEngine;

namespace Portal
{
    public class PortalTeleporter : MonoBehaviour
    {
        #region Serialized Fields

        public Transform @in;
        public Transform @out;

        #endregion

        // Player controller
        private PlayerCharacter _playerCharacter;
        private float _frontDistance, _previousFrontDistance, _sideDistance, _heightDistance;

        #region Event Functions

        private void Awake()
        {
            _playerCharacter = FindObjectOfType<PlayerCharacter>();
        }

        private void Update()
        {
            var destinationFlipRotation = Matrix4x4.TRS(
                Vector3.zero, Quaternion.AngleAxis(180.0f, Vector3.up), Vector3.one);
            var inInvMat = destinationFlipRotation * @in.worldToLocalMatrix;

            // Introduce variable to reduce function calls.
            var inTransform = @in.transform;
            var inTransformForward = inTransform.forward;
            var inTransformPosition = inTransform.position;

            var vecToCurrentPosition = _playerCharacter.transform.position - inTransformPosition;
            var vecToPreviousPosition = _playerCharacter.PreviousPosition - inTransformPosition;

            // Rough distance thresholds we must be within to teleport
            _sideDistance = Vector3.Dot(inTransform.right, vecToCurrentPosition);
            _frontDistance = Vector3.Dot(inTransformForward, vecToCurrentPosition);
            _heightDistance = Vector3.Dot(@in.transform.up, vecToCurrentPosition);
            _previousFrontDistance = Vector3.Dot(inTransformForward, vecToPreviousPosition);

            // Have we just crossed the portal threshold
            if (!(_frontDistance < 0.0f)
                || !(_previousFrontDistance >= 0.0f)
                || !(Mathf.Abs(_sideDistance) < /*approx door_width*/ 1.0f)
                || !(Mathf.Abs(_heightDistance) < /*approx door_height*/ 1.2f))
                return;

            // Introduce variable to reduce function calls.
            var playerCharacterTransform = _playerCharacter.transform;

            // If so, transform the CamController to Out transform space
            Vector3 playerPositionInLocalSpace = inInvMat * playerCharacterTransform.position;
            var newPlayerPositionWorldSpace = @out.TransformPoint(playerPositionInLocalSpace);

            _playerCharacter.IsTeleporting = true;
            _playerCharacter.transform.position = new Vector3(newPlayerPositionWorldSpace.x,
                newPlayerPositionWorldSpace.y - _playerCharacter.CharacterController.height,
                newPlayerPositionWorldSpace.z);
            
            StartCoroutine(TeleportPlayer());
            var cameraRotationInSourceSpace = inInvMat.rotation * playerCharacterTransform.rotation;
            playerCharacterTransform.rotation = @out.rotation * cameraRotationInSourceSpace;
        }

        #endregion

        #region Coroutines

        private IEnumerator TeleportPlayer()
        {
            yield return new WaitForEndOfFrame();
            _playerCharacter.IsTeleporting = false;
        }

        #endregion
    }
}