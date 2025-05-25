using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using System.Collections.Generic;

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

    public PetFeederController petFeederController;

    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private Vector2 touchStartPos;
    private float touchStartTime;

    // ✅ 터치 판단 기준
    public float tapTimeThreshold = 0.3f;
    public float tapDistanceThreshold = 30f;

    void Update()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                touchStartPos = touch.position;
                touchStartTime = Time.time;
                break;

            case TouchPhase.Ended:
                float duration = Time.time - touchStartTime;
                float distance = (touch.position - touchStartPos).magnitude;

                if (duration < tapTimeThreshold && distance < tapDistanceThreshold)
                {
                    TrySpawnFeed(touch.position);
                }
                break;
        }
    }

    // ✅ 햄버거 생성 진입 함수
    void TrySpawnFeed(Vector2 screenPos)
    {
        if (arRaycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            SpawnAndFlyToPet(hitPose.position);
        }
    }

    void SpawnAndFlyToPet(Vector3 spawnPos)
    {
        GameObject hamburger = Instantiate(hamburgerPrefab, spawnPos, Quaternion.identity);

        Vector2 offset = Random.insideUnitCircle * dropRadius;
        Vector3 targetPos = petTransform.position + new Vector3(offset.x, 0f, offset.y);

        StartCoroutine(FlyToTarget(hamburger.transform, targetPos, hamburger));
    }

    IEnumerator FlyToTarget(Transform obj, Vector3 targetPos, GameObject hamburger)
    {
        Vector3 startPos = obj.position;
        float elapsed = 0f;

        while (elapsed < flightDuration)
        {
            float t = Mathf.SmoothStep(0, 1, elapsed / flightDuration);
            Vector3 midPoint = Vector3.Lerp(startPos, targetPos, t);
            midPoint.y += Mathf.Sin(t * Mathf.PI) * 0.5f;
            obj.position = midPoint;
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.position = targetPos;

        yield return new WaitForSeconds(0.6f);
        petFeederController.MoveToTarget(targetPos, hamburger);
    }
}


