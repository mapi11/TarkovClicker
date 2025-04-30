using UnityEngine;
using UnityEngine.UI;

public class PassivePoints : MonoBehaviour
{
    [System.Serializable]
    public class StaminaSettings
    {
        public float baseInterval = 1f;
        public float baseIncome = 1f;
        public float incomeBonusPerLevel = 0.2f;
        public float intervalReductionPerLevel = 0.2f;
        public float minInterval = 0.1f;
    }

    [Header("Settings")]
    [SerializeField] private StaminaSettings _staminaSettings;
    [SerializeField] private ParticleSystem _incomeEffect;

    [Header("Debug")]
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _resumeButton;

    private PointsSystem _pointsSystem;
    private int _currentStaminaLevel = 1;
    private float _currentBonusMultiplier = 1;
    private float _currentInterval;
    private float _timer;
    private bool _isPaused;

    private void Awake()
    {
        _pointsSystem = FindAnyObjectByType<PointsSystem>();
        _pauseButton.onClick.AddListener(PauseIncome);
        _resumeButton.onClick.AddListener(ResumeIncome);

        UpdateStaminaSettings();
    }

    private void Update()
    {
        if (_isPaused) return;

        _timer += Time.deltaTime;
        if (_timer >= _currentInterval)
        {
            AddPoints();
            _timer = 0f;
        }
    }

    public void UpgradeStamina(int newLevel, float bonus)
    {
        _currentStaminaLevel = newLevel;
        _currentBonusMultiplier = 1 + bonus;
        UpdateStaminaSettings();
    }

    private void UpdateStaminaSettings()
    {
        float intervalReduction = 1 + (_currentStaminaLevel * _staminaSettings.intervalReductionPerLevel);
        _currentInterval = Mathf.Max(
            _staminaSettings.minInterval,
            _staminaSettings.baseInterval / intervalReduction
        );
    }

    private void AddPoints()
    {
        float bonus = 1 + (_currentStaminaLevel * _staminaSettings.incomeBonusPerLevel);
        int points = Mathf.RoundToInt(_staminaSettings.baseIncome * bonus);
        _pointsSystem.AddPoints(points);

        if (_incomeEffect != null)
            _incomeEffect.Play();
    }

    public void PauseIncome() => _isPaused = true;
    public void ResumeIncome() => _isPaused = false;
}