using UnityEngine;

namespace ARPetGame
{
    public class PhysicsIntegration : MonoBehaviour
    {
        [Header("Physics Settings")]
        public float gravity = 9.81f;
        public float airResistance = 0.98f;
        public float bounceEnergyLoss = 0.8f;
        
        [Header("Ball Physics")]
        public float ballMass = 0.1f;
        public float ballDrag = 0.1f;
        public float ballAngularDrag = 0.5f;
        
        [Header("Collision Settings")]
        public float minimumCollisionVelocity = 1f;
        public float maximumBounceAngle = 75f;
        public PhysicsMaterial ballPhysicMaterial;
        
        [Header("Wind Effect")]
        public bool enableWind = false;
        public Vector3 windDirection = Vector3.zero;
        public float windStrength = 0.5f;
        
        private BallController ballController;
        private Rigidbody ballRigidbody;
        
        public System.Action<Vector3, float> OnCollisionDetected;
        public System.Action<Vector3> OnBounceCalculated;
        
        private void Start()
        {
            SetupPhysics();
        }
        
        private void SetupPhysics()
        {
            ballController = FindFirstObjectByType<BallController>();
            
            if (ballController != null)
            {
                ballRigidbody = ballController.GetComponent<Rigidbody>();
                ConfigureBallPhysics();
            }
        }
        
        private void ConfigureBallPhysics()
        {
            if (ballRigidbody == null) return;
            
            ballRigidbody.mass = ballMass;
            ballRigidbody.linearDamping = ballDrag;
            ballRigidbody.angularDamping = ballAngularDrag;
            ballRigidbody.useGravity = true;
            
            Collider ballCollider = ballController.GetComponent<Collider>();
            if (ballCollider != null && ballPhysicMaterial != null)
            {
                ballCollider.material = ballPhysicMaterial;
            }
        }
        
        private void FixedUpdate()
        {
            if (enableWind && ballRigidbody != null)
            {
                ApplyWindForce();
            }
            
            ApplyAirResistance();
        }
        
        private void ApplyWindForce()
        {
            if (ballRigidbody.linearVelocity.magnitude > 0.1f)
            {
                Vector3 windForce = windDirection.normalized * windStrength;
                ballRigidbody.AddForce(windForce, ForceMode.Force);
            }
        }
        
        private void ApplyAirResistance()
        {
            if (ballRigidbody != null)
            {
                ballRigidbody.linearVelocity *= airResistance;
            }
        }
        
        public Vector3 CalculateBounceDirection(Vector3 incomingVelocity, Vector3 surfaceNormal)
        {
            Vector3 reflectedVelocity = Vector3.Reflect(incomingVelocity, surfaceNormal);
            
            float bounceAngle = Vector3.Angle(reflectedVelocity, Vector3.up);
            if (bounceAngle > maximumBounceAngle)
            {
                Vector3 adjustedDirection = Vector3.Slerp(reflectedVelocity, Vector3.up, 0.3f);
                reflectedVelocity = adjustedDirection.normalized * reflectedVelocity.magnitude;
            }
            
            reflectedVelocity *= bounceEnergyLoss;
            
            OnBounceCalculated?.Invoke(reflectedVelocity);
            return reflectedVelocity;
        }
        
        public float CalculateCollisionForce(Vector3 velocity, float mass)
        {
            float force = velocity.magnitude * mass;
            
            OnCollisionDetected?.Invoke(velocity, force);
            return force;
        }
        
        public bool IsValidCollision(Vector3 velocity)
        {
            return velocity.magnitude >= minimumCollisionVelocity;
        }
        
        public void ApplyImpulse(Vector3 impulseDirection, float force)
        {
            if (ballRigidbody == null) return;
            
            Vector3 impulse = impulseDirection.normalized * force;
            ballRigidbody.AddForce(impulse, ForceMode.Impulse);
        }
        
        public void SetWindEffect(Vector3 direction, float strength)
        {
            windDirection = direction;
            windStrength = strength;
            enableWind = strength > 0;
        }
        
        public void ResetPhysics()
        {
            if (ballRigidbody != null)
            {
                ballRigidbody.linearVelocity = Vector3.zero;
                ballRigidbody.angularVelocity = Vector3.zero;
            }
        }
        
        public Vector3 PredictBallTrajectory(Vector3 startPos, Vector3 velocity, float timeStep, int steps)
        {
            Vector3 position = startPos;
            Vector3 vel = velocity;
            
            for (int i = 0; i < steps; i++)
            {
                vel += Physics.gravity * timeStep;
                vel *= airResistance;
                position += vel * timeStep;
                
                if (enableWind)
                {
                    vel += windDirection * windStrength * timeStep;
                }
            }
            
            return position;
        }
    }
}