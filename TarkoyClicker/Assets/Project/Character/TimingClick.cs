using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimingClick : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _clickButton;
    [SerializeField] private GameObject _spawnPrefab;
    [SerializeField] private Transform _spawnParent;
    [SerializeField] private GameObject _parentHexagon;
    [SerializeField] private Image _spawnedImage;
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _armBreakSound;
    [SerializeField] private Button _btnHealAdd; // Новая кнопка лечения через рекламу

    [Header("Settings")]
    [SerializeField] private Vector2 _scaleRange = new Vector2(2f, 3f);
    [SerializeField] private Vector2 _targetRange = new Vector2(0.8f, 1f);
    [SerializeField] private float _scaleSpeed = 0.1f;
    [SerializeField] private float _minScale = 0.6f;
    [SerializeField] private int _basePoints = 500;
    [SerializeField] private float _armBreakChance = 0.2f;
    public bool IsArmBroken { get; private set; }

    [Header("Debug")]
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _resumeButton;

    private int _basePointsPerClick = 5;
    private float _pointsMultiplier = 1f;
    private GameObject _spawnedObject;
    private Coroutine _timingClickCoroutine;
    private Vector3 _originalScale;
    private Color _originalColor;

    private void Awake()
    {
        _clickButton.onClick.AddListener(OnClick);
        _pauseButton.onClick.AddListener(ArmBroken);
        _resumeButton.onClick.AddListener(ArmHeal);

        // Инициализация новой кнопки
        _btnHealAdd.onClick.AddListener(OnHealAdButtonClick);
        _btnHealAdd.gameObject.SetActive(false); // Скрываем кнопку по умолчанию

        _basePointsPerClick = _basePoints;
        LoadMultiplier();
        SpawnObject();

        if (_animator == null)
            _animator = GetComponent<Animator>();

        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        if (_parentHexagon != null)
        {
            _originalScale = _parentHexagon.transform.localScale;
            if (_parentHexagon.GetComponent<Image>() != null)
            {
                _originalColor = _parentHexagon.GetComponent<Image>().color;
            }
        }
    }

    public void ArmBroken()
    {
        IsArmBroken = true;
        _btnHealAdd.gameObject.SetActive(true); // Показываем кнопку лечения при поломке

        if (_armBreakSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_armBreakSound);
        }

        if (_parentHexagon != null)
        {
            _parentHexagon.SetActive(false);
        }

        if (_timingClickCoroutine != null)
        {
            StopCoroutine(_timingClickCoroutine);
            _timingClickCoroutine = null;
        }

        if (_spawnedObject != null)
        {
            Destroy(_spawnedObject);
            _spawnedObject = null;
        }
    }

    public void ArmHeal()
    {
        IsArmBroken = false;
        _btnHealAdd.gameObject.SetActive(false); // Скрываем кнопку после лечения

        if (_parentHexagon != null)
        {
            _parentHexagon.SetActive(true);
            _parentHexagon.transform.localScale = _originalScale;
            if (_parentHexagon.GetComponent<Image>() != null)
            {
                _parentHexagon.GetComponent<Image>().color = _originalColor;
            }
        }

        if (_animator != null)
        {
            _animator.enabled = true;
        }

        SpawnObject();
    }

    private void OnHealAdButtonClick()
    {
        // Здесь будет вызов рекламы
        Debug.Log("Показываем рекламу для лечения руки...");

        // В будущем здесь будет коллбэк после просмотра рекламы
        // Пока просто сразу лечим
        ArmHeal();

        // В реальном коде это будет выглядеть примерно так:
        // AdManager.Instance.ShowRewardedAd("arm_heal", () => {
        //     ArmHeal();
        // });
    }

    // Остальные методы остаются без изменений
    public void Update()
    {
        if (IsArmBroken || _spawnedObject == null) return;

        Vector3 newScale = _spawnedObject.transform.localScale - Vector3.one * _scaleSpeed * Time.deltaTime;
        _spawnedObject.transform.localScale = newScale;

        bool inRange = newScale.x >= _targetRange.x && newScale.x <= _targetRange.y;
        _spawnedImage.color = inRange ? Color.green : Color.white;

        if (newScale.x <= _minScale)
            DestroyAndRespawn();
    }

    public void SetPointsMultiplier(float multiplier)
    {
        _pointsMultiplier = multiplier;
        SaveMultiplier();
    }

    private void SaveMultiplier()
    {
        PlayerPrefs.SetFloat("click_multiplier", _pointsMultiplier);
        PlayerPrefs.Save();
    }

    private void LoadMultiplier()
    {
        if (PlayerPrefs.HasKey("click_multiplier"))
        {
            _pointsMultiplier = PlayerPrefs.GetFloat("click_multiplier", 1f);
        }
    }

    private void OnClick()
    {
        if (IsArmBroken) return;

        if (_timingClickCoroutine != null)
        {
            StopCoroutine(_timingClickCoroutine);
        }

        if (_spawnedObject == null) return;

        float scale = _spawnedObject.transform.localScale.x;
        if (scale >= _targetRange.x && scale <= _targetRange.y)
        {
            int points = Mathf.RoundToInt(_basePointsPerClick * _pointsMultiplier);
            FindObjectOfType<PassivePoints>()?.AddPoints(points);
            FindObjectOfType<PerksSystem>()?.AddPowerProgress(1f);

            _timingClickCoroutine = StartCoroutine(TimingClickRoutine());
        }
        else
        {
            if (Random.value <= _armBreakChance)
            {
                ArmBroken();
            }
        }

        DestroyAndRespawn();
    }

    private IEnumerator TimingClickRoutine()
    {
        if (_animator != null)
        {
            _animator.SetBool("TimingClick", true);
            yield return new WaitForSeconds(1.5f);
            _animator.SetBool("TimingClick", false);
        }
    }

    private void SpawnObject()
    {
        if (IsArmBroken) return;

        _spawnedObject = Instantiate(_spawnPrefab, _spawnParent);
        float randomScale = Random.Range(_scaleRange.x, _scaleRange.y);
        _spawnedObject.transform.localScale = new Vector3(randomScale, randomScale, 1f);
        _spawnedImage = _spawnedObject.GetComponent<Image>();
    }

    private void DestroyAndRespawn()
    {
        if (IsArmBroken) return;

        if (_spawnedObject != null)
            Destroy(_spawnedObject);
        SpawnObject();
    }
}