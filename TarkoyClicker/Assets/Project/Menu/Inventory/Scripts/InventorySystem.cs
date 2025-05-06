using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

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

    [Header("Inventory Settings")]
    public List<InventoryTable> tables = new List<InventoryTable>();
    public List<ItemData> itemDatabase = new List<ItemData>();

    [Header("Drag & Drop Settings")]
    private string dragCanvasName = "MainCanvas";
    public Canvas dragItemCanvas;
    public GameObject dragItemPrefab;
    public GameObject tooltipPrefab;

    [Header("References")]
    public PerksSystem perksSystem;
    public TimingClick timingClick;

    public static InventorySystem Instance => InventorySceneSystem.Instance?.GetInventory();

    private GameObject currentDragItem;
    private InventorySlot sourceSlot;
    private GameObject currentTooltip;

    private void Awake()
    {
        perksSystem = FindAnyObjectByType<PerksSystem>();
        timingClick = FindAnyObjectByType<TimingClick>();
    }

    void Start()
    {
        dragItemCanvas = GameObject.Find(dragCanvasName)?.GetComponent<Canvas>();
        if (dragItemCanvas == null)
        {
            Debug.LogError($"Canvas with name {dragCanvasName} not found!");
        }

        InitializeAllTables();
        LoadInventory();
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

    public void SetItemInSlot(InventorySlot slot, ItemData item, int count = 1)
    {
        slot.SetItem(item, count);
        SaveInventory();
    }

    public void ClearSlot(InventorySlot slot)
    {
        slot.ClearSlot();
        SaveInventory();
    }

    public bool AddItemToTable(string tableId, string itemId, int amount)
    {
        InventoryTable table = GetTable(tableId);
        if (table == null)
        {
            Debug.LogError($"Table {tableId} not found!");
            return false;
        }

        ItemData item = itemDatabase.Find(i => i.id == itemId);
        if (item == null)
        {
            Debug.LogError($"Item {itemId} not found in database!");
            return false;
        }

        int remaining = amount;

        foreach (var slot in table.slots)
        {
            if (slot.GetItem() != null && slot.GetItem().id == itemId && slot.GetCount() < item.maxStack)
            {
                int canAdd = item.maxStack - slot.GetCount();
                int willAdd = Mathf.Min(canAdd, remaining);

                slot.SetItem(item, slot.GetCount() + willAdd);
                remaining -= willAdd;

                if (remaining <= 0) break;
            }
        }

        if (remaining > 0)
        {
            foreach (var slot in table.slots)
            {
                if (slot.GetItem() == null)
                {
                    int willAdd = Mathf.Min(item.maxStack, remaining);
                    slot.SetItem(item, willAdd);
                    remaining -= willAdd;

                    if (remaining <= 0) break;
                }
            }
        }

        SaveInventory();
        return remaining == 0;
    }

    public void StartDragItem(InventorySlot slot, PointerEventData eventData)
    {
        if (slot.GetItem() == null) return;

        sourceSlot = slot;
        currentDragItem = Instantiate(dragItemPrefab, dragItemCanvas.transform);
        currentDragItem.GetComponent<Image>().sprite = slot.GetItem().icon;
        currentDragItem.transform.position = eventData.position;
        currentDragItem.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void DragItem(PointerEventData eventData)
    {
        if (currentDragItem != null)
            currentDragItem.transform.position = eventData.position;
    }

    public void EndDragItem(PointerEventData eventData)
    {
        if (currentDragItem == null) return;

        InventorySlot targetSlot = GetTargetSlot(eventData);

        if (targetSlot != null)
        {
            if (targetSlot.GetTableId() == "StaminaSlot")
            {
                HandleStaminaSlotDrop(targetSlot);
                return;
            }
            else if (targetSlot.GetTableId() == "HealthSlot")
            {
                HandleHealthSlotDrop(targetSlot);
                return;
            }
        }

        if (targetSlot != null)
        {
            if (CanMergeItems(sourceSlot, targetSlot))
            {
                MergeItems(sourceSlot, targetSlot);
            }
            else
            {
                SwapItems(sourceSlot, targetSlot);
            }
        }

        Destroy(currentDragItem);
        currentDragItem = null;
        sourceSlot = null;
    }

    private void HandleStaminaSlotDrop(InventorySlot targetSlot)
    {
        if (sourceSlot.GetItem() == null) return;

        ItemData draggedItem = sourceSlot.GetItem();

        if (draggedItem.type == ItemData.ItemType.Food)
        {
            if (perksSystem != null)
            {
                perksSystem.AddStaminaProgress(draggedItem.value);
            }

            int newCount = sourceSlot.GetCount() - 1;
            if (newCount > 0)
            {
                sourceSlot.SetItem(draggedItem, newCount);
            }
            else
            {
                sourceSlot.ClearSlot();
            }

            SaveInventory();
        }

        targetSlot.ClearSlot();
        Destroy(currentDragItem);
        currentDragItem = null;
        sourceSlot = null;
    }

    private void HandleHealthSlotDrop(InventorySlot targetSlot)
    {
        if (sourceSlot.GetItem() == null) return;

        ItemData draggedItem = sourceSlot.GetItem();

        if (draggedItem.type == ItemData.ItemType.Medicine && timingClick != null)
        {
            if (timingClick.IsArmBroken)
            {
                timingClick.ArmHeal();

                int newCount = sourceSlot.GetCount() - 1;
                if (newCount > 0)
                {
                    sourceSlot.SetItem(draggedItem, newCount);
                }
                else
                {
                    sourceSlot.ClearSlot();
                }

                SaveInventory();
            }
            else
            {
                sourceSlot.SetItem(draggedItem, sourceSlot.GetCount());
                Debug.Log("Arm not broken, no need to heal!");
            }
        }
        else
        {
            sourceSlot.SetItem(sourceSlot.GetItem(), sourceSlot.GetCount());
        }

        targetSlot.ClearSlot();
        Destroy(currentDragItem);
        currentDragItem = null;
        sourceSlot = null;
    }

    private bool CanMergeItems(InventorySlot source, InventorySlot target)
    {
        if (target.GetTableId() == "StaminaSlot" || target.GetTableId() == "HealthSlot")
            return false;

        return source.GetItem() != null &&
               target.GetItem() != null &&
               source.GetItem().id == target.GetItem().id &&
               source.GetItem().maxStack > 1 &&
               target.GetCount() < target.GetItem().maxStack;
    }

    private void SwapItems(InventorySlot a, InventorySlot b)
    {
        if (a.GetTableId() == "StaminaSlot" || b.GetTableId() == "StaminaSlot" ||
            a.GetTableId() == "HealthSlot" || b.GetTableId() == "HealthSlot")
            return;

        ItemData aItem = a.GetItem();
        int aCount = a.GetCount();
        ItemData bItem = b.GetItem();
        int bCount = b.GetCount();

        a.SetItem(bItem, bCount);
        b.SetItem(aItem, aCount);

        SaveInventory();
    }

    private void MergeItems(InventorySlot source, InventorySlot target)
    {
        int availableSpace = target.GetItem().maxStack - target.GetCount();
        int transferAmount = Mathf.Min(availableSpace, source.GetCount());

        target.SetItem(target.GetItem(), target.GetCount() + transferAmount);

        int remaining = source.GetCount() - transferAmount;
        if (remaining > 0)
            source.SetItem(source.GetItem(), remaining);
        else
            source.ClearSlot();

        SaveInventory();
    }

    public void SaveInventory()
    {
        try
        {
            foreach (var table in tables)
            {
                for (int i = 0; i < table.slots.Count; i++)
                {
                    string key = $"{table.tableId}_{i}";
                    InventorySlot slot = table.slots[i];

                    if (slot.GetItem() != null)
                    {
                        PlayerPrefs.SetString($"{key}_id", slot.GetItem().id);
                        PlayerPrefs.SetInt($"{key}_count", slot.GetCount());
                    }
                    else
                    {
                        PlayerPrefs.DeleteKey($"{key}_id");
                        PlayerPrefs.DeleteKey($"{key}_count");
                    }
                }
            }
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Save error: {e.Message}");
        }
    }

    public void ForceUpdateInventory()
    {
        LoadInventory();
    }

    public void LoadInventory()
    {
        // Сначала очищаем все слоты
        foreach (var table in tables)
        {
            foreach (var slot in table.slots)
            {
                slot.ClearSlot();
            }
        }

        // Затем загружаем сохраненные данные
        foreach (var table in tables)
        {
            for (int i = 0; i < table.slots.Count; i++)
            {
                string key = $"{table.tableId}_{i}";
                string itemId = PlayerPrefs.GetString($"{key}_id", "");

                if (!string.IsNullOrEmpty(itemId))
                {
                    ItemData item = itemDatabase.Find(x => x.id == itemId);
                    if (item != null)
                    {
                        int count = PlayerPrefs.GetInt($"{key}_count", 1);
                        table.slots[i].SetItem(item, count);
                    }
                }
            }
        }
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

    public InventoryTable GetTable(string tableId) => tables.Find(t => t.tableId == tableId);

    public void ShowTooltip(ItemData item, Vector2 position)
    {
        if (tooltipPrefab == null) return;

        currentTooltip = Instantiate(tooltipPrefab, transform);
        currentTooltip.transform.position = position;
        currentTooltip.GetComponentInChildren<TMP_Text>().text = $"{item.displayName}\n{item.description}";
    }

    public void HideTooltip()
    {
        if (currentTooltip != null)
            Destroy(currentTooltip);
    }

    public bool HasEmptySlots(string tableId)
    {
        InventoryTable table = GetTable(tableId);
        if (table == null) return false;

        foreach (InventorySlot slot in table.slots)
        {
            if (slot.GetItem() == null)
                return true;
        }
        return false;
    }

    public void DeleteSave()
    {
        foreach (var table in tables)
        {
            for (int i = 0; i < table.slotCount; i++)
            {
                string slotKey = $"{table.tableId}_{i}";
                PlayerPrefs.DeleteKey($"{slotKey}_id");
                PlayerPrefs.DeleteKey($"{slotKey}_count");
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

    private void OnDisable()
    {
        SaveInventory();
    }
}