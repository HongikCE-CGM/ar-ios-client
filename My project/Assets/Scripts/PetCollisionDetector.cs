using UnityEngine;

namespace ARPetGame
{
    public class PetCollisionDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        public float detectionRadius = 1f;
        public LayerMask ballLayerMask = -1;
        
        [Header("Hit Force")]
        public float hitForce = 10f;
        public Vector3 hitDirection = Vector3.forward;
        
        private PetController petController;
        private Collider petCollider;
        
        // Events
        public System.Action<GameObject> OnBallDetected;
        public System.Action<GameObject> OnBallHit;
        
        private void Awake()
        {
            petController = GetComponent<PetController>();
            petCollider = GetComponent<Collider>();
        }
        
        private void Start()
        {
            if (petCollider != null)
            {
                petCollider.isTrigger = false; // Physics collision
            }
        }
        
        private void Update()
        {
            DetectBallInRange();
        }
        
        private void DetectBallInRange()
        {
            Collider[] ballsInRange = Physics.OverlapSphere(
                transform.position, 
                detectionRadius, 
                ballLayerMask
            );
            
            foreach (Collider ball in ballsInRange)
            {
                if (ball.CompareTag("Ball"))
                {
                    OnBallDetected?.Invoke(ball.gameObject);
                    
                    // Pet AI: 공이 가까이 오면 액션 준비
                    if (petController != null)
                    {
                        float distance = Vector3.Distance(transform.position, ball.transform.position);
                        if (distance <= detectionRadius * 0.7f) // 70% 거리에서 반응
                        {
                            petController.PerformBallAction();
                        }
                    }
                }
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ball"))
            {
                HandleBallCollision(collision);
            }
        }
        
        private void HandleBallCollision(Collision collision)
        {
            BallController ballController = collision.gameObject.GetComponent<BallController>();
            if (ballController != null)
            {
                // 공의 방향을 계산
                Vector3 hitDir = CalculateHitDirection(collision);
                
                // 공을 튕겨냄
                ballController.HitBall(hitDir);
                
                // 펫 애니메이션 트리거
                if (petController != null)
                {
                    petController.PerformBallAction();
                }
                
                OnBallHit?.Invoke(collision.gameObject);
                
                Debug.Log($"Pet hit ball with direction: {hitDir}");
            }
        }
        
        private Vector3 CalculateHitDirection(Collision collision)
        {
            // 충돌 지점에서 공이 날아갈 방향 계산
            Vector3 contactPoint = collision.contacts[0].point;
            Vector3 ballPosition = collision.transform.position;
            
            // 펫에서 공으로의 방향
            Vector3 petToBall = (ballPosition - transform.position).normalized;
            
            // 약간의 위쪽 각도 추가 (포물선 궤도)
            Vector3 finalDirection = petToBall + Vector3.up * 0.3f;
            
            return finalDirection.normalized;
        }
        
        public void SetBallTarget(Transform ballTransform)
        {
            if (petController != null)
            {
                petController.SetBallTarget(ballTransform);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // 감지 범위 시각화
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            
            // 히트 방향 시각화
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, hitDirection * 2f);
        }
    }
}