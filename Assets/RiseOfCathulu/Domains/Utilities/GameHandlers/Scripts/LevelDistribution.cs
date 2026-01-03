using UnityEngine;

namespace RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts
{
    public static class LevelDistribution
    {
        public static int GetNormalDistributedLevel(
            int meanLevel,
            float standardDeviation,
            int minLevel,
            int maxLevel
        )
        {
            float u1 = Random.value;
            float u2 = Random.value;

            float randStdNormal =
                Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                Mathf.Sin(2.0f * Mathf.PI * u2);

            float randNormal = meanLevel + standardDeviation * randStdNormal;

            return Mathf.Clamp(
                Mathf.RoundToInt(randNormal),
                minLevel,
                maxLevel
            );
        }
    }
}