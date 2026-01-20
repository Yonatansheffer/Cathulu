using UnityEngine;

public class PlayerMinimapIconScaler2D : MonoBehaviour
{
    public Vector2 targetWorldScale = new Vector2(2000f, 2000f);
    public float maxScaleMultiplier = 2f;

    private Vector3 _initialLocalScale;

    void Awake()
    {
        _initialLocalScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (transform.parent == null)
            return;

        Vector3 parentScale = transform.parent.lossyScale;

        if (parentScale.x == 0f || parentScale.y == 0f)
            return;

        Vector3 desiredLocalScale = new Vector3(
            targetWorldScale.x / parentScale.x,
            targetWorldScale.y / parentScale.y,
            _initialLocalScale.z
        );

        // Clamp to max 2× starting scale
        desiredLocalScale.x = Mathf.Min(
            desiredLocalScale.x,
            _initialLocalScale.x * maxScaleMultiplier
        );

        desiredLocalScale.y = Mathf.Min(
            desiredLocalScale.y,
            _initialLocalScale.y * maxScaleMultiplier
        );

        transform.localScale = desiredLocalScale;
    }
}