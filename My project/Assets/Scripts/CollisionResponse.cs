using UnityEngine;

namespace ARPetGame
{
    public class CollisionResponse : MonoBehaviour
    {
        [Header("Response Settings")]
        public float petHitMultiplier = 1.2f;
        public float wallBounceMultiplier = 0.8f;
        public float groundBounceMultiplier = 0.6f;
        
        [Header("Effects")]
        public GameObject hitEffectPrefab;
        public AudioClip[] collisionSounds;
        
        private AudioSource audioSource;
        private GameManager gameManager;
        
        public System.Action<string, Vector3> OnCollisionResponse;
        
        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            
            gameManager = FindFirstObjectByType<GameManager>();
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            Vector3 point = collision.contacts[0].point;
            Vector3 normal = collision.contacts[0].normal;
            float force = collision.relativeVelocity.magnitude;
            
            if (collision.gameObject.CompareTag("Pet"))
            {
                HandlePetCollision(point, normal, force);
            }
            else if (collision.gameObject.CompareTag("Ground"))
            {
                HandleGroundCollision(point, normal, force);
            }
            else
            {
                HandleWallCollision(point, normal, force);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Boundary"))
            {
                OnCollisionResponse?.Invoke("Boundary", other.transform.position);
                Debug.Log("Ball out of bounds");
            }
        }
        
        private void HandlePetCollision(Vector3 point, Vector3 normal, float force)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 bounce = Vector3.Reflect(rb.linearVelocity, normal) * petHitMultiplier;
                rb.linearVelocity = bounce;
            }
            
            CreateEffect(point);
            PlaySound(0);
            OnCollisionResponse?.Invoke("Pet", point);
            
            Debug.Log("Pet hit ball");
        }
        
        private void HandleGroundCollision(Vector3 point, Vector3 normal, float force)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 bounce = Vector3.Reflect(rb.linearVelocity, normal) * groundBounceMultiplier;
                rb.linearVelocity = bounce;
            }
            
            CreateEffect(point);
            PlaySound(1);
            OnCollisionResponse?.Invoke("Ground", point);
            
            Debug.Log("Ground bounce");
        }
        
        private void HandleWallCollision(Vector3 point, Vector3 normal, float force)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 bounce = Vector3.Reflect(rb.linearVelocity, normal) * wallBounceMultiplier;
                rb.linearVelocity = bounce;
            }
            
            CreateEffect(point);
            PlaySound(2);
            OnCollisionResponse?.Invoke("Wall", point);
            
            Debug.Log("Wall bounce");
        }
        
        private void CreateEffect(Vector3 position)
        {
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
        
        private void PlaySound(int index)
        {
            if (audioSource != null && collisionSounds != null && 
                index < collisionSounds.Length && collisionSounds[index] != null)
            {
                audioSource.PlayOneShot(collisionSounds[index]);
            }
        }
    }
}