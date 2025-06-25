using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _camera;
    private Transform _transform;
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
}
