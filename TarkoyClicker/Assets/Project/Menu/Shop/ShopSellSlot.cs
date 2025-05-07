using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ShopSellSlot : MonoBehaviour, IDropHandler
{
    private InventorySystem inventorySystem;
    private PassivePoints passivePoints;

    [Header("Settings")]
    [SerializeField] private string inventoryParentName = "RightContent";

    private void Start()
    {
        FindInventorySystem();
        FindPassivePoints();
    }

    private void FindInventorySystem()
    {
        // »щем родительский объект инвентар€ по имени (включа€ неактивные)
        Transform inventoryParent = FindDeepChild(transform.root, inventoryParentName);

        if (inventoryParent != null)
        {
            inventorySystem = inventoryParent.GetComponentInChildren<InventorySystem>(true);
        }

        if (inventorySystem == null)
        {
            Debug.LogError($"InventorySystem not found! Could not find parent object named '{inventoryParentName}'");
        }
    }

    private void FindPassivePoints()
    {
        passivePoints = FindObjectOfType<PassivePoints>(true); // »щем даже неактивные

        if (passivePoints == null)
        {
            Debug.LogError("PassivePoints not found on scene!");
        }
    }

    // –екурсивный поиск объекта по имени в иерархии
    private Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name) return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }

        return null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (inventorySystem == null || passivePoints == null) return;

        InventorySlot sourceSlot = eventData.pointerDrag?.GetComponent<InventorySlot>();
        if (sourceSlot == null || sourceSlot.GetItem() == null) return;

        SellItem(sourceSlot);
    }

    private void SellItem(InventorySlot slot)
    {
        ItemData item = slot.GetItem();
        int count = slot.GetCount();
        int pointsToAdd = item.Count * count;

        passivePoints.AddPoints(pointsToAdd);
        slot.ClearSlot();
        inventorySystem.SaveInventory();

        Debug.Log($"Sold {count}x {item.displayName} for {pointsToAdd} points");
    }
}