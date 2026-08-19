using UnityEngine;

public class movimentacao : MonoBehaviour
{
    public float velocidade = 5.0f;

    private Rigidbody rb;
    private Transform cameraTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Pega a referência da Câmera Principal automaticamente
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Trava o mouse no centro da tela e esconde o ponteiro
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float movimentoHorizontal = Input.GetAxis("Horizontal");
        float movimentoVertical = Input.GetAxis("Vertical");

        // Câmera está olhando
        Vector3 frenteCamera = cameraTransform.forward;
        Vector3 direitaCamera = cameraTransform.right;

        // Zeramos o Y para o personagem não tentar "voar" ou afundar no chão ao olhar para cima/baixo
        frenteCamera.y = 0f;
        direitaCamera.y = 0f;
        frenteCamera.Normalize();
        direitaCamera.Normalize();

        // movimento baseada na CÂMERA
        Vector3 direcao = (frenteCamera * movimentoVertical + direitaCamera * movimentoHorizontal).normalized;

        if (direcao.magnitude >= 0.1f)
        {
            // Mover na direção da câmera
            Vector3 deslocamento = direcao * velocidade * Time.deltaTime;
            rb.MovePosition(transform.position + deslocamento);

            // Girar o corpo do personagem do olhar na direção em que anda
            Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlvo, Time.deltaTime * 10f);
        }
    }
}