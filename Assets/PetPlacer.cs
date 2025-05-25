using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PetPlacer : MonoBehaviour
{
    [Header("Pet Placement")]
    public GameObject petPrefab; // Inspector에서 연결할 프리팹
    public GameObject existingPet; // 이미 씬에 있는 펫 (옵션)
    
    [Header("Placement Indicator")]
    public GameObject placementIndicator; // 배치 위치 표시자
    
    private GameObject spawnedPet;
    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool hasDetectedPlanes = false;
    private bool petPlaced = false;

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
        
        // 기본 배치 표시자 생성
        if (placementIndicator == null)
        {
            CreateDefaultPlacementIndicator();
        }
    }
    
    void Start()
    {
        // 기존 펫이 있으면 찾기
        if (existingPet == null)
        {
            existingPet = GameObject.FindGameObjectWithTag("Pet");
        }
        
        // 기존 펫이 있으면 비활성화 (평면 감지 후 배치할 때까지)
        if (existingPet != null)
        {
            existingPet.SetActive(false);
            Debug.Log("🐑 기존 펫을 찾았습니다. 평면 감지 후 배치됩니다.");
        }
        
        // 배치 표시자 비활성화
        if (placementIndicator != null)
            placementIndicator.SetActive(false);
    }
    
    void CreateDefaultPlacementIndicator()
    {
        // 기본 배치 표시자 생성 (링 모양)
        placementIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        placementIndicator.transform.localScale = new Vector3(0.5f, 0.01f, 0.5f);
        
        // 머티리얼 설정
        Renderer renderer = placementIndicator.GetComponent<Renderer>();
        Material indicatorMat = new Material(Shader.Find("Standard"));
        indicatorMat.color = new Color(0, 1, 0, 0.5f); // 반투명 초록색
        indicatorMat.SetFloat("_Mode", 3); // Transparent 모드
        indicatorMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        indicatorMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        indicatorMat.SetInt("_ZWrite", 0);
        indicatorMat.DisableKeyword("_ALPHATEST_ON");
        indicatorMat.EnableKeyword("_ALPHABLEND_ON");
        indicatorMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        indicatorMat.renderQueue = 3000;
        renderer.material = indicatorMat;
        
        // Collider 제거
        Destroy(placementIndicator.GetComponent<Collider>());
        
        Debug.Log("✅ 기본 배치 표시자 생성됨");
    }
    
    void CheckForPlanes()
    {
        if (planeManager != null && planeManager.trackables.count > 0 && !hasDetectedPlanes)
        {
            hasDetectedPlanes = true;
            Debug.Log("✅ 평면 감지됨! 이제 터치해서 펫을 배치할 수 있습니다.");
        }
    }

    void Update()
    {
        // 평면 감지 확인
        CheckForPlanes();
        
        // 이미 배치된 경우 더 이상 배치하지 않음
        if (petPlaced)
        {
            if (placementIndicator != null)
                placementIndicator.SetActive(false);
            return;
        }
        
        // 평면이 감지되지 않은 경우 대기
        if (!hasDetectedPlanes)
        {
            if (placementIndicator != null)
                placementIndicator.SetActive(false);
            return;
        }

        // 화면 중앙에서 Raycast 수행 (배치 표시자용)
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        UpdatePlacementIndicator(screenCenter);

        // 터치 입력 처리
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // 터치 시작일 때만 실행
            if (touch.phase == TouchPhase.Began)
            {
                PlacePetAtTouch(touch.position);
            }
        }
    }
    
    void UpdatePlacementIndicator(Vector2 screenPosition)
    {
        if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(true);
                placementIndicator.transform.position = hitPose.position;
                placementIndicator.transform.rotation = hitPose.rotation;
            }
        }
        else
        {
            if (placementIndicator != null)
                placementIndicator.SetActive(false);
        }
    }
    
    void PlacePetAtTouch(Vector2 touchPosition)
    {
        // 📌 Raycast 시도
        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            // 기존 펫이 있으면 이동, 없으면 새로 생성
            if (existingPet != null)
            {
                spawnedPet = existingPet;
                spawnedPet.transform.position = hitPose.position;
                spawnedPet.transform.rotation = hitPose.rotation;
                spawnedPet.SetActive(true);
                Debug.Log("🐑 기존 펫을 평면에 배치했습니다: " + hitPose.position);
            }
            else if (petPrefab != null)
            {
                spawnedPet = Instantiate(petPrefab, hitPose.position, hitPose.rotation);
                Debug.Log("🐑 새 펫을 평면에 배치했습니다: " + hitPose.position);
            }
            else
            {
                Debug.LogWarning("❌ 펫 프리팹이나 기존 펫이 없습니다!");
                return;
            }

            // 카메라 방향으로 회전
            spawnedPet.transform.LookAt(Camera.main.transform);
            spawnedPet.transform.rotation = Quaternion.Euler(0, spawnedPet.transform.eulerAngles.y, 0);

            // 배치 완료 표시
            petPlaced = true;

            // 배치 표시자 비활성화
            if (placementIndicator != null)
                placementIndicator.SetActive(false);

            // FeedSpawner에 펫 정보 전달
            FeedSpawner feedSpawner = FindFirstObjectByType<FeedSpawner>();
            if (feedSpawner != null && feedSpawner.petTransform == null)
            {
                feedSpawner.petTransform = spawnedPet.transform;
                Debug.Log("🔗 FeedSpawner에 펫 연결됨");
            }

            Debug.Log("✅ 펫이 AR 평면에 성공적으로 배치되었습니다!");
        }
        else
        {
            // ❌ 평면을 인식하지 못한 경우
            Debug.LogWarning("❌ 평면을 찾을 수 없습니다. 평면을 향해 터치해주세요.");
        }
    }
}

