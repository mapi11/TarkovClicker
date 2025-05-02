using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ScavSystem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI txtScav;
    [SerializeField] private TextMeshProUGUI txtScavTimer;
    [SerializeField] private Button btnScav;
    [SerializeField] private Button btnScavAdd;
    [SerializeField] private Slider chanceSlider;
    [SerializeField] private TextMeshProUGUI txtChanceValue;
    [SerializeField] private TextMeshProUGUI txtCost;

    [Header("Mission Settings")]
    [SerializeField] private Vector2 scavMissionTimeRange = new Vector2(90f, 150f);
    [SerializeField] private Vector2 scavCooldownTimeRange = new Vector2(180f, 300f);
    [SerializeField] private int baseCostPerChancePoint = 10; // Базовая стоимость
    [SerializeField] private float costIncreasePerLevel = 1.05f;

    [Header("Loot Settings")]
    [SerializeField] private Vector2Int itemsCountRange = new Vector2Int(1, 3);
    [SerializeField] private Vector2Int itemTypesRange = new Vector2Int(1, 3);
    [SerializeField] private List<ItemData> possibleItems = new List<ItemData>();

    [Header("Inventory Settings")]
    [SerializeField] private string targetInventoryId = "Inventory";
    [SerializeField] private int requiredEmptySlots = 1; // Количество необходимых пустых слотов


    private bool isOnMission = false;
    private bool isOnCooldown = false;
    private float currentTimer = 0f;
    private int currentChance = 50;
    private PassivePoints pointsSystem;
    PerksSystem perksSystem;

    private void Awake()
    {
        btnScav.onClick.AddListener(SendScav);
        btnScavAdd.onClick.AddListener(ScavAddWatch);
        pointsSystem = FindObjectOfType<PassivePoints>();
        perksSystem = FindObjectOfType<PerksSystem>();

        chanceSlider.onValueChanged.AddListener(UpdateChanceSettings);
        chanceSlider.minValue = 1;
        chanceSlider.maxValue = 100;
        chanceSlider.value = currentChance;

        UpdateUI();
        LoadState();
        UpdateCostDisplay();
    }

    private void UpdateChanceSettings(float value)
    {
        currentChance = Mathf.RoundToInt(value);
        txtChanceValue.text = $"{currentChance}%";
        UpdateCostDisplay();
    }


    private void UpdateCostDisplay()
    {
        int totalCost = currentChance * CostPerChancePoint; // Используем свойство вместо поля
        txtCost.text = $"Cost: {totalCost}";
    }

    private void Update()
    {
        if (isOnMission || isOnCooldown)
        {
            currentTimer -= Time.deltaTime;
            UpdateTimerDisplay();

            if (currentTimer <= 0)
            {
                if (isOnMission) ScavBack();
                else if (isOnCooldown)
                {
                    isOnCooldown = false;
                    UpdateUI();
                }
            }
        }
    }

    private void SendScav()
    {
        if (isOnMission || isOnCooldown) return;

        // Проверка заполненности инвентаря
        var inventory = InventorySceneSystem.Instance?.GetInventory();
        if (inventory != null && !inventory.HasEmptySlots(targetInventoryId))
        {
            txtScav.text = "Нет места в инвентаре, чтобы отправить дикого";
            btnScav.interactable = false;
            return;
        }

        int totalCost = currentChance * CostPerChancePoint; // Используем свойство

        if (pointsSystem == null || pointsSystem.Points < totalCost)
        {
            Debug.Log("Not enough points!");
            return;
        }

        pointsSystem.AddPoints(-totalCost);
        isOnMission = true;
        currentTimer = Random.Range(scavMissionTimeRange.x, scavMissionTimeRange.y);
        UpdateUI();
        SaveState();
    }

    private int CostPerChancePoint
    {
        get
        {
            if (perksSystem == null) return baseCostPerChancePoint;

            // Рассчитываем стоимость с учетом уровня персонажа
            float cost = baseCostPerChancePoint * Mathf.Pow(costIncreasePerLevel, perksSystem.GetCharacterLevel());
            return Mathf.RoundToInt(cost);
        }
    }

    private void ScavBack()
    {
        isOnMission = false;
        isOnCooldown = true;
        currentTimer = Random.Range(scavCooldownTimeRange.x, scavCooldownTimeRange.y);

        if (Random.Range(0, 100) < currentChance)
        {
            GenerateRandomLoot();
        }
        else
        {
            Debug.Log("Scav returned empty-handed!");
        }

        UpdateUI();
        SaveState();
    }

    private void GenerateRandomLoot()
    {
        if (possibleItems.Count == 0) return;

        // Получаем инвентарь через InventorySceneSystem
        var inventory = InventorySceneSystem.Instance.GetInventory();
        if (inventory == null)
        {
            Debug.LogError("Inventory system not available!");
            return;
        }

        // Очищаем только временные данные перед генерацией
        inventory.ForceUpdateInventory();

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

            // Добавляем предмет напрямую в инвентарь
            inventory.AddItemToTable(targetInventoryId, selectedType.id, amount);
        }

        // Принудительно обновляем инвентарь после добавления
        if (inventory.gameObject.activeSelf)
        {
            inventory.ForceUpdateInventory();
        }
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTimer / 60f);
        int seconds = Mathf.FloorToInt(currentTimer % 60f);
        txtScavTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void UpdateUI()
    {
        if (isOnMission)
        {
            txtScav.text = "Scav returning in:";
            btnScav.interactable = false;
            btnScavAdd.interactable = false;
        }
        else if (isOnCooldown)
        {
            txtScav.text = "Scav resting:";
            btnScav.interactable = false;
            btnScavAdd.interactable = false;
        }
        else
        {
            // Дополнительная проверка при обновлении UI
            var inventory = InventorySceneSystem.Instance?.GetInventory();
            if (inventory != null && !inventory.HasEmptySlots(targetInventoryId))
            {
                txtScav.text = "Нет места в инвентаре, чтобы отправить дикого";
                btnScav.interactable = false;
            }
            else
            {
                txtScav.text = "Send Scav";
                btnScav.interactable = true;
            }

            txtScavTimer.text = "";
            btnScavAdd.interactable = true;
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