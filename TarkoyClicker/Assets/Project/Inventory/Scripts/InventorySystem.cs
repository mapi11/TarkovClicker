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

    [Header("Settings")]
    public List<InventoryTable> tables = new List<InventoryTable>();
    public List<ItemData> itemDatabase = new List<ItemData>();
    public GameObject dragItemPrefab;
    public GameObject tooltipPrefab;

    private GameObject currentDragItem;
    private InventorySlot sourceSlot;
    private GameObject currentTooltip;

    void Start()
    {
        InitializeAllTables();
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
        foreach (var slot in table.slots)
        {
            if (slot != null && slot.gameObject != null)
                Destroy(slot.gameObject);
        }
        table.slots.Clear();

        for (int i = 0; i < table.slotCount; i++)
        {
            GameObject slotObj = Instantiate(table.slotPrefab, table.slotContainer);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            slot.Initialize(this, table.tableId, i);
            table.slots.Add(slot);
        }
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

    public void StartDragItem(InventorySlot slot, PointerEventData eventData)
    {
        if (slot.GetItem() == null) return;

        sourceSlot = slot;
        currentDragItem = Instantiate(dragItemPrefab, transform);
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

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        InventorySlot targetSlot = null;
        foreach (var result in results)
        {
            targetSlot = result.gameObject.GetComponent<InventorySlot>();
            if (targetSlot != null) break;
        }

        if (targetSlot != null)
        {
            SwapItems(sourceSlot, targetSlot);
        }

        Destroy(currentDragItem);
        currentDragItem = null;
        sourceSlot = null;
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
        InventoryTable table = tables.Find(t => t.tableId == tableId);
        if (table == null) return;

        ItemData item = itemDatabase.Find(i => i.id == itemId);
        if (item == null) return;

        foreach (var slot in table.slots)
        {
            if (slot.GetItem() == null)
            {
                slot.SetItem(item, count);
                return;
            }
            else if (slot.GetItem().id == itemId && item.maxStack > 1)
            {
                int newCount = slot.GetCount() + count;
                if (newCount <= item.maxStack)
                {
                    slot.SetItem(item, newCount);
                    return;
                }
            }
        }

        Debug.LogWarning($"No available slots in table {tableId} for item {itemId}");
    }

    public InventoryTable GetTable(string tableId)
    {
        return tables.Find(t => t.tableId == tableId);
    }

    public void SaveInventory(string savePath = "inventorySave.json")
    {
        List<SlotSaveData> saveData = new List<SlotSaveData>();

        foreach (var table in tables)
        {
            foreach (var slot in table.slots)
            {
                if (slot.GetItem() != null)
                {
                    saveData.Add(new SlotSaveData
                    {
                        itemId = slot.GetItem().id,
                        itemCount = slot.GetCount(),
                        slotIndex = slot.GetSlotIndex(),
                        tableId = slot.GetTableId()
                    });
                }
            }
        }

        string jsonData = JsonUtility.ToJson(new Wrapper<SlotSaveData> { Items = saveData }, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, savePath), jsonData);

        Debug.Log("Inventory saved to: " + Path.Combine(Application.persistentDataPath, savePath));
    }

    public void LoadInventory(string savePath = "inventorySave.json")
    {
        string fullPath = Path.Combine(Application.persistentDataPath, savePath);

        if (!File.Exists(fullPath))
        {
            Debug.LogWarning("No save file found at: " + fullPath);
            return;
        }

        string jsonData = File.ReadAllText(fullPath);
        Wrapper<SlotSaveData> wrapper = JsonUtility.FromJson<Wrapper<SlotSaveData>>(jsonData);

        // Сначала очищаем все слоты
        foreach (var table in tables)
        {
            foreach (var slot in table.slots)
            {
                slot.ClearSlot();
            }
        }

        // Затем заполняем сохраненными данными
        foreach (var slotData in wrapper.Items)
        {
            InventoryTable table = GetTable(slotData.tableId);
            if (table == null) continue;

            if (slotData.slotIndex >= 0 && slotData.slotIndex < table.slots.Count)
            {
                ItemData item = itemDatabase.Find(i => i.id == slotData.itemId);
                if (item != null)
                {
                    table.slots[slotData.slotIndex].SetItem(item, slotData.itemCount);
                }
            }
        }

        Debug.Log("Inventory loaded from: " + fullPath);
    }
}