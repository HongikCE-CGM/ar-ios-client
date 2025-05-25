using UnityEngine;

namespace ARPetGame
{
    public class BoundaryManager : MonoBehaviour
    {
        [Header("Boundary Settings")]
        public float boundarySize = 5f;
        public float warningDistance = 1f;
        
        [Header("Visual")]
        public bool showVisual = true;
        public Color warningColor = Color.red;
        public Color normalColor = Color.cyan;
        
        private Transform petTransform;
        private BallController ballController;
        private GameObject[] boundaryWalls;
        
        public System.Action<Vector3> OnBallNearBoundary;
        public System.Action<Vector3> OnBallOutOfBounds;
        
        private void Start()
        {
            SetupComponents();
            CreateBoundaries();
        }
        
        private void SetupComponents()
        {
            GameObject pet = GameObject.FindWithTag("Pet");
            if (pet != null) petTransform = pet.transform;
            
            ballController = FindFirstObjectByType<BallController>();
        }
        
        private void Update()
        {
            CheckBallPosition();
        }
        
        private void CreateBoundaries()
        {
            Vector3 center = petTransform != null ? petTransform.position : Vector3.zero;
            
            boundaryWalls = new GameObject[4];
            
            boundaryWalls[0] = CreateWall("Front", center + Vector3.forward * boundarySize);
            boundaryWalls[1] = CreateWall("Back", center + Vector3.back * boundarySize);
            boundaryWalls[2] = CreateWall("Left", center + Vector3.left * boundarySize);
            boundaryWalls[3] = CreateWall("Right", center + Vector3.right * boundarySize);
        }
        
        private GameObject CreateWall(string name, Vector3 position)
        {
            GameObject wall = new GameObject($"Boundary_{name}");
            wall.transform.position = position;
            wall.transform.SetParent(transform);
            wall.tag = "Boundary";
            
            BoxCollider collider = wall.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(0.1f, 3f, boundarySize * 2f);
            
            if (showVisual)
            {
                CreateVisual(wall);
            }
            
            return wall;
        }
        
        private void CreateVisual(GameObject wall)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(wall.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one;
            
            Destroy(visual.GetComponent<Collider>());
            
            Renderer renderer = visual.GetComponent<Renderer>();
            renderer.material.color = normalColor;
        }
        
        private void CheckBallPosition()
        {
            if (ballController == null) return;
            
            Vector3 ballPos = ballController.transform.position;
            Vector3 center = petTransform != null ? petTransform.position : Vector3.zero;
            
            float distance = Vector3.Distance(ballPos, center);
            
            if (distance > boundarySize - warningDistance)
            {
                OnBallNearBoundary?.Invoke(ballPos);
                SetWarning(true);
            }
            else
            {
                SetWarning(false);
            }
            
            if (distance > boundarySize)
            {
                OnBallOutOfBounds?.Invoke(ballPos);
            }
        }
        
        private void SetWarning(bool warning)
        {
            if (!showVisual || boundaryWalls == null) return;
            
            Color color = warning ? warningColor : normalColor;
            
            foreach (GameObject wall in boundaryWalls)
            {
                if (wall != null)
                {
                    Renderer renderer = wall.GetComponentInChildren<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = color;
                    }
                }
            }
        }
        
        public bool IsWithinBounds(Vector3 position)
        {
            Vector3 center = petTransform != null ? petTransform.position : Vector3.zero;
            return Vector3.Distance(position, center) <= boundarySize;
        }
    }
}