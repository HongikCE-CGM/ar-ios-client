// Assets/FeedSpawner.cs
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
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

    [Header("연결할 컴포넌트")]
    public PetFeederController petFeederController;
    public FeedUIButtonCreator feedUIButtonCreator;  // ★ 드래그해서 할당

    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

    void Update()
    {
        // 1) 터치가 없으면 무시
        if (Input.touchCount == 0)
            return;
        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
            return;

        // 2) UI 위 터치면 무시 (버튼 클릭 우선)
        if (IsPointerOverUI(touch))
            return;

        // 3) “먹이주기” Off 상태면 무시
        if (feedUIButtonCreator != null && !feedUIButtonCreator.IsFeedOn())
            return;

        // 4) AR 레이캐스트 실행
        Vector2 touchPos = touch.position;
        if (arRaycastManager.Raycast(touchPos, hits, TrackableType.PlaneWithinPolygon))
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
            midPoint.y += Mathf.Sin(t * Mathf.PI) * 0.5f;
            obj.position = midPoint;
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.position = targetPos;
        yield return new WaitForSeconds(0.6f);
        petFeederController.MoveToTarget(targetPos, hamburger);
    }

    // 터치 위치에 UI가 있는지 GraphicRaycaster로 검사
    private bool IsPointerOverUI(Touch touch)
    {
        if (EventSystem.current == null)
            return false;
        var pointerData = new PointerEventData(EventSystem.current) { position = touch.position };
        uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(pointerData, uiRaycastResults);
        return uiRaycastResults.Count > 0;
    }
}