using System;
using B.O.S.S.Domains.Weapons.Scripts;
using UnityEngine;

namespace B.O.S.S.Domains.Utilities.GameHandlers.Scripts
{
    public static class GameEvents 
    {
        // GameLoop events
        public static Action BeginGameLoop;
        public static Action BeginGamePlay;
        public static Action FreezeCollected;
        public static Action FreezeLevel;
        public static Action UnFreezeLevel;
        public static Action RestartLevel;
        public static Action<float> AddTime;
        public static Action EndScene;
        public static Action PlayerDefeated;
        public static Action BossEndedDeath;
        public static Action<GameState, int> GameOverUI;

        // UI Events
        public static Action<int> UpdateScoreUI;
        public static Action<int> UpdatePlayerLivesUI;
        public static Action<int> UpdateTimeUI;
        public static Action<int> FreezeUI;
    
        // GamePlay Events
        public static Action<Transform> Shoot;
        public static Action<Vector3> EnemyDestroyed;
        public static Action<WeaponType> WeaponCollected;
        public static Action BossShoots;
        public static Action ToSpawnEnemy;
        public static Action EnemySpawned;
        public static Action SpawnAllWalkingEnemies;
        public static Action<bool> ShieldUpdated;
        public static Action<int> PlayerLostLife;
        public static Action BossDestroyed;
        public static Action<int> BossLivesChanged;
        public static Action<int> AddPoints;
        public static Action ShakeCamera;
        public static Action StopMusic;
        public static Action<Vector2, float,float,float> OnEnteredGravityZone;
        public static Action OnExitedGravityZone;
    
        // Cheat Codes Events
        public static Action<int> AddLifeToPlayer;
    }
}
