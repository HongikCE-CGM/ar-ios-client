using UnityEngine;

namespace ARPetGame
{
    public class BallController : MonoBehaviour
    {
        [Header("Physics Settings")]
        public float baseSpeed = 5f;
        public float speedIncrement = 0.5f;
        public float maxSpeed = 15f;
        public float bounceForce = 8f;
        
        [Header("Trail Settings")]
        public TrailRenderer trailRenderer;
        public float trailTime = 0.5f;
        
        [Header("Audio")]
        public AudioClip bounceSound;
        public AudioClip hitSound;
        
        private Rigidbody rb;
        private AudioSource audioSource;
        private float currentSpeed;
        private int bounceCount = 0;
        
        // Events
        public System.Action<Vector3> OnBallHit;
        public System.Action OnBallMissed;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            audioSource = GetComponent<AudioSource>();
            
            if (trailRenderer == null)
                trailRenderer = GetComponent<TrailRenderer>();
        }
        
        private void Start()
        {
            currentSpeed = baseSpeed;
            SetupTrail();
            SetupPhysics();
        }
        
        private void SetupTrail()
        {
            if (trailRenderer != null)
            {
                trailRenderer.time = trailTime;
                trailRenderer.startWidth = 0.05f;
                trailRenderer.endWidth = 0.01f;
                trailRenderer.material = CreateTrailMaterial();
            }
        }
        
        private void SetupPhysics()
        {
            if (rb != null)
            {
                rb.mass = 0.1f;
                rb.linearDamping = 0.1f;
                rb.angularDamping = 0.5f;
                rb.useGravity = true;
                rb.freezeRotation = false;
            }
        }
        
        private Material CreateTrailMaterial()
        {
            Material trailMat = new Material(Shader.Find("Sprites/Default"));
            trailMat.color = Color.cyan;
            return trailMat;
        }
        
        public void LaunchBall(Vector3 direction, float force = -1f)
        {
            if (rb == null) return;
            
            float launchForce = force > 0 ? force : currentSpeed;
            
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            rb.AddForce(direction.normalized * launchForce, ForceMode.Impulse);
            
            PlaySound(hitSound);
            
            Debug.Log($"Ball launched with force: {launchForce}");
        }
        
        public void HitBall(Vector3 hitDirection)
        {
            if (rb == null) return;
            
            Vector3 reflectDirection = Vector3.Reflect(rb.linearVelocity.normalized, hitDirection);
            
            bounceCount++;
            currentSpeed = Mathf.Min(currentSpeed + speedIncrement, maxSpeed);
            
            rb.linearVelocity = reflectDirection * currentSpeed;
            
            OnBallHit?.Invoke(transform.position);
            PlaySound(bounceSound);
            
            Debug.Log($"Ball hit! Bounce count: {bounceCount}, Speed: {currentSpeed}");
        }
        
        public void ResetBall()
        {
            bounceCount = 0;
            currentSpeed = baseSpeed;
            
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            if (trailRenderer != null)
            {
                trailRenderer.Clear();
            }
        }
        
        public void SetSpeed(float speed)
        {
            currentSpeed = Mathf.Clamp(speed, baseSpeed, maxSpeed);
        }
        
        public float GetCurrentSpeed()
        {
            return currentSpeed;
        }
        
        public int GetBounceCount()
        {
            return bounceCount;
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Pet"))
            {
                Vector3 hitDirection = collision.contacts[0].normal;
                HitBall(hitDirection);
            }
            else if (collision.gameObject.CompareTag("Ground"))
            {
                OnBallMissed?.Invoke();
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Boundary"))
            {
                OnBallMissed?.Invoke();
            }
        }
    }
}