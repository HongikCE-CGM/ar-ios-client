using UnityEngine;

namespace ARPetGame
{
    public class TouchInputHandler : MonoBehaviour
    {
        [Header("Touch Settings")]
        public float maxTouchForce = 10f;
        public float minTouchForce = 3f;
        public LayerMask ballLayerMask = -1;
        
        [Header("Visual Feedback")]
        public bool showTouchIndicator = true;
        public GameObject touchEffectPrefab;
        
        private Camera arCamera;
        private GameManager gameManager;
        private BallController ballController;
        
        public System.Action<Vector2> OnScreenTouched;
        public System.Action<Vector3> OnBallTouched;
        
        private void Start()
        {
            arCamera = Camera.main;
            gameManager = FindObjectOfType<GameManager>();
            ballController = FindObjectOfType<BallController>();
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
                
                if (touch.phase == TouchPhase.Began)
                {
                    ProcessTouch(touch.position);
                }
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                ProcessTouch(Input.mousePosition);
            }
        }
        
        private void ProcessTouch(Vector2 screenPosition)
        {
            OnScreenTouched?.Invoke(screenPosition);
            
            Ray ray = arCamera.ScreenPointToRay(screenPosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ballLayerMask))
            {
                if (hit.collider.CompareTag("Ball"))
                {
                    HitBall(hit.point, ray.direction);
                    OnBallTouched?.Invoke(hit.point);
                }
            }
            
            if (showTouchIndicator)
            {
                ShowTouchEffect(screenPosition);
            }
        }
        
        private void HitBall(Vector3 hitPoint, Vector3 rayDirection)
        {
            if (ballController == null) return;
            
            Vector3 hitDirection = CalculateHitDirection(hitPoint, rayDirection);
            float hitForce = CalculateHitForce();
            
            ballController.HitBall(hitDirection);
            ballController.SetSpeed(hitForce);
            
            Debug.Log($"Ball hit by player! Force: {hitForce}, Direction: {hitDirection}");
        }
        
        private Vector3 CalculateHitDirection(Vector3 hitPoint, Vector3 rayDirection)
        {
            GameObject petObject = GameObject.FindWithTag("Pet");
            if (petObject == null) return -rayDirection;
            
            Vector3 directionToPet = (petObject.transform.position - hitPoint).normalized;
            directionToPet.y = 0.3f;
            
            return directionToPet.normalized;
        }
        
        private float CalculateHitForce()
        {
            return Random.Range(minTouchForce, maxTouchForce);
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
    }
}