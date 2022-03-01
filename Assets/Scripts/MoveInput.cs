using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MoveInput : MonoBehaviour
{
    public static UnityEvent<Vector2> OnUserClickEvent = new UnityEvent<Vector2>();
    public static UnityEvent<Vector2, float> OnUserMoveEvent = new UnityEvent<Vector2, float>();
    public static UnityEvent OnUserReleaseEvent = new UnityEvent();

    [SerializeField]private float _maxDeviationSquared = 200 * 200;
    private Vector2 _clickCoords;
    // Start is called before the first frame update
    void Update()
    {
        CheckClick();
    }
    private void CheckClick()
    {

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        if (Input.GetMouseButtonDown(0))
        {
            _clickCoords = Input.mousePosition;
            OnUserClickEvent.Invoke(_clickCoords);
        }
        if (Input.GetMouseButtonUp(0))
        {
            OnUserReleaseEvent.Invoke();
        }
        if (Input.GetMouseButton(0))
        {
            //OnUserClickEvent.Invoke(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            Vector2 direction = (Vector2)Input.mousePosition - _clickCoords;
            float deviationCoef = Mathf.Clamp01(direction.sqrMagnitude / _maxDeviationSquared);
            OnUserMoveEvent.Invoke(direction.normalized, deviationCoef);
            //Debug.Log("Mouse");
        }
#endif

#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            //OnUserClickEvent.Invoke(Camera.main.ScreenToWorldPoint(touch.position));
            if (touch.phase == TouchPhase.Began)
            {
                _clickCoords = touch.position;
                OnUserClickEvent.Invoke(_clickCoords);
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                Vector2 direction = touch.position - _clickCoords;
                float deviationCoef = Mathf.Clamp01(direction.sqrMagnitude / _maxDeviationSquared);
                OnUserMoveEvent.Invoke(direction,deviationCoef);
            }
            else
            {
                OnUserReleaseEvent.Invoke();
            }
            //Debug.Log("Touch");
        }
#endif

    }
}
