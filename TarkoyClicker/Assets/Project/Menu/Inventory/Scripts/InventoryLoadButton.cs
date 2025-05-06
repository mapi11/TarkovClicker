using UnityEngine;
using UnityEngine.UI;

public class InventoryLoadButton : MonoBehaviour
{
    [SerializeField] private InventorySystem inventorySystem;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(LoadInventory);
    }

    private void LoadInventory()
    {
        if (inventorySystem != null)
        {
            inventorySystem.LoadInventory();
            Debug.Log("Inventory loaded!");
        }
    }
}