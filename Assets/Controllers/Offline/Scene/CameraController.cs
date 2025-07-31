using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Singleplayer
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        private Camera _camera;
        private Transform _transform;
        [SerializeField] private Material grayscaleMaterial;
        [Range(0f, 1f)] private float intensity = 0;

        void Start()
        {
            _camera = GetComponent<Camera>();
            _transform = GetComponent<Transform>();
        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKey(KeyCode.A))
            {
                _transform.position = new Vector3(_transform.position.x - 10, _transform.position.y, _transform.position.z);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                _transform.position = new Vector3(_transform.position.x, _transform.position.y - 10, _transform.position.z);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                _transform.position = new Vector3(_transform.position.x + 10, _transform.position.y, _transform.position.z);
            }
            else if (Input.GetKey(KeyCode.W))
            {
                _transform.position = new Vector3(_transform.position.x, _transform.position.y + 10, _transform.position.z);
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (grayscaleMaterial != null)
            {
                grayscaleMaterial.SetFloat("_Intensity", intensity);
                Graphics.Blit(source, destination, grayscaleMaterial);
            }
            else
            {
                Graphics.Blit(source, destination);
            }
        }

        public void FadeInGrayscale(float duration)
        {
            StartCoroutine(FadeEffect(duration, 1f));
        }

        public void FadeOutGrayscale(float duration)
        {
            StartCoroutine(FadeEffect(duration, 0f));
        }

        private IEnumerator FadeEffect(float duration, float target)
        {
            float start = intensity;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                intensity = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }
            intensity = target;
        }

        public void ForceSnapToPlayer()
        {
            var player = GameManager.Instance.GetEntityWithType(EntityType.Player);
            var playerMono = player as MonoBehaviour;
            transform.position = new Vector3(playerMono.transform.position.x, playerMono.transform.position.y, transform.position.z);
        }
    }
}