using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Examples.Volumetric_Light
{
    [ExecuteInEditMode]
    public class VolumetricLightVolume : MonoBehaviour
    {
        public Material material;
        public bool realtimeUpdate = true;

        private const int StencilMask = 1;
        [SerializeField] private Transform _lightTransform;
        [SerializeField] private Camera _camera;
        [SerializeField] private BoxCollider _boxCollider;
        private void UpdateVolumetricLight()
        {
            if (material != null)
            { ;
                _camera.Render();
                material.SetTexture("_LightDepthTexture", _camera.targetTexture);
                material.SetMatrix("_LightViewProjectionMatrix",
                    _camera.projectionMatrix * _camera.worldToCameraMatrix);
                float3 volumeCenterPosition =  transform.position+ _boxCollider.center;
                var localScale = _boxCollider.size;
                material.SetVector("_VolumeCenterPosition",
                    new Vector4(volumeCenterPosition.x, volumeCenterPosition.y, volumeCenterPosition.z, 0));
                material.SetVector("_VolumeSize",
                    new Vector4(localScale.x,localScale.y,localScale.z, 0));

                var forward = _lightTransform.forward;
                var position = _lightTransform.position;
                material.SetVector("_LightDirection", new Vector4(forward.x, forward.y, forward.z, 0));
                material.SetVector("_LightPosition", new Vector4(position.x, position.y, position.z, 0));
            }
        }

        void Start()
        {
            UpdateVolumetricLight();
        }

        void Update()
        {
            if (realtimeUpdate)
            {
                UpdateVolumetricLight();
            }
        }
    }
}