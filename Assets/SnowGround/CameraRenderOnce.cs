using System;
using UnityEngine;

namespace SnowGround
{
    [ExecuteInEditMode]
    public class CameraRenderOnce : MonoBehaviour
    {
        private Camera _camera;
        private bool _hasRendered = false;

        void Start()
        {
            _camera = GetComponent<Camera>();
        }

        private void Update()
        {
            if (!_hasRendered)
            {
                _camera.Render();
                _hasRendered = true;
            }
        }
    }
}