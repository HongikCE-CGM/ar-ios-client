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
        // 더 긴 대기 시간으로 평면이 안정화될 때까지 기다림
        yield return new WaitForSeconds(1.0f);
        
        // 주기적으로 평면 상태 확인
        for (int i = 0; i < 10; i++)
        {
            UpdateActivePlane();
            yield return new WaitForSeconds(0.2f);
        }
        
        Debug.Log("[PetPlacementController] Plane initialization completed");
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

        // 실시간으로 카메라 중앙의 평면 업데이트 (0.5초마다)
        if (Time.time % 0.5f < Time.deltaTime)
        {
            UpdateActivePlane();
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
            Debug.Log($"[PetPlacementController] New plane added: {plane.gameObject.name}, TrackingState: {plane.trackingState}");
            
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
        }
        
        // 모든 평면의 상태를 확인하고 가장 적합한 평면 선택
        UpdateActivePlane();
        
        foreach (var removedPair in args.removed)
        {
            Debug.Log($"[PetPlacementController] Plane removed: {removedPair.Value?.gameObject.name}");
            if (removedPair.Value == selectedPlane)
            {
                selectedPlane = null;
                Debug.Log("[PetPlacementController] Selected plane was removed, will find new active plane");
                // 새로운 활성 평면 찾기
                UpdateActivePlane();
            }
        }
        
        foreach (ARPlane plane in args.updated)
        {
            // 추적 상태가 변경된 평면 처리
            if (plane.trackingState == TrackingState.Tracking)
            {
                Debug.Log($"[PetPlacementController] Plane {plane.gameObject.name} is now tracking properly");
                // 추적이 복구된 평면이 더 적합하면 전환
                UpdateActivePlane();
            }
            else if (plane.trackingState == TrackingState.None)
            {
                Debug.Log($"[PetPlacementController] Plane {plane.gameObject.name} lost tracking");
            }
        }
    }
    
    void UpdateActivePlane()
    {
        if (selectedPlane != null) return; // 이미 선택된 평면이 있으면 변경하지 않음
        
        ARPlane bestPlane = null;
        float closestDistance = float.MaxValue;
        
        // 카메라 중앙에서 레이캐스트
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        
        foreach (var plane in planeManager.trackables)
        {
            // 추적 상태가 좋고 충분히 큰 평면만 고려
            if (plane.trackingState == TrackingState.Tracking)
            {
                float area = plane.size.x * plane.size.y;
                if (area < 0.2f) continue; // 너무 작은 평면은 제외
                
                // 카메라에서 평면까지의 거리 계산
                Vector3 planeCenter = plane.transform.TransformPoint(plane.center);
                Vector3 cameraPosition = arCamera.transform.position;
                float distance = Vector3.Distance(cameraPosition, planeCenter);
                
                // 카메라 중앙에서 평면으로 레이캐스트하여 실제로 보이는지 확인
                if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
                {
                    foreach (var hit in hits)
                    {
                        ARPlane hitPlane = planeManager.GetPlane(hit.trackableId);
                        if (hitPlane == plane && distance < closestDistance)
                        {
                            closestDistance = distance;
                            bestPlane = plane;
                            break;
                        }
                    }
                }
            }
        }
        
        // 레이캐스트로 찾지 못했다면 가장 가까운 평면 사용
        if (bestPlane == null)
        {
            foreach (var plane in planeManager.trackables)
            {
                if (plane.trackingState == TrackingState.Tracking)
                {
                    float area = plane.size.x * plane.size.y;
                    if (area < 0.2f) continue;
                    
                    Vector3 planeCenter = plane.transform.TransformPoint(plane.center);
                    Vector3 cameraPosition = arCamera.transform.position;
                    float distance = Vector3.Distance(cameraPosition, planeCenter);
                    
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        bestPlane = plane;
                    }
                }
            }
        }
        
        // 모든 평면을 숨기고 카메라 중앙에 가장 가까운 평면만 표시
        foreach (var plane in planeManager.trackables)
        {
            if (plane == bestPlane && plane.trackingState == TrackingState.Tracking)
            {
                plane.gameObject.SetActive(true);
                Debug.Log($"[PetPlacementController] Activated closest plane to camera center: {plane.gameObject.name}, Distance: {closestDistance:F2}m, Area: {plane.size.x * plane.size.y:F2}");
            }
            else
            {
                plane.gameObject.SetActive(false);
            }
        }
    }
} 