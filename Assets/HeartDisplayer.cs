using UnityEngine;

public class HeartDisplayer : MonoBehaviour
{
    [Header("프리팹 & 설정")]
    public GameObject heartPrefab;
    public Transform petTransform;
    public Vector3 offset = new Vector3(0, 0.5f, 0);
    private GameObject heartInstance;

    private Animator petAnimator;

    void Start()
    {
        // 하트 미리 생성
        heartInstance = Instantiate(heartPrefab);
        heartInstance.SetActive(false);

        // 애니메이터 가져오기
        if (petTransform != null)
        {
            petAnimator = petTransform.GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (petAnimator != null)
        {
            AnimatorStateInfo stateInfo = petAnimator.GetCurrentAnimatorStateInfo(0); // 0 = Base Layer

            // 현재 상태 이름이 "Lay"일 경우 하트 표시
            if (stateInfo.IsName("Lay")) // 상태 이름 정확히 입력
            {
                ShowHeart(true);
            }
            else
            {
                ShowHeart(false);
            }
        }

        // 하트 위치 갱신
        if (heartInstance.activeSelf && petTransform != null)
        {
            heartInstance.transform.position = petTransform.position + offset;
        }
    }

    void ShowHeart(bool show)
    {
        if (heartInstance != null)
        {
            heartInstance.SetActive(show);
        }
    }
}
