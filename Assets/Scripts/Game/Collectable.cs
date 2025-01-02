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
            return;
        }

        typeData = type;
        CollectableTier = tier;

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = true;
        }
    }

    public int GetValue()
    {
        if (typeData == null)
        {
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
