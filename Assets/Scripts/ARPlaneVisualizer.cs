using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Rendering;

[RequireComponent(typeof(ARPlane))]
public class ARPlaneVisualizer : MonoBehaviour
{
    private ARPlane arPlane;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private static ARPlaneVisualizer currentActiveVisualizer;
    
    [Header("Visualization Settings")]
    [SerializeField] private Color planeColor = new Color(0.2f, 0.6f, 1f, 0.6f); // 더 진한 파란색
    [SerializeField] private Color selectedColor = new Color(0.2f, 1f, 0.2f, 0.7f); // 선택된 평면은 초록색
    private Material planeMaterial;
    
    private bool isSelected = false;
    
    void Awake()
    {
        Debug.Log($"[ARPlaneVisualizer] Awake called for {gameObject.name}");
        
        arPlane = GetComponent<ARPlane>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        
        if (arPlane == null)
        {
            Debug.LogError($"[ARPlaneVisualizer] ARPlane component not found on {gameObject.name}!");
            return;
        }
        
        if (meshRenderer == null)
        {
            Debug.LogWarning($"[ARPlaneVisualizer] MeshRenderer not found on {gameObject.name}, trying to find in children...");
            meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (meshRenderer == null)
            {
                Debug.LogError($"[ARPlaneVisualizer] Still no MeshRenderer found!");
                return;
            }
        }
        
        if (meshFilter == null)
        {
            Debug.LogWarning($"[ARPlaneVisualizer] MeshFilter not found on {gameObject.name}, trying to find in children...");
            meshFilter = GetComponentInChildren<MeshFilter>();
            if (meshFilter == null)
            {
                Debug.LogError($"[ARPlaneVisualizer] Still no MeshFilter found!");
                return;
            }
        }
        
        Debug.Log($"[ARPlaneVisualizer] Components - ARPlane: {arPlane != null}, MeshRenderer: {meshRenderer != null}, MeshFilter: {meshFilter != null}");
        
        CreatePlaneMaterial();
    }
    
    void CreatePlaneMaterial()
    {
        Debug.Log($"[ARPlaneVisualizer] Creating material for {gameObject.name}");
        
        // 여러 셰이더 시도
        string[] shaderNames = {
            "Universal Render Pipeline/Unlit",
            "Sprites/Default",
            "Unlit/Color",
            "Mobile/Unlit (Supports Lightmap)",
            "Unlit/Transparent"
        };
        
        Shader shader = null;
        foreach (string shaderName in shaderNames)
        {
            shader = Shader.Find(shaderName);
            if (shader != null)
            {
                Debug.Log($"[ARPlaneVisualizer] Found shader: {shaderName}");
                break;
            }
        }
        
        if (shader == null)
        {
            Debug.LogError("[ARPlaneVisualizer] No suitable shader found!");
            return;
        }
        
        planeMaterial = new Material(shader);
        planeMaterial.color = planeColor;
        
        // 투명도 설정
        planeMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        planeMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        planeMaterial.SetInt("_ZWrite", 0);
        planeMaterial.SetFloat("_Mode", 3); // Transparent mode
        planeMaterial.SetInt("_Cull", (int)CullMode.Off);
        planeMaterial.EnableKeyword("_ALPHABLEND_ON");
        planeMaterial.renderQueue = 3000;
        
        meshRenderer.material = planeMaterial;
        meshRenderer.enabled = true;
        
        Debug.Log($"[ARPlaneVisualizer] Material created and assigned. MeshRenderer enabled: {meshRenderer.enabled}, Material: {planeMaterial.name}");
    }
    
    void Start()
    {
        Debug.Log($"[ARPlaneVisualizer] Start called for {gameObject.name}, GameObject active: {gameObject.activeSelf}");
        
        // LineRenderer 비활성화 (검정색 아웃라인 제거)
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            Debug.Log($"[ARPlaneVisualizer] LineRenderer disabled in Start for {gameObject.name}");
        }
        
