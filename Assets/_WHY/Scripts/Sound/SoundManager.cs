using System;
using System.Collections.Generic;
using System.Linq;
using GameHandlers;
using UnityEngine;
using Utilities;

namespace Sound
{
    public class SoundManager : MonoSingleton<SoundManager>
    {
        private readonly List<AudioSourceWrapper> _activeSounds = new();
        [SerializeField] private AudioSettings settings;
        private static AudioSourceWrapper _openingMusic;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            PlayOpeningMusic();
        }

        private void OnEnable()
        {
            GameEvents.BeginGameLoop += PlayOpeningMusic;
            GameEvents.GameOver += ReturnAllSoundWrappersToPool;
            GameEvents.StopMusicCheat += ReturnAllSoundWrappersToPool;
        }
    
        private void OnDisable()
        {   
            GameEvents.BeginGameLoop -= PlayOpeningMusic;
            GameEvents.GameOver -= ReturnAllSoundWrappersToPool;
            GameEvents.StopMusicCheat -= ReturnAllSoundWrappersToPool;
        }

        public void PlaySound(string audioName, Transform spawnTransform)
        {
            var config = FindAudioConfig(audioName);
            if (config == null)
                return;
            var soundObject = SoundPool.Instance.Get();
            _activeSounds.Add(soundObject);
            soundObject.transform.position = spawnTransform.position;
            soundObject.Play(config.clip, config.volume,config.loop);
        }
        
        private void PlayOpeningMusic()
        {
            var config = FindAudioConfig("Opening");
            if (config == null)
                return;
            _openingMusic = SoundPool.Instance.Get();
            _activeSounds.Add(_openingMusic);
            _openingMusic.Play(config.clip, config.volume,config.loop);
        }
        
        
        public static void StopOpeningMusic()
        {
            if (_openingMusic == null)
                return;
            _openingMusic.Reset();
        }

        private AudioConfig FindAudioConfig(string audioName)
        {
            return settings.audioConfigs.FirstOrDefault(config => config.name == audioName);
        }
        
        public void ReturnAllSoundWrappersToPool()
        {
            foreach (var sound in _activeSounds)
            {
                if (sound != null)
                    sound.Reset();
                SoundPool.Instance.Return(sound);

            }
            _activeSounds.Clear();
            _openingMusic = null;
        }

    }
}