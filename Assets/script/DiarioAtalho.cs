using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Vai em CADA fase (dentro do prefab DiarioHUD). Cuida do ícone e de ABRIR o diário.
public class DiarioAtalho : MonoBehaviour
{
    [Tooltip("Posição desta fase na ordem do diário: 0 = primeira (Peru), 1 = segunda...")]
    public int indiceFase;               // Muda em cada fase: é o que libera as páginas
    public Key tecla = Key.D;            // Tecla que abre o diário
    public string cenaDiario = "Diario"; // Nome exato da cena do diário

    [Header("Ícone na tela")]
    [Tooltip("Objetos escondidos durante diálogo, modo cozinha e com o diário aberto (ícone e texto da tecla). Não coloque aqui o objeto que tem este script.")]
    public GameObject[] elementosDoIcone; // Ícone e texto "Diario Tecla L"

    private bool iconeVisivel = true;    // Lembra se o ícone está ligado, para não ligar/desligar à toa

    private void Start()
    {
        // Começo de fase: tudo em ordem
        DiarioProgresso.aberto = false;
        DiarioProgresso.bloqueado = false;
        DiarioProgresso.tecla = tecla;                 // Avisa ao diário qual tecla fecha também
        DiarioProgresso.Registrar(indiceFase);         // Libera as páginas até esta fase
    }

    private void Update()
    {
        // Existe um DialogueManager E há diálogo acontecendo?
        bool dialogoAtivo = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive();

        // Mostra o ícone só quando o diário pode ser usado
        bool podeUsar = !DiarioProgresso.aberto && !DiarioProgresso.bloqueado && !dialogoAtivo;
        AtualizarIcone(podeUsar);

        // Se não pode usar (ou não há teclado), para aqui
        if (!podeUsar || Keyboard.current == null) return;

        // Ignora a tecla no mesmo frame em que o diário acabou de fechar
        // (senão ele fecharia e abriria de novo no mesmo instante)
        if (Time.frameCount <= DiarioProgresso.frameFechado) return;

        // Só continua no instante em que a tecla é apertada
        if (!Keyboard.current[tecla].wasPressedThisFrame) return;

        Abrir();
    }

    // Liga ou desliga o ícone, mas só quando o estado muda
    private void AtualizarIcone(bool visivel)
    {
        if (visivel == iconeVisivel) return; // Já está como deveria: não faz nada
        iconeVisivel = visivel;

        if (elementosDoIcone == null) return;
        // Passa por cada elemento da lista e liga/desliga
        foreach (GameObject elemento in elementosDoIcone)
            if (elemento != null) elemento.SetActive(visivel);
    }

    private void Abrir()
    {
        // Não abre de novo se a cena do diário ainda está carregada
        if (SceneManager.GetSceneByName(cenaDiario).isLoaded) return;

        DiarioProgresso.aberto = true;
        // Guarda a velocidade atual do jogo para restaurar ao fechar (usa 1 se já estiver pausado)
        DiarioProgresso.timeScaleAnterior = Time.timeScale > 0 ? Time.timeScale : 1f;
        Time.timeScale = 0f; // Pausa o jogo enquanto o diário está aberto

        // Additive = abre a cena do diário POR CIMA da fase, sem fechar a fase
        // Async = carrega em segundo plano, sem travar o jogo
        SceneManager.LoadSceneAsync(cenaDiario, LoadSceneMode.Additive);
    }
}
