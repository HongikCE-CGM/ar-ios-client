using UnityEngine;

namespace ARPetGame
{
    public class TouchInputHandler : MonoBehaviour
    {
        [Header("Touch Settings")]
        public float maxTouchForce = 15f;
        public float minTouchForce = 5f;
        public LayerMask ballLayerMask = -1;
        
        [Header("Swipe Detection")]
        public float swipeThreshold = 50f;
        public float maxSwipeTime = 1f;
        public float forceMultiplier = 1.5f;
        
        [Header("Visual Feedback")]
        public bool showTouchIndicator = true;
        public GameObject touchEffectPrefab;
        
        private Camera arCamera;
        private GameManager gameManager;
        private BallController ballController;
        private TrajectoryCalculator trajectoryCalculator;
        
        private Vector2 touchStartPos;
        private float touchStartTime;
        private bool isTouching = false;
        
        public System.Action<Vector2> OnScreenTouched;
        public System.Action<Vector3, Vector3> OnBallTouched;
        public System.Action<Vector2, float> OnSwipeDetected;
        
        private void Start()
        {
            arCamera = Camera.main;
            gameManager = FindFirstObjectByType<GameManager>();
            ballController = FindFirstObjectByType<BallController>();
            trajectoryCalculator = FindFirstObjectByType<TrajectoryCalculator>();
        }
        
        private void Update()
        {
            HandleTouchInput();
        }
        
        private void HandleTouchInput()
        {
            if (gameManager != null && !gameManager.IsGameActive())
                return;
            
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                ProcessTouch(touch.position, touch.phase);
            }
            else if (Input.GetMouseButtonDown(0))
            {
                ProcessTouch(Input.mousePosition, TouchPhase.Began);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                ProcessTouch(Input.mousePosition, TouchPhase.Ended);
            }
        }
        
        private void ProcessTouch(Vector2 screenPosition, TouchPhase phase)
        {
            if (phase == TouchPhase.Began)
            {
                OnTouchBegan(screenPosition);
            }
            else if (phase == TouchPhase.Ended)
            {
                OnTouchEnded(screenPosition);
            }
        }
        
        private void OnTouchBegan(Vector2 screenPosition)
        {
            touchStartPos = screenPosition;
            touchStartTime = Time.time;
            isTouching = true;
            
            OnScreenTouched?.Invoke(screenPosition);
            
            if (showTouchIndicator)
            {
                ShowTouchEffect(screenPosition);
            }
            
            // 궤적 미리보기 표시
            if (trajectoryCalculator != null)
            {
                Vector3[] trajectory = trajectoryCalculator.CalculateTrajectoryFromTouch(screenPosition);
                trajectoryCalculator.ShowTrajectory(trajectory);
            }
        }
        
        private void OnTouchEnded(Vector2 screenPosition)
        {
            if (!isTouching) return;
            
            float touchDuration = Time.time - touchStartTime;
            Vector2 swipeVector = screenPosition - touchStartPos;
            float swipeDistance = swipeVector.magnitude;
            
            // 궤적 숨기기
            if (trajectoryCalculator != null)
            {
                trajectoryCalculator.HideTrajectory();
            }
            
            if (touchDuration <= maxSwipeTime && swipeDistance >= swipeThreshold)
            {
                ProcessSwipeInput(screenPosition, swipeVector);
                OnSwipeDetected?.Invoke(swipeVector.normalized, swipeDistance);
            }
            else
            {
                ProcessTapInput(screenPosition);
            }
            
            isTouching = false;
        }
        
        private void ProcessTapInput(Vector2 screenPosition)
        {
            Ray ray = arCamera.ScreenPointToRay(screenPosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ballLayerMask))
            {
                if (hit.collider.CompareTag("Ball"))
                {
                    Vector3 hitDirection = CalculateHitDirection(hit.point);
                    float hitForce = Random.Range(minTouchForce, maxTouchForce);
                    
                    HitBall(hitDirection, hitForce);
                    OnBallTouched?.Invoke(hit.point, hitDirection);
                }
            }
        }
        
        private void ProcessSwipeInput(Vector2 screenPosition, Vector2 swipeVector)
        {
            Ray ray = arCamera.ScreenPointToRay(screenPosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ballLayerMask))
            {
                if (hit.collider.CompareTag("Ball"))
                {
                    Vector3 hitDirection = CalculateSwipeDirection(hit.point, swipeVector);
                    float hitForce = CalculateSwipeForce(swipeVector.magnitude);
                    
                    HitBall(hitDirection, hitForce);
                    OnBallTouched?.Invoke(hit.point, hitDirection);
                }
            }
        }
        
        private Vector3 CalculateHitDirection(Vector3 hitPoint)
        {
            GameObject petObject = GameObject.FindWithTag("Pet");
            if (petObject == null) return Vector3.forward;
            
            Vector3 directionToPet = (petObject.transform.position - hitPoint).normalized;
            directionToPet.y = Mathf.Clamp(directionToPet.y, 0.1f, 0.5f);
            
            return directionToPet.normalized;
        }
        
        private Vector3 CalculateSwipeDirection(Vector3 hitPoint, Vector2 swipeVector)
        {
            Vector3 cameraRight = arCamera.transform.right;
            Vector3 cameraUp = arCamera.transform.up;
            
            Vector3 swipeDirection3D = (cameraRight * swipeVector.x + cameraUp * swipeVector.y).normalized;
            
            GameObject petObject = GameObject.FindWithTag("Pet");
            if (petObject != null)
            {
                Vector3 toPetDirection = (petObject.transform.position - hitPoint).normalized;
                swipeDirection3D = Vector3.Lerp(swipeDirection3D, toPetDirection, 0.5f);
            }
            
            swipeDirection3D.y = Mathf.Clamp(swipeDirection3D.y, 0.1f, 0.5f);
            return swipeDirection3D.normalized;
        }
        
        private float CalculateSwipeForce(float swipeIntensity)
        {
            float baseForce = Random.Range(minTouchForce, maxTouchForce);
            float normalizedIntensity = Mathf.Clamp01(swipeIntensity / 200f);
            float finalForce = baseForce * (1f + normalizedIntensity * forceMultiplier);
            
            return Mathf.Clamp(finalForce, minTouchForce, maxTouchForce);
        }
        
        private void HitBall(Vector3 hitDirection, float hitForce)
        {
            if (ballController == null) return;
            
            ballController.HitBall(hitDirection);
            ballController.SetSpeed(hitForce);
            
            Debug.Log($"Ball hit! Force: {hitForce}, Direction: {hitDirection}");
        }
        
        private void ShowTouchEffect(Vector2 screenPosition)
        {
            if (touchEffectPrefab != null)
            {
                Vector3 worldPosition = arCamera.ScreenToWorldPoint(
                    new Vector3(screenPosition.x, screenPosition.y, 2f));
                
                GameObject effect = Instantiate(touchEffectPrefab, worldPosition, Quaternion.identity);
                Destroy(effect, 1f);
            }
        }
        
        public void SetTouchEnabled(bool enabled)
        {
            this.enabled = enabled;
        }
        
        public void SetForceRange(float min, float max)
        {
            minTouchForce = min;
            maxTouchForce = max;
        }
    }
}