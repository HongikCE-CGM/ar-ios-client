using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PetMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private float rotationSpeed = 2.0f;
    [SerializeField] private float minWaitTime = 2.0f;
    [SerializeField] private float maxWaitTime = 5.0f;
    [SerializeField] private float boundaryPadding = 0.1f;
    
    [Header("Scale Settings")]
    [SerializeField] private float baseScale = 0.3f;
    [SerializeField] private float scaleMultiplier = 0.5f;
    
    private ARPlane currentPlane;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float waitTimer = 0f;
    private Animator animator;
    private Vector2 planeSize;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        Debug.Log("[PetMovementController] Started");
    }
    
    public void SetPlane(ARPlane plane)
    {
        currentPlane = plane;
        if (currentPlane != null)
        {
            // 평면 크기에 따라 펫 스케일 조정
            float planeArea = currentPlane.size.x * currentPlane.size.y;
            float scaleFactor = baseScale + (planeArea * scaleMultiplier);
            scaleFactor = Mathf.Clamp(scaleFactor, 0.2f, 1.0f);
            transform.localScale = Vector3.one * scaleFactor;

            planeSize = currentPlane.size;
            Debug.Log($"[PetMovementController] Plane set - Size: {planeSize}, Pet Scale: {scaleFactor}");

            // 초기 위치를 평면 중앙(ARPlane Transform 위치)으로 설정
            transform.position = currentPlane.transform.position;

            // 첫 목표 위치 설정
            SetNewTargetPosition();
        }
    }
    
    void Update()
    {
        // **추가된 부분**: 앉거나 눕고 있으면 걸어다니지 않음
        if (animator.GetBool("isSitting") || animator.GetBool("isSleeping"))
        {
            animator.SetBool("isWalking", false);
            return;
        }

        if (currentPlane == null) return;
        
        if (!isMoving)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                SetNewTargetPosition();
                isMoving = true;
                animator.SetBool("isWalking", true);
                Debug.Log($"[PetMovementController] Moving to: {targetPosition}");
            }
        }
        else
        {
            MoveToTarget();
        }
    }
    
    private void SetNewTargetPosition()
    {
        float halfX = planeSize.x * 0.5f - boundaryPadding;
        float halfZ = planeSize.y * 0.5f - boundaryPadding;
        float randomX = Random.Range(-halfX, halfX);
        float randomZ = Random.Range(-halfZ, halfZ);

        Vector3 localOffset = new Vector3(randomX, 0f, randomZ);
        targetPosition = currentPlane.transform.TransformPoint(localOffset);

        waitTimer = Random.Range(minWaitTime, maxWaitTime);
    }
    
    private void MoveToTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0f;

        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance > 0.1f)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
            if (direction != Vector3.zero)
            {
                Quaternion look = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            isMoving = false;
            animator.SetBool("isWalking", false);
            Debug.Log($"[PetMovementController] Reached. Next wait: {waitTimer:F2}s");
        }
    }
    
    void OnDrawGizmos()
    {
        if (currentPlane == null) return;
        Gizmos.color = Color.yellow;
        Vector3 center = currentPlane.transform.position;
        Vector3 size = new Vector3(planeSize.x, 0.01f, planeSize.y);
        Gizmos.DrawWireCube(center, size);
        if (isMoving)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPosition, 0.1f);
        }
    }
}