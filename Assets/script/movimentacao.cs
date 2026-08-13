using UnityEngine;

public class movimentacao : MonoBehaviour
{
    public float velocidade = 5.0f;
    public float forçaPulo = 7.0f;

    private Rigidbody rb;

    void Start()
    {
        // Pega o Rigidbody do personagem
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 1. Entradas de movimento (WASD / Setas)
        float movimentoHorizontal = Input.GetAxis("Horizontal");
        float movimentoVertical = Input.GetAxis("Vertical");

        // 2. Direção para onde o personagem vai andar
        Vector3 direcao = new Vector3(movimentoHorizontal, 0f, movimentoVertical).normalized;

        // 3. Movimentação e Rotação
        if (direcao.magnitude >= 0.1f)
        {
            Vector3 deslocamento = direcao * velocidade * Time.deltaTime;
            rb.MovePosition(transform.position + deslocamento);

            // Aponta o personagem para a direção que está andando
            Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlvo, Time.deltaTime * 10f);
        }

        // 4. Pulo Simples (Aperta Espaço e Pula)
        if (Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * forçaPulo, ForceMode.Impulse);
        }
    }
}