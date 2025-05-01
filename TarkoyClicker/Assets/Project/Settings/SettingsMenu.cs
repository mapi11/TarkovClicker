using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("Volume Settings")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumeTextValue;

    [Header("Graphics Settings")]
    [SerializeField] private TMP_Dropdown graphicsDropdown;

    private void Start()
    {
        // ������������� UI �������� ���������� (������� ��� ���������)
        UpdateVolumeUI(AudioListener.volume);
        UpdateGraphicsUI(QualitySettings.GetQualityLevel());

        // ��������� ����������� �������
        volumeSlider.onValueChanged.AddListener(SetVolume);
        graphicsDropdown.onValueChanged.AddListener(SetGraphicsQuality);
    }

    // ������ ��� ���������� UI (����� ���������� �����)
    public void UpdateVolumeUI(float volume)
    {
        volumeSlider.value = volume;
        volumeTextValue.text = Mathf.RoundToInt(volume * 100) + "%";
    }

    public void UpdateGraphicsUI(int qualityIndex)
    {
        graphicsDropdown.value = qualityIndex;
    }

    public void SetVolume(float volume)
    {
        // ������������� ���������
        AudioListener.volume = volume;
        UpdateVolumeUI(volume);
        PlayerPrefs.SetFloat("masterVolume", volume);
    }

    public void SetGraphicsQuality(int qualityIndex)
    {
        // ������������� �������� �������
        QualitySettings.SetQualityLevel(qualityIndex);
        UpdateGraphicsUI(qualityIndex);
        PlayerPrefs.SetInt("graphicsQuality", qualityIndex);
    }
}