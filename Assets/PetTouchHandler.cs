using UnityEngine;

public class PetTouchHandler : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log($"✅ Hit: {hit.collider.name}");

                if (hit.collider.CompareTag("Pet"))
                {
                    Debug.Log("🐑 Pet was touched!");
                    animator.SetTrigger("isPatted");
                }
            }
            else
            {
                Debug.Log("❌ Raycast did not hit any collider.");
            }
        }
    }


}

