using UnityEngine;

public class StartSettings : MonoBehaviour
{
    private void Start()
    {
        // ��������� ��������� �����
        ApplyVolumeSettings();

        // ��������� ��������� �������
        ApplyGraphicsSettings();
    }

    private void ApplyVolumeSettings()
    {
        // ��������� ���������� ��������� (�� ��������� 1, ���� ��� ����������� ��������)
        float savedVolume = PlayerPrefs.HasKey("masterVolume") ?
                          PlayerPrefs.GetFloat("masterVolume") : 0.5f;

        // ��������� ��������� �����
        AudioListener.volume = savedVolume;

        // ��������� UI � ���� ��������, ���� ��� ����������
        UpdateVolumeUI(savedVolume);
    }

    private void ApplyGraphicsSettings()
    {
        // ��������� ���������� �������� ������� (�� ��������� 1 - High)
        int savedQuality = PlayerPrefs.HasKey("graphicsQuality") ?
                         PlayerPrefs.GetInt("graphicsQuality") : 1;

        // ��������� ��������� ������� �����
        QualitySettings.SetQualityLevel(savedQuality);

        // ��������� UI � ���� ��������, ���� ��� ����������
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