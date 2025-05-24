using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ARPetGame
{
    public class PlaneVisualizer : MonoBehaviour
    {
        [Header("Visual Settings")]
        public Material planeMaterial;
        public bool showPlaneOutline = true;
        public Color planeColor = new Color(0f, 0.5f, 1f, 0.3f);
        public Color outlineColor = Color.cyan;
        
        private ARPlane arPlane;
        private MeshRenderer meshRenderer;
        private LineRenderer lineRenderer;
        
        private void Awake()
        {
            arPlane = GetComponent<ARPlane>();
            meshRenderer = GetComponent<MeshRenderer>();
            
            SetupMaterial();
            SetupOutline();
        }
        
        private void SetupMaterial()
        {
            if (planeMaterial == null)
            {
                planeMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                planeMaterial.color = planeColor;
            }
            
            if (meshRenderer != null)
            {
                meshRenderer.material = planeMaterial;
            }
        }
        
        private void SetupOutline()
        {
            if (showPlaneOutline)
            {
                GameObject outlineObject = new GameObject("PlaneOutline");
                outlineObject.transform.SetParent(transform);
                outlineObject.transform.localPosition = Vector3.zero;
                outlineObject.transform.localRotation = Quaternion.identity;
                
                lineRenderer = outlineObject.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = outlineColor;
                lineRenderer.endColor = outlineColor;
                lineRenderer.startWidth = 0.02f;
                lineRenderer.endWidth = 0.02f;
                lineRenderer.useWorldSpace = false;
                lineRenderer.loop = true;
                lineRenderer.positionCount = 4;
            }
        }
        
        private void Update()
        {
            UpdateOutline();
        }
        
        private void UpdateOutline()
        {
            if (lineRenderer != null && arPlane != null)
            {
                Vector2 size = arPlane.size;
                Vector3[] corners = new Vector3[]
                {
                    new Vector3(-size.x * 0.5f, 0, -size.y * 0.5f),
                    new Vector3(size.x * 0.5f, 0, -size.y * 0.5f),
                    new Vector3(size.x * 0.5f, 0, size.y * 0.5f),
                    new Vector3(-size.x * 0.5f, 0, size.y * 0.5f)
                };
                
                lineRenderer.SetPositions(corners);
            }
        }
    }
}