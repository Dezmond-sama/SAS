using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Joystic : MonoBehaviour
{
    [SerializeField]private GameObject _graphic;
    [SerializeField] private Image _handle;
    [SerializeField] private float _maxDeviation = 100;

    void Start()
    {
        MoveInput.OnUserClickEvent.AddListener(SetPosition);
        MoveInput.OnUserReleaseEvent.AddListener(HideObject);
        MoveInput.OnUserMoveEvent.AddListener(SetHandle);
    }
    private void SetPosition(Vector2 position)
    {
        _graphic.SetActive(true);
        transform.position = position;
    }

    private void SetHandle(Vector2 shift, float deviationCoef)
    {
        _handle.transform.localPosition = shift.normalized * _maxDeviation * deviationCoef;
    }

    private void HideObject()
    {
        _graphic.SetActive(false);
    }

}
