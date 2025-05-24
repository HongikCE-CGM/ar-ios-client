using UnityEngine;

namespace ARPetGame
{
    public class PetPlacer : MonoBehaviour
    {
        [Header("Pet Settings")]
        public GameObject petPrefab;
        public Vector3 petOffset = Vector3.zero;
        public bool lookAtCamera = true;
        
        [Header("Placement Settings")]
        public bool autoPlace = false;
        public float autoPlaceDelay = 2f;
        
        public System.Action<GameObject> OnPetPlaced;
        public System.Action OnPlacementReady;
        
        private ImprovedARPlaneDetector planeDetector;
        private GameObject placedPet;
        private Camera arCamera;
        private bool isPlacementReady = false;
        private float planeDetectedTime = 0f;
        
        public bool IsPetPlaced => placedPet != null;
        public bool IsPlacementReady => isPlacementReady;
        
        private void Awake()
        {
            planeDetector = GetComponent<ImprovedARPlaneDetector>();
            arCamera = Camera.main;
        }
        
        private void Start()
        {
            if (planeDetector != null)
            {
                planeDetector.OnValidPlaneDetected += OnValidPlaneDetected;
                planeDetector.OnValidPlaneLost += OnValidPlaneLost;
            }
            
            GameObject foxObject = GameObject.FindWithTag("Pet");
            if (foxObject != null && petPrefab == null)
            {
                petPrefab = foxObject;
                foxObject.SetActive(false);
            }
        }
        
        private void Update()
        {
            if (autoPlace && isPlacementReady && !IsPetPlaced)
            {
                if (Time.time - planeDetectedTime >= autoPlaceDelay)
                {
                    TryPlacePet();
                }
            }
        }
        
        private void OnValidPlaneDetected(Vector3 position, Quaternion rotation)
        {
            if (!isPlacementReady)
            {
                isPlacementReady = true;
                planeDetectedTime = Time.time;
                OnPlacementReady?.Invoke();
                Debug.Log("Placement ready!");
            }
        }
        
        private void OnValidPlaneLost()
        {
            if (isPlacementReady && !IsPetPlaced)
            {
                isPlacementReady = false;
            }
        }
        
        public bool TryPlacePet()
        {
            if (!isPlacementReady || IsPetPlaced) return false;
            
            if (planeDetector.TryGetPlacementPose(out Vector3 position, out Quaternion rotation))
            {
                return PlacePetAt(position, rotation);
            }
            
            return false;
        }
        
        public bool PlacePetAt(Vector3 position, Quaternion rotation)
        {
            if (petPrefab == null) return false;
            
            Vector3 finalPosition = position + petOffset;
            
            if (petPrefab.scene.name != null)
            {
                placedPet = petPrefab;
                placedPet.SetActive(true);
                placedPet.transform.position = finalPosition;
                placedPet.transform.rotation = rotation;
            }
            else
            {
                placedPet = Instantiate(petPrefab, finalPosition, rotation);
            }
            
            PetController petController = placedPet.GetComponent<PetController>();
            if (petController != null)
            {
                petController.SetState(PetState.Sit);
            }
            
            OnPetPlaced?.Invoke(placedPet);
            Debug.Log("Pet placed successfully!");
            
            return true;
        }
    }
}