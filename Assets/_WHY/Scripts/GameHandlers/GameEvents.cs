using System;
using _WHY.Scripts.Enemies;
using UnityEngine;
using Weapons;

namespace GameHandlers
{
    public class GameEvents 
    {
        // GameLoop events
        public static Action BeginGameLoop;
        public static Action FreezeLevel;
        public static Action UnFreezeLevel;
        public static Action RestartLevel;
        public static Action<float> AddTime;
        public static Action EndScene;
        public static Action GameOver;
        public static Action<GameState, int> GameOverUI;


        // UI Events
        public static Action ReadyStage;
        public static Action<int> UpdateScoreUI;
        public static Action<int> UpdateTimeUI;
        public static Action ResetWeaponUI;
        public static Action HideGameUI;
    
    
        // GamePlay Events
        public static Action<Transform> Shoot;
        public static Action<Enemy,int> EnemyHit;
        public static Action<Vector3> EnemyDestroyed;
        public static Action<WeaponType> WeaponCollected;
        public static Action BossShoots;
        public static Action ToSpawnEnemy;
        public static Action EnemySpawned;
        public static Action ShootBallBullet;

        public static Action SpawnAllWalkingEnemies;


        public static Action<bool> ShieldUpdated;
        public static Action<int> PlayerLivesChanged;
        public static Action BossDestroyed;
        public static Action<int> BossLivesChanged;
        public static Action<int> AddPoints;
    
        // Cheat Codes Events
        public static Action<int> AddLifeToPlayer;
        public static Action StopMusicCheat;
        public static Action DefaultWeapon;
        public static Action KillAllEnemies;
    }
}
