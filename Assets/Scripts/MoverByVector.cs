using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MoverByVector : MonoBehaviour
{
    [SerializeField] private float _maxSpeed = 10f;
    private Rigidbody2D _rigidbody;
    private Animator _animator;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }
    
    public void MoveByVector(Vector2 vector, float deviationCoef)
    {
        if (vector == Vector2.zero) return;
        Vector2 newPosition = Vector2.MoveTowards(_rigidbody.position, _rigidbody.position + vector, _maxSpeed * Time.fixedDeltaTime * deviationCoef);        
        _rigidbody.MovePosition(newPosition);
        _rigidbody.SetRotation(Quaternion.LookRotation(vector, Vector3.back));
        _animator?.SetFloat("Speed", deviationCoef);
    }
    public void StopMoving()
    {
        _animator?.SetFloat("Speed", 0);
    }
}
