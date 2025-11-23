using System.Collections;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button reiniciarButton;
    public Button menuButton;
    public GameObject playerObjectToDisable; // opcional: arrastra el jugador para deshabilitar controles
    [Header("Game Over UI Animation")]
    public CanvasGroup gameOverCanvasGroup;
    public float gameOverFadeDuration = 0.5f;
    public float gameOverStartScale = 0.85f;

    private bool gameOverActivo = false;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if(reiniciarButton != null)
           reiniciarButton.onClick.AddListener(ReiniciarEscena);
        
        if(menuButton != null)
           menuButton.onClick.AddListener(IrAlMenu);
        
            
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReiniciarEscena();
        }

        if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Escape))
        {
            IrAlMenu();
        }
        
    }

    public void GameOver()
    {
        if (gameOverActivo) return;
        gameOverActivo = true;
        // Preparar y mostrar panel con animación
        if (gameOverPanel != null)
        {
            // Aseguramos que el CanvasGroup permita interacción
            if (gameOverCanvasGroup != null)
            {
                gameOverCanvasGroup.alpha = 0f;
                gameOverCanvasGroup.interactable = true;
                gameOverCanvasGroup.blocksRaycasts = true;
            }

            // Hacer visible el cursor y desbloquearlo (útil si usas cursor lock)
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Deshabilitar controles del jugador si se asignó
            if (playerObjectToDisable != null)
            {
                var comps = playerObjectToDisable.GetComponents<MonoBehaviour>();
                foreach (var c in comps)
                {
                    c.enabled = false;
                }
            }

            // Mostrar texto de game over
            if (gameOverText != null)
                gameOverText.text = "GAME OVER - Eres muy malo jugando";

            // Pausar el juego (usamos coroutine con unscaledDeltaTime para animaciones)
            Time.timeScale = 0f;

            // Asegurar escala inicial
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.localScale = Vector3.one * gameOverStartScale;

            // Lanzar animación de fade/scale
            StartCoroutine(FadeInGameOver());
        }
    }

    private IEnumerator FadeInGameOver()
    {
        float t = 0f;
        while (t < gameOverFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / gameOverFadeDuration);
            if (gameOverCanvasGroup != null)
                gameOverCanvasGroup.alpha = p;
            if (gameOverPanel != null)
                gameOverPanel.transform.localScale = Vector3.Lerp(Vector3.one * gameOverStartScale, Vector3.one, p);
            yield return null;
        }

        if (gameOverCanvasGroup != null)
            gameOverCanvasGroup.alpha = 1f;
        if (gameOverPanel != null)
            gameOverPanel.transform.localScale = Vector3.one;

        // Seleccionar el botón Reiniciar para navegación con teclado/mandos
        if (reiniciarButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(reiniciarButton.gameObject);
        }
    }
    public void ReiniciarEscena()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void IrAlMenu()
    {
        Time.timeScale = 1f;
        string menuName = "Menu";
        if (SceneInBuild(menuName))
        {
            SceneManager.LoadScene(menuName);
        }
        else
        {
            Debug.LogError($"La escena '{menuName}' no está en Build Settings. Añádela en File -> Build Settings -> Scenes In Build.");
        }
    }

    // Comprueba si una escena por nombre está en Build Settings
    private bool SceneInBuild(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }
}

