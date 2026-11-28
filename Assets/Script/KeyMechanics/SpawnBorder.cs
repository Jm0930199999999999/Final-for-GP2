using UnityEngine;

// Spawn/despawn ring: a point just outside the play area on one of the four sides.
// Customers enter from here and walk back to the same point when reformed.
public class SpawnBorder : MonoBehaviour
{
    [Tooltip("Distance outside the play area edge. Must be large enough to be off-camera (camera edge + sprite size).")]
    [SerializeField] float margin = 3f;

    public float Margin => margin;

    public Vector3 RandomPoint(Rect area)
    {
        // Points stay within each side's span (never at a corner), so every customer
        // enters in a clean straight line: top -> down, bottom -> up, left -> right, right -> left.
        switch (Random.Range(0, 4))
        {
            case 0:  // top
                return new Vector3(Random.Range(area.xMin, area.xMax), area.yMax + margin, 0f);
            case 1:  // bottom
                return new Vector3(Random.Range(area.xMin, area.xMax), area.yMin - margin, 0f);
            case 2:  // left
                return new Vector3(area.xMin - margin, Random.Range(area.yMin, area.yMax), 0f);
            default: // right
                return new Vector3(area.xMax + margin, Random.Range(area.yMin, area.yMax), 0f);
        }
    }
}
