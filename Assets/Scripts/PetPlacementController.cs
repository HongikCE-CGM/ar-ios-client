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

        HandleTouch();
    }

    void HandleTouch()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPosition = touch.position;
            
            Debug.Log($"[PetPlacementController] Touch detected at: {touchPosition}");
            
            // 터치 위치에서 레이캐스트
            if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                ARPlane hitPlane = null;
                if (hits[0].trackableId != TrackableId.invalidId)
                {
                    hitPlane = planeManager.GetPlane(hits[0].trackableId);
                }
                
                if (hitPlane != null)
                {
                    if (selectedPlane == null)
                    {
                        // 첫 번째 평면 선택
                        SelectPlane(hitPlane);
                        Debug.Log($"[PetPlacementController] First plane selected: {hitPlane.gameObject.name}");
                    }
                    else if (hitPlane == selectedPlane)
                    {
                        // 같은 평면을 다시 터치하면 펫 배치
                        PlacePetAtPosition(hits[0].pose);
                        Debug.Log($"[PetPlacementController] Pet placed on selected plane");
                    }
                    else
                    {
                        // 다른 평면 선택
                        SelectPlane(hitPlane);
                        Debug.Log($"[PetPlacementController] Different plane selected: {hitPlane.gameObject.name}");
                    }
                }
            }
            else
            {
                Debug.Log("[PetPlacementController] Touch did not hit any plane");
            }
        }
    }

    void SelectPlane(ARPlane plane)
    {
        selectedPlane = plane;
        
        // 모든 평면의 선택 상태 업데이트
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

    void PlacePetAtPosition(Pose pose)
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

            // 모든 평면 시각화를 기본 상태로 되돌림
            foreach (var trackablePlane in planeManager.trackables)
            {
                var visualizer = trackablePlane.GetComponent<ARPlaneVisualizer>();
                if (visualizer != null)
                {
                    visualizer.SetSelected(false);
                }
            }
        }
    }

    void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        Debug.Log($"[PetPlacementController] Planes changed - Added: {args.added.Count}, Updated: {args.updated.Count}, Removed: {args.removed.Count}");
        
        // ARPlaneManager의 planePrefab 확인
        if (planeManager != null && planeManager.planePrefab != null)
        {
            Debug.Log($"[PetPlacementController] Plane prefab active: {planeManager.planePrefab.activeSelf}");
            if (!planeManager.planePrefab.activeSelf)
            {
                Debug.LogWarning("[PetPlacementController] WARNING: Plane prefab is inactive! Planes won't be visible!");
            }
        }
        
        foreach (ARPlane plane in args.added)
        {
            Debug.Log($"[PetPlacementController] New plane added: {plane.gameObject.name}, Active: {plane.gameObject.activeSelf}");
            
            // GameObject 활성화
            if (!plane.gameObject.activeSelf)
            {
                plane.gameObject.SetActive(true);
                Debug.Log($"[PetPlacementController] Activated plane GameObject: {plane.gameObject.name}");
            }
            
            // ARPlaneMeshVisualizer 컴포넌트 추가 (메시 생성을 위해)
            if (plane.GetComponent<ARPlaneMeshVisualizer>() == null)
            {
                plane.gameObject.AddComponent<ARPlaneMeshVisualizer>();
                Debug.Log($"[PetPlacementController] Added ARPlaneMeshVisualizer to plane: {plane.gameObject.name}");
            }
            
            // ARPlaneVisualizer 컴포넌트 추가 (색상 표시를 위해)
            if (plane.GetComponent<ARPlaneVisualizer>() == null)
            {
                plane.gameObject.AddComponent<ARPlaneVisualizer>();
                Debug.Log($"[PetPlacementController] Added ARPlaneVisualizer to plane: {plane.gameObject.name}");
            }
            else
            {
                Debug.Log($"[PetPlacementController] ARPlaneVisualizer already exists on plane: {plane.gameObject.name}");
            }
        }
        
        foreach (ARPlane plane in args.updated)
        {
            // 업데이트 로그를 줄임 (너무 많은 로그 방지)
            // Debug.Log($"[PetPlacementController] Plane updated: {plane.gameObject.name}, Size: {plane.size}");
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
} 