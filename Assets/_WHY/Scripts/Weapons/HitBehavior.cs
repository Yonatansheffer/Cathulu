using UnityEngine;

namespace Weapons
{
    public class HitBehavior: Projectile
    {
        private static readonly int HitCeiling = Animator.StringToHash("Hit_Ceiling");

        // This method is called when the projectile hits a solid object and the weapon has special behavior
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