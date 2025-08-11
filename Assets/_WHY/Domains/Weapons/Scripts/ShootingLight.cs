using UnityEngine;

namespace _WHY.Domains.Weapons.Scripts
{
    public class ShootingLight : MonoBehaviour
    {
        private void Awake()
        {
            DeactivateLight();
        }

        public void DeactivateLight()
        {
            gameObject.SetActive(false);
        }
    }
}