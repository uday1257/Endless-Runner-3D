using UnityEngine;

// Attached to the Player. Same idea as the lab PDF: OnTriggerEnter collects coins.
public class CoinCollector : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Coin coin = other.GetComponent<Coin>();
        if (coin != null) coin.Collect();
    }
}
