using UnityEngine;

public enum CollectableTypeEnum
{
    ExtraMove,
    ExtraAttack,
    Health
}

[CreateAssetMenu(fileName = "New Collectable Type", menuName = "Game/Collectable Type")]
public class CollectableType : ScriptableObject
{
    public string displayName;
    public GameObject visualPrefab;
    public int baseValue;
    public CollectableTypeEnum type;
    [TextArea]
    public string description;
}
