using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class SoccerBallShooter : MonoBehaviour
{
    [Header("프리팹 & 설정")]
    public GameObject soccerBallPrefab;
    public Transform petTransform;
    public float swipeThreshold = 50f;
    public float minSwipeTime = 0.1f;
    public float maxSwipeTime = 1.0f;
    public float ballForce = 5f;

    [Header("AR 설정")]
    public ARRaycastManager arRaycastManager;

    private Vector2 touchStartPos;
    private float touchStartTime;

    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

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
                Vector2 touchEndPos = touch.position;
                float swipeDist = (touchEndPos - touchStartPos).magnitude;
                float swipeTime = Time.time - touchStartTime;

                if (swipeDist > swipeThreshold && swipeTime <= maxSwipeTime)
                {
                    ShootBall();
                }
                break;
        }
    }

    void ShootBall()
    {
        // 카메라 앞쪽 위치에서 공 생성
        Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;

        // ✅ 펫보다 약간 위에서 생성 → 떨어지도록
        spawnPos.y = petTransform.position.y + 0.3f;

        GameObject ball = Instantiate(soccerBallPrefab, spawnPos, Quaternion.identity);

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("⚠️ Rigidbody가 없습니다!");
            return;
        }

        Vector3 toPet = (petTransform.position - spawnPos).normalized;
        Vector3 launchDir = (toPet + Vector3.up * 0.8f).normalized;


        rb.AddForce(launchDir * ballForce, ForceMode.Impulse);

    }

}

