using GameHandlers;
using UnityEngine;

namespace MainPlayer
{
    public class PlayerAnimations : MonoBehaviour
    {
        private static readonly int IsMovingRight = Animator.StringToHash("IsMovingRight");
        private static readonly int IsMovingLeft = Animator.StringToHash("IsMovingLeft");
        private static readonly int RightIdle = Animator.StringToHash("RightIdle");
        private static readonly int LeftIdle = Animator.StringToHash("LeftIdle");
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int Reset = Animator.StringToHash("Reset");
        private static readonly int Death = Animator.StringToHash("death");
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;
        [SerializeField] private GameObject capsule;
        [SerializeField] private GameObject gun;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();
        }
        
        private void OnEnable()
        {
            /*GameEvents.BossDestroyed += HidePlayer;
            GameEvents.BeginGamePlay += HidePlayer;
            GameEvents.ReadyStage += ShowPlayer;
            GameEvents.FreezeStage += FreezeAnimation;*/
            //GameEvents.PlayerHit += UpdateHitAnimation;
            GameEvents.PlayerDefeated += DeathAnimation;
        }

        private void OnDisable()
        {
            /*GameEvents.BossDestroyed -= HidePlayer;
            GameEvents.BeginGamePlay -= HidePlayer;
            GameEvents.ReadyStage -= ShowPlayer;
            GameEvents.FreezeStage -= FreezeAnimation;*/
            //GameEvents.PlayerHit -= UpdateHitAnimation;
            GameEvents.PlayerDefeated -= DeathAnimation;
        }
        
        private void DeathAnimation()
        {
            capsule.gameObject.SetActive(false);
            gun.gameObject.SetActive(false);
            _animator.SetTrigger(Death);
            _rb.simulated = false;
            //GameEvents.PlayerLostLife?.Invoke();
        }
        
        private void HidePlayer()
        {
            _spriteRenderer.enabled = false;
        }
        
        private void ShowPlayer()
        {
            _spriteRenderer.enabled = true;
            _animator.SetTrigger(Reset);

        }

        private void Update()
        {
            if (!_rb.simulated) 
                return;
            UpdateAnimator();
        }

        private void UpdateAnimator()
        {
            switch (_rb.linearVelocity.x)
            {
                case 0:
                    _animator.SetBool(IsMovingRight, false);
                    _animator.SetBool(IsMovingLeft, false);
                    break;
                case > 0:
                    _animator.SetBool(IsMovingRight, true);
                    _animator.SetBool(IsMovingLeft, false);
                    break;
                case < 0:
                    _animator.SetBool(IsMovingRight, false);
                    _animator.SetBool(IsMovingLeft, true);
                    break;
            }
        }

        // Freeze the animation when the player got hit 
        private void FreezeAnimation()
        {
            switch (_rb.linearVelocity.x)
            {
                case > 0:
                    _animator.SetTrigger(RightIdle);
                    break;
                case < 0:
                    _animator.SetTrigger(LeftIdle);
                    break;
            }
        }
        
        /*private void UpdateHitAnimation()
        {
            _animator.SetTrigger(Hit);
        }
        
        private void PlayLeftHitAnimation()
        { 
            PlayHitAnimation(false);
        }
        private void PlayRightHitAnimation()
        { 
            PlayHitAnimation(true);
        }
        
        private void PlayHitAnimation(bool isRight)
        {
            var directionMultiplier = isRight ? 1f : -1f;
            var hitSequence = DOTween.Sequence();
            for (var i = 0; i < _hitAnimationVectors.Length; i++)
            {
                var adjustedVector = 
                    new Vector3(_hitAnimationVectors[i].x * directionMultiplier, _hitAnimationVectors[i].y, _hitAnimationVectors[i].z);
                hitSequence.Append(transform.DOMove(transform.position + adjustedVector, 0.5f).
                    SetEase(i % 2 == 0 ? Ease.OutQuad : Ease.InQuad));
            }
            hitSequence.OnKill(() => GameEvents.PlayerLostLife?.Invoke());
        }*/
    }
}
