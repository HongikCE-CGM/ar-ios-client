using UnityEngine;

namespace ARPetGame
{
    public class TrajectoryCalculator : MonoBehaviour
    {
        [Header("Settings")]
        public int trajectoryPoints = 30;
        public float timeInterval = 0.1f;
        public LayerMask collisionLayers = -1;
        
        [Header("Visual")]
        public LineRenderer trajectoryLine;
        public bool showTrajectory = true;
        
        private Camera arCamera;
        
        public System.Action<Vector3[]> OnTrajectoryCalculated;
        
        private void Start()
        {
            arCamera = Camera.main;
            SetupLineRenderer();
        }
        
        private void SetupLineRenderer()
        {
            if (trajectoryLine == null)
                trajectoryLine = gameObject.AddComponent<LineRenderer>();
            
            trajectoryLine.startWidth = 0.02f;
            trajectoryLine.endWidth = 0.01f;
            trajectoryLine.positionCount = trajectoryPoints;
            trajectoryLine.enabled = showTrajectory;
        }
        
        public Vector3[] CalculateTrajectory(Vector3 startPos, Vector3 velocity)
        {
            Vector3[] points = new Vector3[trajectoryPoints];
            Vector3 currentPos = startPos;
            Vector3 currentVel = velocity;
            
            for (int i = 0; i < trajectoryPoints; i++)
            {
                points[i] = currentPos;
                
                currentVel += Physics.gravity * timeInterval;
                Vector3 nextPos = currentPos + currentVel * timeInterval;
                
                if (Physics.Raycast(currentPos, (nextPos - currentPos).normalized, 
                    out RaycastHit hit, Vector3.Distance(currentPos, nextPos), collisionLayers))
                {
                    points[i] = hit.point;
                    System.Array.Resize(ref points, i + 1);
                    break;
                }
                
                currentPos = nextPos;
            }
            
            OnTrajectoryCalculated?.Invoke(points);
            return points;
        }
        
        public Vector3[] CalculateTrajectoryFromTouch(Vector2 screenPos)
        {
            BallController ballController = FindFirstObjectByType<BallController>();
            
            // 공의 현재 위치에서 시작
            Vector3 startPos = ballController != null ? 
                ballController.transform.position : arCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 2f));
            
            // 터치 방향으로 초기 속도 계산
            Ray ray = arCamera.ScreenPointToRay(screenPos);
            Vector3 direction = ray.direction;
            direction.y = Mathf.Clamp(direction.y, 0.1f, 0.5f);
            
            float speed = ballController != null ? ballController.baseSpeed : 5f;
            Vector3 velocity = direction.normalized * speed;
            
            return CalculateTrajectory(startPos, velocity);
        }
        
        public void ShowTrajectory(Vector3[] points)
        {
            if (trajectoryLine == null || !showTrajectory) return;
            
            trajectoryLine.positionCount = points.Length;
            trajectoryLine.SetPositions(points);
            trajectoryLine.enabled = true;
        }
        
        public void HideTrajectory()
        {
            if (trajectoryLine != null)
                trajectoryLine.enabled = false;
        }
        
        public Vector3 PredictLandingPoint(Vector3 startPos, Vector3 velocity)
        {
            Vector3 currentPos = startPos;
            Vector3 currentVel = velocity;
            
            for (int i = 0; i < 50; i++)
            {
                currentVel += Physics.gravity * timeInterval;
                Vector3 nextPos = currentPos + currentVel * timeInterval;
                
                if (nextPos.y <= 0)
                    return nextPos;
                
                currentPos = nextPos;
            }
            
            return currentPos;
        }
    }
}