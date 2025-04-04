using System;
using _WHY.Scripts.Enemies;
using UnityEngine;
using Weapons;

namespace GameHandlers
{
    public class GameEvents 
    {
        // General Events
        public static Action BeginGamePlay;
        public static Action StartGame;
        public static Action FreezeStage;
        public static Action StartStage;
        public static Action<int,int> PassedStage; // present score and time bonus
        public static Action TimeOver;
        public static Action GameOver;

        // UI Events
        public static Action ReadyStage;
        public static Action<int> UpdateLifeUI;
        public static Action<int> UpdatePointsUI;
        public static Action<int> UpdateTimeUI;
        public static Action ResetWeaponUI;
        public static Action HideGameUI;
    
    
        // GamePlay Events
        public static Action<Transform> Shoot;
        public static Action<Enemy,int> EnemyHit;
        public static Action<Enemy> EnemyDestroy;
        public static Action<WeaponType> WeaponCollected;
        public static Action EnemyStopperCollected;
        public static Action ShieldCollected;
        public static Action ShieldHit;
        public static Action PlayerHit;
        public static Action PlayerLostLife;
        public static Action BossDestroyed;
        public static Action<int> BossDamaged;
        public static Action<int> AddPoints;
    
        // Cheat Codes Events
        public static Action<int> AddTime;
        public static Action AddLife;
        public static Action StopMusicCheat;
        public static Action DefaultWeapon;
        public static Action KillAllEnemies;
    }
}
