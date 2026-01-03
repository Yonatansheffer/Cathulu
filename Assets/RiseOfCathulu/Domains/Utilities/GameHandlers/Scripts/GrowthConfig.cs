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
        public float minSpeed = 2f;
        public float maxSpeed = 6f;
    
        [Tooltip("Draw the growth curve here. X = Level (0-1), Y = Scale")]
        public AnimationCurve scaleCurve = AnimationCurve.Linear(0, 0.5f, 1, 3.5f);
        public float GetScale(int level) {
            float t = Mathf.InverseLerp(minLevel, maxLevel, level);
            float curvedValue = scaleCurve.Evaluate(t); // Returns 0 to 1
            return Mathf.Lerp(minScale, maxScale, curvedValue);
        }

        public float GetSpeed(int level) {
            float t = Mathf.InverseLerp(minLevel, maxLevel, level);
            return Mathf.Lerp(minSpeed, maxSpeed, t);
        }
    }
}