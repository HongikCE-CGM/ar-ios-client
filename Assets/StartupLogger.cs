using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class StartupLogger : MonoBehaviour
{
    void Start()
    {
        Debug.Log("✅ 앱 실행됨: StartupLogger Start() 호출됨");
        
        // AR 세션 상태 확인
        ARSession arSession = FindFirstObjectByType<ARSession>();
        if (arSession != null)
        {
            Debug.Log("✅ AR Session 발견됨");
        }
        else
        {
            Debug.LogWarning("❌ AR Session을 찾을 수 없습니다");
        }
        
        // AR 평면 매니저 확인
        ARPlaneManager planeManager = FindFirstObjectByType<ARPlaneManager>();
        if (planeManager != null)
        {
            Debug.Log("✅ AR Plane Manager 발견됨");
            Debug.Log($"평면 감지 모드: {planeManager.requestedDetectionMode}");
        }
        else
        {
            Debug.LogWarning("❌ AR Plane Manager를 찾을 수 없습니다");
        }
        
        Debug.Log("📱 기기를 움직여서 평면을 스캔해주세요!");
    }
}

