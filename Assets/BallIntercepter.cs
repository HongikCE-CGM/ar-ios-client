using UnityEngine;

public class BallInterceptor : MonoBehaviour
{
    public Animator animator;
    public float reactDistance = 1.0f; // 공이 가까워졌을 때 반응
    public float headbuttForce = 5f;
    public Transform petTransform;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball")) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        float randomValue = Random.value; // 0.0 ~ 1.0

        if (rb == null) return;

        // ✅ 1. 애니메이션 트리거
        if (animator != null){

                if (randomValue < 0.8f) {
                 animator.SetTrigger("attack");
                }
                else
                {
                    animator.SetTrigger("death");
                }
        }
        // ✅ 2. 방향 계산: 공 → 사용자
        Vector3 toCamera = (Camera.main.transform.position - transform.position).normalized;
        toCamera.y = 0.2f; // 위로 살짝 치게

        rb.linearVelocity = Vector3.zero; // 기존 속도 초기화
        rb.AddForce(toCamera * headbuttForce, ForceMode.Impulse);
    }



}
