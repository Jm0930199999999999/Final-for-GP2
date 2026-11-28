using System.Collections;
using UnityEngine;

public class IncelSpawner : MonoBehaviour
{
    [SerializeField] Incel prefab;
    [SerializeField] SpawnBorder border;

    [Tooltip("Area incels wander inside (x, y = bottom-left corner). Match the shower's bounds and keep it inside the camera view.")]
    [SerializeField] Rect playArea = new(-8f, -4f, 16f, 8f);

    [SerializeField] float startInterval = 5f, minInterval = 3f;
    [SerializeField] float startWalkSpeed = 2f, maxWalkSpeed = 4.5f;
    [SerializeField] float rampSeconds = 600f;

    IEnumerator Start()
    {
        float t = 0f;
        while (GameManager.Instance.IsPlaying)
        {
            float k = Mathf.Clamp01(t / rampSeconds);
            float interval = Mathf.Lerp(startInterval, minInterval, k);
            float speed = Mathf.Lerp(startWalkSpeed, maxWalkSpeed, k);

            Vector3 spawnPoint = border.RandomPoint(playArea);   // outside the map, any of the 4 sides
            var incel = Instantiate(prefab, spawnPoint, Quaternion.identity);
            incel.Init(spawnPoint, speed, playArea.min, playArea.max);

            yield return new WaitForSeconds(interval);
            t += interval;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Green: play area. Yellow: where incels spawn and despawn (outside the map).
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(playArea.center, playArea.size);

        if (border != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(playArea.center, playArea.size + Vector2.one * (border.Margin * 2f));
        }
    }
}
