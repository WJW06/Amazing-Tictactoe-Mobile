using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager audioManager;

    [Header("#BGM")]
    public AudioClip bgmClip;
    public float bgmVolume;
    AudioSource bgmPlayer;

    [Header("#SFX")]
    public AudioClip[] sfxClips;
    public int channels;
    public float sfxVolume;
    AudioSource[] sfxPlayers;
    int channelIndex;


    public enum SFX
    {
        Player1,
        Player2,
        Item,
        Hammer,
        HandGun,
        Shotgun,
        WildCard,
        Win,
        Lose
    }

    private void Awake()
    {
        audioManager = this;
        bgmVolume = PlayerPrefs.HasKey("BGM") ? PlayerPrefs.GetFloat("BGM") / 10 : 1;
        sfxVolume = PlayerPrefs.HasKey("SFX") ? PlayerPrefs.GetFloat("SFX") / 10 : 1;
        Init();
    }   

    void Init()
    {
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = true;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;

        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];
        for (int i = 0; i < channels; ++i)
        {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = true;
            sfxPlayers[i].volume = sfxVolume;
        }

        PlayBGM(true);
    }

    public void PlayBGM(bool isPlay)
    {
        if (isPlay) bgmPlayer.Play();
        else bgmPlayer.Stop();
    }

    public void PlaySFX(SFX sfx)
    {
        for (int i = 0; i < channels; ++i)
        {
           int loopIndex = (i + channelIndex) % channels;

            if (sfxPlayers[loopIndex].isPlaying) continue;

            channelIndex = loopIndex;
            sfxPlayers[channelIndex].clip = sfxClips[(int)sfx];
            if (sfx == SFX.WildCard) sfxPlayers[channelIndex].pitch = 2;
            else sfxPlayers[channelIndex].pitch = 1;
            sfxPlayers[channelIndex].Play();
            break;
        }
    }

    public void ChangeBGMVolume(float volume)
    {
        bgmVolume = volume;
        bgmPlayer.volume = bgmVolume;
    }

    public void ChangeSFXVolume(float volume)
    {
        sfxVolume = volume;
        for (int i = 0; i < channels; ++i)
        {
            sfxPlayers[i].volume = sfxVolume;
        }
    }
}
