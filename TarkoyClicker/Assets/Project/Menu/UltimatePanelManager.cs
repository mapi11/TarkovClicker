using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class UltimatePanelManager : MonoBehaviour
{
    public enum SlideDirection { LeftToRight, RightToLeft }

    [System.Serializable]
    public class PanelConfig
    {
        public Button button;
        public TMP_Text buttonText; // Изменено на TextMeshPro
        public GameObject panelPrefab;
        [HideInInspector] public string defaultText;
    }

    [Header("Main References")]
    [SerializeField] private RectTransform slidingWindow;
    [SerializeField] private RectTransform contentContainer;

    [Header("Slide Settings")]
    [SerializeField] private SlideDirection slideDirection = SlideDirection.RightToLeft;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private Ease slideEase = Ease.OutBack;
    [SerializeField] private List<PanelConfig> panels = new List<PanelConfig>();

    [Header("UI Settings")]
    [SerializeField] private string closeSymbol = "X";
    [SerializeField] private float contentSwitchEffectDuration = 0.3f;

    private Vector2 hiddenPosition;
    private Vector2 shownPosition;
    private GameObject currentPanel;
    private Tween currentTween;
    private bool isVisible;
    private int currentPanelIndex = -1;

    private void Awake()
    {
        InitializePanelSystem();
    }

    private void InitializePanelSystem()
    {
        CalculatePositions();
        StoreDefaultTexts();
        SetupButtonListeners();
    }

    private void CalculatePositions()
    {
        shownPosition = slidingWindow.anchoredPosition;
        float hiddenOffset = slidingWindow.rect.width;
        hiddenPosition = shownPosition + new Vector2(
            slideDirection == SlideDirection.RightToLeft ? hiddenOffset : -hiddenOffset,
            0);
        slidingWindow.anchoredPosition = hiddenPosition;
    }

    private void StoreDefaultTexts()
    {
        foreach (var panel in panels)
        {
            if (panel.buttonText != null)
            {
                panel.defaultText = panel.buttonText.text;
            }
        }
    }

    private void SetupButtonListeners()
    {
        for (int i = 0; i < panels.Count; i++)
        {
            int index = i;
            panels[i].button.onClick.AddListener(() => HandlePanelAction(index));
        }
    }

    private void HandlePanelAction(int panelIndex)
    {
        if (currentTween != null && currentTween.IsActive()) return;

        // Если клик по активной панели - закрываем
        if (isVisible && currentPanelIndex == panelIndex)
        {
            HidePanel();
            return;
        }

        // Если другая панель открыта - заменяем контент
        if (isVisible)
        {
            ReplaceContent(panelIndex);
            return;
        }

        // Если панель закрыта - открываем новую
        ShowPanel(panelIndex);
    }

    private void ShowPanel(int panelIndex)
    {
        currentPanelIndex = panelIndex;
        CreateContent(panelIndex);
        UpdateButtonText(panelIndex, closeSymbol);

        currentTween = slidingWindow.DOAnchorPos(shownPosition, slideDuration)
            .SetEase(slideEase)
            .OnStart(() => isVisible = true);
    }

    private void HidePanel()
    {
        currentTween = slidingWindow.DOAnchorPos(hiddenPosition, slideDuration)
            .SetEase(slideEase)
            .OnComplete(() => {
                isVisible = false;
                DestroyCurrentPanel();
                ResetAllButtonTexts();
                currentPanelIndex = -1;
            });
    }

    private void ReplaceContent(int panelIndex)
    {
        // Эффект переключения контента
        currentTween = contentContainer.DOShakeScale(contentSwitchEffectDuration, 0.1f)
            .OnComplete(() => {
                DestroyCurrentPanel();
                currentPanelIndex = panelIndex;
                CreateContent(panelIndex);
                UpdateButtonText(panelIndex, closeSymbol);
            });
    }

    private void CreateContent(int panelIndex)
    {
        currentPanel = Instantiate(panels[panelIndex].panelPrefab, contentContainer);
        currentPanel.name = panels[panelIndex].panelPrefab.name;
    }

    private void UpdateButtonText(int panelIndex, string text)
    {
        if (panelIndex < 0 || panelIndex >= panels.Count) return;

        // Сбрасываем все тексты
        ResetAllButtonTexts();

        // Устанавливаем новый текст для активной кнопки
        if (panels[panelIndex].buttonText != null)
        {
            panels[panelIndex].buttonText.text = text;
        }
    }

    private void ResetAllButtonTexts()
    {
        foreach (var panel in panels)
        {
            if (panel.buttonText != null)
            {
                panel.buttonText.text = panel.defaultText;
            }
        }
    }

    private void DestroyCurrentPanel()
    {
        if (currentPanel != null)
        {
            Destroy(currentPanel);
            currentPanel = null;
        }
    }

    private void OnDestroy()
    {
        foreach (var panel in panels)
        {
            if (panel.button != null)
            {
                panel.button.onClick.RemoveAllListeners();
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Автоматически находим TextMeshPro компонент если не установлен
        foreach (var panel in panels)
        {
            if (panel.button != null && panel.buttonText == null)
            {
                panel.buttonText = panel.button.GetComponentInChildren<TMP_Text>();
            }
        }
    }
#endif
}