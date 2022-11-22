using System;
using UnityEngine;

namespace Portal
{
    /// <summary>
    ///     Portal with a camera that renders the the image to a render-texture.
    /// </summary>
    public class PortalWithRenderTexture : MonoBehaviour
    {
        #region Serialized Fields

        public Transform destination;

        #endregion

        #region Private Fields

        private Transform _source;
        private Camera _mainCamera;
        private Camera _portalCamera;

        #endregion

        #region Event Functions

        private void Awake()
        {
            _source = transform;
            _mainCamera = Camera.main;
            _portalCamera = GetComponentInChildren<Camera>();
        }

        private void LateUpdate()
        {
            // Get variables for the main camera to save on function calls.
            var mainCameraTransform = _mainCamera.transform;
            var mainCameraPosition = mainCameraTransform.position;

            // Calculate translation and rotation of MainCamera in Source space.
            var sourceWorldToLocalMatrix = _source.worldToLocalMatrix;
            var cameraPositionInSourceSpace = sourceWorldToLocalMatrix * new Vector4(mainCameraPosition.x, mainCameraPosition.y, mainCameraPosition.z, 1.0f);
            var cameraRotationInSourceSpace = sourceWorldToLocalMatrix.rotation * mainCameraTransform.rotation;

            // Transform Portal Camera to World Space relative to Destination transform,
            // matching the Main Camera position/orientation
            Transform portalCameraTransform;
            (portalCameraTransform = _portalCamera.transform).position = destination.TransformPoint(cameraPositionInSourceSpace);
            portalCameraTransform.rotation = destination.rotation * cameraRotationInSourceSpace;

            // Calculate clip plane for portal (for culling of objects in-between destination camera and portal)
            var clipPlaneCameraSpace = CalculateClipPlane(_portalCamera, destination);

            // Update projection based on new clip plane
            // Note: http://aras-p.info/texts/obliqueortho.html and http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
            _portalCamera.projectionMatrix = _mainCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Moves the cameras clip plane so geometry between the portal camera and portal isn't rendered.
        /// </summary>
        /// <param name="cam">The camera whose clip planes we want to manipulate.</param>
        /// <param name="target">The center of the portal.</param>
        /// <returns>The new clip planes for the camera.</returns>
        private Vector4 CalculateClipPlane(Camera cam, Transform target)
        {
            var forward = target.forward;
            var clipPlaneWorldSpace =
                new Vector4(
                    forward.x,
                    forward.y,
                    forward.z,
                    Vector3.Dot(target.position, -forward));

            return Matrix4x4.Transpose(Matrix4x4.Inverse(cam.worldToCameraMatrix)) * clipPlaneWorldSpace;
        }

        #endregion
    }
}