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
    [Header("Stomp / Events")]
    public float stompHeight = 0.4f; // altura relativa para considerar que el jugador pisa al enemigo

    // Eventos estáticos para desacoplar comportamiento (otros sistemas pueden suscribirse)
    public static System.Action<EnemySimpleMove, GameObject> OnEnemyStomped;
    public static System.Action<EnemySimpleMove, GameObject> OnEnemyHitPlayer;
    [Header("Collision tuning")]
    public float collisionBackoff = 0.25f; // cuánto retroceder al chocar
    public float collisionCooldown = 0.15f; // evitar múltiples reverses rápidos

    [Header("Jump")]
    public float jumpInterval = 5f; // segundos entre saltitos
    public float jumpImpulse = 3f; // impulso vertical si hay Rigidbody
    public float jumpFloatHeight = 0.25f; // altura del salto simulado si no hay Rigidbody
    public float jumpDuration = 0.35f; // duración total del salto simulado
    private float jumpTimer = 0f;
    private bool isJumping = false;

    void Start()
    {
        startPos = transform.position;
        direction = startRight ? 1 : -1;
        if (flipVisual) SetFacing(direction);
        rb = GetComponent<Rigidbody>();
        // Si hay Rigidbody, hacerlo kinemático para que el movimiento mediante transform.Translate
        // no genere fuerzas inesperadas que lancen al jugador. Usaremos salto simulado en lugar de física.
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Ajustar BoxCollider por defecto para evitar que el jugador pueda "subirse" encima
        BoxCollider bc = GetComponent<BoxCollider>();
        if (bc != null)
        {
            // Valores recomendados para humanoide pequeño; el usuario puede sobreescribirlos en el Inspector
            Vector3 recommendedSize = new Vector3(0.8f, 1.6f, 0.8f);
            Vector3 recommendedCenter = new Vector3(0f, 0.8f, 0f);
            // Aplicar solo si el tamaño actual es claramente incorrecto (muy ancho o muy bajo)
            if (bc.size.y < 0.9f || bc.size.x > 2.0f || bc.size.z > 2.0f)
            {
                bc.size = recommendedSize;
                bc.center = recommendedCenter;
            }
        }
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
        // ignore rapid collisions for non-player objects
        if (Time.time - lastCollisionTime < collisionCooldown && !collision.gameObject.CompareTag("Player"))
        {
            if (Time.time - lastCollisionTime < streakWindow)
                collisionStreak++;
            return;
        }

        if (collision.gameObject.CompareTag("Untagged"))
            return; // ignora objetos sin tag

        if (collision.gameObject.CompareTag("Suelo"))
            return; // ignora suelo

        GameObject other = collision.gameObject;

        // Si colisionamos con el jugador, comprobamos si fue un "stomp" (pisada)
        if (other.CompareTag("Player"))
        {
            bool stomp = false;
            // Preferir usar punto de contacto si está disponible
            if (collision.contacts != null && collision.contacts.Length > 0)
            {
                Vector3 contactPoint = collision.contacts[0].point;
                if (contactPoint.y > transform.position.y + stompHeight)
                    stomp = true;
            }
            else
            {
                // fallback: usar la posición del jugador
                float otherY = other.transform.position.y;
                if (otherY > transform.position.y + stompHeight)
                    stomp = true;
            }

            if (stomp)
            {
                // El jugador pisó al enemigo: notificar y eliminar enemigo
                OnEnemyStomped?.Invoke(this, other);
                Destroy(this.gameObject);
                return;
            }

            // Lateral: mitigar lanzamiento del jugador
            Rigidbody otherRb = other.GetComponent<Rigidbody>();
            if (otherRb != null)
            {
                otherRb.linearVelocity = Vector3.zero;
                otherRb.angularVelocity = Vector3.zero;
            }
            else
            {
                var cc = other.GetComponent<CharacterController>();
                if (cc != null)
                    cc.Move(Vector3.right * (-direction) * collisionBackoff);
            }

            // Notify hit and reverse direction immediately
            OnEnemyHitPlayer?.Invoke(this, other);
            ReverseDirection();
            lastCollisionTime = Time.time;
            collisionStreak = 0;
            return;
        }

        // Para objetos no-jugador: limpiar velocidad del enemigo y retroceder un poco
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (collision.contacts != null && collision.contacts.Length > 0)
        {
            Vector3 normal = collision.contacts[0].normal;
            transform.position += normal * collisionBackoff;
        }
        else
        {
            transform.position += Vector3.right * (-direction) * collisionBackoff;
        }

        // Manejar streak: invertir sólo si hay múltiples colisiones rápidas
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

    // También sirve si usas colliders tipo trigger
    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - lastCollisionTime < collisionCooldown && !other.CompareTag("Player"))
        {
            if (Time.time - lastCollisionTime < streakWindow)
                collisionStreak++;
            return;
        }

        if (other.CompareTag("Untagged"))
            return;

        if (other.CompareTag("Suelo"))
            return;

        // Si trigger con jugador
        if (other.CompareTag("Player"))
        {
            float otherY = other.transform.position.y;
            float myY = transform.position.y;
            if (otherY > myY + stompHeight)
            {
                OnEnemyStomped?.Invoke(this, other.gameObject);
                Destroy(this.gameObject);
                return;
            }

            Rigidbody otherRb = other.GetComponent<Rigidbody>();
            if (otherRb != null)
            {
                otherRb.linearVelocity = Vector3.zero;
                otherRb.angularVelocity = Vector3.zero;
            }
            else
            {
                var cc = other.GetComponent<CharacterController>();
                if (cc != null)
                    cc.Move(Vector3.right * (-direction) * collisionBackoff);
            }

            OnEnemyHitPlayer?.Invoke(this, other.gameObject);
            ReverseDirection();
            lastCollisionTime = Time.time;
            collisionStreak = 0;
            return;
        }

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
        // Usar salto simulado por transform para evitar física no deseada
        StartCoroutine(SimulatedJump(0f));
    }

    // Permite solicitar un salto con altura extra (por ejemplo cuando golpea al jugador)
    public void TriggerJump(float extraHeight)
    {
        if (!isJumping)
        {
            StartCoroutine(SimulatedJump(extraHeight));
        }
    }

    private System.Collections.IEnumerator SimulatedJump(float extraHeight)
    {
        isJumping = true;
        float half = jumpDuration * 0.5f;
        float t = 0f;
        Vector3 orig = transform.position;
        float height = jumpFloatHeight + extraHeight;
        while (t < half)
        {
            float frac = t / half;
            transform.position = Vector3.Lerp(orig, orig + Vector3.up * height, frac);
            t += Time.deltaTime;
            yield return null;
        }
        // descendir
        t = 0f;
        while (t < half)
        {
            float frac = t / half;
            transform.position = Vector3.Lerp(orig + Vector3.up * height, orig, frac);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = orig;
        isJumping = false;
    }
}
