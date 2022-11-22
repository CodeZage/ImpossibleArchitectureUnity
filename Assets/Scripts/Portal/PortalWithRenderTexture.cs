using UnityEngine;

namespace Portal
{
    /// <summary>
    ///     Portal with a camera that renders the the image to a render-texture.
    /// </summary>
    public class PortalWithRenderTexture : MonoBehaviour
    {
        #region Serialized Fields

        public Camera mainCamera;

        // One to render to texture, and another to render normally to switch between (preview)
        public Camera portalCamera;
        public Transform source;
        public Transform destination;

        #endregion

        #region Event Functions

        private void LateUpdate()
        {
            // Rotate Source 180 degrees so PortalCamera is mirror image of MainCamera
            var destinationFlipRotation =
                Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(180.0f, Vector3.up), Vector3.one);
            var sourceInvMat = destinationFlipRotation * source.worldToLocalMatrix;

            // Get variables for the main camera to save on function calls.
            var mainCameraTransform = mainCamera.transform;
            var mainCameraPosition = mainCameraTransform.position;

            // Calculate translation and rotation of MainCamera in Source space.
            Vector3 cameraPositionInSourceSpace = sourceInvMat * new Vector4(mainCameraPosition.x,
                mainCameraPosition.y, mainCameraPosition.z, 1.0f);

            var cameraRotationInSourceSpace =
                sourceInvMat.rotation * mainCameraTransform.rotation;

            // Transform Portal Camera to World Space relative to Destination transform,
            // matching the Main Camera position/orientation
            Transform portalCameraTransform;
            (portalCameraTransform = portalCamera.transform).position =
                destination.TransformPoint(cameraPositionInSourceSpace);
            portalCameraTransform.rotation = destination.rotation * cameraRotationInSourceSpace;

            // Calculate clip plane for portal (for culling of objects in-between destination camera and portal)
            var clipPlaneCameraSpace = CalculateClipPlane(portalCamera, destination);

            // Update projection based on new clip plane
            // Note: http://aras-p.info/texts/obliqueortho.html and http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
            portalCamera.projectionMatrix = mainCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }

        #endregion

        #region Private Functions

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