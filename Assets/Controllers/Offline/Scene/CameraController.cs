using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Singleplayer
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        private const string BLOCKING_SWIPE_LAYER_NAME = "BlockSwipe";
        private Camera _camera;
        private Transform _transform;

        [SerializeField] private Material grayscaleMaterial;

        [Header("Moving settings")]
        [SerializeField] private float speedMultiplier;
        [SerializeField] private float swipeSpeedMultiplier;
        [SerializeField] private int additionalBorderValue;

        [Header("Zoom settings")]
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minZoom = 2f;
        [SerializeField] private float maxZoom = 10f;

        [Range(0f, 1f)] private float intensity = 0;

        private Transform panelGridParent;

        private Vector3? lastMousePos = null;

        private float minX, maxX, minY, maxY;

        private bool canSwipe = true;

        void Start()
        {
            panelGridParent = MapManager.Instance.ParentOfAllPanels.transform;
            _camera = GetComponent<Camera>();
            _transform = GetComponent<Transform>();

            CalculateBounds();
        }

        void Update()
        {
            Vector3 pos = _transform.position;

            // ====== Клавиши движения ======
            if (Input.GetKey(KeyCode.A)) pos.x -= (1 * speedMultiplier) * Time.deltaTime;
            if (Input.GetKey(KeyCode.D)) pos.x += (1 * speedMultiplier) * Time.deltaTime;
            if (Input.GetKey(KeyCode.S)) pos.y -= (1 * speedMultiplier) * Time.deltaTime;
            if (Input.GetKey(KeyCode.W)) pos.y += (1 * speedMultiplier) * Time.deltaTime;

            // ====== Свайп пальцем ======
            if (Input.touchCount == 1 && canSwipe && IsInsideSwipeArea(Input.mousePosition))
            {
                Touch touch = Input.GetTouch(0);

                // Если палец над UI, игнорируем свайп камеры
                if (!IsPointerOverBlockingUI(Input.mousePosition))
                {
                    if (touch.phase == TouchPhase.Moved)
                    {
                        Vector2 delta = touch.deltaPosition;
                        float adjustedMultiplier = GetAdjustedSwipeMultiplier();
                        pos.x -= delta.x * adjustedMultiplier * Time.deltaTime;
                        pos.y -= delta.y * adjustedMultiplier * Time.deltaTime;
                    }
                }
            }

            // ====== Свайп мышью ======
            if (Input.GetMouseButton(0) && canSwipe && IsInsideSwipeArea(Input.mousePosition))
            {
                // Если мышь над UI, игнорируем свайп камеры
                if (!IsPointerOverBlockingUI(Input.mousePosition))
                {
                    if (lastMousePos == null)
                        lastMousePos = Input.mousePosition;
                    else
                    {
                        Vector3 delta = Input.mousePosition - lastMousePos.Value;
                        float adjustedMultiplier = GetAdjustedSwipeMultiplier();
                        pos.x -= delta.x * adjustedMultiplier * Time.deltaTime;
                        pos.y -= delta.y * adjustedMultiplier * Time.deltaTime;
                        lastMousePos = Input.mousePosition;
                    }
                }
                else
                {
                    lastMousePos = null;
                }
            }
            else
            {
                lastMousePos = null;
            }

            // ====== Zoom ======
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
                _camera.orthographicSize -= scroll * zoomSpeed;

            if (Input.touchCount == 2)
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);

                Vector2 t0Prev = t0.position - t0.deltaPosition;
                Vector2 t1Prev = t1.position - t1.deltaPosition;

                float prevMag = (t0Prev - t1Prev).magnitude;
                float currentMag = (t0.position - t1.position).magnitude;

                float difference = currentMag - prevMag;

                _camera.orthographicSize -= difference * zoomSpeed * 0.01f;
            }

            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, minZoom, maxZoom);

            // ====== Ограничения позиции ======
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);

            _transform.position = pos;
        }

        private float GetAdjustedSwipeMultiplier()
        {
            return swipeSpeedMultiplier * (_camera.orthographicSize / maxZoom);
        }

        bool IsPointerOverBlockingUI(Vector2 screenPos)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = screenPos
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var res in results)
            {
                if (res.gameObject.layer == LayerMask.NameToLayer(BLOCKING_SWIPE_LAYER_NAME))
                    return true;
            }
            return false;
        }

        bool IsInsideSwipeArea(Vector2 screenPos)
        {
            return screenPos.y > Screen.height * 0.3f;
        }

        public void ToggleSwipes(bool isActive) => canSwipe = isActive;

        private void CalculateBounds()
        {
            if (panelGridParent == null || panelGridParent.childCount == 0) return;

            Renderer firstRenderer = panelGridParent.GetChild(0).GetComponent<Renderer>();
            if (firstRenderer == null) return;

            Bounds bounds = firstRenderer.bounds;

            foreach (Transform child in panelGridParent)
            {
                if (child.TryGetComponent<Renderer>(out var r)) bounds.Encapsulate(r.bounds);
            }

            bounds.Expand(new Vector3(additionalBorderValue, additionalBorderValue, 0));

            float camHeight = _camera.orthographicSize;
            float camWidth = camHeight * _camera.aspect;

            minX = bounds.min.x + camWidth;
            maxX = bounds.max.x - camWidth;
            minY = bounds.min.y + camHeight;
            maxY = bounds.max.y - camHeight;

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