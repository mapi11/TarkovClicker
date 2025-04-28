using UnityEngine;
using UnityEngine.UI;

public class ItemAdderOnClick : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private string _targetInventoryId = "Player_Inventory";
    [SerializeField] private string _itemIdToAdd = "health_potion";
    [SerializeField] private int _count = 1;

    [Header("Ссылки на кнопки")]
    [SerializeField] private Button _addItemButton;
    [SerializeField] private Button _removeItemButton;

    void Start()
    {
        // Настройка кнопки добавления
        if (_addItemButton == null)
            _addItemButton = GetComponent<Button>();

        _addItemButton.onClick.AddListener(AddItem);

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