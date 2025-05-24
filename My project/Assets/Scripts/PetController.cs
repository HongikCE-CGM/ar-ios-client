using UnityEngine;

namespace ARPetGame
{
    /// <summary>
    /// AR 펫의 애니메이션과 행동을 제어하는 컨트롤러
    /// </summary>
    public class PetController : MonoBehaviour
    {
        [Header("Animation Settings")]
        private Animator animator;
        
        [Header("Pet States")]
        public PetState currentState = PetState.Sit;
        
        [Header("Animation Parameters")]
        private static readonly int SitHash = Animator.StringToHash("Sit");
        private static readonly int IdleHash = Animator.StringToHash("Idle");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int BounceHash = Animator.StringToHash("Bounce");
        private static readonly int LayHash = Animator.StringToHash("Lay");
        private static readonly int SpinHash = Animator.StringToHash("Spin");
        
        [Header("Ball Interaction")]
        public Transform ballTarget;
        public float hitDistance = 1.5f;
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("PetController: Animator component not found!");
            }
        }
        
        private void Start()
        {
            // 게임 시작 시 Sit 상태로 시작
            SetState(PetState.Sit);
        }
        
        /// <summary>
        /// 펫의 상태를 변경합니다
        /// </summary>
        public void SetState(PetState newState)
        {
            if (currentState == newState) return;
            
            currentState = newState;
            TriggerAnimation(newState);
            
            Debug.Log($"Pet State Changed: {newState}");
        }
        
        /// <summary>
        /// 상태에 따른 애니메이션 트리거
        /// </summary>
        private void TriggerAnimation(PetState state)
        {
            if (animator == null) return;
            
            // 모든 트리거 초기화
            ResetAllTriggers();
            
            switch (state)
            {
                case PetState.Sit:
                    animator.SetTrigger(SitHash);
                    break;
                case PetState.Idle:
                    animator.SetTrigger(IdleHash);
                    break;
                case PetState.Jump:
                    animator.SetTrigger(JumpHash);
                    break;
                case PetState.Attack:
                    animator.SetTrigger(AttackHash);
                    break;
                case PetState.Bounce:
                    animator.SetTrigger(BounceHash);
                    break;
                case PetState.Lay:
                    animator.SetTrigger(LayHash);
                    break;
                case PetState.Spin:
                    animator.SetTrigger(SpinHash);
                    break;
            }
        }
        
        /// <summary>
        /// 모든 애니메이션 트리거 초기화
        /// </summary>
        private void ResetAllTriggers()
        {
            animator.ResetTrigger(SitHash);
            animator.ResetTrigger(IdleHash);
            animator.ResetTrigger(JumpHash);
            animator.ResetTrigger(AttackHash);
            animator.ResetTrigger(BounceHash);
            animator.ResetTrigger(LayHash);
            animator.ResetTrigger(SpinHash);
        }
        
        /// <summary>
        /// 공을 향해 액션 수행
        /// </summary>
        public void PerformBallAction()
        {
            if (ballTarget == null) return;
            
            // 공과의 거리 계산
            float distance = Vector3.Distance(transform.position, ballTarget.position);
            
            if (distance <= hitDistance)
            {
                // 랜덤하게 액션 선택 (Jump, Attack, Bounce)
                PetState[] actions = { PetState.Jump, PetState.Attack, PetState.Bounce };
                PetState randomAction = actions[Random.Range(0, actions.Length)];
                
                SetState(randomAction);
                
                // 애니메이션 완료 후 Idle 상태로 복귀
                Invoke(nameof(ReturnToIdle), 1.5f);
            }
        }
        
        /// <summary>
        /// Idle 상태로 복귀
        /// </summary>
        private void ReturnToIdle()
        {
            SetState(PetState.Idle);
        }
        
        /// <summary>
        /// 게임 시작 시 호출
        /// </summary>
        public void OnGameStart()
        {
            SetState(PetState.Idle);
        }
        
        /// <summary>
        /// 게임 승리 시 호출
        /// </summary>
        public void OnGameWin()
        {
            SetState(PetState.Spin);
        }
        
        /// <summary>
        /// 게임 오버 시 호출
        /// </summary>
        public void OnGameOver()
        {
            SetState(PetState.Lay);
        }
        
        /// <summary>
        /// 공의 타겟 설정
        /// </summary>
        public void SetBallTarget(Transform ball)
        {
            ballTarget = ball;
        }
    }
    
    /// <summary>
    /// 펫의 상태 열거형
    /// </summary>
    public enum PetState
    {
        Sit,        // 초기 대기 상태
        Idle,       // 기본 대기 애니메이션
        Jump,       // 공 치기 동작 1
        Attack,     // 공 치기 동작 2
        Bounce,     // 공 치기 동작 3
        Lay,        // 지면에 누워있기
        Spin        // 승리 애니메이션
    }
}