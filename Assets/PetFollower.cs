using UnityEngine;

public class PetFollower : MonoBehaviour
{
    public float baseSpeed = 4.5f;                       // ✅ 기본 속도 증가
    public float stopThreshold = 0.3f;
    public float followDistance = 3.5f;
    public float yOffset = 0f;

    public float closeFollowDistance = 2.0f;
    public float frontBoostMultiplier = 1.5f;

    public float followSmoothTime = 0.5f;

    public PetFeederController feederController;

    private Animator animator;
    private Quaternion lastCameraRotation;
    private Vector3 lastPosition;
    private float idleCheckTimer = 0f;
    private float idleThresholdTime = 0.5f;
    private float movementSpeedThreshold = 0.01f;

    private Vector3 smoothedTargetPosition;

    void Start()
    {
        lastCameraRotation = Camera.main.transform.rotation;
        yOffset = transform.position.y;
        animator = GetComponent<Animator>();
        lastPosition = transform.position;
        smoothedTargetPosition = transform.position;
    }

    void Update()
    {
        if (feederController != null && feederController.IsMovingToFood)
            return;

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

        // ✅ 자연스럽게 따라가는 목표 위치
        smoothedTargetPosition = Vector3.Lerp(smoothedTargetPosition, targetPosition, Time.deltaTime / followSmoothTime);

        float distance = Vector3.Distance(transform.position, smoothedTargetPosition);

        if (distance > stopThreshold)
        {
            SetRunning(true);
            idleCheckTimer = 0f;

            Quaternion currentRot = Camera.main.transform.rotation;
            float rotationSpeed = Quaternion.Angle(lastCameraRotation, currentRot) / Time.deltaTime;
            lastCameraRotation = currentRot;

            float distanceRatio = Mathf.Clamp01(distance / followDistance);
            float frontBoost = Mathf.Lerp(1f, frontBoostMultiplier, frontDot);

            // ✅ 이동 속도 보정: 더 빠르게
            float speed = (baseSpeed * distanceRatio + rotationSpeed * 0.03f) * frontBoost;

            Vector3 newPosition = Vector3.MoveTowards(transform.position, smoothedTargetPosition, speed * Time.deltaTime);

            // ✅ 이동 방향을 기준으로 회전
            Vector3 moveDirection = newPosition - transform.position;
            moveDirection.y = 0f;

            if (moveDirection != Vector3.zero)
            {
                Quaternion moveRot = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, moveRot, Time.deltaTime * 6f);
            }

            transform.position = newPosition;
        }
        else
        {
            float movement = (transform.position - lastPosition).magnitude / Time.deltaTime;

            if (movement < movementSpeedThreshold)
            {
                idleCheckTimer += Time.deltaTime;
                if (idleCheckTimer >= idleThresholdTime)
                {
                    SetRunning(false);
                    if (animator != null)
                        animator.speed = 1f; // Idle 상태에서는 속도 초기화
                }
            }
            else
            {
                SetRunning(true);
                idleCheckTimer = 0f;

                // ✅ 이동 속도에 따라 애니메이션 속도 보정
                if (animator != null)
                {
                    float animSpeed = Mathf.Clamp(movement / baseSpeed, 0.6f, 1.5f);
                    animator.speed = animSpeed;
                }
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







