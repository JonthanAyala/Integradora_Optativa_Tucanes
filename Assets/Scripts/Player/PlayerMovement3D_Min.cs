using UnityEngine;

public class PlayerMovement3D_Min : MonoBehaviour
{
    public Transform cameraTransform;
    [Header("Camera Follow (side-scroller)")]
    public Vector3 cameraOffset = new Vector3(0f, 2f, -10f);
    public float cameraFollowSpeed = 8f;
    public float movementSpeed = 5f;
    private CharacterController controller;
    
    public float jumpForce = 10f; // Fuerza vertical para un salto notable (aumentada)
    private Vector3 verticalVelocity = Vector3.zero; // Velocidad vertical (Gravedad + Salto)
    // -------------------------------------------
    
    public Animator animator;
    public readonly int movementSpeedHash = Animator.StringToHash("MovementSpeed");
    
    
// Start is called once before the first execution of Update after the MonoBehaviour created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Ya no se guarda normalHeight
        // Si no se asignó la cámara en el inspector, intentamos usar Camera.main como fallback
        if (cameraTransform == null)
        {
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning("PlayerMovement3D_Min: cameraTransform no está asignada y Camera.main es null. Se usará el transform del jugador como fallback.");
            }
        }

        // Si la cámara está como hija del jugador, la desapegamos para evitar que rote junto al jugador
        if (cameraTransform != null && cameraTransform.parent == transform)
        {
            cameraTransform.SetParent(null);
            Debug.Log("PlayerMovement3D_Min: cameraTransform estaba parented al jugador. Se desapegó para evitar rotaciones no deseadas.");
        }

        // Calcular offset por defecto si no se estableció explícitamente
        if (cameraTransform != null && cameraOffset == Vector3.zero)
        {
            cameraOffset = cameraTransform.position - transform.position;
        }

        // Asegurarse de tener un Animator si no fue asignado en el Inspector
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                Debug.LogWarning("PlayerMovement3D_Min: animator no asignado y no se encontró un Animator en el mismo GameObject.");
        }

        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
            if (controller == null)
                Debug.LogError("PlayerMovement3D_Min: No se encontró CharacterController en el GameObject. El movimiento no funcionará.");
        }
        else
        {
            // Reducir el Step Offset para evitar que el jugador suba sobre colliders pequeños
            controller.stepOffset = 0.15f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Obtener Inputs
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        // Usar la tecla Jump (por defecto espacio) para saltar en vez del eje Vertical
        bool jumpPressed = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);

        // --- MANEJO DE GRAVEDAD Y SALTO ---
        if (controller != null && controller.isGrounded)
        {
            // Reinicia la velocidad vertical para evitar acumulación
            verticalVelocity.y = -0.5f; // Un pequeño valor para asegurar que "toque" el suelo
            
            // Detectar Salto (tecla Jump/Space)
            if (jumpPressed)
            {
                verticalVelocity.y = jumpForce; // Aplicar fuerza de salto
            }
        }
        else
        {
            // Aplicar la gravedad (fuerza constante)
            verticalVelocity.y += Physics.gravity.y * Time.deltaTime;
        }
        
        // --- SE ELIMINÓ TODA LA LÓGICA DE AGACHADO ---
        // Se asegura que el CharacterController Center y Height se mantengan en sus valores iniciales
        // (los valores establecidos en el Inspector)
        
        // --- CÁLCULO DE LA DIRECCIÓN DE MOVIMIENTO HORIZONTAL (side-scroller) ---
        // Para un juego 3D con vista lateral, movemos al jugador a lo largo del eje X del mundo.
        var direction = Vector3.right * horizontalInput;
        
        // --- APLICAR MOVIMIENTO ---
        // Movimiento Horizontal (Solo se usa movementSpeed, ya que no hay agachado)
        Vector3 horizontalMove = direction.normalized * movementSpeed;
        
        // Movimiento Final (Horizontal + Vertical)
        Vector3 finalMovement = horizontalMove + verticalVelocity;

        // Mover el controlador (multiplicado por Time.deltaTime)
        if (controller != null)
            controller.Move(finalMovement * Time.deltaTime);

        // --- ROTACIÓN Y ANIMACIÓN ---
        if (direction != Vector3.zero)
        {
            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f );
        }
        
        if (animator != null)
            animator.SetFloat(movementSpeedHash, Mathf.Abs(horizontalInput));
    }

    // LateUpdate para mover la cámara detrás del jugador (sin rotarla)
    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Objetivo: que la cámara siga al jugador en X manteniendo su Y,Z
        Vector3 target = cameraTransform.position;
        target.x = transform.position.x + cameraOffset.x;
        // Opcional: también seguir Y si quieres (descomentar)
        // target.y = transform.position.y + cameraOffset.y;
        // Mantener Z para conseguir efecto lateral

        cameraTransform.position = Vector3.Lerp(cameraTransform.position, target, cameraFollowSpeed * Time.deltaTime);
    }
}