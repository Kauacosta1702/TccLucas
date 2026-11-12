using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DiarioAtalho : MonoBehaviour
{
    [Tooltip("Posição desta fase na ordem do diário: 0 = primeira (Peru), 1 = segunda...")]
    public int indiceFase;
    public Key tecla = Key.L;
    public string cenaDiario = "DiarioProgresso";

    private void Start()
    {
        DiarioProgresso.aberto = false;
        DiarioProgresso.tecla = tecla;
        DiarioProgresso.Registrar(indiceFase);
    }

    private void Update()
    {
        if (DiarioProgresso.aberto || Keyboard.current == null) return;
        if (!Keyboard.current[tecla].wasPressedThisFrame) return;

        // Não abre durante um diálogo
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive()) return;

        Abrir();
    }

    private void Abrir()
    {
        DiarioProgresso.aberto = true;
        DiarioProgresso.timeScaleAnterior = Time.timeScale > 0 ? Time.timeScale : 1f;
        Time.timeScale = 0f; // Pausa o jogo enquanto o diário está aberto
        SceneManager.LoadSceneAsync(cenaDiario, LoadSceneMode.Additive);
    }
}