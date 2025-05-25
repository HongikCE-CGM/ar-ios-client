using UnityEngine;

public class PetFeederController : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float stopDistance = 0.2f;

    private Animator animator;
    private Vector3? targetPos = null;
    private GameObject currentFood = null;

    public bool IsMovingToFood => targetPos.HasValue;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!targetPos.HasValue) return;

        Vector3 direction = targetPos.Value - transform.position;
        direction.y = 0f;

        if (direction.magnitude > stopDistance)
        {
            transform.position += direction.normalized * moveSpeed * Time.deltaTime;

            if (direction != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
            }

            animator?.SetBool("isRunning", true);
        }
        else
        {
            animator?.SetBool("isRunning", false);
            animator?.SetTrigger("eat");

            if (currentFood != null)
            {
                Destroy(currentFood);
                currentFood = null;
            }

            targetPos = null;
        }
    }

    public void MoveToTarget(Vector3 foodPos, GameObject food)
    {
        this.targetPos = foodPos;
        this.currentFood = food;
    }
}


