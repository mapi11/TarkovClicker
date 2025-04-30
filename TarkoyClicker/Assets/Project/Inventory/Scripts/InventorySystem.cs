using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.IO;

public class InventorySystem : MonoBehaviour
{
    [System.Serializable]
    public class InventoryTable
    {
        public string tableId;
        public Transform slotContainer;
        public List<InventorySlot> slots = new List<InventorySlot>();
        public int slotCount = 10;
        public GameObject slotPrefab;
    }

    [System.Serializable]
    public class SlotSaveData
    {
        public string itemId;
        public int itemCount;
        public int slotIndex;
        public string tableId;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> Items;
    }

    [Header("Inventory Settings")]
    public List<InventoryTable> tables = new List<InventoryTable>();
    public List<ItemData> itemDatabase = new List<ItemData>();

    [Header("Drag & Drop Settings")]
    public GameObject dragItemCanvas; // Перетаскиваемый Canvas (должен быть выше основного UI)
    public GameObject dragItemPrefab;

    [Space]
    public GameObject tooltipPrefab;

    [Header("Save Settings")]
    public bool loadOnStart = true;
    public string saveFileName = "inventorySave.json";

    private GameObject currentDragItem;
    private InventorySlot sourceSlot;
    private GameObject currentTooltip;

    void Start()
    {
        InitializeAllTables();

        if (loadOnStart)
        {
            LoadInventory();
        }

        dragItemCanvas = GameObject.Find("MainCanvas");
    }

    void InitializeAllTables()
    {
        foreach (var table in tables)
        {
            InitializeTable(table);
        }
    }

    public void InitializeTable(InventoryTable table)
    {
        // Очистка старых слотов
        foreach (var slot in table.slots)
        {
            if (slot != null && slot.gameObject != null)
                Destroy(slot.gameObject);
        }
        table.slots.Clear();

        // Создание новых слотов
        for (int i = 0; i < table.slotCount; i++)
        {
            CreateNewSlot(table, i);
        }
    }

    private void CreateNewSlot(InventoryTable table, int index)
    {
        GameObject slotObj = Instantiate(table.slotPrefab, table.slotContainer);
        InventorySlot slot = slotObj.GetComponent<InventorySlot>();
        slot.Initialize(this, table.tableId, index);
        table.slots.Add(slot);
    }

    public void CreateNewTable(string tableId, Transform container, int slotCount, GameObject slotPrefab)
    {
        InventoryTable newTable = new InventoryTable
        {
            tableId = tableId,
            slotContainer = container,
            slotCount = slotCount,
            slotPrefab = slotPrefab
        };

        tables.Add(newTable);
        InitializeTable(newTable);
    }

    public void RemoveTable(string tableId)
    {
        InventoryTable tableToRemove = tables.Find(t => t.tableId == tableId);
        if (tableToRemove != null)
        {
            foreach (var slot in tableToRemove.slots)
            {
                if (slot != null && slot.gameObject != null)
                    Destroy(slot.gameObject);
            }
            tables.Remove(tableToRemove);
        }
    }

    // === Сохранение/Загрузка ===
    public void SaveInventoryButton()
    {
        SaveInventory();
        Debug.Log("Inventory saved by button!");
    }

    public void SaveInventory()
    {
        foreach (var table in tables)
        {
            foreach (var slot in table.slots)
            {
                string slotKey = $"{table.tableId}_{slot.GetSlotIndex()}";

                if (slot.GetItem() != null)
                {
                    PlayerPrefs.SetString($"{slotKey}_itemId", slot.GetItem().id);
                    PlayerPrefs.SetInt($"{slotKey}_itemCount", slot.GetCount());
                }
                else
                {
                    PlayerPrefs.DeleteKey($"{slotKey}_itemId");
                    PlayerPrefs.DeleteKey($"{slotKey}_itemCount");
                }
            }
        }
        PlayerPrefs.Save(); // Важно сохранить изменения
    }


