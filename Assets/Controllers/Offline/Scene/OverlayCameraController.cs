using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    [RequireComponent(typeof(Camera))]
    public class OverlayCameraController : MonoBehaviour
    {
        public Transform targetCamera;

        void LateUpdate()
        {
            transform.position = targetCamera.position;
            transform.rotation = targetCamera.rotation;
        }
    }
}