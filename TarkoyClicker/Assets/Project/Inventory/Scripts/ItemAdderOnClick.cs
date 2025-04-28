using UnityEngine;
using UnityEngine.UI;

public class ItemAdderOnClick : MonoBehaviour
{
    [Header("�������� ���������")]
    [SerializeField] private string _targetInventoryId = "Player_Inventory";
    [SerializeField] private string _itemIdToAdd = "health_potion";
    [SerializeField] private int _count = 1;

    [Header("������ �� ������")]
    [SerializeField] private Button _addItemButton;
    [SerializeField] private Button _removeItemButton;

    void Start()
    {
        // ��������� ������ ����������
        if (_addItemButton == null)
            _addItemButton = GetComponent<Button>();

        _addItemButton.onClick.AddListener(AddItem);

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