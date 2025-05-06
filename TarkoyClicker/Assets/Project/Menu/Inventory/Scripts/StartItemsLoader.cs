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

    [Header("���������")]
    public string targetInventory = "Player_Inventory"; // ID ���������
    public List<StartItem> itemsToAdd = new List<StartItem>();

    void Start()
    {
        // �������� ������� ���������
        InventorySystem inventory = FindObjectOfType<InventorySystem>();

        if (inventory == null)
        {
            Debug.LogError("InventorySystem �� ������ �� �����!");
            return;
        }

        // ��������� ��� ��������� ��������
        foreach (StartItem item in itemsToAdd)
        {
            inventory.AddItemToTable(targetInventory, item.itemId, item.count);
        }
    }
}