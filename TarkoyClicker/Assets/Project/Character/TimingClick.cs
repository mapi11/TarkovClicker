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

    [Header("Settings")]
    [SerializeField] private Vector2 _scaleRange = new Vector2(2f, 3f);
    [SerializeField] private Vector2 _targetRange = new Vector2(0.8f, 1f);
    [SerializeField] private float _scaleSpeed = 0.1f;
    [SerializeField] private float _minScale = 0.6f;
    [SerializeField] private int _pointsPerClick = 5;
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

        SpawnObject();

        if (_animator == null)
            _animator = GetComponent<Animator>();

        // Сохраняем оригинальные параметры
        if (_parentHexagon != null)
        {
            _originalScale = _parentHexagon.transform.localScale;
            if (_parentHexagon.GetComponent<Image>() != null)
            {
                _originalColor = _parentHexagon.GetComponent<Image>().color;
            }
        }
    }

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

    public void ArmBroken()
    {
        IsArmBroken = true;

        // Выключаем родительский объект
        if (_parentHexagon != null)
        {
            _parentHexagon.SetActive(false);
        }

        // Останавливаем все корутины
        if (_timingClickCoroutine != null)
        {
            StopCoroutine(_timingClickCoroutine);
            _timingClickCoroutine = null;
        }


        // Уничтожаем текущий spawnedObject
        if (_spawnedObject != null)
        {
            Destroy(_spawnedObject);
            _spawnedObject = null;
        }
    }

    public void ArmHeal()
    {
        IsArmBroken = false;

        // Включаем родительский объект
        if (_parentHexagon != null)
        {
            _parentHexagon.SetActive(true);

            // Восстанавливаем оригинальные параметры
            _parentHexagon.transform.localScale = _originalScale;
            if (_parentHexagon.GetComponent<Image>() != null)
            {
                _parentHexagon.GetComponent<Image>().color = _originalColor;
            }
        }

        // Включаем аниматор
        if (_animator != null)
        {
            _animator.enabled = true;
        }

        // Перезапускаем систему
        SpawnObject();
    }

    public void SetPointsMultiplier(float multiplier)
    {
        _pointsMultiplier = multiplier;
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