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
        public static Action RestartLevel;
        public static Action<int,int> PassedStage; // present score and time bonus
        public static Action TimeOver;
        public static Action<bool> GameOver;


        // UI Events
        public static Action ReadyStage;
        public static Action<int, bool> UpdateHealthUI;
        public static Action<int> UpdatePointsUI;
        public static Action<int> UpdateTimeUI;
        public static Action ResetWeaponUI;
        public static Action HideGameUI;
    
    
        // GamePlay Events
        public static Action<Transform> Shoot;
        public static Action<Enemy,int> EnemyHit;
        public static Action<Enemy> EnemyDestroy;
        public static Action<WeaponType> WeaponCollected;
        public static Action BossShoots;
        public static Action<bool> ToSpawnEnemy;

        public static Action EnemyStopperCollected;
        public static Action ShieldCollected;
        public static Action ShieldHit;
        public static Action<int> PlayerHit;
        public static Action<int> PlayerLivesChanged;
        public static Action BossDestroyed;
        public static Action<int> BossLivesChanged;
        public static Action<int> AddPoints;
    
        // Cheat Codes Events
        public static Action<int> AddTime;
        public static Action<int> AddLifeToPlayer;
        public static Action StopMusicCheat;
        public static Action DefaultWeapon;
        public static Action KillAllEnemies;
    }
}
