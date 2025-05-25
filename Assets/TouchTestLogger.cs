using UnityEngine;

public class TouchTestLogger : MonoBehaviour
{
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                Debug.Log("✅ 터치 인식됨! 위치: " + t.position);
            }
        }
    }
}

