using UnityEngine;
using System.Collections.Generic;

public class StartItemsLoader : MonoBehaviour
{
    [System.Serializable]
    public class StartItem
    {
        public string itemId;
        public int count = 1;
    }

    [Header("Настройки")]
    public string targetInventory = "Player_Inventory"; // ID инвентаря
    public List<StartItem> itemsToAdd = new List<StartItem>();

    void Start()
    {
        // Получаем систему инвентаря
        InventorySystem inventory = FindObjectOfType<InventorySystem>();

        if (inventory == null)
        {
            Debug.LogError("InventorySystem не найден на сцене!");
            return;
        }

        // Добавляем все стартовые предметы
        foreach (StartItem item in itemsToAdd)
        {
            inventory.AddItemToTable(targetInventory, item.itemId, item.count);
        }
    }
}