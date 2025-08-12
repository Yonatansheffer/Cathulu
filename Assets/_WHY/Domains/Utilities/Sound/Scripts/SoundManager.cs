using System.Collections.Generic;
using System.Linq;
using _WHY.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;
using Utilities;
using AudioSettings = _WHY.Domains.Utilities.Sound.Scripts.AudioSettings;

namespace _WHY.Domains.Utilities.Sound.Scripts
{
    public class SoundManager : MonoSingleton<SoundManager>
    {
        private readonly List<AudioSourceWrapper> _activeSounds = new();
        [SerializeField] private AudioSettings settings;
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
        
        private void OnEnable()
        {
            GameEvents.StopMusic += ReturnAllSoundWrappersToPool;
            GameEvents.FreezeLevel += ReturnAllSoundWrappersToPool;
        }
    
        private void OnDisable()
        {   
            GameEvents.StopMusic -= ReturnAllSoundWrappersToPool;
            GameEvents.FreezeLevel -= ReturnAllSoundWrappersToPool;
        }

        public void PlaySound(string audioName, Transform spawnTransform)
        {

            var config = FindAudioConfig(audioName);
            if (config == null)  return;
            var soundObject = SoundPool.Instance.Get();
            _activeSounds.Add(soundObject);
            soundObject.transform.position = spawnTransform.position;
            soundObject.Play(config.clip, config.volume,config.loop);
        }

        private AudioConfig FindAudioConfig(string audioName)
        {
            return settings.audioConfigs.FirstOrDefault(config => config.name == audioName);
        }

        private void ReturnAllSoundWrappersToPool()
        {
            foreach (var sound in _activeSounds)
            {
                if (sound != null) sound.Reset();
                SoundPool.Instance.Return(sound);
            }
            _activeSounds.Clear();
        }
    }
}