using UnityEngine;
using UnityEngine.UI;

public class InventorySaveButton : MonoBehaviour
{
    [SerializeField] private InventorySystem inventorySystem;
    public Button btnSave;
    public Button btnDelSave;

    private void Awake()
    {
        btnSave.onClick.AddListener(SaveInventory);
        btnDelSave.onClick.AddListener(DelSaveInventtory);
    }

    private void SaveInventory()
    {
        if (inventorySystem != null)
        {
            inventorySystem.SaveInventory();
            inventorySystem.LoadInventory();
            Debug.Log("Inventory saved!");
        }
    }
    private void DelSaveInventtory()
    {
        inventorySystem.DeleteSave();
    }
}