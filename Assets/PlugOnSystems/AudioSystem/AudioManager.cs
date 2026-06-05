using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
}

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource oneShotSfxSource;
    public AudioSource loopedSfxSource;

    [Header("Sound Libraries")]
    public List<Sound> musicSounds;
    public List<Sound> oneShotSfxSounds;
    public List<Sound> loopedSfxSounds;

    [Header("Internal Settings")]
    public float musicFadeDuration = 0.5f;
    public float loopedSfxFadeDuration = 0.5f;

    private void OnEnable()
    {
        GameEvents.OnPlayMusic += PlayMusic;
        GameEvents.OnStopMusic += StopMusic;
        GameEvents.OnStopLoopedSFX += StopLoopedSFX;
        GameEvents.OnPlayOneShotSFX += PlayOneShotSFX;
        GameEvents.OnPlayLoopedSFX += PlayLoopedSFX;
        GameEvents.OnSkipMusicTime += SkipMusicTime;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayMusic -= PlayMusic;
        GameEvents.OnStopMusic -= StopMusic;
        GameEvents.OnStopLoopedSFX -= StopLoopedSFX;
        GameEvents.OnPlayOneShotSFX -= PlayOneShotSFX;
        GameEvents.OnPlayLoopedSFX -= PlayLoopedSFX;
        GameEvents.OnSkipMusicTime -= SkipMusicTime;
    }

    private void PlayMusic(string name)
    {
        Sound s = musicSounds.Find(x => x.name == name);

        if (s == null)
        {
            Debug.LogWarning("Müzik bulunamadı: " + name);
            return;
        }

        if (musicSource.clip == s.clip && musicSource.isPlaying) return;

        musicSource.clip = s.clip;
        musicSource.volume = s.volume;
        musicSource.Play();
    }

    private void StopMusic()
    {
        StartCoroutine(FadeOutAndStopRoutine(musicSource, musicFadeDuration));
    }

    private void StopLoopedSFX()
    {
        StartCoroutine(FadeOutAndStopRoutine(loopedSfxSource, loopedSfxFadeDuration));
    }

    private IEnumerator FadeOutAndStopRoutine(AudioSource source, float fadeTime)
    {
        if (source == null || !source.isPlaying) yield break;

        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
        source.volume = startVolume;
    }

    private void PlayOneShotSFX(string name)
    {
        Sound s = oneShotSfxSounds.Find(x => x.name == name);

        if (s == null)
        {
            Debug.LogWarning("SFX bulunamadı: " + name);
            return;
        }

        oneShotSfxSource.PlayOneShot(s.clip, s.volume);
    }

    private void PlayLoopedSFX(string name)
    {
        Sound s = loopedSfxSounds.Find(x => x.name == name);
        if (s == null)
        {
            Debug.LogWarning("Looped SFX bulunamadı: " + name);
            return;
        }
        if (loopedSfxSource.clip == s.clip && loopedSfxSource.isPlaying) return;
        loopedSfxSource.clip = s.clip;
        loopedSfxSource.volume = s.volume;
        loopedSfxSource.Play();
    }

    private void SkipMusicTime(float seconds)
    {
        if (musicSource == null || musicSource.clip == null) return;

        float newTime = musicSource.time + seconds;

        musicSource.time = Mathf.Clamp(newTime, 0f, musicSource.clip.length);
    }
}