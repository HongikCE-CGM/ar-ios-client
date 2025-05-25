using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PetPlacer : MonoBehaviour
{
    public GameObject petPrefab; // Inspector에서 연결할 프리팹
    private GameObject spawnedPet;
    private ARRaycastManager raycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        // 이미 배치된 경우 더 이상 배치하지 않음
        if (spawnedPet != null)
            return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // 터치 시작일 때만 실행
            if (touch.phase == TouchPhase.Began)
            {
                // 📌 Raycast 시도
                if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;

                    // 양 배치
                    spawnedPet = Instantiate(petPrefab, hitPose.position, hitPose.rotation);

                    // 카메라 방향으로 회전
                    spawnedPet.transform.LookAt(Camera.main.transform);
                    spawnedPet.transform.rotation = Quaternion.Euler(0, spawnedPet.transform.eulerAngles.y, 0);

                    Debug.Log("🐑 Pet placed at: " + hitPose.position);
                }
                else
                {
                    // ❌ 평면을 인식하지 못한 경우
                    Debug.LogWarning("Raycast failed: Plane not detected");
                }
            }
        }
    }
}

