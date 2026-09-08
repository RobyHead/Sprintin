using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SongPreviewManager : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Fade")]
    [SerializeField] private float fadeInMs = 500f;
    [SerializeField] private float fadeOutMs = 500f;

    [Header("Stop")]
    [SerializeField] private float stopFadeMs = 300f;

    [Header("Loop")]
    [SerializeField] private float loopDelayMs = 2000f;

    private SongData _currentSong;
    private string _currentPackId;
    private string _currentSongsPath;
    private AudioClip _clip;
    private Coroutine _previewRoutine;
    private Coroutine _stopFadeRoutine;

    public void FadeOutAndStop()
    {
        if (_stopFadeRoutine != null)
        {
            StopCoroutine(_stopFadeRoutine);
            _stopFadeRoutine = null;
        }

        if (_previewRoutine != null)
        {
            StopCoroutine(_previewRoutine);
            _previewRoutine = null;
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            _stopFadeRoutine = StartCoroutine(StopFadeRoutine());
        }
        else
        {
            CleanupAudio();
        }
    }

    private IEnumerator StopFadeRoutine()
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;
        while (elapsed < stopFadeMs / 1000f)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (stopFadeMs / 1000f));
            yield return null;
        }
        CleanupAudio();
    }

    private void CleanupAudio()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.volume = 0f;
            audioSource.clip = null;
        }
        _clip = null;
        _currentSong = null;
    }

    public void OnSongSelected(SongData song, string packId, string songsPath)
    {
        if (_currentSong == song)
            return;

        _currentSong = song;
        _currentPackId = packId;
        _currentSongsPath = songsPath;

        if (_previewRoutine != null)
        {
            StopCoroutine(_previewRoutine);
            _previewRoutine = null;
        }

        _previewRoutine = StartCoroutine(LoadAndPreview());
    }

    private IEnumerator LoadAndPreview()
    {
        var path = Path.Combine(_currentSongsPath, _currentPackId, _currentSong.id, "track.mp3");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Audio not found: {path}");
            yield break;
        }

        var uri = new Uri(path).AbsoluteUri;
        using var request = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG);
        var handler = (DownloadHandlerAudioClip)request.downloadHandler;
        handler.streamAudio = true;
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Failed to load preview audio: {request.error}");
            yield break;
        }

        _clip = DownloadHandlerAudioClip.GetContent(request);
        if (_clip == null)
            yield break;

        audioSource.clip = _clip;

        while (true)
        {
            yield return PlayPreviewOnce();
            yield return new WaitForSeconds(loopDelayMs / 1000f);
        }
    }

    private IEnumerator PlayPreviewOnce()
    {
        if (_clip == null || audioSource == null)
            yield break;

        float beginSec = _currentSong.viewbegin / 1000f;
        float endSec = _currentSong.viewend / 1000f;
        float duration = endSec - beginSec;
        float fadeInSec = fadeInMs / 1000f;
        float fadeOutSec = fadeOutMs / 1000f;

        audioSource.time = beginSec;
        audioSource.volume = 0f;
        audioSource.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float volume = 1f;
            if (elapsed < fadeInSec)
                volume = elapsed / fadeInSec;
            else if (elapsed > duration - fadeOutSec)
                volume = Mathf.Max(0f, (duration - elapsed) / fadeOutSec);

            audioSource.volume = volume;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = 0f;
    }
}