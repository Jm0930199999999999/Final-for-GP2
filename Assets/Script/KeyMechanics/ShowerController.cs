using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class ShowerController : MonoBehaviour
{
    const int MaxSlots = 5;

    [Header("Movement (WASD)")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float maxMoveSpeed = 12f;
    [SerializeField] float speedBoost = 1.25f;            // multiplier per speed upgrade
    [SerializeField] Vector2 minBounds = new(-8f, -4f);   // match the spawner's play area
    [SerializeField] Vector2 maxBounds = new(8f, 4f);

    [Header("Cleaning (automatic once an incel is in a slot)")]
    [SerializeField] Transform[] slots;             // create 5 child transforms; only the first `capacity` are used
    [SerializeField] int capacity = 1;              // starts at 1, max 5
    [SerializeField] float waterStrength = 1f;      // how fast incels get cleaned in the slots
    [SerializeField] float maxWaterStrength = 3f;
    [SerializeField] float strengthBoost = 0.5f;    // added per water strength upgrade
    [SerializeField] ParticleSystem sprayFx;        // optional, plays while anyone is being cleaned

    readonly List<Incel> caught = new();
    public int Capacity => capacity;
    public float WaterStrength => waterStrength;

    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;   // keeps trigger events firing
    }

    void Update()
    {
        if (!GameManager.Instance.IsPlaying) return;

        Move();
        CleanCaught();
    }

    void Move()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // WASD on both axes
        Vector2 input = new(
            (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f),
            (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f));
        if (input.sqrMagnitude > 1f) input.Normalize();   // same speed on diagonals

        Vector2 pos = (Vector2)transform.position + input * moveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
        transform.position = pos;
    }

    // Every caught incel is cleaned automatically, all at the same time
    void CleanCaught()
    {
        for (int i = caught.Count - 1; i >= 0; i--)
            caught[i].Clean(Time.deltaTime * waterStrength);

        if (sprayFx != null)
        {
            bool cleaning = caught.Count > 0;
            if (cleaning && !sprayFx.isPlaying) sprayFx.Play();
            else if (!cleaning && sprayFx.isPlaying) sprayFx.Stop();
        }
    }

    // Stay (not Enter) so an incel standing in the zone is still caught once a slot frees up
    void OnTriggerStay2D(Collider2D other)
    {
        if (!GameManager.Instance.IsPlaying) return;

        if (other.TryGetComponent(out Upgrade u))
        {
            ApplyUpgrade(u.Type);
            Destroy(u.gameObject);
            return;
        }

        if (caught.Count >= capacity) return;
        if (!other.TryGetComponent(out Incel incel) || incel.State != Incel.IncelState.Wandering) return;

        incel.Catch(slots[caught.Count]);
        caught.Add(incel);
        incel.OnCleansed += HandleIncelCleansed;
    }

    void ApplyUpgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Speed:
                moveSpeed = Mathf.Min(moveSpeed * speedBoost, maxMoveSpeed);
                break;
            case UpgradeType.CleaningSlot:
                capacity = Mathf.Min(capacity + 1, Mathf.Min(MaxSlots, slots.Length));
                break;
            case UpgradeType.WaterStrength:
                waterStrength = Mathf.Min(waterStrength + strengthBoost, maxWaterStrength);
                break;
        }
    }

    void HandleIncelCleansed(Incel incel)
    {
        incel.OnCleansed -= HandleIncelCleansed;
        caught.Remove(incel);

        // Re-seat the remaining incels so slots fill in order
        for (int i = 0; i < caught.Count; i++)
            caught[i].transform.SetParent(slots[i], false);
    }
}
