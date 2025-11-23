using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemySimpleMove : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 3f;            // Velocidad de movimiento
    public float moveDistance = 3f;     // Distancia máxima desde el punto inicial
    public bool startRight = true;      // Dirección inicial (true = derecha)
    public bool flipVisual = true;      // Si quieres que se voltee visualmente

    private Vector3 startPos;
    private int direction;              // 1 = derecha, -1 = izquierda
    private Rigidbody rb;
    private float lastCollisionTime = -1f;
    private int collisionStreak = 0;
    public float streakWindow = 0.5f; // tiempo para contar colisiones consecutivas
    [Header("Collision tuning")]
    public float collisionBackoff = 0.25f; // cuánto retroceder al chocar
    public float collisionCooldown = 0.15f; // evitar múltiples reverses rápidos

    [Header("Jump")]
    public float jumpInterval = 5f; // segundos entre saltitos
    public float jumpImpulse = 3f; // impulso vertical si hay Rigidbody
    public float jumpFloatHeight = 0.25f; // altura del salto simulado si no hay Rigidbody
    public float jumpDuration = 0.35f; // duración total del salto simulado
    private float jumpTimer = 0f;

    void Start()
    {
        startPos = transform.position;
        direction = startRight ? 1 : -1;
        if (flipVisual) SetFacing(direction);
        rb = GetComponent<Rigidbody>();
        jumpTimer = jumpInterval;
    }

    void Update()
    {
        // Movimiento lateral simple (en espacio mundial)
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime, Space.World);

        // Jump timer
        jumpTimer -= Time.deltaTime;
        if (jumpTimer <= 0f)
        {
            StartJump();
            jumpTimer = jumpInterval;
        }

        // Cambiar dirección si alcanza el límite de distancia
        if (Mathf.Abs(transform.position.x - startPos.x) >= moveDistance)
        {
            ReverseDirection();
        }
    }

    void ReverseDirection()
    {
        direction *= -1;
        if (flipVisual) SetFacing(direction);
    }

    void SetFacing(int d)
    {
        transform.rotation = Quaternion.Euler(0f, d == 1 ? 0f : 180f, 0f);
    }

    // Cambia de dirección si choca con algo que tenga tag (y no esté Untagged)
    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastCollisionTime < collisionCooldown)
        {
            // incrementar streak si es muy seguida
            if (Time.time - lastCollisionTime < streakWindow)
                collisionStreak++;
            return;
        }

        if (collision.gameObject.CompareTag("Untagged"))
            return; // ignora objetos sin tag

        if (collision.gameObject.CompareTag("Suelo"))
            return; // ignora suelo

        // Si colisionamos con el jugador, tratar de reducir su velocidad para evitar que salga disparado
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();
            if (otherRb != null)
            {
                otherRb.velocity = Vector3.zero;
                otherRb.angularVelocity = Vector3.zero;
            }
            else
            {
                // Si el jugador usa CharacterController, intentar moverlo ligeramente hacia atrás
                var cc = collision.gameObject.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.Move(Vector3.right * (-direction) * collisionBackoff);
                }
            }
        }

        // Evitar rebotes usando física: si tenemos Rigidbody, limpiamos velocidad del enemigo
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Intentar retroceder ligeramente usando la normal del contacto principal
        if (collision.contacts != null && collision.contacts.Length > 0)
        {
            Vector3 normal = collision.contacts[0].normal;
            transform.position += normal * collisionBackoff;
        }
        else
        {
            // fallback: retroceder en la dirección opuesta al movimiento
            transform.position += Vector3.right * (-direction) * collisionBackoff;
        }

        // Manejar streak de colisiones: si se colisiona varias veces en un corto periodo, invertir
        if (Time.time - lastCollisionTime < streakWindow)
        {
            collisionStreak++;
        }
        else
        {
            collisionStreak = 1;
        }

        if (collisionStreak >= 2)
        {
            ReverseDirection();
            collisionStreak = 0;
        }

        lastCollisionTime = Time.time;
    }

    // También sirve si usas colliders tipo trigger
    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - lastCollisionTime < collisionCooldown)
        {
            if (Time.time - lastCollisionTime < streakWindow)
                collisionStreak++;
            return;
        }

        if (other.CompareTag("Untagged"))
            return;

        if (other.CompareTag("Suelo"))
            return;

        // Si colisionamos con el jugador por trigger, intentar mitigar su velocidad
        if (other.CompareTag("Player"))
        {
            Rigidbody otherRb = other.GetComponent<Rigidbody>();
            if (otherRb != null)
            {
                otherRb.velocity = Vector3.zero;
                otherRb.angularVelocity = Vector3.zero;
            }
            else
            {
                var cc = other.GetComponent<CharacterController>();
                if (cc != null)
                    cc.Move(Vector3.right * (-direction) * collisionBackoff);
            }
        }

        // retroceder ligeramente
        transform.position += Vector3.right * (-direction) * collisionBackoff;

        if (Time.time - lastCollisionTime < streakWindow)
            collisionStreak++;
        else
            collisionStreak = 1;

        if (collisionStreak >= 2)
        {
            ReverseDirection();
            collisionStreak = 0;
        }

        lastCollisionTime = Time.time;
    }

    private void StartJump()
    {
        // Si tenemos Rigidbody, aplicamos un impulso hacia arriba
        if (rb != null && !rb.isKinematic)
        {
            rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
        }
        else
        {
            // Simular salto local sin Rigidbody
            StartCoroutine(SimulatedJump());
        }
    }

    private System.Collections.IEnumerator SimulatedJump()
    {
        float half = jumpDuration * 0.5f;
        float t = 0f;
        Vector3 orig = transform.position;
        while (t < half)
        {
            float frac = t / half;
            transform.position = Vector3.Lerp(orig, orig + Vector3.up * jumpFloatHeight, frac);
            t += Time.deltaTime;
            yield return null;
        }
        // descendir
        t = 0f;
        while (t < half)
        {
            float frac = t / half;
            transform.position = Vector3.Lerp(orig + Vector3.up * jumpFloatHeight, orig, frac);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = orig;
    }
}
