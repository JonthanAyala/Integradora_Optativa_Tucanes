using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyDamageOnTouch : MonoBehaviour
{
    public int damage = 1;
    public bool useTrigger = false; 
    public string playerTag = "Player";

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = useTrigger;
    }

    void OnCollisionEnter(Collision c)
    {
        if (useTrigger) return;
        if (!c.collider.CompareTag(playerTag)) return;

        var ph = c.collider.GetComponent<PlayerHealth>();
        if (ph != null && !ph.IsInvulnerable())
        {
            ph.TakeDamage(damage, transform.position);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!useTrigger) return;
        if (!other.CompareTag(playerTag)) return;

        var ph = other.GetComponent<PlayerHealth>();
        if (ph != null && !ph.IsInvulnerable())
        {
            ph.TakeDamage(damage, transform.position);
        }
    }
}
