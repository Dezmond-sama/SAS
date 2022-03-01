using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public enum WallSide { wsLeft, wsRight, wsTop, wsBottom };
    [SerializeField] private GameObject _leftWall;
    [SerializeField] private GameObject _bottomWall;
    [SerializeField] private GameObject _rightWall;
    [SerializeField] private GameObject _topWall;
    [SerializeField] private GameObject _ceil;

    public void SetWallEnabledState(WallSide wallSide, bool value)
    {
        switch (wallSide)
        {
            case WallSide.wsLeft:
                _leftWall?.SetActive(value);
                break;
            case WallSide.wsRight:
                _rightWall?.SetActive(value);
                break;
            case WallSide.wsTop:
                _topWall?.SetActive(value);
                break;
            case WallSide.wsBottom:
                _bottomWall?.SetActive(value);
                break;
            default:
                break;
        }
    }

    public void SetCeilEnabledState(bool value)
    {
        _ceil?.SetActive(value);                
    }
    public int RoomIndex { get; set; } = 0;
}
