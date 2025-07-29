using UnityEngine;

namespace GameHandlers
{
    public class CheatCodesManager : MonoBehaviour
    {
        private void Update()
        {
            HandleCheatCodes();
        }

        private static void HandleCheatCodes()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                print("Restarting gameplay");
                GameEvents.HideGameUI?.Invoke();
                GameEvents.StopMusicCheat?.Invoke();
                GameEvents.FreezeLevel?.Invoke();
                GameEvents.BeginGameLoop?.Invoke();
            }
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
                print("Adding 10 seconds to time");
                GameEvents.AddTime?.Invoke(10);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                print("SpawnAllWalkingEnemies");
                GameEvents.SpawnAllWalkingEnemies?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                print("Adding 1 life");
                GameEvents.AddLifeToPlayer?.Invoke(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                print("Destroying shots and returning to default weapon");
                GameEvents.BossShoots?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                print("spawning enemy");
                GameEvents.ToSpawnEnemy?.Invoke();
            }
        }
    }
}



