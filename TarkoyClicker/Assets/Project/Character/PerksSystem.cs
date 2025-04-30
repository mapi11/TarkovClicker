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
        public float bonusGrowth = 0.1f; // Ќа сколько увеличиваетс€ бонус с каждым уровнем

        [Header("UI References")]
        public Slider progressSlider;
        public TextMeshProUGUI levelText;

        // ƒинамически рассчитываемый бонус
        public float CurrentBonus => baseBonusPerLevel + (level - 1) * bonusGrowth;
    }

    [Header("Perks")]
    public Perk staminaPerk;
    public Perk powerPerk;

    [Header("Settings")]
    [SerializeField] private float _powerClickMultiplier = 1.5f;
    [SerializeField] private float _staminaProgressPerClick = 1f;

    public Button upgradeButton;

    private PassivePoints _passivePoints;
    private TimingClick _timingClick;

    private void Awake()
    {
        _passivePoints = FindAnyObjectByType<PassivePoints>();
        _timingClick = FindAnyObjectByType<TimingClick>();

        InitPerk(ref staminaPerk);
        InitPerk(ref powerPerk);

       upgradeButton.onClick.AddListener(() => AddStaminaProgress(_staminaProgressPerClick));
    }

    private void InitPerk(ref Perk perk)
    {
        perk.currentProgress = 0f;
        UpdatePerkUI(perk);
    }

    public void AddStaminaProgress(float amount)
    {
        AddPerkProgress(ref staminaPerk, amount, () =>
        {
            _passivePoints.UpgradeStamina(staminaPerk.level, staminaPerk.CurrentBonus);
        });
    }

    public void AddPowerProgress(float amount)
    {
        AddPerkProgress(ref powerPerk, amount, () =>
        {
            float multiplier = Mathf.Pow(_powerClickMultiplier, powerPerk.level - 1) * (1 + powerPerk.CurrentBonus);
            _timingClick.SetPointsMultiplier(multiplier);
        });
    }

    private void AddPerkProgress(ref Perk perk, float amount, System.Action onLevelUp)
    {
        perk.currentProgress += amount;

        if (perk.currentProgress >= perk.nextLevelRequirement)
        {
            perk.level++;
            perk.currentProgress = 0f;
            perk.nextLevelRequirement *= perk.progressMultiplier;
            onLevelUp?.Invoke();
        }

        UpdatePerkUI(perk);
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
}