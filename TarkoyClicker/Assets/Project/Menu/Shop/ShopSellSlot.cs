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
        passivePoints = FindObjectOfType<PassivePoints>(true);

        if (passivePoints == null)
        {
            Debug.LogError("PassivePoints not found on scene!");
        }
    }

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

        SellSingleItem(sourceSlot);
    }

    private void SellSingleItem(InventorySlot slot)
    {
        ItemData item = slot.GetItem();
        int currentCount = slot.GetCount();

        // Если предметов больше одного - уменьшаем количество, иначе очищаем слот
        if (currentCount > 1)
        {
            slot.SetItem(item, currentCount - 1);
        }
        else
        {
            slot.ClearSlot();
        }

        // Начисляем очки за 1 предмет
        passivePoints.AddPoints(item.Count);
        inventorySystem.SaveInventory();

        Debug.Log($"Sold 1x {item.displayName} for {item.value} points. Remaining: {currentCount - 1}");
    }
}