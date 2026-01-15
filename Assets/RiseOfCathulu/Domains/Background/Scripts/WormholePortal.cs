using UnityEngine;
using System.Collections;

public class WormholePortal : MonoBehaviour
{
    private WormholePair pair;
    private WormholePortal exitPortal;
    private SpriteRenderer spriteRenderer;

    private Color normalColor;
    private Color greyedColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        normalColor = spriteRenderer.color;
    }

    public void SetPair(WormholePair pair, WormholePortal exit)
    {
        this.pair = pair;
        this.exitPortal = exit;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!pair.CanTeleport()) return;

        pair.StartCooldown();
        StartCoroutine(Teleport(other.transform));
    }

    IEnumerator Teleport(Transform player)
    {
        Vector3 start = player.position;
        Vector3 end = exitPortal.transform.position;
        float elapsed = 0f;

        while (elapsed < pair.travelTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pair.travelTime;
            player.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        player.position = end;
    }

    public void SetGreyed(bool greyed)
    {
        spriteRenderer.color = greyed ? greyedColor : normalColor;
    }
}