        // GameObject가 비활성화되어 있다면 활성화
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            Debug.Log($"[ARPlaneVisualizer] Activated GameObject {gameObject.name}");
        }
    }
    
    void OnEnable()
    {
        Debug.Log($"[ARPlaneVisualizer] OnEnable called for {gameObject.name}, TrackingState: {arPlane?.trackingState}");
        
        // LineRenderer 비활성화 (검정색 아웃라인 제거)
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            Debug.Log($"[ARPlaneVisualizer] LineRenderer disabled in OnEnable for {gameObject.name}");
        }
        
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
            Debug.Log($"[ARPlaneVisualizer] MeshRenderer enabled for {gameObject.name}");
        }
        
        // 메시 정보 로그
        if (meshFilter != null && meshFilter.mesh != null)
        {
            Debug.Log($"[ARPlaneVisualizer] Mesh info - Vertices: {meshFilter.mesh.vertexCount}, Triangles: {meshFilter.mesh.triangles.Length / 3}");
        }
        else
        {
            Debug.LogWarning($"[ARPlaneVisualizer] No mesh found for {gameObject.name}");
        }
    }
    
    void Update()
    {
        if (arPlane == null) return;
        
        // 트래킹 상태에 따라 시각화 업데이트
        if (arPlane.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.None)
        {
            if (meshRenderer != null && meshRenderer.enabled)
            {
                meshRenderer.enabled = false;
                Debug.Log($"[ARPlaneVisualizer] Disabled renderer for {gameObject.name} due to tracking loss");
            }
        }
        else
        {
            if (meshRenderer != null && !meshRenderer.enabled)
            {
                meshRenderer.enabled = true;
                Debug.Log($"[ARPlaneVisualizer] Re-enabled renderer for {gameObject.name}");
            }
        }
        
        // 메시 업데이트 확인
        if (meshFilter != null && meshFilter.mesh != null && Time.frameCount % 60 == 0) // 1초마다 체크
        {
            Debug.Log($"[ARPlaneVisualizer] {gameObject.name} - Active: {gameObject.activeSelf}, Renderer: {meshRenderer.enabled}, Vertices: {meshFilter.mesh.vertexCount}");
        }
    }
    
    public void SetVisualizationActive(bool active)
    {
        Debug.Log($"[ARPlaneVisualizer] SetVisualizationActive({active}) called for {gameObject.name}");
        
        if (active)
        {
            // 이전 활성 평면 비활성화
            if (currentActiveVisualizer != null && currentActiveVisualizer != this)
            {
                currentActiveVisualizer.SetSelected(false);
            }
            
            currentActiveVisualizer = this;
            SetSelected(true);
        }
        else
        {
            SetSelected(false);
            if (currentActiveVisualizer == this)
            {
                currentActiveVisualizer = null;
            }
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (planeMaterial != null)
        {
            Color newColor = selected ? selectedColor : planeColor;
            planeMaterial.color = newColor;
            Debug.Log($"[ARPlaneVisualizer] {gameObject.name} color changed to: {newColor} (selected: {selected})");
        }
        
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
            Debug.Log($"[ARPlaneVisualizer] {gameObject.name} renderer enabled: {meshRenderer.enabled}");
        }
        
        // GameObject 활성화 확인
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            Debug.Log($"[ARPlaneVisualizer] Activated GameObject {gameObject.name}");
        }
    }
    
    public bool IsSelected()
    {
        return isSelected;
    }
    
    public static ARPlaneVisualizer GetCurrentActiveVisualizer()
    {
        return currentActiveVisualizer;
    }
    
    public ARPlane GetARPlane()
    {
        return arPlane;
    }
    
    void OnDestroy()
    {
        if (currentActiveVisualizer == this)
        {
            currentActiveVisualizer = null;
        }
        Debug.Log($"[ARPlaneVisualizer] Destroyed {gameObject.name}");
    }
} 