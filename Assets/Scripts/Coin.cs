using UnityEngine;

// Collectible coin: spins, bobs, and is collected by CoinCollector (score +1).
public class Coin : MonoBehaviour
{
    public Transform visual;
    bool collected;
    float phase;

    void Awake() { phase = Random.value * 6.28f; }

    void Update()
    {
        if (visual == null) return;
        transform.Rotate(0f, 200f * Time.deltaTime, 0f, Space.Self);
        Vector3 lp = visual.localPosition;
        lp.y = Mathf.Sin(Time.time * 3f + phase) * 0.1f;
        visual.localPosition = lp;
    }

    public void Collect()
    {
        if (collected) return;
        collected = true;
        if (GameManager.Instance != null) GameManager.Instance.AddCoin(transform.position);
        Destroy(gameObject);
    }
}
