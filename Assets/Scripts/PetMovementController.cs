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
    private Vector3 planeCenter;
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
            
            planeCenter = currentPlane.center;
            planeSize = currentPlane.size;
            
            Debug.Log($"[PetMovementController] Plane set - Center: {planeCenter}, Size: {planeSize}, Pet Scale: {scaleFactor}");
            
            // 초기 위치를 평면 중앙으로 설정
            transform.position = currentPlane.transform.TransformPoint(planeCenter);
            
            // 첫 목표 위치 설정
            SetNewTargetPosition();
        }
    }
    
    void Update()
    {
        if (currentPlane == null) return;
        
        // 평면 정보 업데이트
        planeCenter = currentPlane.center;
        planeSize = currentPlane.size;
        
        if (!isMoving)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                SetNewTargetPosition();
                isMoving = true;
                if (animator != null)
                {
                    animator.SetBool("isWalking", true);
                }
                Debug.Log($"[PetMovementController] Starting movement to: {targetPosition}");
            }
        }
        else
        {
            MoveToTarget();
        }
    }
    
    void SetNewTargetPosition()
    {
        // 평면 내에서 랜덤한 위치 선택
        float randomX = Random.Range(-planeSize.x / 2 + boundaryPadding, planeSize.x / 2 - boundaryPadding);
        float randomZ = Random.Range(-planeSize.y / 2 + boundaryPadding, planeSize.y / 2 - boundaryPadding);
        
        Vector3 localPosition = planeCenter + new Vector3(randomX, 0, randomZ);
        targetPosition = currentPlane.transform.TransformPoint(localPosition);
        
        Debug.Log($"[PetMovementController] New target position set: Local({randomX}, 0, {randomZ}) -> World({targetPosition})");
    }
    
    void MoveToTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Y축 이동 제거
        
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        if (distance > 0.1f)
        {
            // 이동
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            // 회전
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            // 평면 경계 체크
            Vector3 localPos = currentPlane.transform.InverseTransformPoint(transform.position);
            localPos.x = Mathf.Clamp(localPos.x, planeCenter.x - planeSize.x / 2 + boundaryPadding, 
                                    planeCenter.x + planeSize.x / 2 - boundaryPadding);
            localPos.z = Mathf.Clamp(localPos.z, planeCenter.z - planeSize.y / 2 + boundaryPadding, 
                                    planeCenter.z + planeSize.y / 2 - boundaryPadding);
            transform.position = currentPlane.transform.TransformPoint(localPos);
        }
        else
        {
            // 목표 지점 도달
            isMoving = false;
            waitTimer = Random.Range(minWaitTime, maxWaitTime);
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
            }
            Debug.Log($"[PetMovementController] Reached target. Waiting for {waitTimer} seconds");
        }
    }
    
    void OnDrawGizmos()
    {
        if (currentPlane != null)
        {
            // 평면 경계 시각화
            Gizmos.color = Color.yellow;
            Vector3 worldCenter = currentPlane.transform.TransformPoint(planeCenter);
            Vector3 worldSize = new Vector3(planeSize.x, 0.01f, planeSize.y);
            Gizmos.DrawWireCube(worldCenter, worldSize);
            
            // 목표 위치 시각화
            if (isMoving)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(targetPosition, 0.1f);
            }
        }
    }
} 