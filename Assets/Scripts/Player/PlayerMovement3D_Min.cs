using UnityEngine;

public class PlayerMovement3D_Min : MonoBehaviour
{
    public Transform cameraTransform;
    public float movementSpeed = 5f;
    private CharacterController controller;
    
    // --- VARIABLES DE SALTO ---
    public float jumpForce = 8f; // Fuerza vertical para un salto notable
    // Ya no se necesitan crouchHeight, normalHeight, isCrouching, etc.
    private Vector3 verticalVelocity = Vector3.zero; // Velocidad vertical (Gravedad + Salto)
    // -------------------------------------------
    
    public Animator animator;
    public readonly int movementSpeedHash = Animator.StringToHash("MovementSpeed");
    
    
// Start is called once before the first execution of Update after the MonoBehaviour created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Ya no se guarda normalHeight
    }

    // Update is called once per frame
    void Update()
    {
        // Obtener Inputs
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var verticalInput = Input.GetAxisRaw("Vertical"); // Se usará solo para el Salto

        // --- MANEJO DE GRAVEDAD Y SALTO ---
        if (controller.isGrounded)
        {
            // Reinicia la velocidad vertical para evitar acumulación
            verticalVelocity.y = -0.5f; // Un pequeño valor para asegurar que "toque" el suelo
            
            // Detectar Salto (Input Vertical Positivo)
            if (verticalInput > 0.1f)
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
        
        // --- CÁLCULO DE LA DIRECCIÓN DE MOVIMIENTO HORIZONTAL ---
        
        // Solo usamos el input HORIZONTAL (horizontalInput) para la dirección en XZ.
        var cameraForward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z);
        var cameraRight = new Vector3(cameraTransform.right.x, 0, cameraTransform.right.z);
        
        var direction = cameraRight * horizontalInput;
        
        // --- APLICAR MOVIMIENTO ---
        
        // Movimiento Horizontal (Solo se usa movementSpeed, ya que no hay agachado)
        Vector3 horizontalMove = direction.normalized * movementSpeed;
        
        // Movimiento Final (Horizontal + Vertical)
        Vector3 finalMovement = horizontalMove + verticalVelocity;

        // Mover el controlador (multiplicado por Time.deltaTime)
        controller.Move(finalMovement * Time.deltaTime);

        // --- ROTACIÓN Y ANIMACIÓN ---
        if (direction != Vector3.zero)
        {
         var targetRotation = Quaternion.LookRotation(direction);
         transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f );
        }
        
        animator.SetFloat(movementSpeedHash, Mathf.Abs(horizontalInput));
    }
}