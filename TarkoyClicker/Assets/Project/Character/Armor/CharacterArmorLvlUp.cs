using UnityEngine;

public class CharacterArmorLvlUp : MonoBehaviour
{
    [Header("Настройки брони")]
    public Transform armorParent; // Родительский объект для брони
    public GameObject[] armorPrefabs;

    [Header("Настройки шлемов")]
    public Transform helmetParent; // Родительский объект для шлемов
    public GameObject[] helmetPrefabs;

    private GameObject currentArmorInstance;
    private GameObject currentHelmetInstance;
    private PerksSystem perksSystem;

    private void Awake()
    {
        perksSystem = FindObjectOfType<PerksSystem>();
        if (perksSystem != null)
        {
            perksSystem.OnCharacterUpgrade += UpdateEquipment;
        }
    }

    private void Start()
    {
        UpdateEquipment();
    }

    private void OnDestroy()
    {
        if (perksSystem != null)
        {
            perksSystem.OnCharacterUpgrade -= UpdateEquipment;
        }
    }

    private void UpdateEquipment()
    {
        int currentLevel = perksSystem.GetCharacterLevel();
        UpdateArmor(currentLevel);
        UpdateHelmet(currentLevel);
    }

    private void UpdateArmor(int level)
    {
        // Броня появляется с 10 уровня и меняется каждые 10 уровней
        if (level < 10)
        {
            ClearEquipment(ref currentArmorInstance);
            return;
        }

        int armorIndex = Mathf.FloorToInt((level - 10) / 10);
        armorIndex = Mathf.Clamp(armorIndex, 0, armorPrefabs.Length - 1);

        SpawnEquipment(armorPrefabs, armorParent, ref currentArmorInstance, armorIndex);
    }

    private void UpdateHelmet(int level)
    {
        // Шлемы появляются с 5 уровня и меняются каждые 5 уровней
        if (level < 5)
        {
            ClearEquipment(ref currentHelmetInstance);
            return;
        }

        int helmetIndex = Mathf.FloorToInt((level - 5) / 5);
        helmetIndex = Mathf.Clamp(helmetIndex, 0, helmetPrefabs.Length - 1);

        SpawnEquipment(helmetPrefabs, helmetParent, ref currentHelmetInstance, helmetIndex);
    }

    private void SpawnEquipment(GameObject[] prefabs, Transform parent, ref GameObject currentInstance, int index)
    {
        if (prefabs.Length == 0 || index >= prefabs.Length || prefabs[index] == null)
            return;

        ClearEquipment(ref currentInstance);

        currentInstance = Instantiate(prefabs[index], parent.position, parent.rotation, parent);
    }

    private void ClearEquipment(ref GameObject instance)
    {
        if (instance != null)
        {
            Destroy(instance);
            instance = null;
        }
    }
}