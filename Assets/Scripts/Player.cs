using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoverByVector))]
public class Player : MonoBehaviour
{
    private MoverByVector _mover;
    // Start is called before the first frame update
    private void Start()
    {
        _mover = GetComponent<MoverByVector>();
        MoveInput.OnUserMoveEvent.AddListener(_mover.MoveByVector);
        MoveInput.OnUserReleaseEvent.AddListener(_mover.StopMoving);
        CameraInit();
    }
    public void CameraInit()
    {
        FindObjectOfType<CameraFollow>()?.SetTarget(transform);
    }
}
