using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurakshaAR.Data;

/// <summary>
/// VoiceGuidanceManager
/// ====================
/// Centralized voice & audio guidance manager for the SurakshaAR Fire AR training module.
/// Supports offline-first pre-generated female voice clips for all Fire AR steps (Steps 1 to 6).
///
/// LANGUAGES:
///   - English: Natural professional female voice (en-IN-NeerjaNeural).
///   - Hindi: Natural professional female voice (hi-IN-SwaraNeural).
///   - Santali: Ol Chiki text guidance; audio requires verified native Santali recordings.
///              Never substitutes Hindi or English audio for Santali.
///
/// AUDIO PATH CONVENTION:
///   Resources/Audio/FireTraining/{Language}/step{1..6}.mp3
/// </summary>
[DisallowMultipleComponent]
public class VoiceGuidanceManager : MonoBehaviour
{
    private static VoiceGuidanceManager _instance;
    public static VoiceGuidanceManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<VoiceGuidanceManager>();
                if (_instance == null)
                {
                    var go = new GameObject("VoiceGuidanceManager");
                    _instance = go.AddComponent<VoiceGuidanceManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    // ── Public State ─────────────────────────────────────────────────────
    public bool IsAvailableForEnglish => true;
    public bool IsAvailableForHindi   => true;
    public bool IsAvailableForSantali => CheckSantaliAvailability();

    /// <summary>True while audio is actively playing.</summary>
    public bool IsSpeaking { get; private set; }

    /// <summary>Current step being spoken (1-6), or 0 if none.</summary>
    public int CurrentSpeakingStep { get; private set; }

    /// <summary>Fires when speaking state changes (true = started, false = stopped).</summary>
    public event Action<bool> OnSpeakingChanged;

    /// <summary>Fires when missing/pending voice asset is requested (e.g. Santali recording pending).</summary>
    public event Action<AppLanguage, int> OnVoiceAssetPending;

    // ── Audio Source & Coroutines ─────────────────────────────────────────
    private AudioSource _audioSource;
    private Coroutine _playMonitorRoutine;
    private Coroutine _visualFeedbackRoutine;
    private const float VisualFeedbackDuration = 1.5f;

    // Cache loaded clips to avoid repeated I/O
    private readonly Dictionary<string, AudioClip> _clipCache = new Dictionary<string, AudioClip>();

    // ── Lifecycle ─────────────────────────────────────────────────────────
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSource();
    }

    private void EnsureAudioSource()
    {
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.spatialBlend = 0f; // 2D clean UI audio
            _audioSource.volume = 1.0f;
            _audioSource.mute = false;
        }
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>Check whether voice guidance is available for the given language and step.</summary>
    public bool IsAvailableFor(AppLanguage language, int stepIndex = 1)
    {
        switch (language)
        {
            case AppLanguage.English:
                return true;
            case AppLanguage.Hindi:
                return true;
            case AppLanguage.Santali:
                return HasAudioClip(AppLanguage.Santali, stepIndex);
            default:
                return false;
        }
    }

    /// <summary>
    /// Toggle playback of the given step in the specified language.
    /// If currently speaking, stops playback. If idle, starts playback.
    /// </summary>
    public void ToggleStepVoice(int stepIndex, AppLanguage language)
    {
        if (IsSpeaking)
        {
            StopSpeaking();
        }
        else
        {
            PlayStepVoice(stepIndex, language);
        }
    }

    /// <summary>
    /// Play the pre-generated offline female voice audio clip for the given step and language.
    /// </summary>
    public void PlayStepVoice(int stepIndex, AppLanguage language)
    {
        EnsureAudioSource();
        StopSpeaking();

        int step = Mathf.Clamp(stepIndex, 1, 6);
        CurrentSpeakingStep = step;
        string langCode = language == AppLanguage.English ? "en" : (language == AppLanguage.Hindi ? "hi" : "sat");

        // Santali check: Never fabricate or substitute incorrect audio
        if (language == AppLanguage.Santali)
        {
            AudioClip santaliClip = LoadAudioClip(AppLanguage.Santali, step);
            if (santaliClip != null)
            {
                Debug.Log($"[AUDIO]\nListen pressed\nstep=Step{step}\nlanguage={langCode}\nclip={santaliClip.name}\naudioSource={_audioSource?.name}\nplaying=true");
                Debug.Log($"[AUDIO]\naudioKey = fire.step{step}.audio\nlanguage = {langCode}\nclip = {santaliClip.name}");
                PlayClip(santaliClip, step);
                return;
            }

            Debug.Log($"[AUDIO]\nListen pressed\nstep=Step{step}\nlanguage={langCode}\nclip=null (pending genuine recording)\naudioSource={_audioSource?.name}\nplaying=false");
            Debug.Log($"[VoiceGuidance] Santali voice audio asset pending for Step {step}. UI displays genuine Ol Chiki.");
            OnVoiceAssetPending?.Invoke(AppLanguage.Santali, step);
            TriggerVisualFeedbackOnly();
            return;
        }

        AudioClip clip = LoadAudioClip(language, step);
        if (clip != null)
        {
            Debug.Log($"[AUDIO]\nListen pressed\nstep=Step{step}\nlanguage={langCode}\nclip={clip.name}\naudioSource={_audioSource?.name}\nplaying=true");
            Debug.Log($"[AUDIO]\naudioKey = fire.step{step}.audio\nlanguage = {langCode}\nclip = {clip.name}");
            PlayClip(clip, step);
        }
        else
        {
            Debug.LogWarning($"[AUDIO]\nListen pressed\nstep=Step{step}\nlanguage={langCode}\nclip=null\naudioSource={_audioSource?.name}\nplaying=false");
            Debug.LogWarning($"[VoiceGuidance] Audio clip missing for {language} Step {step}.");
            TriggerVisualFeedbackOnly();
        }
    }

