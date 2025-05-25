using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.Collections; 

public class FeedSpawner : MonoBehaviour
{
    [Header("필수 프리팹")]
    public GameObject hamburgerPrefab;
    public Transform petTransform;

    [Header("설정")]
    public float dropRadius = 1.0f;
    public float flightDuration = 1.2f;

    [Header("AR 매니저")]
    public ARRaycastManager arRaycastManager;

    public PetFeederController petFeederController; // ✅ 여기 연결

    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        if (Input.touchCount == 0 || Input.GetTouch(0).phase != TouchPhase.Began)
            return;

        Vector2 touchPos = Input.GetTouch(0).position;

        if (arRaycastManager.Raycast(touchPos, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            SpawnAndFlyToPet(hitPose.position);
        }
    }

    void SpawnAndFlyToPet(Vector3 spawnPos)
    {
        GameObject hamburger = Instantiate(hamburgerPrefab, spawnPos, Quaternion.identity);

        // 펫 근처 랜덤 위치 계산
        Vector2 offset = Random.insideUnitCircle * dropRadius;
        Vector3 targetPos = petTransform.position + new Vector3(offset.x, 0f, offset.y);

        // ✅ 먹이 비행 → 착지 후 → 펫 이동
        StartCoroutine(FlyToTarget(hamburger.transform, targetPos, hamburger));

         // ✅ 핵심 코드
        petFeederController.MoveToTarget(targetPos, hamburger);
    }

    IEnumerator FlyToTarget(Transform obj, Vector3 targetPos, GameObject hamburger)
    {
        Vector3 startPos = obj.position;
        float elapsed = 0f;

        while (elapsed < flightDuration)
        {
            float t = Mathf.SmoothStep(0, 1, elapsed / flightDuration);
            Vector3 midPoint = Vector3.Lerp(startPos, targetPos, t);
            midPoint.y += Mathf.Sin(t * Mathf.PI) * 0.5f;  // 포물선 곡선

            obj.position = midPoint;
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.position = targetPos;

        yield return new WaitForSeconds(0.6f); // ✅ 착지 후 반응 딜레이

        // ✅ 비행 완료 후에 펫에게 이동 명령
        petFeederController.MoveToTarget(targetPos, hamburger);
    }
}

