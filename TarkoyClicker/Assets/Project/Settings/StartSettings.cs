using UnityEngine;

public class StartSettings : MonoBehaviour
{
    private void Start()
    {
        // Применяем настройки звука
        ApplyVolumeSettings();

        // Применяем настройки графики
        ApplyGraphicsSettings();
    }

    private void ApplyVolumeSettings()
    {
        // Загружаем сохранённую громкость (по умолчанию 1, если нет сохранённого значения)
        float savedVolume = PlayerPrefs.HasKey("masterVolume") ?
                          PlayerPrefs.GetFloat("masterVolume") : 0.5f;

        // Применяем громкость сразу
        AudioListener.volume = savedVolume;

        // Обновляем UI в меню настроек, если оно существует
        UpdateVolumeUI(savedVolume);
    }

    private void ApplyGraphicsSettings()
    {
        // Загружаем сохранённое качество графики (по умолчанию 1 - High)
        int savedQuality = PlayerPrefs.HasKey("graphicsQuality") ?
                         PlayerPrefs.GetInt("graphicsQuality") : 1;

        // Применяем настройки графики сразу
        QualitySettings.SetQualityLevel(savedQuality);

        // Обновляем UI в меню настроек, если оно существует
        UpdateGraphicsUI(savedQuality);
    }

    private void UpdateVolumeUI(float volume)
    {
        SettingsMenu settingsMenu = FindObjectOfType<SettingsMenu>();
        if (settingsMenu != null)
        {
            settingsMenu.UpdateVolumeUI(volume);
        }
    }

    private void UpdateGraphicsUI(int qualityIndex)
    {
        SettingsMenu settingsMenu = FindObjectOfType<SettingsMenu>();
        if (settingsMenu != null)
        {
            settingsMenu.UpdateGraphicsUI(qualityIndex);
        }
    }
}