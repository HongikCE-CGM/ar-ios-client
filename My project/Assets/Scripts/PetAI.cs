using UnityEngine;

namespace ARPetGame
{
    public class PetAI : MonoBehaviour
    {
        [Header("AI Settings")]
        public float detectionRange = 2f;
        public float reactionTime = 0.5f;
        public float trackingSpeed = 3f;
        
        private PetController petController;
        private Transform ballTransform;
        private BallController ballController;
        private Rigidbody ballRigidbody;
        
        private bool isTrackingBall = false;
        private bool canHit = true;
        private Vector3 ballVelocity;
        
        public System.Action<Vector3> OnBallTracked;
        public System.Action OnHitTriggered;
        
        private void Awake()
        {
            petController = GetComponent<PetController>();
        }
        
        private void Start()
        {
            FindBall();
        }
        
        private void Update()
        {
            if (ballTransform != null)
            {
                TrackBall();
                CheckHitOpportunity();
            }
            else
            {
                FindBall();
            }
        }
        
        private void FindBall()
        {
            GameObject ballObject = GameObject.FindWithTag("Ball");
            if (ballObject != null)
            {
                ballTransform = ballObject.transform;
                ballController = ballObject.GetComponent<BallController>();
                ballRigidbody = ballObject.GetComponent<Rigidbody>();
                Debug.Log("Pet AI found ball");
            }
        }
        
        private void TrackBall()
        {
            if (ballTransform == null) return;
            
            float distance = Vector3.Distance(transform.position, ballTransform.position);
            
            if (distance <= detectionRange)
            {
                if (!isTrackingBall)
                {
                    isTrackingBall = true;
                    if (petController != null)
                        petController.SetState(PetState.Idle);
                }
                
                UpdateBallVelocity();
                LookAtBall();
                OnBallTracked?.Invoke(ballTransform.position);
            }
            else if (isTrackingBall)
            {
                isTrackingBall = false;
                if (petController != null)
                    petController.SetState(PetState.Sit);
            }
        }
        
        private void UpdateBallVelocity()
        {
            if (ballRigidbody != null)
            {
                ballVelocity = ballRigidbody.linearVelocity;
            }
        }
        
        private void LookAtBall()
        {
            Vector3 direction = ballTransform.position - transform.position;
            direction.y = 0;
            
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 
                                                     trackingSpeed * Time.deltaTime);
            }
        }
        
        private void CheckHitOpportunity()
        {
            if (!isTrackingBall || !canHit) return;
            
            float distance = Vector3.Distance(transform.position, ballTransform.position);
            Vector3 ballDirection = ballVelocity.normalized;
            Vector3 directionToPet = (transform.position - ballTransform.position).normalized;
            
            float approachDot = Vector3.Dot(ballDirection, directionToPet);
            
            if (distance <= 1.5f && approachDot > 0.5f)
            {
                TriggerHit();
            }
        }
        
        private void TriggerHit()
        {
            OnHitTriggered?.Invoke();
            
            if (petController != null)
            {
                petController.PerformBallAction();
            }
            
            canHit = false;
            Invoke(nameof(ResetHit), 1f);
            Debug.Log("Pet hit ball!");
        }
        
        private void ResetHit()
        {
            canHit = true;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}