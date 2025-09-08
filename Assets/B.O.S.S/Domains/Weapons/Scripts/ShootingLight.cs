using UnityEngine;

namespace B.O.S.S.Domains.Weapons.Scripts
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