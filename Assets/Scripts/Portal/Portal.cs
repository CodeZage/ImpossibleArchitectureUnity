using System;
using UnityEngine;

namespace Portal
{
    public class Portal : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Portal linkedPortal;

        #endregion

        #region Private Fields

        private MeshRenderer _portalScreen;
        private RenderTexture _renderTexture;
        private Camera _playerCamera;
        private Camera _portalCamera;
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        #endregion

        #region Unity Event Functions

        private void Awake()
        {
            _playerCamera = Camera.main;
            _portalCamera = GetComponentInChildren<Camera>();
            _portalScreen = GetComponent<MeshRenderer>();
        }

        #endregion

        #region Public Functions

        public void RenderPortal()
        {
            _portalScreen.enabled = false;
            CreateViewTexture();

            var viewMatrix = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix *
                             _playerCamera.transform.localToWorldMatrix;

            _portalCamera.transform.SetPositionAndRotation(viewMatrix.GetColumn(3),
                viewMatrix.rotation);

            _portalCamera.Render();
            _portalScreen.enabled = true;
        }

        #endregion

        #region Private Functions

        private void CreateViewTexture()
        {
            if (_renderTexture != null && _renderTexture.width == Screen.width &&
                _renderTexture.height == Screen.height) return;

            if (_renderTexture != null)
            {
                _renderTexture.Release();
            }

            _renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
            // Render the view from the portal camera to the view texture
            _portalCamera.targetTexture = _renderTexture;
            // Display the view texture on the screen of the linked portal
            linkedPortal._portalScreen.material.SetTexture("_MainTex", _renderTexture);

            #endregion
        }
    }
}