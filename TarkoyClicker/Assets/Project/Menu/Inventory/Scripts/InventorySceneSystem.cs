using UnityEngine;

public class InventorySceneSystem : MonoBehaviour
{
    public static InventorySceneSystem Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private GameObject inventoryPrefab;
    [SerializeField] private Vector3 hiddenPosition = new Vector3(9999, 9999, 9999);

    private InventorySystem _currentInventory;
    private bool _isInitialized = false;

    //private static bool _isCreated = false;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            //// Проверяем, не был ли объект уже сохранен
            //if (!_isCreated)
            //{
            //    DontDestroyOnLoad(gameObject);
            //    _isCreated = true;
            //}

            InitializeInventory();
        }
        else
        {
            // Если уже есть экземпляр, уничтожаем новый
            Destroy(gameObject);
        }
    }

    private void InitializeInventory()
    {
        if (_isInitialized || inventoryPrefab == null) return;

        GameObject inventoryObj = Instantiate(inventoryPrefab, hiddenPosition, Quaternion.identity);
        inventoryObj.transform.SetParent(transform);
        _currentInventory = inventoryObj.GetComponent<InventorySystem>();

        if (_currentInventory == null)
        {
            Debug.LogError("Inventory prefab is missing InventorySystem component!");
            return;
        }

        _currentInventory.LoadInventory();
        _currentInventory.gameObject.SetActive(false);
        _isInitialized = true;
    }

    public InventorySystem GetInventory()
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("Inventory system not initialized!");
            return null;
        }
        return _currentInventory;
    }

    public void ShowInventory(Vector3 position)
    {
        if (_currentInventory != null)
        {
            _currentInventory.transform.position = position;
            _currentInventory.gameObject.SetActive(true);
            _currentInventory.ForceUpdateInventory();
        }
    }

    public void HideInventory()
    {
        if (_currentInventory != null)
        {
            _currentInventory.SaveInventory();
            _currentInventory.gameObject.SetActive(false);
            _currentInventory.transform.position = hiddenPosition;
        }
    }

    [ContextMenu("Force Save Inventory")]
    public void SaveInventoryManually()
    {
        if (_currentInventory != null)
        {
            _currentInventory.SaveInventory();
            Debug.Log("Inventory manually saved!");
        }
        else
        {
            Debug.LogWarning("Can't save - inventory not initialized!");
        }
    }

    private void OnApplicationQuit()
    {
        if (_currentInventory != null)
        {
            _currentInventory.SaveInventory();
        }
    }
}