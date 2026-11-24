using UnityEngine;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections.Generic;
public class SettingMenu : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider volumeSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        volumeSlider.onValueChanged.AddListener(SetVolume);
        LoadSettings();
    }

    void LoadSettings()
    {
        float volume = PlayerPrefs.GetFloat("volume");
        SetVolume(volume);
    }

    public void SetVolume(float value)
    {
        mixer.SetFloat("volume", value);
        volumeSlider.value = value;
        PlayerPrefs.SetFloat("volume", value);

    }

    
}
