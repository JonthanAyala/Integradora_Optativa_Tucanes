using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyEventsHandler : MonoBehaviour
{
    public AudioClip stompClip;
    public AudioClip hitClip;
    public int stompPoints = 100;

    [Header("VFX")]
    public ParticleSystem stompParticles;
    public ParticleSystem hitParticles;

    AudioSource src;

    void Awake()
    {
        src = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        EnemySimpleMove.OnEnemyStomped += HandleStomp;
        EnemySimpleMove.OnEnemyHitPlayer += HandleHitPlayer;
    }

    void OnDisable()
    {
        EnemySimpleMove.OnEnemyStomped -= HandleStomp;
        EnemySimpleMove.OnEnemyHitPlayer -= HandleHitPlayer;
    }

    void HandleStomp(EnemySimpleMove enemy, GameObject player)
    {
        // reproducir sonido en la posición del enemigo
        if (stompClip != null && src != null)
            src.PlayOneShot(stompClip);

        // partículas en la posición del enemigo
        if (stompParticles != null && enemy != null)
        {
            var p = Instantiate(stompParticles, enemy.transform.position, Quaternion.identity);
            p.Play();
            Destroy(p.gameObject, 2f);
        }

        // sumar puntos
        ScoreManager.AddPoints(stompPoints);
    }

    void HandleHitPlayer(EnemySimpleMove enemy, GameObject player)
    {
        if (hitClip != null && src != null)
            src.PlayOneShot(hitClip);

        if (hitParticles != null && enemy != null)
        {
            var p = Instantiate(hitParticles, enemy.transform.position, Quaternion.identity);
            p.Play();
            Destroy(p.gameObject, 2f);
        }

        // aquí podrías reducir vida del jugador, mostrar efecto, etc.
        Debug.Log($"Enemy hit player: {enemy.name}");
    }
}
