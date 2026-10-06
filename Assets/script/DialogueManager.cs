using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.Cinemachine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    // EVENTO QUE AVISA QUANDO O DIÁLOGO ACABA
    public static event System.Action OnDialogueFinished;

    [Header("Referências de UI")]
    public GameObject dialogueBox;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Image portraitImage;

    [Header("Configurações")]
    public float typingSpeed = 0.03f;

    [Header("Controle de Câmera")]
    public CinemachineInputAxisController cameraInputController;

    private DialogueLine[] currentLines;
    private int currentIndex = 0;
    private bool isTyping = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }

    void Update()
    {
        if (dialogueBox == null || !dialogueBox.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = currentLines[currentIndex].text;
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue(DialogueData data)
    {
        currentLines = data.lines;
        currentIndex = 0;
        if (dialogueBox != null)
            dialogueBox.SetActive(true);

        if (cameraInputController != null)
            cameraInputController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowLine();
    }

    void ShowLine()
    {
        DialogueLine line = currentLines[currentIndex];
        nameText.text = line.characterName;

        if (portraitImage != null)
        {
            if (line.portrait != null)
            {
                portraitImage.sprite = line.portrait;
                portraitImage.enabled = true;
            }
            else
            {
                portraitImage.enabled = false;
            }
        }

        StopAllCoroutines();
        StartCoroutine(TypeText(line.text));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    void NextLine()
    {
        currentIndex++;
        if (currentIndex < currentLines.Length)
        {
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        if (cameraInputController != null)
            cameraInputController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // DISPARA O EVENTO INFORMANDO QUE O DIÁLOGO ACABOU
        OnDialogueFinished?.Invoke();
    }

    public bool IsDialogueActive()
    {
        return dialogueBox != null && dialogueBox.activeSelf;
    }
}