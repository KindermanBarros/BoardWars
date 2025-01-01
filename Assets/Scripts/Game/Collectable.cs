using UnityEngine;

public class Collectable : MonoBehaviour
{
    public enum Tier
    {
        Small,
        Medium,
        Large
    }

    private CollectableType typeData;
    public CollectableTypeEnum CollectableType => typeData?.type ?? CollectableTypeEnum.ExtraMove;
    public Tier CollectableTier { get; private set; }

    public void Initialize(CollectableType type, Tier tier)
    {
        if (type == null)
        {
            Debug.LogError("Trying to initialize Collectable with null CollectableType!");
            return;
        }

        typeData = type;
        CollectableTier = tier;

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = true;
        }

        Debug.Log($"Initialized collectable: {type.displayName} - Tier: {tier}");
    }

    public int GetValue()
    {
        if (typeData == null)
        {
            Debug.LogError("Collectable not properly initialized!");
            return 0;
        }

        return CollectableTier switch
        {
            Tier.Small => typeData.baseValue / 2,
            Tier.Medium => typeData.baseValue * 1,
            Tier.Large => typeData.baseValue * 2,
            _ => typeData.baseValue
        };
    }
}
