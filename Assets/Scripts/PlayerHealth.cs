using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHP = 3;
    public float invulnTime = 0.8f;   // tiempo invulnerable tras recibir golpe
    public float knockbackForce = 8f; // empujón al recibir golpe
    public float maxKnockbackSpeed = 6f;

    public int hp;
    float invulnTimer;
    Rigidbody rb;

    void Awake()
    {
        hp = maxHP;
        rb = GetComponent<Rigidbody>();
    }

    public void TakeDamage(int dmg, Vector3 hitFromDirection)
    {
        if (invulnTimer > 0f) return; // aún invulnerable

        hp -= dmg;
        invulnTimer = invulnTime;

        // Knockback (empujón en X/Z + un poco hacia arriba)
        if (rb != null)
        {
            Vector3 push = (transform.position - hitFromDirection).normalized;
            push.y = 0.25f; // levanta tantito
            // Resetear velocidad horizontal para un empujón consistente
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            rb.AddForce(push * knockbackForce, ForceMode.Impulse);

            // Limitar la velocidad horizontal resultante para evitar que el jugador salga disparado
            Vector3 lv = rb.linearVelocity;
            lv.x = Mathf.Clamp(lv.x, -maxKnockbackSpeed, maxKnockbackSpeed);
            lv.z = Mathf.Clamp(lv.z, -maxKnockbackSpeed, maxKnockbackSpeed);
            rb.linearVelocity = lv;
        }
        else
        {
            // Si no hay Rigidbody, intentar un pequeño nudge si existe CharacterController
            var cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                Vector3 push = (transform.position - hitFromDirection).normalized;
                push.y = 0f;
                cc.Move(push * 0.8f);
            }
        }

        Debug.Log($"Player HP: {hp}/{maxHP}");

        if (hp <= 0) Die();
    }

    void Die()
    {
        Debug.Log("PLAYER DEAD");
        // TODO: desactivar control, reproducir animación, recargar escena o Game Over
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (invulnTimer > 0f) invulnTimer -= Time.deltaTime;
    }

    public bool IsInvulnerable() => invulnTimer > 0f;
}
