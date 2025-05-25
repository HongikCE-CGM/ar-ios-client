using UnityEngine;

public class PetSleepSequence : MonoBehaviour
{
    public float dragThreshold = 60f;
    private Vector2 touchStartPos;
    private bool isTouchingPet = false;
    private Animator animator;

    [Header("하트 설정")]
    public string heartPrefabPath = "Heart2"; // ✅ Resources 폴더 내 경로
    public Vector3 heartOffset = new Vector3(0, 0.8f, 0);
    private GameObject heartPrefab;
    private GameObject heartInstance;

    void Start()
    {
        animator = GetComponent<Animator>();

        // ✅ Resources에서 하트 프리팹 로드
        heartPrefab = Resources.Load<GameObject>(heartPrefabPath);

        if (heartPrefab != null)
        {
            heartInstance = Instantiate(heartPrefab, transform.position + heartOffset, Quaternion.identity);
            heartInstance.SetActive(false);
        }
        else
        {
            Debug.LogWarning("⚠️ 하트 프리팹을 Resources에서 불러올 수 없습니다. 경로 확인: " + heartPrefabPath);
        }
    }

    void Update()
    {
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
                    animator.SetTrigger("startSleep");
                    isTouchingPet = false;

                    if (heartInstance != null)
                    {
                        heartInstance.transform.position = transform.position + heartOffset;
                        heartInstance.SetActive(true);
                        CancelInvoke(nameof(HideHeart));
                        Invoke(nameof(HideHeart), 4f);
                    }
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                isTouchingPet = false;
                break;
        }

        if (heartInstance != null && heartInstance.activeSelf)
        {
            heartInstance.transform.position = transform.position + heartOffset;
        }
    }

    private void HideHeart()
    {
        if (heartInstance != null)
        {
            heartInstance.SetActive(false);
        }
    }

    private bool RaycastToSelf(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == this.gameObject;
    }
}
