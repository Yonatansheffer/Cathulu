using UnityEngine;

public class WormholePair : MonoBehaviour
{
    public WormholePortal portalA;
    public WormholePortal portalB;

    public float travelTime = 1f;
    public float cooldownTime = 5f;

    private bool onCooldown = false;

    private void Awake()
    {
        portalA.SetPair(this, portalB);
        portalB.SetPair(this, portalA);
    }

    public bool CanTeleport()
    {
        return !onCooldown;
    }

    public void StartCooldown()
    {
        onCooldown = true;
        portalA.SetGreyed(true);
        portalB.SetGreyed(true);

        Invoke(nameof(EndCooldown), cooldownTime);
    }

    private void EndCooldown()
    {
        onCooldown = false;
        portalA.SetGreyed(false);
        portalB.SetGreyed(false);
    }
}