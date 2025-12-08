using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsUI;
    [SerializeField] private GameObject kbmControlsUI;
    [SerializeField] private GameObject controllerControlsUI;
    [SerializeField] private GameObject volumeUI;

    public void ToggleSettingsUI()
    {
        settingsUI.SetActive(!settingsUI.activeSelf);
        transform.Find("Sensitivity").GetComponent<UnityEngine.UI.Slider>().value = PlayerPrefs.GetFloat("sensitivity", 0.5f);
        transform.Find("MasterVolume").GetComponent<UnityEngine.UI.Slider>().value = PlayerPrefs.GetFloat("masterVolume", 1f);
        transform.Find("SFXVolume").GetComponent<UnityEngine.UI.Slider>().value = PlayerPrefs.GetFloat("sfxVolume", 1f);
        transform.Find("MusicVolume").GetComponent<UnityEngine.UI.Slider>().value = PlayerPrefs.GetFloat("musicVolume", 1f);
    }

    public void ToggleKBMControlsUI()
    {
        kbmControlsUI.SetActive(!kbmControlsUI.activeSelf);
    }

    public void ToggleControllerControlsUI()
    {
        controllerControlsUI.SetActive(!controllerControlsUI.activeSelf);
    }

    public void ToggleVolumeUI()
    {
        volumeUI.SetActive(!volumeUI.activeSelf);
    }

    public void OnSensitivityChanged(float newSensitivity)
    {
        PlayerPrefs.SetFloat("sensitivity", newSensitivity);
    }

    public void OnMasterVolumeChanged(float newVolume)
    {
        PlayerPrefs.SetFloat("masterVolume", newVolume);
    }

    public void OnSFXVolumeChanged(float newVolume)
    {
        PlayerPrefs.SetFloat("sfxVolume", newVolume);
    }

    public void OnMusicVolumeChanged(float newVolume)
    {
        PlayerPrefs.SetFloat("musicVolume", newVolume);
    }
}
