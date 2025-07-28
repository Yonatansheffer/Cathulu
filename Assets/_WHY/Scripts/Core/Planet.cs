namespace _WHY.Scripts.Core
{
    using UnityEngine;

    public class Planet : MonoBehaviour
    {
        public float gravity = 9.8f;

        public Vector2 GetGravityDirection(Vector2 fromPosition)
        {
            return (transform.position - (Vector3)fromPosition).normalized;
        }
    }

}