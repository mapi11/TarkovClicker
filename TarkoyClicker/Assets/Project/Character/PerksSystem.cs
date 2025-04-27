using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerksSystem : MonoBehaviour
{
    [System.Serializable]
    public class Perk
    {
        public string name;
        public int level = 1;
        public float currentProgress;
        public float nextLevelRequirement = 1f;
        public float progressMultiplier = 2f;
        public float bonusPerLevel;
    }

    [Header("Perks")]
    public Perk powerPerk;
    public Perk staminaPerk;

    [Header("UI References")]
    [SerializeField] private Slider _powerProgressSlider;
    [SerializeField] private TextMeshProUGUI _powerLevelText;
    [Space]
    [SerializeField] private Slider _staminaProgressSlider;
    [SerializeField] private TextMeshProUGUI _staminaLevelText;

    [Header("Settings")]
    [SerializeField] private float _baseProgressPerClick = 0.1f;
    [SerializeField] private TimingClick _timingClick;
    [SerializeField] private float _staminaIncomeBonus = 0.2f;

    [Header("Test")]
    public Button staminaUp;

    private void Start()
    {
        InitializePerks();
        UpdateUI();

        staminaUp.onClick.AddListener(AddStaminaProgress);
    }

    private void InitializePerks()
    {
        powerPerk = new Perk()
        {
            name = "Power",
            bonusPerLevel = 0.5f
        };

        staminaPerk = new Perk()
        {
            name = "Stamina",
            bonusPerLevel = 0.2f
        };
    }

    public void AddPowerProgress()
    {
        float staminaBonus = 1f + (staminaPerk.level * staminaPerk.bonusPerLevel);
        powerPerk.currentProgress += _baseProgressPerClick * staminaBonus;

        if (powerPerk.currentProgress >= powerPerk.nextLevelRequirement)
        {
            PowerLevelUp();
        }

        UpdateUI();
    }

    private void PowerLevelUp()
    {
        powerPerk.level++;
        _timingClick._pointsToAdd += powerPerk.bonusPerLevel;

        powerPerk.currentProgress = 0f;
        powerPerk.nextLevelRequirement *= powerPerk.progressMultiplier;
    }

    public void AddStaminaProgress()
    {
        staminaPerk.currentProgress += _baseProgressPerClick;

        if (staminaPerk.currentProgress >= staminaPerk.nextLevelRequirement)
        {
            StaminaLevelUp();
        }

        UpdateUI();
    }

    private void StaminaLevelUp()
    {
        staminaPerk.level++;
        staminaPerk.currentProgress = 0f;
        staminaPerk.nextLevelRequirement *= staminaPerk.progressMultiplier;

        // Обновляем интервал пассивного дохода в TimingClick
        if (_timingClick != null)
        {
            _timingClick.UpdatePassiveIncomeInterval(staminaPerk.level);
        }
    }

    private void UpdateUI()
    {
        if (_powerProgressSlider != null)
        {
            _powerProgressSlider.maxValue = powerPerk.nextLevelRequirement;
            _powerProgressSlider.value = powerPerk.currentProgress;
        }

        if (_powerLevelText != null)
        {
            _powerLevelText.text = $"Lvl {powerPerk.level}";
        }

        if (_staminaProgressSlider != null)
        {
            _staminaProgressSlider.maxValue = staminaPerk.nextLevelRequirement;
            _staminaProgressSlider.value = staminaPerk.currentProgress;
        }

        if (_staminaLevelText != null)
        {
            _staminaLevelText.text = $"Lvl {staminaPerk.level}";
        }
    }

    public float GetStaminaIncomeBonus()
    {
        return 1f + (staminaPerk.level * _staminaIncomeBonus);
    }
}