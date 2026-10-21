using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class movimentacao : MonoBehaviour
{
    public float velocidade = 5.0f;
    public float velocidadeRotacao = 10f;
    public float gravidade = -20f;

    private CharacterController controller;
    private Animator anim;
    private Transform cameraTransform;
    private Vector3 direcaoMovimento;
    private float velocidadeVertical;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        bool emDialogo = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive();

        if (emDialogo || cameraTransform == null)
        {
            direcaoMovimento = Vector3.zero;
        }
        else
        {
            float movimentoHorizontal = Input.GetAxis("Horizontal");
            float movimentoVertical = Input.GetAxis("Vertical");

            Quaternion rotacaoCamera = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
            Vector3 frenteCamera = rotacaoCamera * Vector3.forward;
            Vector3 direitaCamera = rotacaoCamera * Vector3.right;

            direcaoMovimento = (frenteCamera * movimentoVertical + direitaCamera * movimentoHorizontal).normalized;

            Quaternion rotacaoAlvo = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlvo, Time.deltaTime * velocidadeRotacao);
        }

        // Gravidade
        if (controller.isGrounded && velocidadeVertical < 0f)
        {
            velocidadeVertical = -2f;
        }
        velocidadeVertical += gravidade * Time.deltaTime;

        Vector3 movimentoFinal = direcaoMovimento * velocidade;
        movimentoFinal.y = velocidadeVertical;

        controller.Move(movimentoFinal * Time.deltaTime);

        // Animação
        if (anim != null)
        {
            anim.SetBool("andando", direcaoMovimento.sqrMagnitude > 0.01f);
        }
    }
}