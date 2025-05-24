using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

namespace ARPetGame
{
    public class ImprovedARPlaneDetector : MonoBehaviour
    {
        [Header("AR Components")]
        public ARPlaneManager planeManager;
        public ARRaycastManager raycastManager;
        public Camera arCamera;
        
        [Header("Detection Settings")]
        public float minPlaneSize = 0.5f;
        public float maxPlaneAngle = 15f;
        
        [Header("Visual Feedback")]
        public GameObject placementIndicator;
        
        private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
        private ARPlane currentDetectedPlane;
        private Vector3 lastValidPosition;
        private Quaternion lastValidRotation;
        private bool isValidPlaneDetected = false;
        
        public System.Action<Vector3, Quaternion> OnValidPlaneDetected;
        public System.Action OnValidPlaneLost;
        
        public bool IsValidPlaneDetected => isValidPlaneDetected;
        
        private void Awake()
        {
            if (arCamera == null)
                arCamera = Camera.main;
                
            if (planeManager == null)
                planeManager = FindObjectOfType<ARPlaneManager>();
                
            if (raycastManager == null)
                raycastManager = FindObjectOfType<ARRaycastManager>();
        }
        
        private void Start()
        {
            SetupPlacementIndicator();
        }
        
        private void Update()
        {
            UpdatePlaneDetection();
            UpdatePlacementIndicator();
        }
        
        private void SetupPlacementIndicator()
        {
            if (placementIndicator == null)
            {
                placementIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                placementIndicator.name = "PlacementIndicator";
                placementIndicator.transform.localScale = new Vector3(0.5f, 0.01f, 0.5f);
                
                var renderer = placementIndicator.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mat.color = new Color(0f, 1f, 0f, 0.5f);
                    renderer.material = mat;
                }
                
                placementIndicator.SetActive(false);
            }
        }
        
        private void UpdatePlaneDetection()
        {
            if (raycastManager == null || arCamera == null) return;
            
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            
            if (raycastManager.Raycast(screenCenter, raycastHits, TrackableType.PlaneWithinBounds))
            {
                ARRaycastHit hit = raycastHits[0];
                ARPlane hitPlane = hit.trackable as ARPlane;
                
                if (hitPlane != null && IsValidPlane(hitPlane))
                {
                    ProcessValidPlane(hit, hitPlane);
                }
                else
                {
                    ProcessInvalidPlane();
                }
            }
            else
            {
                ProcessNoPlaneDetected();
            }
        }
        
        private bool IsValidPlane(ARPlane plane)
        {
            if (plane.size.x * plane.size.y < minPlaneSize)
                return false;
            
            Vector3 planeNormal = plane.normal;
            float angle = Vector3.Angle(planeNormal, Vector3.up);
            if (angle > maxPlaneAngle)
                return false;
            
            return true;
        }
        
        private void ProcessValidPlane(ARRaycastHit hit, ARPlane plane)
        {
            lastValidPosition = hit.pose.position;
            lastValidRotation = hit.pose.rotation;
            currentDetectedPlane = plane;
            
            if (!isValidPlaneDetected)
            {
                isValidPlaneDetected = true;
                OnValidPlaneDetected?.Invoke(lastValidPosition, lastValidRotation);
                Debug.Log("Valid plane detected!");
            }
        }
        
        private void ProcessInvalidPlane()
        {
            if (isValidPlaneDetected)
            {
                isValidPlaneDetected = false;
                currentDetectedPlane = null;
                OnValidPlaneLost?.Invoke();
            }
        }
        
        private void ProcessNoPlaneDetected()
        {
            if (isValidPlaneDetected)
            {
                isValidPlaneDetected = false;
                currentDetectedPlane = null;
                OnValidPlaneLost?.Invoke();
            }
        }
        
        private void UpdatePlacementIndicator()
        {
            if (placementIndicator == null) return;
            
            if (isValidPlaneDetected)
            {
                placementIndicator.SetActive(true);
                placementIndicator.transform.position = lastValidPosition;
                placementIndicator.transform.rotation = lastValidRotation;
            }
            else
            {
                placementIndicator.SetActive(false);
            }
        }
        
        public bool TryGetPlacementPose(out Vector3 position, out Quaternion rotation)
        {
            position = lastValidPosition;
            rotation = lastValidRotation;
            return isValidPlaneDetected;
        }
    }
}