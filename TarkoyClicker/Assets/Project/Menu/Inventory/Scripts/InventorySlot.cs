using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class InventorySlot : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public Image itemIcon;
    public TextMeshProUGUI itemCountText;
    [Header("Обводка (если надо)")]
    public GameObject highlight;

    private InventorySystem inventorySystem;
    private string tableId;
    private int slotIndex;
    private ItemData currentItem;
    private int currentCount = 1;

    public void Initialize(InventorySystem system, string table, int index)
    {
        inventorySystem = system;
        tableId = table;
        slotIndex = index;
        ClearSlot();
    }

    public void SetItem(ItemData item, int count = 1)
    {
        currentItem = item;
        currentCount = count;

        if (item != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = true;

            if (itemCountText != null)
                itemCountText.text = count > 1 ? count.ToString() : "";
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        currentCount = 0;
        itemIcon.sprite = null;
        itemIcon.enabled = false;

        if (itemCountText != null)
            itemCountText.text = "";
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentItem != null && eventData.button == PointerEventData.InputButton.Left)
        {
            inventorySystem.StartDragItem(this, eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        inventorySystem.DragItem(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inventorySystem.EndDragItem(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlight != null)
            highlight.SetActive(true);

        if (currentItem != null)
        {
            inventorySystem.ShowTooltip(currentItem, transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlight != null)
            highlight.SetActive(false);

        inventorySystem.HideTooltip();
    }

    public ItemData GetItem() => currentItem;
    public int GetCount() => currentCount;
    public string GetTableId() => tableId;
    public int GetSlotIndex() => slotIndex;
}