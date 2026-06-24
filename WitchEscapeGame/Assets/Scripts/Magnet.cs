using UnityEngine;

public class Magnet : MonoBehaviour
{
    [Header("Magnet Settings")]
    public float magnetRadius = 8f;
    public float pullSpeed = 10f;

    private void Update()
    {
        PullCoins();
    }

    private void PullCoins()
    {
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");

        foreach (GameObject coin in coins)
        {
            if (coin == null)
                continue;

            float distance = Vector2.Distance(transform.position, coin.transform.position);

            if (distance <= magnetRadius)
            {
                coin.transform.position = Vector2.MoveTowards(
                    coin.transform.position,
                    transform.position,
                    pullSpeed * Time.deltaTime
                );
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }
}