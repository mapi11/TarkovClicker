using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using YG;

public class ScavSystem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI txtScav;
    [SerializeField] private TextMeshProUGUI txtScavTimer;
    [SerializeField] private Button btnScav;
    [SerializeField] private Button btnScavAdd;
    [SerializeField] private Button btnSkipCooldown;
    [SerializeField] private Button btnSkipMissionTime;
    [SerializeField] private Button btnScavUpdate;
    [SerializeField] private Slider chanceSlider;
    [SerializeField] private TextMeshProUGUI txtChanceValue;
    [SerializeField] private TextMeshProUGUI txtCost;

    [Header("Mission Settings")]
    [SerializeField] private Vector2 scavMissionTimeRange = new Vector2(90f, 150f);
    [SerializeField] private Vector2 scavCooldownTimeRange = new Vector2(180f, 300f);
    [SerializeField] private int baseCostPerChancePoint = 10;
    [SerializeField] private float costMultiplierPerLevel = 1.2f;
    [SerializeField] private int skipCooldownCost = 100;

    [Header("Loot Settings")]
    [SerializeField] private Vector2Int itemsCountRange = new Vector2Int(1, 3);
    [SerializeField] private Vector2Int itemTypesRange = new Vector2Int(1, 3);
    [SerializeField] private List<ItemData> possibleItems = new List<ItemData>();

    [Header("Inventory Settings")]
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private string targetInventoryId = "Inventory";
    [SerializeField] private int requiredEmptySlots = 1;

    [Header("Inventory Settings")]
    public YandexGame yandexGame;
    private const int REWARD_AD_ID_ADD_LOOT = 1;
    private const int REWARD_AD_ID_SKIP_COOLDOWN = 2;
    private const int REWARD_AD_ID_SKIP_MISSION_TIME = 3;

    [SerializeField] private UltimateOpenPanelManager panelManager;

    private Coroutine closePanelCoroutine;
    private bool isOnMission = false;
    private bool isOnCooldown = false;
    private float currentTimer = 0f;
    private int currentChance = 50;
    private PassivePoints pointsSystem;
    private PerksSystem perksSystem;

    private void Awake()
    {
        btnScav.onClick.AddListener(SendScav);
        btnScavAdd.onClick.AddListener(ScavAddReward);
        btnSkipCooldown.onClick.AddListener(SkipCooldown);
        btnSkipMissionTime.onClick.AddListener(SkipMissionTime);
        btnScavUpdate.onClick.AddListener(CheckInventoryAndReturn);
        pointsSystem = FindObjectOfType<PassivePoints>();
        perksSystem = FindObjectOfType<PerksSystem>(); // Инициализация системы перков


        perksSystem = FindObjectOfType<PerksSystem>();
        if (perksSystem != null)
        {
            perksSystem.OnLevelUp += HandleLevelUp; // Подписка на событие
        }

        LoadState();

        chanceSlider.onValueChanged.AddListener(UpdateChanceSettings);
        chanceSlider.minValue = 1;
        chanceSlider.maxValue = 100;
        chanceSlider.value = currentChance;

        UpdateUI();
        YandexGame.RewardVideoEvent += OnRewardedAdSuccess;
    }

    private void Start()
    {
        // Рассчитываем базовую стоимость с учетом уровня при старте
        if (perksSystem != null)
        {
            baseCostPerChancePoint = Mathf.RoundToInt(baseCostPerChancePoint *
                                    Mathf.Pow(costMultiplierPerLevel, perksSystem._characterLevel - 1));
        }
        UpdateCostDisplay();
    }

    private void UpdateCostDisplay()
    {
        int totalCost = currentChance * baseCostPerChancePoint;
        txtCost.text = $"{totalCost} ";
    }

    private void OnApplicationQuit()
    {
        SaveState();
    }

    private void OnDisable()
    {
        SaveState();
    }

    private void Update()
    {
        if (isOnMission)
        {
            currentTimer -= Time.deltaTime;
            UpdateTimerDisplay();

            if (currentTimer <= 0)
            {
                if (inventorySystem != null && inventorySystem.HasEmptySlots(targetInventoryId))
                {
                    ScavBack();
                }
                else
                {
                    UpdateUI();
                }
            }
        }
        else if (isOnCooldown)
        {
            currentTimer -= Time.deltaTime;
            UpdateTimerDisplay();

            if (currentTimer <= 0)
            {
                isOnCooldown = false;
                UpdateUI();
            }
        }
    }

    private void UpdateChanceSettings(float value)
    {
        currentChance = Mathf.RoundToInt(value);
        txtChanceValue.text = $"{currentChance}%";
        UpdateCostDisplay();
    }

    private void SendScav()
    {
        if (isOnMission || isOnCooldown) return;

        int totalCost = currentChance * baseCostPerChancePoint;

        if (pointsSystem == null || pointsSystem.Points < totalCost)
        {
            Debug.Log("Not enough points!");
            return;
        }

        pointsSystem.AddPoints(-totalCost);
        isOnMission = true;
        currentTimer = Random.Range(scavMissionTimeRange.x, scavMissionTimeRange.y);

        if (panelManager != null)
        {
            float closeDelay = Mathf.Max(currentTimer - 2f, 0.1f);
            closePanelCoroutine = StartCoroutine(ClosePanelWithDelay(closeDelay));
        }

        UpdateUI();
        SaveState();
    }

    private IEnumerator ClosePanelWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        panelManager.HidePanel();
    }

    private void ScavBack()
    {
        btnScavUpdate.gameObject.SetActive(false);

        if (inventorySystem == null || !inventorySystem.HasEmptySlots(targetInventoryId))
        {
            Debug.Log("Not enough space in inventory!");
            return;
        }

        if (closePanelCoroutine != null)
        {
            StopCoroutine(closePanelCoroutine);
        }

        isOnMission = false;
        isOnCooldown = true;
        currentTimer = Random.Range(scavCooldownTimeRange.x, scavCooldownTimeRange.y);

        if (Random.Range(0, 100) < currentChance)
        {
            GenerateRandomLoot();
        }
        else
        {
            Debug.Log("Scavenger returned empty-handed");
        }

        UpdateUI();
        SaveState();
    }

    private void GenerateRandomLoot()
    {
        if (possibleItems.Count == 0 || inventorySystem == null) return;

        int itemTypes = Mathf.Clamp(
            Random.Range(itemTypesRange.x, itemTypesRange.y + 1),
            1,
            possibleItems.Count
        );

        int totalItems = Random.Range(itemsCountRange.x, itemsCountRange.y + 1);

        List<ItemData> selectedTypes = new List<ItemData>();
        List<ItemData> availableItems = new List<ItemData>(possibleItems);

        for (int i = 0; i < itemTypes && availableItems.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableItems.Count);
            selectedTypes.Add(availableItems[randomIndex]);
            availableItems.RemoveAt(randomIndex);
        }

        for (int i = 0; i < totalItems; i++)
        {
            ItemData selectedType = selectedTypes[Random.Range(0, selectedTypes.Count)];
            int amount = Random.Range(1, selectedType.maxStack + 1);
            inventorySystem.AddItemToTable(targetInventoryId, selectedType.id, amount);
        }

        inventorySystem.ForceUpdateInventory();
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTimer / 60f);
        int seconds = Mathf.FloorToInt(currentTimer % 60f);
        txtScavTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void SkipCooldown()
    {
        if (!isOnCooldown) return;

        YandexGame.RewVideoShow(REWARD_AD_ID_SKIP_COOLDOWN);
    }
    private void SkipMissionTime()
    {
        if (!isOnMission) return;
        YandexGame.RewVideoShow(REWARD_AD_ID_SKIP_MISSION_TIME);
    }

    private void UpdateUI()
    {
        if (isOnMission)
        {
            if (currentTimer <= 0)
            {
                bool hasSpace = inventorySystem != null && inventorySystem.HasEmptySlots(targetInventoryId);

                btnScavUpdate.gameObject.SetActive(!hasSpace);
                txtScav.text = hasSpace ? "Дикий возвращается..." : "Недостаточно места в инвентаре";
                btnSkipMissionTime.gameObject.SetActive(false); // Скрыть кнопку
            }
            else
            {
                txtScav.text = "Дикий верёнтся через:";
                btnScavUpdate.gameObject.SetActive(false);
                btnSkipMissionTime.gameObject.SetActive(true); // Показать кнопку
            }

            btnScav.interactable = false;
            btnScavAdd.interactable = false;
            btnSkipCooldown.gameObject.SetActive(false);
        }
        else if (isOnCooldown)
        {
            txtScav.text = "Дикий отдыхает:";
            btnScav.interactable = false;
            btnScavAdd.interactable = false;
            btnScavUpdate.gameObject.SetActive(false);
            btnSkipCooldown.gameObject.SetActive(true);
            btnSkipMissionTime.gameObject.SetActive(false);
        }
        else
        {
            txtScav.text = "Отправить дикого";
            btnScav.interactable = true;
            btnScavAdd.interactable = true;
            txtScavTimer.text = "";
            btnScavUpdate.gameObject.SetActive(false);
            btnSkipCooldown.gameObject.SetActive(false);
            btnSkipMissionTime.gameObject.SetActive(false);
        }
    }

    private void CheckInventoryAndReturn()
    {
        if (inventorySystem != null && inventorySystem.HasEmptySlots(targetInventoryId))
        {
            ScavBack();
        }
        else
        {
            Debug.Log("Still no space available!");
        }
    }

    private void SaveState()
    {
        PlayerPrefs.SetInt("scav_isOnMission", isOnMission ? 1 : 0);
        PlayerPrefs.SetInt("scav_isOnCooldown", isOnCooldown ? 1 : 0);
        PlayerPrefs.SetFloat("scav_currentTimer", currentTimer);
        PlayerPrefs.SetInt("scav_currentChance", currentChance);
        PlayerPrefs.Save();
    }

    private void LoadState()
    {
        isOnMission = PlayerPrefs.GetInt("scav_isOnMission", 0) == 1;
        isOnCooldown = PlayerPrefs.GetInt("scav_isOnCooldown", 0) == 1;
        currentTimer = PlayerPrefs.GetFloat("scav_currentTimer", 0f);
        currentChance = PlayerPrefs.GetInt("scav_currentChance", 50);

        if (chanceSlider != null)
        {
            chanceSlider.value = currentChance;
        }

        UpdateUI();
    }

    public void ScavAddWatch()
    {
        if (isOnMission || isOnCooldown) return;

        int originalChance = currentChance;
        currentChance = 100;
        ScavBack();
        currentChance = originalChance;
    }
    public void ScavAddReward()
    {
        YandexGame.RewVideoShow(REWARD_AD_ID_ADD_LOOT);
    }
    private void OnRewardedAdSuccess(int rewardId)
    {
        switch (rewardId)
        {
            case REWARD_AD_ID_ADD_LOOT:
                ScavAddWatch();
                break;

            case REWARD_AD_ID_SKIP_COOLDOWN:
                // Пропускаем кулдаун только после полного просмотра
                currentTimer = 0f;
                isOnCooldown = false;
                UpdateUI();
                SaveState();
                break;
            case REWARD_AD_ID_SKIP_MISSION_TIME:
                if (isOnMission)
                {
                    currentTimer = 0f;
                    UpdateUI();
                    SaveState();
                }
                break;

            default:
                Debug.LogWarning($"Unknown reward ID: {rewardId}");
                break;
        }
    }

    private void OnDestroy()
    {
        if (perksSystem != null)
        {
            perksSystem.OnLevelUp -= HandleLevelUp; // Отписка при уничтожении
        }
        YandexGame.RewardVideoEvent -= OnRewardedAdSuccess;
    }

    private void HandleLevelUp()
    {
        baseCostPerChancePoint = Mathf.RoundToInt(baseCostPerChancePoint * costMultiplierPerLevel);
        UpdateCostDisplay(); // Обновление текста стоимости
    }

    public void ClearAllData()
    {
        PlayerPrefs.DeleteKey("scav_isOnMission");
        PlayerPrefs.DeleteKey("scav_isOnCooldown");
        PlayerPrefs.DeleteKey("scav_currentTimer");
        PlayerPrefs.DeleteKey("scav_currentChance");
        PlayerPrefs.Save();

        isOnMission = false;
        isOnCooldown = false;
        currentTimer = 0f;
        currentChance = 50;
        UpdateUI();
        UpdateCostDisplay();
    }
}