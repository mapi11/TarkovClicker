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
    [SerializeField] private Slider chanceSlider;
    [SerializeField] private TextMeshProUGUI txtChanceValue;
    [SerializeField] private TextMeshProUGUI txtCost;

    [Header("Mission Settings")]
    [SerializeField] private Vector2 scavMissionTimeRange = new Vector2(90f, 150f);
    [SerializeField] private Vector2 scavCooldownTimeRange = new Vector2(180f, 300f);
    [SerializeField] private int costPerChancePoint = 10;
    
    [Header("Loot Settings")]
    [SerializeField] private Vector2Int itemsCountRange = new Vector2Int(1, 3);
    [SerializeField] private Vector2Int itemTypesRange = new Vector2Int(1, 3);
    [SerializeField] private List<ItemData> possibleItems = new List<ItemData>();

    [Header("Inventory Settings")]
    [SerializeField] private string targetInventoryId = "Inventory";

    private bool isOnMission = false;
    private bool isOnCooldown = false;
    private float currentTimer = 0f;
    private int currentChance = 0; // Стартовое значение шанса
    private InventorySystem inventory;
    private PassivePoints pointsSystem;
    private List<ItemReward> pendingRewards = new List<ItemReward>();

    [System.Serializable]
    private class ItemReward
    {
        public string itemId;
        public int amount;
        public string saveKey;
    }

    private void Awake()
    {
        btnScav.onClick.AddListener(SendScav);
        pointsSystem = FindObjectOfType<PassivePoints>();

        UpdateCostDisplay();

        // Настройка слайдера
        chanceSlider.onValueChanged.AddListener(UpdateChanceSettings);
        chanceSlider.minValue = 1;
        chanceSlider.maxValue = 100;
        chanceSlider.value = currentChance;

        UpdateUI();
        LoadPendingRewards();
        LoadState();
    }

    private void UpdateChanceSettings(float value)
    {
        currentChance = Mathf.RoundToInt(value);
        txtChanceValue.text = $"{currentChance}%";
        UpdateCostDisplay();
    }

    private void UpdateCostDisplay()
    {
        int totalCost = currentChance * costPerChancePoint;
        txtCost.text = $"Цена: {totalCost}";
        txtCost.color = (pointsSystem != null && totalCost <= pointsSystem.Points) ? Color.green : Color.red;
    }

    private void Update()
    {
        if (isOnMission || isOnCooldown)
        {
            currentTimer -= Time.deltaTime;
            UpdateTimerDisplay();

            if (currentTimer <= 0)
            {
                if (isOnMission)
                {
                    ScavBack();
                }
                else if (isOnCooldown)
                {
                    isOnCooldown = false;
                    UpdateUI();
                }
            }
        }

        if (pendingRewards.Count > 0)
        {
            TryDeliverPendingRewards();
        }
    }

    private void SendScav()
    {
        if (isOnMission || isOnCooldown) return;

        int totalCost = currentChance * costPerChancePoint;
        
        // Проверка наличия средств
        if (pointsSystem == null || pointsSystem.Points < totalCost)
        {
            Debug.Log("Not enough points!");
            return;
        }

        // Оплата
        pointsSystem.AddPoints(-totalCost);

        isOnMission = true;
        currentTimer = Random.Range(scavMissionTimeRange.x, scavMissionTimeRange.y);
        txtScav.text = "Дикий вернётся через:";
        btnScav.interactable = false;
        SaveState();
    }

    private void ScavBack()
    {
        isOnMission = false;
        isOnCooldown = true;
        currentTimer = Random.Range(scavCooldownTimeRange.x, scavCooldownTimeRange.y);

        // Проверка шанса на получение добычи
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
            SaveReward(selectedType.id, amount);
        }
    }

    private void TryDeliverPendingRewards()
    {
        if (inventory == null)
        {
            inventory = FindObjectOfType<InventorySystem>();
            if (inventory == null) return;
        }

        for (int i = pendingRewards.Count - 1; i >= 0; i--)
        {
            ItemReward reward = pendingRewards[i];
            inventory.AddItemToTable(targetInventoryId, reward.itemId, reward.amount);
            ClearReward(reward);
            pendingRewards.RemoveAt(i);
        }
    }

    private void SaveReward(string itemId, int amount)
    {
        int rewardCount = PlayerPrefs.GetInt("scav_rewards_count", 0);
        string key = $"scav_reward_{rewardCount}";
        
        PlayerPrefs.SetString(key, $"{itemId},{amount}");
        PlayerPrefs.SetInt("scav_rewards_count", rewardCount + 1);
        
        pendingRewards.Add(new ItemReward {
            itemId = itemId,
            amount = amount,
            saveKey = key
        });
        
        PlayerPrefs.Save();
    }

    private void ClearReward(ItemReward reward)
    {
        if (!string.IsNullOrEmpty(reward.saveKey))
        {
            PlayerPrefs.DeleteKey(reward.saveKey);
        }
        PlayerPrefs.Save();
    }

    public void LoadPendingRewards()
    {
        int rewardCount = PlayerPrefs.GetInt("scav_rewards_count", 0);
        
        for (int i = 0; i < rewardCount; i++)
        {
            string key = $"scav_reward_{i}";
            if (PlayerPrefs.HasKey(key))
            {
                string rewardData = PlayerPrefs.GetString(key);
                string[] parts = rewardData.Split(',');

                if (parts.Length == 2 && int.TryParse(parts[1], out int amount))
                {
                    pendingRewards.Add(new ItemReward {
                        itemId = parts[0],
                        amount = amount,
                        saveKey = key
                    });
                }
            }
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
            txtScav.text = "Дикий вернётся через:";
            btnScav.interactable = false;
        }
        else if (isOnCooldown)
        {
            txtScav.text = "Дикий отдыхает:";
            btnScav.interactable = false;
        }
        else
        {
            txtScav.text = "Отправить дикого";
            txtScavTimer.text = "";
            btnScav.interactable = true;
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

        if (isOnMission || isOnCooldown)
        {
            UpdateUI();
        }
    }

    public void AddTestItem(ItemData item)
    {
        if (!possibleItems.Contains(item))
        {
            possibleItems.Add(item);
        }
    }

    public void ClearAllData()
    {
        PlayerPrefs.DeleteKey("scav_isOnMission");
        PlayerPrefs.DeleteKey("scav_isOnCooldown");
        PlayerPrefs.DeleteKey("scav_currentTimer");
        PlayerPrefs.DeleteKey("scav_currentChance");
        
        int rewardCount = PlayerPrefs.GetInt("scav_rewards_count", 0);
        for (int i = 0; i < rewardCount; i++)
        {
            PlayerPrefs.DeleteKey($"scav_reward_{i}");
        }
        PlayerPrefs.DeleteKey("scav_rewards_count");
        
        PlayerPrefs.Save();
        pendingRewards.Clear();
        
        isOnMission = false;
        isOnCooldown = false;
        currentTimer = 0f;
        currentChance = 50;
        UpdateUI();
        UpdateCostDisplay();
    }
}