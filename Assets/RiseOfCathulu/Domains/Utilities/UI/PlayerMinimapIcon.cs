using UnityEngine;

public class PlayerMinimapIconScaler2D : MonoBehaviour
{
    public Vector2 targetWorldScale = new Vector2(100f,100f);

    void Start()
    {
        if (transform.parent == null)
            return;

        Vector3 parentScale = transform.parent.lossyScale;

        if (parentScale.x == 0 || parentScale.y == 0)
            return;

        transform.localScale = new Vector3(
            targetWorldScale.x / parentScale.x,
            targetWorldScale.y / parentScale.y,
            1f
        );
    }


}
