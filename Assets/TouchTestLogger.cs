using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class TouchTestLogger : MonoBehaviour
{
    private ARPlaneManager planeManager;
    private float lastPlaneCountLogTime = 0f;
    private float planeCountLogInterval = 2f; // 2초마다 평면 개수 로깅
    
    void Start()
    {
        planeManager = FindFirstObjectByType<ARPlaneManager>();
    }
    
    void Update()
    {
        // 터치 로깅
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                Debug.Log("✅ 터치 인식됨! 위치: " + t.position);
                
                // 현재 감지된 평면 개수도 함께 로깅
                if (planeManager != null)
                {
                    Debug.Log($"📐 현재 감지된 평면 개수: {planeManager.trackables.count}");
                }
            }
        }
        
        // 주기적으로 평면 개수 로깅
        if (Time.time - lastPlaneCountLogTime > planeCountLogInterval)
        {
            lastPlaneCountLogTime = Time.time;
            
            if (planeManager != null && planeManager.trackables.count > 0)
            {
                Debug.Log($"📐 감지된 평면: {planeManager.trackables.count}개");
            }
        }
    }
}

