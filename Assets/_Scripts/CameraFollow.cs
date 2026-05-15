using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform _target;
    [SerializeField] Vector3 _offset = new Vector3(0, 3, -6);
    [SerializeField] float _followSpeed = 8f;

    void LateUpdate()
    {
        if(_target == null) return;

        Vector3 targetPos = _target.position + _offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, _followSpeed * Time.deltaTime);

        Vector3 lookTarget = _target.position + Vector3.up * 1.5f;
        transform.LookAt(lookTarget);
    }
}
