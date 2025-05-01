using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private TextMeshProUGUI _txtPoints;

    public int _currentPoints = 0;
    private int _currentStaminaLevel = 1;
    private float _currentBonusMultiplier = 1;
    private float _currentInterval;
    private float _timer;

    private const string POINTS_KEY = "PlayerPoints"; // Ключ для сохранения очков

    private void Awake()
    {
        // Загрузка сохраненных очков
        _currentPoints = PlayerPrefs.GetInt(POINTS_KEY, 0);

        // Инициализация текста очков
        if (_txtPoints == null)
        {
            _txtPoints = GameObject.Find("txtPoints")?.GetComponent<TextMeshProUGUI>();
        }

        UpdateStaminaSettings();
        UpdatePointsUI();
    }

    private void OnApplicationQuit()
    {
        SavePoints();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SavePoints();
        }
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _currentInterval)
        {
            GeneratePassiveIncome();
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

    private void GeneratePassiveIncome()
    {
        float bonus = 1 + (_currentStaminaLevel * _staminaSettings.incomeBonusPerLevel);
        int points = Mathf.RoundToInt(_staminaSettings.baseIncome * bonus * _currentBonusMultiplier);
        AddPoints(points);

        if (_incomeEffect != null)
            _incomeEffect.Play();
    }

    public void AddPoints(int amount)
    {
        _currentPoints += amount;
        UpdatePointsUI();
        SavePoints(); // Сохраняем после каждого изменения
    }

    private void UpdatePointsUI()
    {
        if (_txtPoints != null)
        {
            _txtPoints.text = $"Points: {_currentPoints}";
        }
    }

    private void SavePoints()
    {
        PlayerPrefs.SetInt(POINTS_KEY, _currentPoints);
        PlayerPrefs.Save();
    }

    // Свойство для доступа к текущим очкам
    public int Points => _currentPoints;

    // Метод для сброса очков (по желанию)
    public void ResetPoints()
    {
        _currentPoints = 0;
        PlayerPrefs.DeleteKey(POINTS_KEY);
        UpdatePointsUI();
    }
}