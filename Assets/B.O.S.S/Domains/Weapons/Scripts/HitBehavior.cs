using UnityEngine;

namespace B.O.S.S.Domains.Weapons.Scripts
{
    public class HitBehavior: Projectile
    {
        private static readonly int HitCeiling = Animator.StringToHash("Hit_Ceiling");

        protected override void HandleHit(Collider2D other)
        {
            if (other.gameObject.CompareTag("Planet") ||
                other.gameObject.CompareTag("Step") || other.gameObject.CompareTag("Background"))
            {
                Stop();
                Animator.SetTrigger(HitCeiling);
            }
            else
            {
                EndShot();
            }
        }
    }
}