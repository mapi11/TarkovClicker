using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TutorialSystem : MonoBehaviour
{
    [System.Serializable]
    public class TutorialSlide
    {
        public GameObject slidePrefab;  // ������ ������
        [HideInInspector] public GameObject spawnedInstance;  // ��������� ���������
    }

    [Header("References")]
    [SerializeField] private Transform _slideParent;  // �������� ��� ������ �������
    [SerializeField] private TextMeshProUGUI _slideCounterText;
    [SerializeField] private Button _skipAllButton;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _prevButton;

    [Header("Tutorial Content")]
    [SerializeField] private List<TutorialSlide> _slides = new List<TutorialSlide>();

    private PerksSystem _perksSystem;
    private int _currentSlideIndex = 0;

    private void Start()
    {
        _perksSystem = FindObjectOfType<PerksSystem>();

        if (_perksSystem != null && _perksSystem._characterLevel <= 0)
        {
            InitializeTutorial();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeTutorial()
    {
        // ��������� ����������� ������
        if (_slideParent == null)
        {
            Debug.LogError("Slide Parent not assigned!");
            return;
        }

        // ����������� ������
        _nextButton.onClick.AddListener(NextSlide);
        _prevButton.onClick.AddListener(PreviousSlide);
        _skipAllButton.onClick.AddListener(SkipAll);

        ShowSlide(0);
    }

    private void ShowSlide(int index)
    {
        if (_slides.Count == 0 || _slideParent == null) return;

        // ������� ���������� �����
        if (_currentSlideIndex >= 0 && _currentSlideIndex < _slides.Count &&
            _slides[_currentSlideIndex].spawnedInstance != null)
        {
            Destroy(_slides[_currentSlideIndex].spawnedInstance);
        }

        // ��������� ������
        _currentSlideIndex = Mathf.Clamp(index, 0, _slides.Count - 1);

        // ������� ����� �����
        if (_slides[_currentSlideIndex].slidePrefab != null)
        {
            _slides[_currentSlideIndex].spawnedInstance = Instantiate(
                _slides[_currentSlideIndex].slidePrefab,
                _slideParent,
                false
            );
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        // ��������� ������ ���������
        _prevButton.interactable = _currentSlideIndex > 0;

        if (_currentSlideIndex == _slides.Count - 1)
        {
            _nextButton.onClick.RemoveAllListeners();
            _nextButton.onClick.AddListener(SkipAll);
            _nextButton.GetComponentInChildren<TextMeshProUGUI>().text = "���������";
        }
        else
        {
            _nextButton.onClick.RemoveAllListeners();
            _nextButton.onClick.AddListener(NextSlide);
            _nextButton.GetComponentInChildren<TextMeshProUGUI>().text = "�����";
        }

        // ��������� �������
        if (_slideCounterText != null)
        {
            _slideCounterText.text = $"{_currentSlideIndex + 1}/{_slides.Count}";
        }
    }

    private void NextSlide()
    {
        ShowSlide(_currentSlideIndex + 1);
    }

    private void PreviousSlide()
    {
        ShowSlide(_currentSlideIndex - 1);
    }

    private void SkipAll()
    {
        // ������� ��� ������
        foreach (var slide in _slides)
        {
            if (slide.spawnedInstance != null)
            {
                Destroy(slide.spawnedInstance);
            }
        }

        // ������� ��� ��������
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // �������������� ������� �� ������, ���� ������ ��� ��������� ��� ������ SkipAll
        foreach (var slide in _slides)
        {
            if (slide.spawnedInstance != null)
            {
                Destroy(slide.spawnedInstance);
            }
        }
    }
}