using UnityEngine;

public class PetSleepSequence : MonoBehaviour
{
    public float dragThreshold = 60f;
    private Vector2 touchStartPos;
    private bool isTouchingPet = false;
    private Animator animator;
    public SleepUIButtonCreator sleepUIButtonCreator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (sleepUIButtonCreator != null && !sleepUIButtonCreator.IsSleepOn())
            return;
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                if (RaycastToSelf(touch.position))
                {
                    isTouchingPet = true;
                    touchStartPos = touch.position;
                }
                break;

            case TouchPhase.Moved:
                if (!isTouchingPet) break;

                float dragDistance = (touch.position - touchStartPos).magnitude;

                if (dragDistance > dragThreshold && animator.GetBool("isSitting") && !animator.GetBool("isSleeping"))
                {
                    // ✅ 수면 시퀀스 시작
                    animator.SetTrigger("startSleep");

                    // ✅ 누운 상태 플래그는 이후 LieDown 상태 진입 시 On
                    isTouchingPet = false;
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                isTouchingPet = false;
                break;
        }
    }

    private bool RaycastToSelf(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == this.gameObject;
    }
}

