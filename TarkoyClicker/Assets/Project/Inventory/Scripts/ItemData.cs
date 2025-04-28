using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    [TextArea] public string description;
    public int maxStack = 1;
    public ItemType type;
    public int value;

    public enum ItemType
    {
        Food,
        Medicine,
        Loot
    }
}