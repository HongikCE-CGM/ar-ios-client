using UnityEngine;

namespace ARPetGame
{
    public class EnhancedTouchInput : MonoBehaviour
    {
        [Header("Touch Settings")]
        public float maxForce = 15f;
        public float minForce = 5f;
        public float swipeThreshold = 50f;
        
        private Camera arCamera;
        private GameManager gameManager;
        private BallController ballController;
        
        private Vector2 touchStart;
        private float touchTime;
        private bool touching = false;
        
        private void Start()
        {
            arCamera = Camera.main;
            gameManager = FindFirstObjectByType<GameManager>();
            ballController = FindFirstObjectByType<BallController>();
        }
        
        private void Update()
        {
            HandleInput();
        }
        
        private void HandleInput()
        {
            if (gameManager != null && !gameManager.IsGameActive())
                return;
            
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                if (touch.phase == TouchPhase.Began)
                {
                    StartTouch(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    EndTouch(touch.position);
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                StartTouch(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                EndTouch(Input.mousePosition);
            }
        }
        
        private void StartTouch(Vector2 position)
        {
            touchStart = position;
            touchTime = Time.time;
            touching = true;
        }
        
        private void EndTouch(Vector2 position)
        {
            if (!touching) return;
            
            Vector2 swipe = position - touchStart;
            float duration = Time.time - touchTime;
            
            if (swipe.magnitude > swipeThreshold && duration < 1f)
            {
                ProcessSwipe(position, swipe);
            }
            else
            {
                ProcessTap(position);
            }
            
            touching = false;
        }
        
        private void ProcessTap(Vector2 screenPos)
        {
            Ray ray = arCamera.ScreenPointToRay(screenPos);
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Ball"))
                {
                    Vector3 direction = GetDirectionToPet(hit.point);
                    float force = Random.Range(minForce, maxForce);
                    
                    HitBall(direction, force);
                }
            }
        }
        
        private void ProcessSwipe(Vector2 screenPos, Vector2 swipe)
        {
            Ray ray = arCamera.ScreenPointToRay(screenPos);
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Ball"))
                {
                    Vector3 direction = GetSwipeDirection(hit.point, swipe);
                    float force = Mathf.Lerp(minForce, maxForce, swipe.magnitude / 200f);
                    
                    HitBall(direction, force);
                }
            }
        }
        
        private Vector3 GetDirectionToPet(Vector3 hitPoint)
        {
            GameObject pet = GameObject.FindWithTag("Pet");
            if (pet == null) return Vector3.forward;
            
            Vector3 dir = (pet.transform.position - hitPoint).normalized;
            dir.y = 0.3f;
            return dir.normalized;
        }
        
        private Vector3 GetSwipeDirection(Vector3 hitPoint, Vector2 swipe)
        {
            Vector3 right = arCamera.transform.right;
            Vector3 up = arCamera.transform.up;
            
            Vector3 swipeDir = (right * swipe.x + up * swipe.y).normalized;
            
            GameObject pet = GameObject.FindWithTag("Pet");
            if (pet != null)
            {
                Vector3 toPet = (pet.transform.position - hitPoint).normalized;
                swipeDir = Vector3.Lerp(swipeDir, toPet, 0.5f);
            }
            
            swipeDir.y = Mathf.Clamp(swipeDir.y, 0.1f, 0.5f);
            return swipeDir.normalized;
        }
        
        private void HitBall(Vector3 direction, float force)
        {
            if (ballController == null) return;
            
            ballController.HitBall(direction);
            ballController.SetSpeed(force);
            
            Debug.Log($"Ball hit with force: {force}");
        }
    }
}