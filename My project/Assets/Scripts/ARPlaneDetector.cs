using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

namespace ARPetGame
{
    public class ARPlaneDetector : MonoBehaviour
    {
        [Header("AR Components")]
        public ARPlaneManager planeManager;
        public ARRaycastManager raycastManager;
        
        [Header("Pet Placement")]
        public Transform petPrefab;
        public float minPlaneSize = 0.5f;
        
        [Header("Visual Feedback")]
        public GameObject placementIndicator;
        
        private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
        private Camera arCamera;
        private bool isPetPlaced = false;
        
        private void Awake()
        {
            arCamera = Camera.main;
            
            if (planeManager == null)
                planeManager = FindObjectOfType<ARPlaneManager>();
                
            if (raycastManager == null)
                raycastManager = FindObjectOfType<ARRaycastManager>();
        }
        
        private void Start()
        {
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(false);
            }
        }
        
        private void Update()
        {
            if (isPetPlaced) return;
            
            UpdatePlacementIndicator();
        }
        
        private void UpdatePlacementIndicator()
        {
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            
            if (raycastManager.Raycast(screenCenter, raycastHits, TrackableType.PlaneWithinPolygon))
            {
                ARRaycastHit hit = raycastHits[0];
                
                if (placementIndicator != null)
                {
                    placementIndicator.SetActive(true);
                    placementIndicator.transform.SetPositionAndRotation(hit.pose.position, hit.pose.rotation);
                }
            }
            else
            {
                if (placementIndicator != null)
                    placementIndicator.SetActive(false);
            }
        }
        
        public bool TryPlacePet()
        {
            if (isPetPlaced) return false;
            
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            
            if (raycastManager.Raycast(screenCenter, raycastHits, TrackableType.PlaneWithinPolygon))
            {
                ARRaycastHit hit = raycastHits[0];
                PlacePet(hit.pose.position, hit.pose.rotation);
                return true;
            }
            
            return false;
        }
        
        private void PlacePet(Vector3 position, Quaternion rotation)
        {
            if (petPrefab != null)
            {
                Vector3 adjustedPosition = position + Vector3.up * 0.1f;
                Transform pet = Instantiate(petPrefab, adjustedPosition, rotation);
                pet.name = "PlacedPet";
            }
            
            isPetPlaced = true;
            
            if (placementIndicator != null)
                placementIndicator.SetActive(false);
                
            Debug.Log("Pet placed successfully!");
        }
    }
}