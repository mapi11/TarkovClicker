using UnityEngine;
using UnityEngine.UI;

public class TimingClick : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _btnTimingClick;
    [SerializeField] private GameObject _spawnPrefab;
    [SerializeField] private Transform _spawnParent;
    [SerializeField] private PointsSystem _pointsSystem;
    [SerializeField] private GameObject _parentHexagon;
    [SerializeField] private Image _spawnedImage;

    [Header("Debug Buttons")]
    [SerializeField] private Button _TestArmBroken; // Кнопка остановки
    [SerializeField] private Button _TestArmHeal;   // Кнопка возобновления

    [Header("Scaling Settings")]
    [SerializeField] private Vector2 _randomScaleRange = new Vector2(2f, 3f);
    [SerializeField] private Vector2 _targetScaleRange = new Vector2(0.8f, 1f);
    [SerializeField] private Vector2 _parentScaleRange = new Vector2(0.5f, 1.5f);
    [SerializeField] private float _scaleSpeed = 0.1f;
    [SerializeField] private float _minScaleThreshold = 0.6f;
    public float _pointsToAdd = 5f;

    [Header("Color Settings")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _targetColor = Color.green;

    [Header("Passive Income")]
    [SerializeField] private float _basePassiveIncomeInterval = 1f;
    [SerializeField] private float _basePassiveIncome = 1f;

    private float _currentPassiveIncomeInterval;
    private GameObject _spawnedObject;
    private float _passiveTimer;
    private PerksSystem _perksSystem;
    private bool _isPassiveIncomePaused = false;

    private void Awake()
    {
        // Инициализация основных кнопок
        _btnTimingClick.onClick.AddListener(OnClick);

        // Инициализация debug-кнопок
        _TestArmBroken.onClick.AddListener(PausePassiveIncome);
        _TestArmHeal.onClick.AddListener(ResumePassiveIncome);

        _perksSystem = FindObjectOfType<PerksSystem>();
        _currentPassiveIncomeInterval = _basePassiveIncomeInterval;
        SpawnObject();
    }

    private void Update()
    {
        // Логика масштабирования объекта
        if (_spawnedObject != null)
        {
            Vector3 newScale = _spawnedObject.transform.localScale - Vector3.one * _scaleSpeed * Time.deltaTime;
            _spawnedObject.transform.localScale = newScale;

            bool isInRange = newScale.x >= _targetScaleRange.x && newScale.x <= _targetScaleRange.y;
            _spawnedImage.color = isInRange ? _targetColor : _normalColor;

            if (newScale.x <= _minScaleThreshold)
            {
                DestroyAndRespawn();
            }
        }

        // Логика пассивного дохода (если не на паузе)
        if (!_isPassiveIncomePaused)
        {
            _passiveTimer += Time.deltaTime;
            if (_passiveTimer >= _currentPassiveIncomeInterval)
            {
                AddPassiveIncome();
                _passiveTimer = 0f;
            }
        }
    }

    // Метод для паузы пассивного дохода
    public void PausePassiveIncome()
    {
        _isPassiveIncomePaused = true;
        Debug.Log("Пассивный доход приостановлен!");
    }

    // Метод для возобновления пассивного дохода
    public void ResumePassiveIncome()
    {
        _isPassiveIncomePaused = false;
        _passiveTimer = 0f; // Сбрасываем таймер
        Debug.Log("Пассивный доход возобновлен!");
    }

    public void UpdatePassiveIncomeInterval(int staminaLevel)
    {
        _currentPassiveIncomeInterval = Mathf.Max(0.1f, _basePassiveIncomeInterval / (1 + staminaLevel * 0.2f));
    }

    private void SpawnObject()
    {
        _spawnedObject = Instantiate(_spawnPrefab, _spawnParent);
        float randomScale = Random.Range(_randomScaleRange.x, _randomScaleRange.y);
        _spawnedObject.transform.localScale = new Vector3(randomScale, randomScale, 1f);
        _spawnedImage = _spawnedObject.GetComponent<Image>();
    }

    private void OnClick()
    {
        if (_spawnedObject == null) return;

        float currentScale = _spawnedObject.transform.localScale.x;
        bool isInRange = currentScale >= _targetScaleRange.x && currentScale <= _targetScaleRange.y;

        if (isInRange)
        {
            _pointsSystem._PointsCount += _pointsToAdd;
            _perksSystem.AddPowerProgress();
        }

        if (_parentHexagon != null)
        {
            float randomParentScale = Random.Range(_parentScaleRange.x, _parentScaleRange.y);
            _parentHexagon.transform.localScale = new Vector3(randomParentScale, randomParentScale, 1f);
        }

        DestroyAndRespawn();
    }

    private void DestroyAndRespawn()
    {
        if (_spawnedObject != null)
        {
            Destroy(_spawnedObject);
        }
        SpawnObject();
    }

    private void AddPassiveIncome()
    {
        if (_pointsSystem == null || _perksSystem == null) return;

        float totalIncome = _basePassiveIncome * _perksSystem.GetStaminaIncomeBonus();
        _pointsSystem._PointsCount += Mathf.RoundToInt(totalIncome);
    }
}