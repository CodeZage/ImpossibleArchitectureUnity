using UnityEngine;

namespace Portal
{
    /// <summary>
    /// Portal with a camera that renders the the image to a render-texture.
    /// </summary>
    public class PortalWithRenderTexture : MonoBehaviour
    {
        public Camera mainCamera;

        // One to render to texture, and another to render normally to switch between (preview)
        public Camera[] portalCameras;
        public Transform source;
        public Transform destination;

        private void LateUpdate()
        {
            foreach (var portalCamera in portalCameras)
            {
                // Rotate Source 180 degrees so PortalCamera is mirror image of MainCamera
                Matrix4x4 destinationFlipRotation =
                    Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(180.0f, Vector3.up), Vector3.one);
                Matrix4x4 sourceInvMat = destinationFlipRotation * source.worldToLocalMatrix;

                // Calculate translation and rotation of MainCamera in Source space
                Vector3 cameraPositionInSourceSpace =
                    MathUtil.ToV3(sourceInvMat * MathUtil.PosToV4(mainCamera.transform.position));
                Quaternion cameraRotationInSourceSpace =
                    MathUtil.QuaternionFromMatrix(sourceInvMat) * mainCamera.transform.rotation;

                // Transform Portal Camera to World Space relative to Destination transform,
                // matching the Main Camera position/orientation
                Transform portalCameraTransform;
                (portalCameraTransform = portalCamera.transform).position = destination.TransformPoint(cameraPositionInSourceSpace);
                portalCameraTransform.rotation = destination.rotation * cameraRotationInSourceSpace;

                // Calculate clip plane for portal (for culling of objects in-between destination camera and portal)
                var forward = destination.forward;
                Vector4 clipPlaneWorldSpace =
                    new Vector4(
                        forward.x,
                        forward.y,
                        forward.z,
                        Vector3.Dot(destination.position, -forward));

                Vector4 clipPlaneCameraSpace =
                    Matrix4x4.Transpose(Matrix4x4.Inverse(portalCamera.worldToCameraMatrix)) * clipPlaneWorldSpace;

                // Update projection based on new clip plane
                // Note: http://aras-p.info/texts/obliqueortho.html and http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
                portalCamera.projectionMatrix = mainCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
            }
        }
    }

    public static class MathUtil
    {
        public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
        {
            return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
        }

        public static Vector4 PosToV4(Vector3 v)
        {
            return new Vector4(v.x, v.y, v.z, 1.0f);
        }

        public static Vector3 ToV3(Vector4 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static Vector3 ZeroV3 = new Vector3(0.0f, 0.0f, 0.0f);
        public static Vector3 OneV3 = new Vector3(1.0f, 1.0f, 1.0f);
    }
}