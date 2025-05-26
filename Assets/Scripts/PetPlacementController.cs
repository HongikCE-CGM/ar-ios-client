using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;

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
        
        // 시작 시 모든 평면을 숨기고 첫 번째 평면만 활성화
        StartCoroutine(InitializePlanes());
        
        Debug.Log("[PetPlacementController] Start completed");
    }
    
    System.Collections.IEnumerator InitializePlanes()
    {
        // 프레임 대기 (평면이 생성될 시간을 줌)
        yield return new WaitForSeconds(0.5f);
        
        // 가장 큰 평면만 활성화
        ARPlane largestPlane = null;
        float largestArea = 0f;
        
        foreach (var plane in planeManager.trackables)
        {
            float area = plane.size.x * plane.size.y;
            if (area > largestArea)
            {
                largestArea = area;
                largestPlane = plane;
            }
        }
        
        // 모든 평면을 숨기고 가장 큰 평면만 표시
        foreach (var plane in planeManager.trackables)
        {
            if (plane == largestPlane)
            {
                plane.gameObject.SetActive(true);
                Debug.Log($"[PetPlacementController] Keeping largest plane active: {plane.gameObject.name}, Area: {largestArea}");
            }
            else
            {
                plane.gameObject.SetActive(false);
                Debug.Log($"[PetPlacementController] Hiding smaller plane: {plane.gameObject.name}");
            }
        }
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
                Debug.Log($"[PetPlacementController] Raycast hit! Number of hits: {hits.Count}");
                
                ARPlane hitPlane = null;
                if (hits[0].trackableId != TrackableId.invalidId)
                {
                    hitPlane = planeManager.GetPlane(hits[0].trackableId);
                    Debug.Log($"[PetPlacementController] Hit plane found: {hitPlane != null}, TrackableId: {hits[0].trackableId}");
                }
                
                if (hitPlane != null)
                {
                    Debug.Log($"[PetPlacementController] Current selected plane: {selectedPlane?.gameObject.name ?? "None"}");
                    Debug.Log($"[PetPlacementController] Hit plane: {hitPlane.gameObject.name}");
                    
                    if (selectedPlane == null)
                    {
                        // 첫 번째 평면 선택
                        SelectPlane(hitPlane);
                        Debug.Log($"[PetPlacementController] First plane selected: {hitPlane.gameObject.name}");
                    }
                    else if (hitPlane == selectedPlane)
                    {
                        // 같은 평면을 다시 터치하면 펫 배치
                        Debug.Log($"[PetPlacementController] Same plane touched again, placing pet...");
                        PlacePetAtPosition(hits[0].pose);
                    }
                    else
                    {
                        // 다른 평면 선택
                        SelectPlane(hitPlane);
                        Debug.Log($"[PetPlacementController] Different plane selected: {hitPlane.gameObject.name}");
                    }
                }
                else
                {
                    Debug.LogWarning("[PetPlacementController] Hit plane is null!");
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
        
        // 모든 평면을 숨기고 선택된 평면만 표시
        foreach (var trackablePlane in planeManager.trackables)
        {
            if (trackablePlane == selectedPlane)
            {
                trackablePlane.gameObject.SetActive(true);
                var visualizer = trackablePlane.GetComponent<ARPlaneVisualizer>();
                if (visualizer != null)
                {
                    visualizer.SetSelected(true);
                }
            }
            else
            {
                trackablePlane.gameObject.SetActive(false);
            }
        }
        
        Debug.Log($"[PetPlacementController] Selected plane: {plane.gameObject.name}, Size: {plane.size}");
    }

    void PlacePetAtPosition(Pose pose)
    {
        Debug.Log($"[PetPlacementController] PlacePetAtPosition called with pose: {pose.position}");
        
        if (petPrefab == null || selectedPlane == null) 
        {
            Debug.LogError($"[PetPlacementController] Cannot place pet - petPrefab: {petPrefab != null}, selectedPlane: {selectedPlane != null}");
            return;
        }

        Debug.Log($"[PetPlacementController] Pet prefab name: {petPrefab.name}, Is in scene: {petPrefab.scene.IsValid()}");

        if (petPrefab.scene.IsValid())
        {
            spawnedPet = petPrefab;
            Debug.Log("[PetPlacementController] Using existing pet from scene");
        }
        else
        {
            spawnedPet = Instantiate(petPrefab);
            Debug.Log("[PetPlacementController] Instantiated new pet");
        }
        
        if (spawnedPet != null)
        {
            // 펫의 위치를 평면 위에 정확히 배치
            Vector3 petPosition = pose.position;
            
            // 펫의 Renderer bounds를 확인하여 정확한 높이 계산
            Renderer petRenderer = spawnedPet.GetComponentInChildren<Renderer>();
            if (petRenderer != null)
            {
                // 펫의 바운드 박스 하단이 평면에 닿도록 조정
                float petBottomOffset = petRenderer.bounds.center.y - petRenderer.bounds.min.y;
                petPosition.y = pose.position.y + petBottomOffset;
                Debug.Log($"[PetPlacementController] Pet bounds offset: {petBottomOffset}, Final Y: {petPosition.y}");
            }
            else
            {
                // Renderer가 없으면 기본값 사용
                petPosition.y = pose.position.y;
                Debug.LogWarning("[PetPlacementController] No renderer found on pet, using default position");
            }
            
            spawnedPet.transform.position = petPosition;
            Debug.Log($"[PetPlacementController] Pet position set to: {spawnedPet.transform.position}, Plane Y: {pose.position.y}");
            
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
                Debug.Log("[PetPlacementController] Added PetMovementController to pet");
            }
            petMovementController.SetPlane(selectedPlane);

            spawnedPet.SetActive(true);
            petPlaced = true;
            
            Debug.Log($"[PetPlacementController] Pet placed successfully! Active: {spawnedPet.activeSelf}, Position: {spawnedPet.transform.position}");

            // 펫 배치 후 모든 평면을 숨김
            foreach (var trackablePlane in planeManager.trackables)
            {
                trackablePlane.gameObject.SetActive(false);
                Debug.Log($"[PetPlacementController] Hiding plane: {trackablePlane.gameObject.name}");
            }
            
            // 평면 감지 비활성화
            planeManager.enabled = false;
            Debug.Log("[PetPlacementController] Plane detection disabled after pet placement");
        }
        else
        {
            Debug.LogError("[PetPlacementController] Failed to create pet GameObject!");
        }
    }

    void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        Debug.Log($"[PetPlacementController] Planes changed - Added: {args.added.Count}, Updated: {args.updated.Count}, Removed: {args.removed.Count}");
        
        // 펫이 이미 배치되었으면 평면 업데이트 무시
        if (petPlaced)
        {
            Debug.Log("[PetPlacementController] Pet already placed, ignoring plane updates");
            return;
        }
        
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
            
            // 선택된 평면이 없으면 새 평면을 활성화
            if (selectedPlane == null)
            {
                // 기존 평면들을 모두 숨김
                foreach (var existingPlane in planeManager.trackables)
                {
                    if (existingPlane != plane)
                    {
                        existingPlane.gameObject.SetActive(false);
                    }
                }
                
                // 새 평면 활성화
                plane.gameObject.SetActive(true);
                Debug.Log($"[PetPlacementController] Activated new plane: {plane.gameObject.name}");
            }
            else
            {
                // 선택된 평면이 있으면 새 평면은 숨김
                plane.gameObject.SetActive(false);
                Debug.Log($"[PetPlacementController] Hiding new plane {plane.gameObject.name} as we have a selected plane");
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
            // 선택되지 않은 평면이 업데이트되고 더 크면 전환
            if (selectedPlane == null && plane.gameObject.activeSelf)
            {
                // 현재 활성화된 평면 찾기
                ARPlane currentActivePlane = null;
                foreach (var p in planeManager.trackables)
                {
                    if (p.gameObject.activeSelf && p != plane)
                    {
                        currentActivePlane = p;
                        break;
                    }
                }
                
                // 새 평면이 더 크면 전환
                if (currentActivePlane != null)
                {
                    float currentArea = currentActivePlane.size.x * currentActivePlane.size.y;
                    float newArea = plane.size.x * plane.size.y;
                    
                    if (newArea > currentArea * 1.5f) // 1.5배 이상 크면 전환
                    {
                        currentActivePlane.gameObject.SetActive(false);
                        plane.gameObject.SetActive(true);
                        Debug.Log($"[PetPlacementController] Switched to larger plane: {plane.gameObject.name} (Area: {newArea} vs {currentArea})");
                    }
                }
            }
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