    /// <summary>
    /// Backward-compatible Speak API.
    /// Maps localized text or speaks the active step.
    /// </summary>
    public void Speak(string localizedText, AppLanguage language)
    {
        if (string.IsNullOrEmpty(localizedText)) return;

        Debug.Log($"[VoiceGuidance] SPEAK REQUEST — lang={language}, available={IsAvailableFor(language)}, text=\"{localizedText}\"");

        int step = CurrentSpeakingStep > 0 ? CurrentSpeakingStep : 1;
        PlayStepVoice(step, language);
    }

    /// <summary>Stop speaking immediately and reset playback state.</summary>
    public void StopSpeaking()
    {
        if (_playMonitorRoutine != null)
        {
            StopCoroutine(_playMonitorRoutine);
            _playMonitorRoutine = null;
        }

        if (_visualFeedbackRoutine != null)
        {
            StopCoroutine(_visualFeedbackRoutine);
            _visualFeedbackRoutine = null;
        }

        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }

        CurrentSpeakingStep = 0;
        SetSpeaking(false);
    }

    // ── Internal Helpers ──────────────────────────────────────────────────

    private void PlayClip(AudioClip clip, int step)
    {
        if (_audioSource == null || clip == null) return;

        _audioSource.clip = clip;
        _audioSource.Play();
        SetSpeaking(true);

        if (_playMonitorRoutine != null) StopCoroutine(_playMonitorRoutine);
        _playMonitorRoutine = StartCoroutine(MonitorPlaybackRoutine());
    }

    private IEnumerator MonitorPlaybackRoutine()
    {
        // Wait until AudioSource finishes playing or is stopped
        while (_audioSource != null && _audioSource.isPlaying)
        {
            yield return null;
        }

        CurrentSpeakingStep = 0;
        SetSpeaking(false);
        _playMonitorRoutine = null;
    }

    private void TriggerVisualFeedbackOnly()
    {
        SetSpeaking(true);
        if (_visualFeedbackRoutine != null) StopCoroutine(_visualFeedbackRoutine);
        _visualFeedbackRoutine = StartCoroutine(ResetAfterDelay(VisualFeedbackDuration));
    }

    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CurrentSpeakingStep = 0;
        SetSpeaking(false);
        _visualFeedbackRoutine = null;
    }

    private void SetSpeaking(bool speaking)
    {
        if (IsSpeaking == speaking) return;
        IsSpeaking = speaking;
        OnSpeakingChanged?.Invoke(speaking);
    }

    private string GetResourcePath(AppLanguage language, int step)
    {
        string langFolder = language.ToString(); // "English", "Hindi", "Santali"
        return $"Audio/FireTraining/{langFolder}/step{step}";
    }

    private AudioClip LoadAudioClip(AppLanguage language, int step)
    {
        string path = GetResourcePath(language, step);
        if (_clipCache.TryGetValue(path, out AudioClip cached) && cached != null)
        {
            return cached;
        }

        AudioClip clip = Resources.Load<AudioClip>(path);
        if (clip != null)
        {
            _clipCache[path] = clip;
        }
        return clip;
    }

    private bool HasAudioClip(AppLanguage language, int step)
    {
        return LoadAudioClip(language, step) != null;
    }

    private bool CheckSantaliAvailability()
    {
        // Only return true if at least step 1 Santali clip is present in Resources
        return HasAudioClip(AppLanguage.Santali, 1);
    }
}
