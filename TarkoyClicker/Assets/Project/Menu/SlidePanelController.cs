using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class FixedPanelManager : MonoBehaviour
{
    [System.Serializable]
    public class PanelConfig
    {
        public Button button;
        public GameObject panelPrefab;
        [HideInInspector] public bool isActive;
    }

    [Header("Main References")]
    [SerializeField] private RectTransform slidingWindow;
    [SerializeField] private RectTransform contentContainer;

    [Header("Settings")]
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private Ease slideEase = Ease.OutBack;
    [SerializeField] private List<PanelConfig> panels = new List<PanelConfig>();

    private Vector2 hiddenPosition;
    private Vector2 shownPosition;
    private GameObject currentPanel;
    private Tween currentTween;

    private void Awake()
    {
        InitializePositions();
        SetupButtonListeners();
    }

    private void InitializePositions()
    {
        shownPosition = slidingWindow.anchoredPosition;
        hiddenPosition = shownPosition + new Vector2(slidingWindow.rect.width, 0);
        slidingWindow.anchoredPosition = hiddenPosition;
    }

    private void SetupButtonListeners()
    {
        for (int i = 0; i < panels.Count; i++)
        {
            int index = i;
            panels[i].button.onClick.AddListener(() => TogglePanel(index));
        }
    }

    private void TogglePanel(int panelIndex)
    {
        // Отменяем текущую анимацию
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }

        // Если нажата кнопка активной панели - закрываем
        if (panels[panelIndex].isActive)
        {
            HideWindow();
            panels[panelIndex].isActive = false;
            return;
        }

        // Деактивируем все панели
        foreach (var panel in panels)
        {
            panel.isActive = false;
        }

        // Активируем выбранную панель
        panels[panelIndex].isActive = true;

        // Заменяем контент
        ReplaceContent(panelIndex);

        // Анимируем окно
        if (slidingWindow.anchoredPosition == hiddenPosition)
        {
            ShowWindow();
        }
        else
        {
            // Если окно открыто - сначала закрываем, потом открываем
            currentTween = slidingWindow.DOAnchorPos(hiddenPosition, slideDuration / 2)
                .SetEase(slideEase)
                .OnComplete(() => {
                    ReplaceContent(panelIndex);
                    ShowWindow();
                });
        }
    }

    private void ReplaceContent(int panelIndex)
    {
        DestroyCurrentPanel();
        currentPanel = Instantiate(panels[panelIndex].panelPrefab, contentContainer);
        currentPanel.name = panels[panelIndex].panelPrefab.name;
    }

    private void ShowWindow()
    {
        currentTween = slidingWindow.DOAnchorPos(shownPosition, slideDuration)
            .SetEase(slideEase);
    }

    private void HideWindow()
    {
        currentTween = slidingWindow.DOAnchorPos(hiddenPosition, slideDuration)
            .SetEase(slideEase)
            .OnComplete(DestroyCurrentPanel);
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
                panel.button.onClick.RemoveAllListeners();
        }
    }
}