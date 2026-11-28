using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Collider2D))]
public class Incel : MonoBehaviour
{
    public enum IncelState { Wandering, Caught, Cleansed }

    public event Action<Incel> OnCleansed;   // fires the moment they finish being cleaned

    [Header("Movement")]
    [SerializeField] float turnTimeMin = 1f;       // seconds before picking a new direction
    [SerializeField] float turnTimeMax = 3f;
    [SerializeField] float exitSpeed = 4f;         // speed when walking out after being cleansed

    [Header("Cleaning")]
    [SerializeField] float cleanDuration = 5f;     // seconds to fully cleanse at base water strength
    [SerializeField] Sprite reformedSprite;        // swapped in once cleansed (the incel art is the prefab's own sprite)
    [SerializeField] int basePoints = 100;

    [Header("Upgrade drop")]
    [SerializeField] Upgrade upgradePrefab;
    [SerializeField, Range(0f, 1f)] float upgradeChance = 0.35f;
    [SerializeField] float upgradeDropDistance = 2f;   // so it isn't instantly grabbed by the shower

    public IncelState State { get; private set; } = IncelState.Wandering;

    static readonly Vector2[] Directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    Rigidbody2D rb;
    SpriteRenderer sr;
    Collider2D col;

    Vector3 homePoint;          // where this incel entered; they walk back here to despawn
    Vector2 areaMin, areaMax;   // play area they wander inside
    Vector2 heading;
    float speed;
    float turnTimer;
    bool inArea;
    float dirt = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
    }

    public void Init(Vector3 home, float walkSpeed, Vector2 playMin, Vector2 playMax)
    {
        homePoint = home;
        speed = walkSpeed;
        areaMin = playMin;
        areaMax = playMax;

        Vector2 p = transform.position;
        if (InsideArea(p))
        {
            inArea = true;
            PickDirection();
        }
        else
        {
            // Enter in a straight line, perpendicular to whichever side of the area they spawned on
            if (p.y > areaMax.y) heading = Vector2.down;
            else if (p.y < areaMin.y) heading = Vector2.up;
            else if (p.x < areaMin.x) heading = Vector2.right;
            else heading = Vector2.left;

            rb.linearVelocity = heading * speed;
        }
    }

    public void Catch(Transform slot)
    {
        State = IncelState.Caught;
        rb.linearVelocity = Vector2.zero;
        transform.SetParent(slot, false);
        transform.localPosition = Vector3.zero;
    }

    // Called by the shower every frame while this incel is in a slot.
    // dt is already scaled by the shower's water strength.
    public void Clean(float dt)
    {
        if (State != IncelState.Caught) return;

        dirt = Mathf.Max(0f, dirt - dt / cleanDuration);

        if (dirt <= 0f) Cleanse();
    }

    void Cleanse()
    {
        State = IncelState.Cleansed;
        if (reformedSprite != null) sr.sprite = reformedSprite;   // incel -> reformed
        rb.linearVelocity = Vector2.zero;
        col.enabled = false;                 // can't be re-caught while heading out
        GameManager.Instance.RegisterCleansed(basePoints);

        if (upgradePrefab != null && Random.value < upgradeChance)
            DropUpgrade();

        OnCleansed?.Invoke(this);            // shower frees the slot
        transform.SetParent(null);           // leave the shower, keep world position
    }

    void DropUpgrade()
    {
        Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle.normalized * upgradeDropDistance;
        pos.x = Mathf.Clamp(pos.x, areaMin.x, areaMax.x);
        pos.y = Mathf.Clamp(pos.y, areaMin.y, areaMax.y);
        Instantiate(upgradePrefab, pos, Quaternion.identity);
    }

    void Update()
    {
        switch (State)
        {
            case IncelState.Wandering: UpdateWandering(); break;
            case IncelState.Cleansed: HeadOut(); break;
        }
    }

    void UpdateWandering()
    {
        Vector2 p = transform.position;

        // Still walking in from the border
        if (!inArea)
        {
            if (InsideArea(p))
            {
                inArea = true;
                PickDirection();
            }
            return;
        }

        // Keep inside the play area
        p.x = Mathf.Clamp(p.x, areaMin.x, areaMax.x);
        p.y = Mathf.Clamp(p.y, areaMin.y, areaMax.y);
        transform.position = p;

        turnTimer -= Time.deltaTime;

        bool hittingEdge =
            (p.x <= areaMin.x && heading.x < 0f) || (p.x >= areaMax.x && heading.x > 0f) ||
            (p.y <= areaMin.y && heading.y < 0f) || (p.y >= areaMax.y && heading.y > 0f);

        if (turnTimer <= 0f || hittingEdge) PickDirection();
    }

    // Picks a random up/down/left/right direction that doesn't walk out of the area
    void PickDirection()
    {
        Vector2 p = transform.position;
        Vector2 chosen = -heading;   // fallback: turn around

        for (int i = 0; i < 10; i++)
        {
            Vector2 d = Directions[Random.Range(0, Directions.Length)];
            Vector2 next = p + d * 0.5f;
            if (InsideArea(next)) { chosen = d; break; }
        }

        heading = chosen;
        rb.linearVelocity = heading * speed;
        turnTimer = Random.Range(turnTimeMin, turnTimeMax);
    }

    bool InsideArea(Vector2 p) =>
        p.x >= areaMin.x && p.x <= areaMax.x && p.y >= areaMin.y && p.y <= areaMax.y;

    void HeadOut()
    {
        transform.position = Vector3.MoveTowards(transform.position, homePoint,
                                                 exitSpeed * Time.deltaTime);
        if ((transform.position - homePoint).sqrMagnitude < 0.01f)
            Destroy(gameObject);             // reached the border, off-screen
    }
}