    public void LoadInventory()
    {
        foreach (var table in tables)
        {
            foreach (var slot in table.slots)
            {
                string slotKey = $"{table.tableId}_{slot.GetSlotIndex()}";
                string itemId = PlayerPrefs.GetString($"{slotKey}_itemId", "");

                if (!string.IsNullOrEmpty(itemId))
                {
                    ItemData item = itemDatabase.Find(i => i.id == itemId);
                    if (item != null)
                    {
                        int count = PlayerPrefs.GetInt($"{slotKey}_itemCount", 1);
                        slot.SetItem(item, count);
                    }
                }
                else
                {
                    slot.ClearSlot();
                }
            }
        }
    }

    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, saveFileName);
    }

    // === Drag & Drop ===
    public void StartDragItem(InventorySlot slot, PointerEventData eventData)
    {
        if (slot.GetItem() == null) return;

        sourceSlot = slot;

        // Спавним DragItem в нужном Canvas
        currentDragItem = Instantiate(dragItemPrefab, dragItemCanvas.transform);

        currentDragItem.GetComponent<Image>().sprite = slot.GetItem().icon;
        currentDragItem.transform.position = eventData.position;
        currentDragItem.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void DragItem(PointerEventData eventData)
    {
        if (currentDragItem != null)
        {
            currentDragItem.transform.position = eventData.position;
        }
    }

    public void EndDragItem(PointerEventData eventData)
    {
        if (currentDragItem == null) return;

        InventorySlot targetSlot = GetTargetSlot(eventData);

        if (targetSlot != null)
        {
            // Если перетаскиваем на слот с тем же предметом и есть стакаемость
            if (targetSlot.GetItem() != null &&
                sourceSlot.GetItem() != null &&
                targetSlot.GetItem().id == sourceSlot.GetItem().id &&
                targetSlot.GetItem().maxStack > 1)
            {
                // Вычисляем сколько можно добавить
                int availableSpace = targetSlot.GetItem().maxStack - targetSlot.GetCount();
                int transferAmount = Mathf.Min(availableSpace, sourceSlot.GetCount());

                if (transferAmount > 0)
                {
                    // Объединяем стаки
                    targetSlot.SetItem(targetSlot.GetItem(), targetSlot.GetCount() + transferAmount);

                    // Обновляем исходный слотА
                    int remaining = sourceSlot.GetCount() - transferAmount;
                    if (remaining > 0)
                    {
                        sourceSlot.SetItem(sourceSlot.GetItem(), remaining);
                    }
                    else
                    {
                        sourceSlot.ClearSlot();
                    }
                }
                else
                {
                    // Если нельзя объединить - просто меняем местами
                    SwapItems(sourceSlot, targetSlot);
                }
            }
            else
            {
                // Если предметы разные или не стакаются - обычный обмен
                SwapItems(sourceSlot, targetSlot);
            }
        }

        Destroy(currentDragItem);
        currentDragItem = null;
        sourceSlot = null;

        // Сохраняем изменения
        SaveInventory();
    }

    private InventorySlot GetTargetSlot(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            InventorySlot slot = result.gameObject.GetComponent<InventorySlot>();
            if (slot != null) return slot;
        }
        return null;
    }

    private void SwapItems(InventorySlot source, InventorySlot target)
    {
        ItemData sourceItem = source.GetItem();
        int sourceCount = source.GetCount();
        ItemData targetItem = target.GetItem();
        int targetCount = target.GetCount();

        source.SetItem(targetItem, targetCount);
        target.SetItem(sourceItem, sourceCount);
    }

    // === Вспомогательные методы ===
    public void ShowTooltip(ItemData item, Vector2 position)
    {
        if (tooltipPrefab == null) return;

        if (currentTooltip == null)
        {
            currentTooltip = Instantiate(tooltipPrefab, transform);
        }

        currentTooltip.transform.position = position + new Vector2(0, 50);
        currentTooltip.GetComponentInChildren<TextMeshProUGUI>().text =
            $"<b>{item.displayName}</b>\n{item.description}";
        currentTooltip.SetActive(true);
    }

    public void HideTooltip()
    {
        if (currentTooltip != null)
        {
            currentTooltip.SetActive(false);
        }
    }

    public bool HasEmptySlots(string tableId)
    {
        InventoryTable table = GetTable(tableId);
        if (table == null) return false;

        foreach (InventorySlot slot in table.slots)
        {
            if (slot.GetItem() == null) return true;
        }
        return false;
    }

    public void AddItemToTable(string tableId, string itemId, int count = 1)
    {
        InventoryTable table = GetTable(tableId);
        if (table == null) return;

        ItemData item = itemDatabase.Find(i => i.id == itemId);
        if (item == null) return;

        // Сначала пробуем добавить к существующим неполным стакам
        foreach (var slot in table.slots)
        {
            if (slot.GetItem() != null &&
                slot.GetItem().id == itemId &&
                item.maxStack > 1 &&
                slot.GetCount() < item.maxStack)
            {
                int canAdd = item.maxStack - slot.GetCount();
                int willAdd = Mathf.Min(canAdd, count);

                slot.SetItem(item, slot.GetCount() + willAdd);
                count -= willAdd;

                if (count <= 0) return; // Все предметы добавлены
            }
        }

        // Если остались предметы - ищем пустые слоты
        while (count > 0)
        {
            bool added = false;

            foreach (var slot in table.slots)
            {
                if (slot.GetItem() == null)
                {
                    int willAdd = Mathf.Min(item.maxStack, count);
                    slot.SetItem(item, willAdd);
                    count -= willAdd;
                    added = true;

                    if (count <= 0) return;
                    break;
                }
            }

            if (!added)
            {
                Debug.LogWarning($"Не удалось добавить {count} предметов {itemId} - инвентарь полон!");
                return;
            }
        }
    }

    public InventoryTable GetTable(string tableId)
    {
        return tables.Find(t => t.tableId == tableId);
    }

    public void DeleteSave()
    {
        foreach (var table in tables)
        {
            for (int i = 0; i < table.slotCount; i++)
            {
                string slotKey = $"{table.tableId}_{i}";
                PlayerPrefs.DeleteKey($"{slotKey}_itemId");
                PlayerPrefs.DeleteKey($"{slotKey}_itemCount");
            }
        }
        PlayerPrefs.Save();
        ClearAllInventory();
    }

    private void ClearAllInventory()
    {
        foreach (var table in tables)
        {
            foreach (var slot in table.slots)
            {
                slot.ClearSlot();
            }
        }
    }
}