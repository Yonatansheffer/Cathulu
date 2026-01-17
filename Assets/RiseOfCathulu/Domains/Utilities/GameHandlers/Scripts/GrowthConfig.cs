using UnityEngine;

namespace RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts
{
    [CreateAssetMenu(fileName = "GrowthConfig", menuName = "B.O.S.S/Growth Config")]
    public class GrowthConfig : ScriptableObject
    {
        public int minLevel;
        public int maxLevel = 20;
        public float minScale = 0.5f;
        public float maxScale = 3.5f;

        [Header("Movement – Absolute Max Speed")]
        public float minMaxSpeed = 8f;
        public float maxMaxSpeed = 30f;

        [Header("Movement – Convergence (Handling)")]
        [Tooltip("Higher = snappier, Lower = heavier")]
        public float minConvergence = 6f;
        public float maxConvergence = 2.5f;
        
        
        [Tooltip("Draw the growth curve here. X = Level (0-1), Y = Scale")]
        public AnimationCurve scaleCurve = AnimationCurve.Linear(0, 0.5f, 1, 3.5f);
        public float GetScale(int level) {
            float t = Mathf.InverseLerp(minLevel, maxLevel, level);
            float curvedValue = scaleCurve.Evaluate(t); 
            return Mathf.Lerp(minScale, maxScale, curvedValue);
        }

        public float GetMaxSpeed(int level)
        {
            float t = Mathf.InverseLerp(minLevel, maxLevel, level);
            float curvedValue = scaleCurve.Evaluate(t);
            return Mathf.Lerp(minMaxSpeed, maxMaxSpeed, curvedValue);
        }

        public float GetConvergence(int level)
        {
            float t = Mathf.InverseLerp(minLevel, maxLevel, level);
            float curvedValue = scaleCurve.Evaluate(t);
            return Mathf.Lerp(minConvergence, maxConvergence, curvedValue);
        }

    }
}