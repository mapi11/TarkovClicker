using UnityEngine;
using UnityEngine.UI;

public class ItemAdderOnClick : MonoBehaviour
{
    [Header("�������� ���������")]
    [SerializeField] private string _targetInventoryId = "Inventory";
    [SerializeField] private string _itemIdToAdd = "0";
    [SerializeField] private string _itemIdToAdd2 = "1";
    [SerializeField] private int _count = 1;
    [SerializeField] private int _count2 = 1;

    [Header("������ �� ������")]
    [SerializeField] private Button _addItemButton;
    [SerializeField] private Button _addItemButton2;
    [SerializeField] private Button _removeItemButton;

    void Start()
    {
        _addItemButton.onClick.AddListener(AddItem);
        _addItemButton2.onClick.AddListener(AddItem2);

        // ��������� ������ ��������
        if (_removeItemButton != null)
            _removeItemButton.onClick.AddListener(RemoveItemFromSlot0);
    }

    void AddItem()
    {
        InventorySystem inventory = FindObjectOfType<InventorySystem>();
        if (inventory == null) return;

        // ��������� ������� ��������� ������
        if (!inventory.HasEmptySlots(_targetInventoryId))
        {
            Debug.Log("��������� �����!");
            return;
        }

        inventory.AddItemToTable(_targetInventoryId, _itemIdToAdd, _count);
    }
    void AddItem2()
    {
        InventorySystem inventory = FindObjectOfType<InventorySystem>();
        if (inventory == null) return;

        // ��������� ������� ��������� ������
        if (!inventory.HasEmptySlots(_targetInventoryId))
        {
            Debug.Log("��������� �����!");
            return;
        }

        inventory.AddItemToTable(_targetInventoryId, _itemIdToAdd2, _count2);
    }

    void RemoveItemFromSlot0()
    {
        InventorySystem inventory = FindObjectOfType<InventorySystem>();
        if (inventory == null) return;

        // �������� ������� ���������
        var table = inventory.GetTable(_targetInventoryId);
        if (table == null || table.slots.Count == 0)
        {
            Debug.LogWarning("��������� �� ������ ��� ����!");
            return;
        }

        // ������� ������� �� ����� 0
        table.slots[0].ClearSlot();
        Debug.Log("������� �� ����� 0 ������");
    }

    public void SetItemToAdd(string newItemId, int newCount = 1)
    {
        _itemIdToAdd = newItemId;
        _count = newCount;
    }
}