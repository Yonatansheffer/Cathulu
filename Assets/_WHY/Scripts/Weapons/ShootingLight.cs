using System;
using UnityEngine;

namespace Weapons
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