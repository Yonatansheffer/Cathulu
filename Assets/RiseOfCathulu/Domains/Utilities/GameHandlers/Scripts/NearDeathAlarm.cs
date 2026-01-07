using UnityEngine;

public class AlarmLight : MonoBehaviour
{
    [SerializeField] private SpriteRenderer lightSprite;
    [SerializeField] private float sizeThreshold = 5f;
    [SerializeField] private float flashSpeed = 3f;

    private void Update()
    {
        if (transform.localScale.x < sizeThreshold)
        {
            float t = Mathf.PingPong(Time.time * flashSpeed, 1f);
            lightSprite.color = Color.Lerp(Color.red, Color.white, t);
        }
        else
        {
            // Optional: reset color when not alarming
            lightSprite.color = Color.white;
        }
    }
}