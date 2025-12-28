using UnityEngine;

namespace RiseOfCathulu.Domains.Weapons.Scripts
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