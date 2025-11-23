using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyDamageOnTouch : MonoBehaviour
{
    public int damage = 1;
    public bool useTrigger = false; 
    public string playerTag = "Player";
    [Header("Reaction")]
    // cuánto se empuja al jugador si no tiene Rigidbody (CharacterController)
    public float playerNudge = 1.0f;
    // cuánto extra saltito hará el enemigo cuando dañe al jugador (simulado)
    public float enemyExtraJump = 0.5f;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = useTrigger;
    }

    void OnCollisionEnter(Collision c)
    {
        if (useTrigger) return;
        if (!c.collider.CompareTag(playerTag)) return;

        // Evitar aplicar daño si el jugador está pisando (stomp) al enemigo.
        var enemyComp = GetComponentInParent<EnemySimpleMove>();
        float stompH = enemyComp != null ? enemyComp.stompHeight : 0.4f;
        if (c.contacts != null && c.contacts.Length > 0)
        {
            Vector3 contactPoint = c.contacts[0].point;
            if (contactPoint.y > transform.position.y + stompH)
            {
                // Se considera stomp: no aplicar daño aquí (otro componente manejará la muerte)
                return;
            }
        }

        var ph = c.collider.GetComponent<PlayerHealth>();
        if (ph != null && !ph.IsInvulnerable())
        {
            ph.TakeDamage(damage, transform.position);

            // Si el player no tiene Rigidbody (por ejemplo usa CharacterController), aplicar un pequeño nudge
            var prb = c.collider.GetComponent<Rigidbody>();
            if (prb == null)
            {
                var cc = c.collider.GetComponent<CharacterController>();
                if (cc != null)
                {
                    Vector3 pushDir = (c.collider.transform.position - transform.position).normalized;
                    pushDir.y = 0f;
                    cc.Move(pushDir * playerNudge);
                }
            }

            // Si este daño vino de un enemigo, decirle al enemigo que haga un salto extra
            var enemy = GetComponentInParent<EnemySimpleMove>();
            if (enemy != null)
            {
                enemy.TriggerJump(enemyExtraJump);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!useTrigger) return;
        if (!other.CompareTag(playerTag)) return;

        // Evitar daño si el jugador cae encima (stomp) — usar la posición del jugador como fallback
        var enemyComp = GetComponentInParent<EnemySimpleMove>();
        float stompH = enemyComp != null ? enemyComp.stompHeight : 0.4f;
        if (other.transform.position.y > transform.position.y + stompH)
        {
            return;
        }

        var ph = other.GetComponent<PlayerHealth>();
        if (ph != null && !ph.IsInvulnerable())
        {
            ph.TakeDamage(damage, transform.position);

            // Nudge CharacterController players if no Rigidbody
            var prb = other.GetComponent<Rigidbody>();
            if (prb == null)
            {
                var cc = other.GetComponent<CharacterController>();
                if (cc != null)
                {
                    Vector3 pushDir = (other.transform.position - transform.position).normalized;
                    pushDir.y = 0f;
                    cc.Move(pushDir * playerNudge);
                }
            }

            var enemy = GetComponentInParent<EnemySimpleMove>();
            if (enemy != null)
            {
                enemy.TriggerJump(enemyExtraJump);
            }
        }
    }
}
