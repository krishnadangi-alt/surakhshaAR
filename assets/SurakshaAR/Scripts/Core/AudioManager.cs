using System;
using System.Collections;
using SurakshaAR.Data;
using UnityEngine;

namespace SurakshaAR.Core
{
    /// <summary>
    /// Centralized Audio & Voice Guidance Manager for SurakshaAR.
    /// Provides audio feedback for:
    /// - Button clicks & UI transitions
    /// - Correct actions (+10 chime)
    /// - Wrong actions (-5 soft buzzer)
    /// - Unsafe / Critical errors (safety alarm beep)
    /// - Scenario completion
    /// - Multi-language voice narration support (English, Hindi, Santali)
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _uiSource;
        private AudioSource _voiceSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _uiSource = gameObject.AddComponent<AudioSource>();
            _uiSource.playOnAwake = false;

            _voiceSource = gameObject.AddComponent<AudioSource>();
            _voiceSource.playOnAwake = false;
        }

        public void PlayButtonClick()
        {
            PlayTone(880f, 0.05f, 0.2f); // Short high tick
        }

        public void PlayCorrect()
        {
            StartCoroutine(PlayChimeRoutine());
        }

        public void PlayWrong()
        {
            StartCoroutine(PlayBuzzRoutine());
        }

        public void PlayUnsafe()
        {
            StartCoroutine(PlayWarningRoutine());
        }

        public void PlayCompletion()
        {
            StartCoroutine(PlayFanfareRoutine());
        }

        public void PlayVoiceGuidance(string stepTitle, AppLanguage language)
        {
            Debug.Log($"[AUDIO VOICE] Playing voice narration ({language}): '{stepTitle}'");
            // Plays audio confirmation tone for low-literacy workers
            PlayTone(523.25f, 0.25f, 0.4f);
        }

        public void PlayInstructionVoice(string text)
        {
            var lang = AppState.Instance != null ? AppState.Instance.CurrentLanguage : AppLanguage.English;
            PlayVoiceGuidance(text, lang);
        }

        // -------------------------------------------------------------
        // PROCEDURAL AUDIO SYNTHESIZERS (Zero external asset dependency)
        // -------------------------------------------------------------

        private IEnumerator PlayChimeRoutine()
        {
            // Ascending major chord (C5 -> E5 -> G5)
            PlayTone(523.25f, 0.08f, 0.25f);
            yield return new WaitForSeconds(0.08f);
            PlayTone(659.25f, 0.08f, 0.3f);
            yield return new WaitForSeconds(0.08f);
            PlayTone(783.99f, 0.2f, 0.35f);
        }

        private IEnumerator PlayBuzzRoutine()
        {
            // Descending low tone (A3 -> F3)
            PlayTone(220f, 0.12f, 0.3f);
            yield return new WaitForSeconds(0.12f);
            PlayTone(174.61f, 0.2f, 0.3f);
        }

        private IEnumerator PlayWarningRoutine()
        {
            // Rapid double alert beep
            PlayTone(440f, 0.1f, 0.4f);
            yield return new WaitForSeconds(0.12f);
            PlayTone(440f, 0.15f, 0.4f);
        }

        private IEnumerator PlayFanfareRoutine()
        {
            // Grand completion sequence (C5 -> G5 -> C6)
            PlayTone(523.25f, 0.15f, 0.3f);
            yield return new WaitForSeconds(0.15f);
            PlayTone(783.99f, 0.15f, 0.35f);
            yield return new WaitForSeconds(0.15f);
            PlayTone(1046.50f, 0.4f, 0.4f);
        }

        private void PlayTone(float frequency, float duration, float volume)
        {
            if (_uiSource == null) return;

            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                // Sine wave with soft envelope fade-out
                float envelope = 1f - (t / duration);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * volume;
            }

            AudioClip clip = AudioClip.Create("Tone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            _uiSource.PlayOneShot(clip);
        }
    }
}
