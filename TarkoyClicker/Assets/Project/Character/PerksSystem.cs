using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerksSystem : MonoBehaviour
{
    [System.Serializable]
    public class Perk
    {
        public string name;
        public int level = 1;
        public float currentProgress;
        public float nextLevelRequirement = 10f;
        public float progressMultiplier = 1.5f;
        public float baseBonusPerLevel = 0.5f;
        public float bonusGrowth = 0.1f;

        [Header("UI References")]
        public Slider progressSlider;
        public TextMeshProUGUI levelText;

        public float CurrentBonus => baseBonusPerLevel + (level - 1) * bonusGrowth;

        public void Save(string prefix)
        {
            PlayerPrefs.SetInt(prefix + "_level", level);
            PlayerPrefs.SetFloat(prefix + "_currentProgress", currentProgress);
            PlayerPrefs.SetFloat(prefix + "_nextLevelReq", nextLevelRequirement);
            PlayerPrefs.Save();
        }

        public void Load(string prefix)
        {
            level = PlayerPrefs.GetInt(prefix + "_level", 1);
            currentProgress = PlayerPrefs.GetFloat(prefix + "_currentProgress", 0f);
            nextLevelRequirement = PlayerPrefs.GetFloat(prefix + "_nextLevelReq", 10f);
        }
    }

    [Header("Perks")]
    public Perk staminaPerk;
    public Perk powerPerk;

    [Header("Settings")]
    [SerializeField] private float _powerClickMultiplier = 1.4f;
    [SerializeField] private float _staminaProgressPerClick = 1f;
    [Space]
    [SerializeField] private float _powerUpgradeBaseCost = 500f;
    [SerializeField] private float _powerUpgradeCostMultiplier = 1.2f;

    [Header("Character Level UI")]
    [SerializeField] private TextMeshProUGUI _txtCharacterLvl;
    [SerializeField] private Slider _characterLevelSlider;

    [Header("Buttons")]
    public Button upgradeButton;
    public Button btnUpgradePower;
    [SerializeField] private TextMeshProUGUI txtUpgradePower;

    private PassivePoints _passivePoints;
    private TimingClick _timingClick;

    private int _characterLevel = 0;
    private int _lastTriggeredLevel = 0;

    private void Awake()
    {
        _passivePoints = FindAnyObjectByType<PassivePoints>();
        _timingClick = FindAnyObjectByType<TimingClick>();

        LoadPerks();

        if (_characterLevelSlider != null)
        {
            _characterLevelSlider.minValue = 0;
            _characterLevelSlider.maxValue = 10;
            _characterLevelSlider.value = 0;
        }

        upgradeButton.onClick.AddListener(() => AddStaminaProgress(_staminaProgressPerClick));
        btnUpgradePower.onClick.AddListener(UpgradePower);

        UpdatePowerButtonText(); // Обновляем текст кнопки при старте
        UpdateCharacterLevel();
    }

    private void UpdatePowerButtonText()
    {
        if (txtUpgradePower != null)
        {
            int cost = CalculatePowerUpgradeCost();
            txtUpgradePower.text = $"Upgrade Power\n<size=80%>(Cost: {cost})</size>";
        }
    }

    public int GetCharacterLevel()
    {
        return _characterLevel;
    }

    private void UpgradePower()
    {
        int cost = CalculatePowerUpgradeCost();
        if (_passivePoints._currentPoints >= cost)
        {
            _passivePoints._currentPoints -= cost;
            AddPowerProgress(1f);
            UpdatePowerButtonText(); // Обновляем текст после улучшения
        }
        else
        {
            Debug.Log("Not enough passive points!");
        }
    }

    private int CalculatePowerUpgradeCost()
    {
        return Mathf.RoundToInt(_powerUpgradeBaseCost * Mathf.Pow(_powerUpgradeCostMultiplier, powerPerk.level - 1));
    }

    private void OnApplicationQuit() => SavePerks();
    private void OnApplicationPause(bool pause) { if (pause) SavePerks(); }

    private void SavePerks()
    {
        staminaPerk.Save("stamina");
        powerPerk.Save("power");
    }

    private void LoadPerks()
    {
        staminaPerk.Load("stamina");
        powerPerk.Load("power");
        UpdatePerkUI(staminaPerk);
        UpdatePerkUI(powerPerk);
        ApplyPowerMultiplier();
    }
    private void ApplyPowerMultiplier()
    {
        if (_timingClick != null)
        {
            float multiplier = Mathf.Pow(_powerClickMultiplier, powerPerk.level - 1);
            _timingClick.SetPointsMultiplier(multiplier);
        }
    }

    public void AddStaminaProgress(float amount)
    {
        staminaPerk.currentProgress += amount;
        SaveImmediately("stamina", ref staminaPerk, () =>
        {
            _passivePoints.UpgradeStamina(staminaPerk.level, staminaPerk.CurrentBonus);
            UpdateCharacterLevel();
        });
    }

    public void AddPowerProgress(float amount)
    {
        powerPerk.currentProgress += amount;
        SaveImmediately("power", ref powerPerk, () =>
        {
            ApplyPowerMultiplier(); // Используем общий метод
            UpdateCharacterLevel();
        });
    }

    private void SaveImmediately(string prefix, ref Perk perk, System.Action onLevelUp)
    {
        bool levelUp = false;

        if (perk.currentProgress >= perk.nextLevelRequirement)
        {
            perk.level++;
            perk.currentProgress = 0f;
            perk.nextLevelRequirement *= perk.progressMultiplier;
            levelUp = true;
        }

        UpdatePerkUI(perk);
        perk.Save(prefix);

        if (levelUp)
        {
            onLevelUp?.Invoke();
            if (prefix == "power") UpdatePowerButtonText(); // Обновляем текст при повышении уровня силы
        }
    }

    private void UpdatePerkUI(Perk perk)
    {
        if (perk.progressSlider != null)
        {
            perk.progressSlider.maxValue = perk.nextLevelRequirement;
            perk.progressSlider.value = perk.currentProgress;
        }

        if (perk.levelText != null)
        {
            perk.levelText.text = $"Lv.{perk.level} (+{perk.CurrentBonus * 100:F1}%)";
        }
    }

    private void UpdateCharacterLevel()
    {
        int newLevel = staminaPerk.level + powerPerk.level;
        if (newLevel != _characterLevel)
        {
            _characterLevel = newLevel;
            UpdateCharacterLevelUI();
            CheckForNewCharacterLevel();
        }
    }

    private void UpdateCharacterLevelUI()
    {
        if (_txtCharacterLvl != null) _txtCharacterLvl.text = $"Level: {_characterLevel}";

        if (_characterLevelSlider != null)
        {
            _characterLevelSlider.value = _characterLevel % 10;
            if (_characterLevel % 10 == 0) _characterLevelSlider.value = 10;
        }
    }

    private void CheckForNewCharacterLevel()
    {
        int threshold = _characterLevel / 10;
        if (threshold > _lastTriggeredLevel)
        {
            _lastTriggeredLevel = threshold;
            CharacterNewLevel(_characterLevel);
        }
    }

    private void CharacterNewLevel(int newLevel)
    {
        Debug.Log($"New milestone: {newLevel}");
    }
}