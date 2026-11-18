using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.Cinemachine;

// Controla a caixa de diálogo: mostra o nome, o retrato e o texto "digitando" letra por letra.
// Existe um só na cena, e qualquer script pode chamá-lo.
public class DialogueManager : MonoBehaviour
{
    // Singleton: guarda "o único DialogueManager", acessível de qualquer script
    // com DialogueManager.Instance
    public static DialogueManager Instance;

    // Evento: outros scripts podem "assinar" para serem avisados quando o diálogo acabar
    public static event System.Action OnDialogueFinished;

    [Header("Referências de UI")]
    public GameObject dialogueBox;  // A caixa inteira de diálogo
    public TMP_Text nameText;       // Nome de quem fala
    public TMP_Text dialogueText;   // O texto da fala
    public Image portraitImage;     // Retrato do personagem

    [Header("Configurações")]
    public float typingSpeed = 0.03f; // Segundos entre uma letra e outra

    [Header("Controle de Câmera")]
    // Trava o giro da câmera com o mouse enquanto o diálogo está aberto
    public CinemachineInputAxisController cameraInputController;

    private DialogueLine[] currentLines; // Falas do diálogo atual
    private int currentIndex = 0;        // Qual fala estamos mostrando
    private bool isTyping = false;       // O texto ainda está sendo "digitado"?

    // Awake roda antes do Start. Aqui registra este objeto como o Singleton.
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Começa com a caixa escondida
        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }

    void Update()
    {
        // Se a caixa não existe ou está escondida, não há nada a fazer
        if (dialogueBox == null || !dialogueBox.activeSelf) return;

        // Espaço ou E avança o diálogo
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                // Ainda digitando: completa o texto na hora (pula a animação)
                StopAllCoroutines();
                dialogueText.text = currentLines[currentIndex].text;
                isTyping = false;
            }
            else
            {
                // Já terminou de digitar: vai para a próxima fala
                NextLine();
            }
        }
    }

    // Começa um diálogo. Chamado pelo NPCInteraction.
    public void StartDialogue(DialogueData data)
    {
        currentLines = data.lines; // Pega as falas do arquivo de dados
        currentIndex = 0;          // Começa pela primeira
        if (dialogueBox != null)
            dialogueBox.SetActive(true);

        // Trava o giro da câmera
        if (cameraInputController != null)
            cameraInputController.enabled = false;

        // Libera o mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowLine();
    }

    // Mostra a fala atual (nome, retrato e texto)
    void ShowLine()
    {
        DialogueLine line = currentLines[currentIndex];
        nameText.text = line.characterName;

        if (portraitImage != null)
        {
            if (line.portrait != null)
            {
                portraitImage.sprite = line.portrait;
                portraitImage.enabled = true;  // Mostra o retrato
            }
            else
            {
                portraitImage.enabled = false; // Fala sem retrato: esconde a imagem
            }
        }

        StopAllCoroutines();              // Cancela a digitação anterior, se houver
        StartCoroutine(TypeText(line.text)); // Começa a digitar a nova fala
    }

    // Corrotina: escreve o texto uma letra por vez (efeito de máquina de escrever)
    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in text) // Passa por cada letra do texto
        {
            dialogueText.text += c;                        // Adiciona a letra
            yield return new WaitForSeconds(typingSpeed);  // Espera um pouquinho
        }
        isTyping = false; // Terminou de digitar
    }

    // Vai para a próxima fala, ou termina se acabou
    void NextLine()
    {
        currentIndex++;
        if (currentIndex < currentLines.Length)
        {
            ShowLine(); // Ainda há falas
        }
        else
        {
            EndDialogue(); // Era a última
        }
    }

    // Fecha o diálogo e devolve o controle ao jogador
    void EndDialogue()
    {
        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        // Destrava o giro da câmera
        if (cameraInputController != null)
            cameraInputController.enabled = true;

        // Trava e esconde o mouse de novo
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Avisa quem estiver "ouvindo" que o diálogo acabou (o "?." evita erro se ninguém ouvir)
        OnDialogueFinished?.Invoke();
    }

    // Outros scripts perguntam: "tem diálogo rolando agora?"
    public bool IsDialogueActive()
    {
        return dialogueBox != null && dialogueBox.activeSelf;
    }
}