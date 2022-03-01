using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MoverToCoord : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;

    private Vector2 _target;
    private bool _moveToTarget = false;
    private Rigidbody2D _rigidbody;
    private Vector2 _oldPosition;
    private bool _checkPositionFlag = false;
    // Start is called before the first frame update
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _oldPosition = _rigidbody.position;
    }

    public void SetTarget(Vector2 target)
    {
        _target = target;
        _moveToTarget = true;
    }

    private void FixedUpdate()
    {
        MoveToTarget();
        
    }
    private void MoveToTarget()
    {
        if (!_moveToTarget) return;
        Vector2 newPosition = Vector2.MoveTowards(_rigidbody.position, _target, _speed * Time.fixedDeltaTime);        
        _rigidbody.MovePosition(newPosition);
        if (_rigidbody.position == _oldPosition)
        {
            if (!_checkPositionFlag) _checkPositionFlag = true;//Обновление MovePosition происходит на следующем фрейме, поэтому при начале движения старое и новое положение одинаковы
            else
            {
                _checkPositionFlag = false;
                _moveToTarget = false;
                Debug.Log("Complete");
            }
        }
        else
        {
            _oldPosition = _rigidbody.position;
        }
            

    }
}
