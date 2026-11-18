using UnityEngine;

// Obriga o objeto a ter um CharacterController (a Unity adiciona sozinha se faltar)
[RequireComponent(typeof(CharacterController))]
public class movimentacao : MonoBehaviour
{
    public float velocidade = 5.0f;        // Velocidade de andar
    public float velocidadeRotacao = 10f;  // Quão rápido o personagem vira para onde a câmera olha
    public float gravidade = -20f;         // Força da gravidade (negativa = puxa para baixo)

    private CharacterController controller; // Componente que move o personagem e detecta colisão
    private Animator anim;                  // Controla as animações (andando / parado)
    private Transform cameraTransform;      // A câmera, para andar "para onde ela olha"
    private Vector3 direcaoMovimento;       // Para onde o jogador quer ir agora
    private float velocidadeVertical;       // Velocidade de queda/pulo

    void Start()
    {
        // Pega os componentes que estão neste mesmo objeto
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        // Pega a Main Camera
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Trava e esconde o mouse durante o jogo
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Existe DialogueManager e há diálogo acontecendo?
        bool emDialogo = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive();

        if (emDialogo || cameraTransform == null)
        {
            // Durante o diálogo (ou sem câmera), o personagem fica parado
            direcaoMovimento = Vector3.zero;
        }
        else
        {
            // Lê as teclas: A/D (ou setas) = horizontal, W/S = vertical. Valores de -1 a 1.
            float movimentoHorizontal = Input.GetAxis("Horizontal");
            float movimentoVertical = Input.GetAxis("Vertical");

            // Pega só o giro da câmera de lado (eixo Y), ignorando se ela olha para cima/baixo
            Quaternion rotacaoCamera = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
            Vector3 frenteCamera = rotacaoCamera * Vector3.forward; // "Frente" segundo a câmera
            Vector3 direitaCamera = rotacaoCamera * Vector3.right;  // "Direita" segundo a câmera

            // Combina as teclas com as direções da câmera. normalized = tamanho 1,
            // para andar na diagonal não ficar mais rápido.
            direcaoMovimento = (frenteCamera * movimentoVertical + direitaCamera * movimentoHorizontal).normalized;

            // O personagem vira, aos poucos, para onde a câmera olha
            Quaternion rotacaoAlvo = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
            // Slerp = gira suavemente da rotação atual até a desejada
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlvo, Time.deltaTime * velocidadeRotacao);
        }

        // Gravidade
        // Se está no chão e caindo, "gruda" levemente no chão (-2) em vez de acumular velocidade
        if (controller.isGrounded && velocidadeVertical < 0f)
        {
            velocidadeVertical = -2f;
        }
        // A gravidade aumenta a velocidade de queda a cada segundo
        velocidadeVertical += gravidade * Time.deltaTime;

        // Junta o movimento horizontal (andar) com o vertical (gravidade)
        Vector3 movimentoFinal = direcaoMovimento * velocidade;
        movimentoFinal.y = velocidadeVertical;

        // Move o personagem. Time.deltaTime deixa a velocidade igual em qualquer computador.
        controller.Move(movimentoFinal * Time.deltaTime);

        // Animação
        // Liga a animação de andar se há movimento (sqrMagnitude > 0.01 = está se mexendo)
        if (anim != null)
        {
            anim.SetBool("andando", direcaoMovimento.sqrMagnitude > 0.01f);
        }
    }
}