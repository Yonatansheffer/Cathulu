using System.Linq;
using GameHandlers;
using UnityEngine;
using Utilities;

namespace Sound
{
    public class SoundManager : MonoSingleton<SoundManager>
    {
        [SerializeField] private AudioSettings settings;
        private AudioSourceWrapper _backgroundMusic;
        private AudioSourceWrapper _continueMusic;


        private void OnEnable()
        {
            GameEvents.StopMusicCheat += StopBackgroundMusic;
            GameEvents.StartGame += StopContinueMusic;
            GameEvents.StartStage += PlayBackgroundMusic;
            GameEvents.BossDestroyed += StopBackgroundMusic;
            GameEvents.ReadyStage += StopBackgroundMusic;
            GameEvents.GameOver += StopBackgroundMusic;
            GameEvents.GameOver += PlayContinueMusic;
            GameEvents.BossDestroyed += PlayPassedStageSound;
        }
    
        private void OnDisable()
        {   
            GameEvents.StopMusicCheat -= StopBackgroundMusic;
            GameEvents.StartGame -= StopContinueMusic;
            GameEvents.StartStage -= PlayBackgroundMusic;
            GameEvents.BossDestroyed -= StopBackgroundMusic;
            GameEvents.ReadyStage -= StopBackgroundMusic;
            GameEvents.GameOver -= StopBackgroundMusic;
            GameEvents.GameOver -= PlayContinueMusic;
            GameEvents.BossDestroyed -= PlayPassedStageSound;
        }

        public void PlaySound(string audioName, Transform spawnTransform)
        {
            var config = FindAudioConfig(audioName);
            if (config == null)
                return;
            var soundObject = SoundPool.Instance.Get();
            soundObject.transform.position = spawnTransform.position;
            soundObject.Play(config.clip, config.volume,config.loop);
        }
    
        private void PlayBackgroundMusic()
        {
            var config = FindAudioConfig("Background");
            if (config == null)
                return;
            _backgroundMusic = SoundPool.Instance.Get();
            _backgroundMusic.Play(config.clip, config.volume,config.loop);
        }
    
        
        private void PlayPassedStageSound()
        {
            var config = FindAudioConfig("Passed Stage");
            if (config == null)
                return;
            _backgroundMusic = SoundPool.Instance.Get();
            _backgroundMusic.Play(config.clip, config.volume,config.loop);
        }
    
        private void PlayContinueMusic()
        {
            var config = FindAudioConfig("Continue");
            if (config == null)
                return;
            _continueMusic = SoundPool.Instance.Get();
            _continueMusic.Play(config.clip, config.volume,config.loop);
        }

    
        private void StopBackgroundMusic()
        {
            if (_backgroundMusic == null)
                return;
            _backgroundMusic.Reset();
        }
    
        private void StopContinueMusic()
        {
            if (_continueMusic == null)
                return;
            _continueMusic.Reset();
        }

        private AudioConfig FindAudioConfig(string audioName)
        {
            return settings.audioConfigs.FirstOrDefault(config => config.name == audioName);
        }
    }
}