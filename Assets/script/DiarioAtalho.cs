using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DiarioAtalho : MonoBehaviour
{
    [Tooltip("Posição desta fase na ordem do diário: 0 = primeira (Peru), 1 = segunda...")]
    public int indiceFase;
    public Key tecla = Key.D;
    public string cenaDiario = "Diario";

    [Header("Ícone na tela")]
    [Tooltip("Objetos escondidos durante diálogo, modo cozinha e com o diário aberto (ícone e texto da tecla). Não coloque aqui o objeto que tem este script.")]
    public GameObject[] elementosDoIcone;

    private bool iconeVisivel = true;

    private void Start()
    {
        DiarioProgresso.aberto = false;
        DiarioProgresso.bloqueado = false;
        DiarioProgresso.tecla = tecla;
        DiarioProgresso.Registrar(indiceFase);
    }

    private void Update()
    {
        bool dialogoAtivo = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive();

        // Mostra o ícone só quando o diário pode ser usado
        bool podeUsar = !DiarioProgresso.aberto && !DiarioProgresso.bloqueado && !dialogoAtivo;
        AtualizarIcone(podeUsar);

        if (!podeUsar || Keyboard.current == null) return;

        // Ignora a tecla no mesmo frame em que o diário acabou de fechar
        if (Time.frameCount <= DiarioProgresso.frameFechado) return;

        if (!Keyboard.current[tecla].wasPressedThisFrame) return;

        Abrir();
    }

    private void AtualizarIcone(bool visivel)
    {
        if (visivel == iconeVisivel) return;
        iconeVisivel = visivel;

        if (elementosDoIcone == null) return;
        foreach (GameObject elemento in elementosDoIcone)
            if (elemento != null) elemento.SetActive(visivel);
    }

    private void Abrir()
    {
        // Não abre de novo se a cena do diário ainda está carregada
        if (SceneManager.GetSceneByName(cenaDiario).isLoaded) return;

        DiarioProgresso.aberto = true;
        DiarioProgresso.timeScaleAnterior = Time.timeScale > 0 ? Time.timeScale : 1f;
        Time.timeScale = 0f; // Pausa o jogo enquanto o diário está aberto
        SceneManager.LoadSceneAsync(cenaDiario, LoadSceneMode.Additive);
    }
}