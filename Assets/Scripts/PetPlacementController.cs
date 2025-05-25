using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PetPlacementController : MonoBehaviour
{
    public ARPlaneManager planeManager;
    public ARRaycastManager raycastManager;
    public GameObject petPrefab;
    public Camera arCamera;
    public GameObject placementIndicatorPrefab;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool petPlaced = false;
    private GameObject spawnedPet;
    private GameObject placementIndicator;

    void Awake()
    {
        if (planeManager == null)
        {
            planeManager = FindObjectOfType<ARPlaneManager>();
        }
        if (raycastManager == null)
        {
            raycastManager = FindObjectOfType<ARRaycastManager>();
        }

        if (arCamera == null)
        {
            arCamera = Camera.main;
        }
        
        if (petPrefab == null)
        {
            Debug.LogError("Pet Prefab is not assigned.");
            enabled = false;
            return;
        }

        if (placementIndicatorPrefab != null)
        {
            placementIndicator = Instantiate(placementIndicatorPrefab);
            placementIndicator.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
        }
    }

    void OnDisable()
    {
        if (planeManager != null)
        {
            planeManager.planesChanged -= OnPlanesChanged;
        }
    }

    void Start()
    {
        if (petPrefab.scene.IsValid())
        {
            petPrefab.SetActive(false);
        }
    }

    void Update()
    {
        if (petPlaced) 
        {
            if(placementIndicator != null && placementIndicator.activeSelf)
            {
                placementIndicator.SetActive(false);
            }
            return;
        }

        UpdatePlacementIndicator();

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (placementIndicator != null && placementIndicator.activeSelf)
            {
                PlacePetAtIndicator();
            }
        }
    }

    void UpdatePlacementIndicator()
    {
        if (petPlaced || placementIndicatorPrefab == null) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            if (placementIndicator != null)
            {
                placementIndicator.transform.position = hitPose.position;
                placementIndicator.transform.rotation = hitPose.rotation;
                placementIndicator.SetActive(true);
            }
        }
        else
        {
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(false);
            }
        }
    }

    void PlacePetAtIndicator()
    {
        if (placementIndicator == null || !placementIndicator.activeSelf) return;

        Pose placementPose = new Pose(placementIndicator.transform.position, placementIndicator.transform.rotation);
        PlacePet(placementPose);
    }

    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        foreach (ARPlane plane in args.added)
        {
            plane.gameObject.SetActive(true);
            EnablePlaneVisuals(plane);
        }
        foreach (ARPlane plane in args.updated)
        {
            EnablePlaneVisuals(plane);
        }
    }

    void EnablePlaneVisuals(ARPlane plane)
    {
        MeshRenderer meshRenderer = plane.GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }

        LineRenderer lineRenderer = plane.GetComponentInChildren<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }
    }

    void PlacePet(Pose pose)
    {
        if (petPrefab == null) return;

        if (petPrefab.scene.IsValid())
        {
            spawnedPet = petPrefab;
        }
        else
        {
            spawnedPet = Instantiate(petPrefab);
        }
        
        if (spawnedPet != null)
        {
            spawnedPet.transform.position = pose.position;
            Vector3 cameraPosition = arCamera.transform.position;
            Vector3 directionToCamera = (cameraPosition - spawnedPet.transform.position).normalized;
            directionToCamera.y = 0; 
            if (directionToCamera != Vector3.zero)
            {
                spawnedPet.transform.rotation = Quaternion.LookRotation(directionToCamera);
            }
            else
            {
                spawnedPet.transform.rotation = pose.rotation;
            }

            spawnedPet.SetActive(true);
            petPlaced = true;
            Debug.Log($"Pet placed at {pose.position}");

            if (placementIndicator != null)
            {
                placementIndicator.SetActive(false);
            }
        }
    }
} 