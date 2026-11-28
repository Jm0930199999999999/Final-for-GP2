using UnityEngine;

public enum UpgradeType { Speed, CleaningSlot, WaterStrength }

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class Upgrade : MonoBehaviour
{
    [SerializeField] float lifetime = 8f;   // despawns if the shower doesn't grab it

    public UpgradeType Type { get; private set; }

    void Awake()
    {
        Type = (UpgradeType)Random.Range(0, 3);   // 1/3 chance each

        // Placeholder tints until you have real icons
        GetComponent<SpriteRenderer>().color = Type switch
        {
            UpgradeType.Speed => Color.yellow,
            UpgradeType.CleaningSlot => Color.green,
            _ => Color.cyan
        };

        Destroy(gameObject, lifetime);
    }
}
