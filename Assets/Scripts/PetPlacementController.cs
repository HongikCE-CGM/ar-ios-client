using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PetPlacementController : MonoBehaviour
{
    public ARPlaneManager planeManager;
    public ARRaycastManager raycastManager;
    public GameObject petPrefab;
    public Camera arCamera;
    public GameObject placementIndicatorPrefab;
    public GameObject planePrefab; // 평면 시각화용 프리팹

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool petPlaced = false;
    private GameObject spawnedPet;
    private GameObject placementIndicator;
    private ARPlane selectedPlane;
    private PetMovementController petMovementController;

    void Awake()
    {
        Debug.Log("[PetPlacementController] Awake called");
        
        if (planeManager == null)
        {
            planeManager = FindFirstObjectByType<ARPlaneManager>();
            Debug.Log($"[PetPlacementController] ARPlaneManager found: {planeManager != null}");
        }
        if (raycastManager == null)
        {
            raycastManager = FindFirstObjectByType<ARRaycastManager>();
            Debug.Log($"[PetPlacementController] ARRaycastManager found: {raycastManager != null}");
        }

        if (arCamera == null)
        {
            arCamera = Camera.main;
            Debug.Log($"[PetPlacementController] Camera found: {arCamera != null}");
        }
        
        if (petPrefab == null)
        {
            Debug.LogError("[PetPlacementController] Pet Prefab is not assigned!");
            enabled = false;
            return;
        }

        // 평면 프리팹 설정
        if (planePrefab == null && planeManager != null)
        {
            planePrefab = planeManager.planePrefab;
            Debug.Log($"[PetPlacementController] Using plane prefab from ARPlaneManager");
        }

        if (placementIndicatorPrefab != null)
        {
            placementIndicator = Instantiate(placementIndicatorPrefab);
            placementIndicator.SetActive(false);
            Debug.Log("[PetPlacementController] Placement indicator created");
        }
    }

    void OnEnable()
    {
        if (planeManager != null)
        {
            planeManager.trackablesChanged.AddListener(OnPlanesChanged);
            Debug.Log("[PetPlacementController] Subscribed to trackablesChanged event");
        }
    }

    void OnDisable()
    {
        if (planeManager != null)
        {
            planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
        }
    }

    void Start()
    {
        if (petPrefab.scene.IsValid())
        {
            petPrefab.SetActive(false);
        }
        Debug.Log("[PetPlacementController] Start completed");
    }

    void Update()
    {
        if (petPlaced) 
        {
            if(placementIndicator != null && placementIndicator.activeSelf)
            {
                placementIndicator.SetActive(false);
            }
            return;
        }

        UpdatePlacementIndicator();

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Debug.Log("[PetPlacementController] Touch detected");
            if (placementIndicator != null && placementIndicator.activeSelf)
            {
                PlacePetAtIndicator();
            }
        }
    }

    void UpdatePlacementIndicator()
    {
        if (petPlaced || placementIndicatorPrefab == null) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            
            // 현재 레이캐스트가 맞은 평면 찾기
            ARPlane hitPlane = null;
            if (hits[0].trackableId != TrackableId.invalidId)
            {
                hitPlane = planeManager.GetPlane(hits[0].trackableId);
            }
            
            if (hitPlane != null && hitPlane != selectedPlane)
            {
                // 새로운 평면 선택
                SelectPlane(hitPlane);
            }
            
            if (placementIndicator != null)
            {
                placementIndicator.transform.position = hitPose.position;
                placementIndicator.transform.rotation = hitPose.rotation;
                placementIndicator.SetActive(true);
            }
        }
        else
        {
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(false);
            }
        }
    }

    void SelectPlane(ARPlane plane)
    {
        selectedPlane = plane;
        
        // 모든 평면의 시각화를 비활성화하고 선택된 평면만 활성화
        foreach (var trackablePlane in planeManager.trackables)
        {
            var visualizer = trackablePlane.GetComponent<ARPlaneVisualizer>();
            if (visualizer != null)
            {
                visualizer.SetVisualizationActive(trackablePlane == selectedPlane);
            }
        }
        
        Debug.Log($"[PetPlacementController] Selected plane: {plane.gameObject.name}, Size: {plane.size}");
    }

    void PlacePetAtIndicator()
    {
        if (placementIndicator == null || !placementIndicator.activeSelf || selectedPlane == null) 
        {
            Debug.LogWarning("[PetPlacementController] Cannot place pet - missing indicator or plane");
            return;
        }

        Pose placementPose = new Pose(placementIndicator.transform.position, placementIndicator.transform.rotation);
        PlacePet(placementPose);
    }

    void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        Debug.Log($"[PetPlacementController] Planes changed - Added: {args.added.Count}, Updated: {args.updated.Count}, Removed: {args.removed.Count}");
        
        foreach (ARPlane plane in args.added)
        {
            // ARPlaneVisualizer 컴포넌트 추가
            if (plane.GetComponent<ARPlaneVisualizer>() == null)
            {
                plane.gameObject.AddComponent<ARPlaneVisualizer>();
                Debug.Log($"[PetPlacementController] Added ARPlaneVisualizer to plane: {plane.gameObject.name}");
            }
            
            // 초기에는 모든 평면 시각화 비활성화
            var visualizer = plane.GetComponent<ARPlaneVisualizer>();
            if (visualizer != null)
            {
                visualizer.SetVisualizationActive(false);
            }
        }
        
        foreach (ARPlane plane in args.updated)
        {
            Debug.Log($"[PetPlacementController] Plane updated: {plane.gameObject.name}, Size: {plane.size}");
        }
        
        foreach (var removedPair in args.removed)
        {
            if (removedPair.Value == selectedPlane)
            {
                selectedPlane = null;
                Debug.Log("[PetPlacementController] Selected plane was removed");
            }
        }
    }

    void PlacePet(Pose pose)
    {
        if (petPrefab == null || selectedPlane == null) 
        {
            Debug.LogError("[PetPlacementController] Cannot place pet - missing prefab or selected plane");
            return;
        }

        if (petPrefab.scene.IsValid())
        {
            spawnedPet = petPrefab;
        }
        else
        {
            spawnedPet = Instantiate(petPrefab);
        }
        
        if (spawnedPet != null)
        {
            spawnedPet.transform.position = pose.position;
            
            // 카메라를 향하도록 회전
            Vector3 cameraPosition = arCamera.transform.position;
            Vector3 directionToCamera = (cameraPosition - spawnedPet.transform.position).normalized;
            directionToCamera.y = 0; 
            if (directionToCamera != Vector3.zero)
            {
                spawnedPet.transform.rotation = Quaternion.LookRotation(directionToCamera);
            }
            else
            {
                spawnedPet.transform.rotation = pose.rotation;
            }

            // PetMovementController 추가 및 설정
            petMovementController = spawnedPet.GetComponent<PetMovementController>();
            if (petMovementController == null)
            {
                petMovementController = spawnedPet.AddComponent<PetMovementController>();
            }
            petMovementController.SetPlane(selectedPlane);

            spawnedPet.SetActive(true);
            petPlaced = true;
            
            Debug.Log($"[PetPlacementController] Pet placed at {pose.position} on plane {selectedPlane.gameObject.name}");

            if (placementIndicator != null)
            {
                placementIndicator.SetActive(false);
            }
            
            // 모든 평면 시각화 비활성화
            foreach (var trackablePlane in planeManager.trackables)
            {
                var visualizer = trackablePlane.GetComponent<ARPlaneVisualizer>();
                if (visualizer != null)
                {
                    visualizer.SetVisualizationActive(false);
                }
            }
        }
    }
} 