using UnityEngine;

public class PetTouchToggleSit : MonoBehaviour
{
    private Animator animator;
    private bool isSitting = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.gameObject == this.gameObject)
                {
                    ToggleSit();
                }
            }
        }
    }

    void ToggleSit()
    {
        isSitting = !isSitting;
        animator.SetBool("isSitting", isSitting);
    }
}

