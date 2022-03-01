using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]private Vector3 _shift = new Vector3(0,0,-10f);
    [SerializeField]private float _speed = .5f;

    private Transform _target;
    private Vector3 _currentVelocity;

    private void FixedUpdate()
    {
        if (!_target) return;

        transform.position = Vector3.SmoothDamp(transform.position, CalculatePosition(), ref _currentVelocity, _speed);
    }
    private Vector3 CalculatePosition()
    {
        if (_target) return _shift + _target.position;
        else return _shift;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        transform.position = CalculatePosition();
    }
}
