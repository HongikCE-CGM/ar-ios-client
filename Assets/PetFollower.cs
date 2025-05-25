using UnityEngine;

public class PetFollower : MonoBehaviour
{
    public float baseSpeed = 2f;
    public float stopThreshold = 0.3f;                 // ✅ 반경 넓힘
    public float followDistance = 3.5f;
    public float yOffset = 0f;

    private Quaternion lastCameraRotation;
    private Animator animator;

    private float closeFollowDistance = 2.0f;
    private float frontBoostMultiplier = 1.5f;

    private Vector3 lastPosition;
    private float idleCheckTimer = 0f;
    private float idleThresholdTime = 0.5f;            // ✅ 정지 판단 시간
    private float movementSpeedThreshold = 0.01f;      // ✅ 움직임 미미할 때

    void Start()
    {
        lastCameraRotation = Camera.main.transform.rotation;
        yOffset = transform.position.y;
        animator = GetComponent<Animator>();
        lastPosition = transform.position;
    }

    void Update()
    {
        if (Camera.main == null) return;

        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 camPos = Camera.main.transform.position;

        Vector3 toPet = (transform.position - camPos).normalized;
        float frontDot = Vector3.Dot(cameraForward, toPet);
        float dynamicFollowDistance = Mathf.Lerp(closeFollowDistance, followDistance, 1 - frontDot);

        Vector3 targetPosition = camPos + cameraForward * dynamicFollowDistance;
        targetPosition.y = yOffset;

        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance > stopThreshold)
        {
            // 거리 먼 경우 → 무조건 run
            SetRunning(true);
            idleCheckTimer = 0f;

            Quaternion currentRot = Camera.main.transform.rotation;
            float rotationSpeed = Quaternion.Angle(lastCameraRotation, currentRot) / Time.deltaTime;
            lastCameraRotation = currentRot;

            float distanceRatio = Mathf.Clamp01(distance / followDistance);
            float frontBoost = Mathf.Lerp(1f, frontBoostMultiplier, frontDot);
            float speed = (baseSpeed * distanceRatio + rotationSpeed * 0.05f) * frontBoost;

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            Vector3 lookDirection = camPos - transform.position;
            lookDirection.y = 0f;
            if (lookDirection != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
            }
        }
        else
        {
            // ✅ 속도 감지 기반 idle 전이
            float movement = (transform.position - lastPosition).magnitude / Time.deltaTime;
            if (movement < movementSpeedThreshold)
            {
                idleCheckTimer += Time.deltaTime;
                if (idleCheckTimer >= idleThresholdTime)
                {
                    SetRunning(false);
                }
            }
            else
            {
                SetRunning(true);
                idleCheckTimer = 0f;
            }
        }

        lastPosition = transform.position;
    }

    private void SetRunning(bool isRunning)
    {
        if (animator != null)
            animator.SetBool("isRunning", isRunning);
    }
}





