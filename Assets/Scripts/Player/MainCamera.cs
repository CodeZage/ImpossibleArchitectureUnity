using System;
using UnityEngine;


namespace Player
{
    public class MainCamera : MonoBehaviour
    {
        private Portal.Portal[] _portals;

        private void Awake()
        {
            _portals = FindObjectsOfType<Portal.Portal>();
        }

        private void OnPreCull()
        {
            foreach (var portal in _portals)
            {
                portal.RenderPortal();
            }
        }
    }
}