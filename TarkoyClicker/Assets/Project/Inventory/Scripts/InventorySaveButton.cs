using UnityEngine;
using UnityEngine.UI;

public class InventorySaveButton : MonoBehaviour
{
    [SerializeField] private InventorySystem inventorySystem;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SaveInventory);
    }

    private void SaveInventory()
    {
        if (inventorySystem != null)
        {
            inventorySystem.SaveInventory();
            Debug.Log("Inventory saved!");
        }
    }
}