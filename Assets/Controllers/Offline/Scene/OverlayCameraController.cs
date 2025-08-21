using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    [RequireComponent(typeof(Camera))]
    public class OverlayCameraController : MonoBehaviour
    {
        private Camera targetCameraScript;
        private Camera currentCameraScript;

        public Transform targetCamera;


        private void Start()
        {
            currentCameraScript = GetComponent<Camera>();
            targetCameraScript = targetCamera.GetComponent<Camera>();
        }

        void LateUpdate()
        {
            currentCameraScript.orthographicSize = targetCameraScript.orthographicSize;
            transform.SetPositionAndRotation(targetCamera.position, targetCamera.rotation);
        }
    }
}