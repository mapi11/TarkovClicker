using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TutorialSystem : MonoBehaviour
{
    [System.Serializable]
    public class TutorialSlide
    {
        public GameObject slidePrefab;  // Префаб слайда
        [HideInInspector] public GameObject spawnedInstance;  // Созданный экземпляр
    }

    [Header("References")]
    [SerializeField] private Transform _slideParent;  // Родитель для спавна слайдов
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
        // Проверяем необходимые ссылки
        if (_slideParent == null)
        {
            Debug.LogError("Slide Parent not assigned!");
            return;
        }

        // Настраиваем кнопки
        _nextButton.onClick.AddListener(NextSlide);
        _prevButton.onClick.AddListener(PreviousSlide);
        _skipAllButton.onClick.AddListener(SkipAll);

        ShowSlide(0);
    }

    private void ShowSlide(int index)
    {
        if (_slides.Count == 0 || _slideParent == null) return;

        // Удаляем предыдущий слайд
        if (_currentSlideIndex >= 0 && _currentSlideIndex < _slides.Count &&
            _slides[_currentSlideIndex].spawnedInstance != null)
        {
            Destroy(_slides[_currentSlideIndex].spawnedInstance);
        }

        // Обновляем индекс
        _currentSlideIndex = Mathf.Clamp(index, 0, _slides.Count - 1);

        // Спавним новый слайд
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
        // Обновляем кнопки навигации
        _prevButton.interactable = _currentSlideIndex > 0;

        if (_currentSlideIndex == _slides.Count - 1)
        {
            _nextButton.onClick.RemoveAllListeners();
            _nextButton.onClick.AddListener(SkipAll);
            _nextButton.GetComponentInChildren<TextMeshProUGUI>().text = "Завершить";
        }
        else
        {
            _nextButton.onClick.RemoveAllListeners();
            _nextButton.onClick.AddListener(NextSlide);
            _nextButton.GetComponentInChildren<TextMeshProUGUI>().text = "Далее";
        }

        // Обновляем счетчик
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
        // Удаляем все слайды
        foreach (var slide in _slides)
        {
            if (slide.spawnedInstance != null)
            {
                Destroy(slide.spawnedInstance);
            }
        }

        // Удаляем сам туториал
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Дополнительная очистка на случай, если объект был уничтожен без вызова SkipAll
        foreach (var slide in _slides)
        {
            if (slide.spawnedInstance != null)
            {
                Destroy(slide.spawnedInstance);
            }
        }
    }
}