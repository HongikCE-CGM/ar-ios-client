using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARPlane))]
[RequireComponent(typeof(MeshRenderer))]
public class ARPlaneVisualizer : MonoBehaviour
{
    private ARPlane arPlane;
    private MeshRenderer meshRenderer;
    private static ARPlaneVisualizer currentActiveVisualizer;
    
    [Header("Visualization Settings")]
    [SerializeField] private Color planeColor = new Color(0.5f, 0.8f, 1f, 0.3f); // 연한 파란색
    [SerializeField] private Material planeMaterial;
    
    void Awake()
    {
        arPlane = GetComponent<ARPlane>();
        meshRenderer = GetComponent<MeshRenderer>();
        
        // 기본 머티리얼 설정
        if (planeMaterial == null)
        {
            planeMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            planeMaterial.color = planeColor;
            planeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            planeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            planeMaterial.SetInt("_ZWrite", 0);
            planeMaterial.renderQueue = 3000;
        }
        
        meshRenderer.material = planeMaterial;
    }
    
    void OnEnable()
    {
        Debug.Log($"[ARPlaneVisualizer] Plane enabled: {gameObject.name}, TrackingState: {arPlane.trackingState}");
        UpdateVisualization();
    }
    
    void Update()
    {
        // 트래킹 상태에 따라 시각화 업데이트
        if (arPlane.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.None)
        {
            SetVisualizationActive(false);
        }
    }
    
    public void SetVisualizationActive(bool active)
    {
        if (active)
        {
            // 이전 활성 평면 비활성화
            if (currentActiveVisualizer != null && currentActiveVisualizer != this)
            {
                currentActiveVisualizer.SetVisualizationActive(false);
            }
            
            currentActiveVisualizer = this;
            meshRenderer.enabled = true;
            Debug.Log($"[ARPlaneVisualizer] Activated plane visualization: {gameObject.name}, Size: {arPlane.size}");
        }
        else
        {
            meshRenderer.enabled = false;
            if (currentActiveVisualizer == this)
            {
                currentActiveVisualizer = null;
            }
            Debug.Log($"[ARPlaneVisualizer] Deactivated plane visualization: {gameObject.name}");
        }
    }
    
    private void UpdateVisualization()
    {
        // 평면 크기에 따라 시각화 업데이트
        if (arPlane.size.x * arPlane.size.y > 0.1f) // 최소 크기 체크
        {
            SetVisualizationActive(true);
        }
    }
    
    public static ARPlaneVisualizer GetCurrentActiveVisualizer()
    {
        return currentActiveVisualizer;
    }
    
    public ARPlane GetARPlane()
    {
        return arPlane;
    }
} 