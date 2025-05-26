using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARPlane))]
[RequireComponent(typeof(MeshRenderer))]
public class ARPlaneMaterialSetup : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        SetupPlaneMaterial();
    }
    
    void SetupPlaneMaterial()
    {
        // 간단한 반투명 머티리얼 생성
        Material planeMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        planeMaterial.color = new Color(0.5f, 0.8f, 1f, 0.3f); // 연한 파란색
        
        // 블렌딩 설정
        planeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        planeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        planeMaterial.SetInt("_ZWrite", 0);
        planeMaterial.renderQueue = 3000;
        
        meshRenderer.material = planeMaterial;
        
        Debug.Log($"[ARPlaneMaterialSetup] Material setup completed for {gameObject.name}");
    }
} 