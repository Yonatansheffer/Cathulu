using GameHandlers;
using UnityEngine;

namespace Frame
{
    public class RedFrame : MonoBehaviour
    {
        private static readonly int Activate = Animator.StringToHash("Activate");
        private Animator _animator;
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        
        private void OnEnable()
        {
            GameEvents.PlayerHit += OnPlayerHit;
        }
        
        private void OnDisable()
        {
            GameEvents.PlayerHit -= OnPlayerHit;
        }
        
        private void OnPlayerHit()
        {
            _animator.SetTrigger(Activate);
        }

    }
}