using UnityEngine;
using UnityEngine.UI;

public class TimingClick : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _clickButton;
    [SerializeField] private GameObject _spawnPrefab;
    [SerializeField] private Transform _spawnParent;
    [SerializeField] private GameObject _parentHexagon;
    [SerializeField] private Image _spawnedImage;

    [Header("Settings")]
    [SerializeField] private Vector2 _scaleRange = new Vector2(2f, 3f);
    [SerializeField] private Vector2 _targetRange = new Vector2(0.8f, 1f);
    [SerializeField] private float _scaleSpeed = 0.1f;
    [SerializeField] private float _minScale = 0.6f;
    [SerializeField] private int _pointsPerClick = 5;

    private int _basePointsPerClick = 5;
    private float _pointsMultiplier = 1f;
    private GameObject _spawnedObject;

    private void Awake()
    {
        _clickButton.onClick.AddListener(OnClick);
        SpawnObject();
    }

    private void Update()
    {
        if (_spawnedObject == null) return;

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
    }

    private void OnClick()
    {
        if (_spawnedObject == null) return;

        float scale = _spawnedObject.transform.localScale.x;
        if (scale >= _targetRange.x && scale <= _targetRange.y)
        {
            int points = Mathf.RoundToInt(_basePointsPerClick * _pointsMultiplier);
            FindObjectOfType<PointsSystem>()?.AddPoints(points);
            FindObjectOfType<PerksSystem>()?.AddPowerProgress(1f);
        }

        DestroyAndRespawn();
    }

    private void SpawnObject()
    {
        _spawnedObject = Instantiate(_spawnPrefab, _spawnParent);
        float randomScale = Random.Range(_scaleRange.x, _scaleRange.y);
        _spawnedObject.transform.localScale = new Vector3(randomScale, randomScale, 1f);
        _spawnedImage = _spawnedObject.GetComponent<Image>();
    }

    private void DestroyAndRespawn()
    {
        if (_spawnedObject != null)
            Destroy(_spawnedObject);
        SpawnObject();
    }
}