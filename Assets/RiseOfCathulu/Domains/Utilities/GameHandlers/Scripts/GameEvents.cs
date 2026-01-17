using System;
using RiseOfCathulu.Domains.Weapons.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts
{
    public static class GameEvents 
    {
        // GameLoop events
        public static Action FreezeCollected;
        public static Action FreezeLevel;
        public static Action UnFreezeLevel;
        public static Action RestartLevel;
        public static Action<float> AddTime;
        public static Action EndScene;
        
        public static Action PlayerDefeated;
        public static Action PlayerWon;

        public static Action PlanetEnemyEndedDeath;
        public static Action<GameState, int> GameOverUI;

        // UI Events
        public static Action<int> UpdateScoreUI;
        public static Action ContinueUI;
        public static Action<int> FreezeUI;
    
        // GamePlay Events
        public static Action PlayerFirstMoved;
        public static Action<Transform> Shoot;
        public static Action<Vector3> EnemyDestroyed;
        public static Action<WeaponType> WeaponCollected;
        public static Action PlanetEnemyShoots;
        public static Action ToSpawnEnemy;
        public static Action EnemySpawned;
        public static Action SpawnAllWalkingEnemies;
        public static Action<bool> ShieldUpdated;
        public static Action<bool> EnemyShieldUpdated;
        public static Action<int> PlayerLostLife;
        public static Action<Transform> PlanetEnemyDestroyed;
        public static Action<int> AddPoints;
        public static Action ShakeCamera;
        public static Action StopMusic;
        public static Action<Vector2, float,float,float> OnEnteredGravityZone;
        public static Action OnExitedGravityZone;
        public static Action DestroyedSun;
        public static Action SnitchCaught;
        public static Action PlayerRequestedSizeDecrease;
        public static Action PlayerRequestedSizeIncrease;
        public static Action<int> PlayerGrow;
        public static Action<Transform> PlanetDestroyed;


        
    
        // Cheat Codes Events
        public static Action<int> ChangePlayerSize;
    }
}
