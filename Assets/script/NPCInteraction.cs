using Cozinha;
using Unity.Cinemachine;
using UnityEngine;

// Vai no NPC. Mostra o aviso "E" quando o jogador chega perto, inicia o diálogo
// e, se você quiser, abre o modo cozinha quando o diálogo termina.
public class NPCInteraction : MonoBehaviour
{
    [Header("Diálogo")]
    public DialogueData dialogueData;     // O diálogo que este NPC fala
    public GameObject interactionPrompt;  // O aviso "Aperte E" na tela
    public float interactionRange = 3f;   // Distância para o aviso aparecer

    [Header("Modo Cozinha (opcional)")]
    [Tooltip("Marque para abrir a cozinha depois do diálogo. Desmarcado, o NPC só conversa.")]
    public bool abrirCozinhaAposDialogo = false;
    [Tooltip("Marcado: o diálogo só acontece na primeira vez. Depois o E vai direto para a cozinha.")]
    public bool dialogoSoUmaVez = true;
    public CinemachineCamera cameraBancada; // Câmera virtual que olha para a mesa
    public CozinhaManager cozinhaManager;   // O "cérebro" da cozinha

    private Transform player;                       // O jogador (achado pela tag "Player")
    private bool playerInRange = false;             // O jogador está perto agora?
    private bool aguardandoFimDoDialogo = false;    // Estamos esperando o diálogo acabar para abrir a cozinha?
    private bool dialogoJaOcorreu = false;          // Já conversou uma vez?
    private bool emModoCozinha = false;             // Está na cozinha agora?

    void Start()
    {
        // Começa com o aviso escondido
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Procura o jogador na cena pela tag "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return; // Sem jogador, não há o que fazer

        // Existe DialogueManager E há um diálogo acontecendo agora?
        bool dialogoAtivo = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive();

        // Esperava o diálogo acabar para abrir a cozinha
        if (aguardandoFimDoDialogo && !dialogoAtivo)
        {
            aguardandoFimDoDialogo = false;
            EntrarModoCozinha();
            return; // Sai agora para não processar o E deste mesmo frame
        }

        // Dentro da cozinha: E sai
        if (emModoCozinha)
        {
            if (Input.GetKeyDown(KeyCode.E))
                SairModoCozinha();
            return; // Dentro da cozinha, nada abaixo precisa rodar
        }

        // Mede a distância entre o NPC e o jogador
        float distance = Vector3.Distance(transform.position, player.position);
        bool inRangeNow = distance <= interactionRange;

        // Detecta quando entra ou sai do alcance (só age quando MUDA, não todo frame)
        if (inRangeNow && !playerInRange)
        {
            playerInRange = true;
            // Mostra o aviso, mas não se já estiver falando
            if (interactionPrompt != null && !dialogoAtivo)
                interactionPrompt.SetActive(true);
        }
        else if (!inRangeNow && playerInRange)
        {
            playerInRange = false;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }

        // Perto + sem diálogo rolando + apertou E = interagir
        if (playerInRange && !dialogoAtivo && Input.GetKeyDown(KeyCode.E))
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);

            // Já falou antes e só quer cozinhar: vai direto
            if (abrirCozinhaAposDialogo && dialogoSoUmaVez && dialogoJaOcorreu)
            {
                EntrarModoCozinha();
                return;
            }

            // Sem DialogueManager ou sem diálogo configurado: mostra erro e para
            if (DialogueManager.Instance == null || dialogueData == null)
            {
                Debug.LogError("DialogueManager não encontrado ou DialogueData está vazio!", this);
                return;
            }

            dialogoJaOcorreu = true;
            // Só espera o fim do diálogo se for para abrir a cozinha depois
            aguardandoFimDoDialogo = abrirCozinhaAposDialogo;
            DialogueManager.Instance.StartDialogue(dialogueData);
        }
    }

    // Liga o modo cozinha: câmera na mesa, jogador parado, aviso "R"
    void EntrarModoCozinha()
    {
        emModoCozinha = true;
        DiarioProgresso.bloqueado = true; // Diário não abre durante a cozinha

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Prioridade maior = o Cinemachine passa a usar esta câmera (vai para a mesa)
        if (cameraBancada != null)
            cameraBancada.Priority = 20;

        SetMovimento(false); // Trava o jogador

        // Mostra o aviso "Aperte R para começar a cozinhar"
        if (cozinhaManager != null)
            cozinhaManager.AbrirMesa();
    }

    // Desliga o modo cozinha e devolve tudo ao normal
    void SairModoCozinha()
    {
        emModoCozinha = false;
        DiarioProgresso.bloqueado = false;

        if (cozinhaManager != null)
            cozinhaManager.Sair(); // Esconde a interface e para a receita

        // Prioridade menor = a câmera do jogador volta a ser usada
        if (cameraBancada != null)
            cameraBancada.Priority = 0;

        SetMovimento(true); // Libera o jogador
    }

    // Liga ou desliga o script de movimento do jogador
    void SetMovimento(bool ligado)
    {
        if (player == null) return;
        var movScript = player.GetComponent<movimentacao>(); // Pega o script "movimentacao"
        if (movScript != null)
            movScript.enabled = ligado; // enabled = false faz o script parar de rodar
    }
}