using UnityEngine;
using UnityEngine.UI;

public class ItemAdderOnClick : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private string _targetInventoryId = "Inventory";
    [SerializeField] private string _itemIdToAdd = "0";
    [SerializeField] private string _itemIdToAdd2 = "1";
    [SerializeField] private int _count = 1;
    [SerializeField] private int _count2 = 1;

    [Header("Ссылки на кнопки")]
    [SerializeField] private Button _addItemButton;
    [SerializeField] private Button _addItemButton2;
    [SerializeField] private Button _removeItemButton;

    void Start()
    {
        _addItemButton.onClick.AddListener(AddItem);
        _addItemButton2.onClick.AddListener(AddItem2);

        // Настройка кнопки удаления
        if (_removeItemButton != null)
            _removeItemButton.onClick.AddListener(RemoveItemFromSlot0);
    }

    void AddItem()
    {
        InventorySystem inventory = FindObjectOfType<InventorySystem>();
        if (inventory == null) return;

        // Проверяем наличие свободных слотов
        if (!inventory.HasEmptySlots(_targetInventoryId))
        {
            Debug.Log("Инвентарь полон!");
            return;
        }

        inventory.AddItemToTable(_targetInventoryId, _itemIdToAdd, _count);
    }
    void AddItem2()
    {
        InventorySystem inventory = FindObjectOfType<InventorySystem>();
        if (inventory == null) return;

        // Проверяем наличие свободных слотов
        if (!inventory.HasEmptySlots(_targetInventoryId))
        {
            Debug.Log("Инвентарь полон!");
            return;
        }

        inventory.AddItemToTable(_targetInventoryId, _itemIdToAdd2, _count2);
    }

    void RemoveItemFromSlot0()
    {
        InventorySystem inventory = FindObjectOfType<InventorySystem>();
        if (inventory == null) return;

        // Получаем таблицу инвентаря
        var table = inventory.GetTable(_targetInventoryId);
        if (table == null || table.slots.Count == 0)
        {
            Debug.LogWarning("Инвентарь не найден или пуст!");
            return;
        }

        // Удаляем предмет из слота 0
        table.slots[0].ClearSlot();
        Debug.Log("Предмет из слота 0 удален");
    }

    public void SetItemToAdd(string newItemId, int newCount = 1)
    {
        _itemIdToAdd = newItemId;
        _count = newCount;
    }
}