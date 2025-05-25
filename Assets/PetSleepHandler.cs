using UnityEngine;

public class PetSleepHandler : MonoBehaviour
{
    public float dragThreshold = 60f;
    public float tapTimeThreshold = 0.3f;

    private Animator animator;
    private Vector2 touchStartPos;
    private float touchStartTime;
    private bool isTouchingPet = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == this.gameObject)
                {
                    isTouchingPet = true;
                    touchStartPos = touch.position;
                    touchStartTime = Time.time;
                }
                break;

            case TouchPhase.Moved:
                if (!isTouchingPet) break;

                float dragDistance = (touch.position - touchStartPos).magnitude;

                if (dragDistance > dragThreshold)
                {
                    // ✅ 현재 Sit 상태일 때만 Sleep 전이 허용
                    if (animator.GetBool("isSitting") && !animator.GetBool("isSleeping"))
                    {
                        animator.SetBool("isSleeping", true);
                        StartCoroutine(WakeUpAfterSeconds(5f)); // 5초 후 깨어남
                    }

                    isTouchingPet = false; // 한 번만 반응
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                isTouchingPet = false;
                break;
        }
    }

    System.Collections.IEnumerator WakeUpAfterSeconds(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetBool("isSleeping", false);
    }
}

