using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaneVisualizer : MonoBehaviour
{
    [Header("Plane Visualization")]
    public GameObject dotPrefab; // 점 프리팹 (작은 구체)
    public float dotSpacing = 0.3f; // 점 간격
    public Material planeMaterial; // 평면 머티리얼
    
    private ARPlaneManager planeManager;
    private Dictionary<ARPlane, List<GameObject>> planeDots = new Dictionary<ARPlane, List<GameObject>>();
    
    void Start()
    {
        planeManager = GetComponent<ARPlaneManager>();
        
        // 기본 점 프리팹이 없으면 생성
        if (dotPrefab == null)
        {
            CreateDefaultDotPrefab();
        }
    }
    
    void CreateDefaultDotPrefab()
    {
        // 기본 점 프리팹 생성 (작은 구체)
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.transform.localScale = Vector3.one * 0.05f; // 작은 크기
        
        // 머티리얼 설정
        Renderer renderer = dot.GetComponent<Renderer>();
        Material dotMat = new Material(Shader.Find("Standard"));
        dotMat.color = Color.cyan;
        dotMat.SetFloat("_Metallic", 0.5f);
        dotMat.SetFloat("_Smoothness", 0.8f);
        renderer.material = dotMat;
        
        // Collider 제거 (필요없음)
        Destroy(dot.GetComponent<Collider>());
        
        dotPrefab = dot;
        dot.SetActive(false); // 프리팹으로 사용하기 위해 비활성화
        
        Debug.Log("✅ 기본 점 프리팹 생성됨");
    }
    
    void Update()
    {
        if (planeManager == null) return;
        
        // 현재 활성화된 모든 평면 확인
        foreach (var plane in planeManager.trackables)
        {
            if (plane.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                // 새로운 평면이면 점들 생성
                if (!planeDots.ContainsKey(plane))
                {
                    CreateDotsForPlane(plane);
                    Debug.Log($"✅ 새 평면 감지됨: {plane.trackableId}");
                }
                // 기존 평면이 업데이트되었으면 점들 업데이트
                else if (plane.subsumedBy == null) // 다른 평면에 병합되지 않은 경우
                {
                    UpdateDotsForPlane(plane);
                }
            }
        }
        
        // 제거된 평면들 정리
        var planesToRemove = new List<ARPlane>();
        foreach (var plane in planeDots.Keys)
        {
            if (plane == null || plane.trackingState != UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                planesToRemove.Add(plane);
            }
        }
        
        foreach (var plane in planesToRemove)
        {
            RemoveDotsForPlane(plane);
            Debug.Log($"❌ 평면 제거됨: {plane?.trackableId}");
        }
    }
    
    void CreateDotsForPlane(ARPlane plane)
    {
        if (planeDots.ContainsKey(plane))
            return;
            
        List<GameObject> dots = new List<GameObject>();
        planeDots[plane] = dots;
        
        UpdateDotsForPlane(plane);
    }
    
    void UpdateDotsForPlane(ARPlane plane)
    {
        if (!planeDots.ContainsKey(plane))
            return;
            
        // 기존 점들 제거
        foreach (var dot in planeDots[plane])
        {
            if (dot != null)
                Destroy(dot);
        }
        planeDots[plane].Clear();
        
        // 평면 경계 가져오기
        if (plane.boundary.Length == 0)
            return;
            
        // 평면의 중심과 크기 계산
        Vector3 center = plane.center;
        Vector3 size = plane.size;
        
        // 점들을 격자 형태로 배치
        int dotsX = Mathf.Max(1, Mathf.RoundToInt(size.x / dotSpacing));
        int dotsZ = Mathf.Max(1, Mathf.RoundToInt(size.z / dotSpacing));
        
        for (int x = 0; x < dotsX; x++)
        {
            for (int z = 0; z < dotsZ; z++)
            {
                Vector3 localPos = new Vector3(
                    (x - dotsX * 0.5f + 0.5f) * dotSpacing,
                    0,
                    (z - dotsZ * 0.5f + 0.5f) * dotSpacing
                );
                
                // 평면의 로컬 좌표를 월드 좌표로 변환
                Vector3 worldPos = plane.transform.TransformPoint(localPos);
                
                // 점 생성
                GameObject dot = Instantiate(dotPrefab, worldPos, plane.transform.rotation);
                dot.SetActive(true);
                dot.transform.SetParent(plane.transform);
                
                planeDots[plane].Add(dot);
            }
        }
        
        Debug.Log($"🔵 평면에 {planeDots[plane].Count}개 점 생성됨");
    }
    
    void RemoveDotsForPlane(ARPlane plane)
    {
        if (!planeDots.ContainsKey(plane))
            return;
            
        foreach (var dot in planeDots[plane])
        {
            if (dot != null)
                Destroy(dot);
        }
        
        planeDots.Remove(plane);
    }
}