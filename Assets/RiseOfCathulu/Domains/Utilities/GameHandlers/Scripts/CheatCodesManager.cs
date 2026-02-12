using UnityEngine;

namespace RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts
{
    public class CheatCodesManager : MonoBehaviour
    {
        private void Update()
        {
            HandleCheatCodes();
        }

        private static void HandleCheatCodes()
        {

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                print("FreezeLevel");
                GameEvents.FreezeLevel?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                print("UnFreezeLevel");
                GameEvents.UnFreezeLevel?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                print("Activating shield");
                GameEvents.ShieldUpdated?.Invoke(true);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                print("Adding speed for 8 seconds ");
                GameEvents.AddSpeed?.Invoke(1.5f,8f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                print("Adding 1 life");
                GameEvents.ChangePlayerSize?.Invoke(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                GameEvents.PlanetEnemyShoots?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                GameEvents.ToSpawnEnemy?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                GameEvents.PlayerWon?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                GameEvents.PlayerDefeated?.Invoke();
            }
        }
    }
}



