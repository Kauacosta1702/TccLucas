using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DiarioUI : MonoBehaviour
{
    public DiarioData dados;

    [Header("Elementos da página")]
    public TMP_Text textoComida;
    public Image imagemComida;
    public TMP_Text textoTitulo;
    public TMP_Text textoCorpo;
    public TMP_Text textoPagina;   // "2/5" (opcional)
    public Image imagem;

    [Header("Setas (só visuais)")]
    public GameObject setaEsquerda;
    public GameObject setaDireita;

    [Header("Teclas")]
    public Key teclaAnterior = Key.LeftArrow;
    public Key teclaProxima = Key.RightArrow;

    private int pagina, total;

    private void Start()
    {
        total = dados != null && dados.entradas != null
            ? Mathf.Min(dados.entradas.Length, DiarioProgresso.faseMaxima + 1) : 0;
        pagina = Mathf.Max(0, total - 1); // Abre na página do país atual
        Atualizar();
    }

    private void Update()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado == null) return;

        if (teclado[DiarioProgresso.tecla].wasPressedThisFrame)
        {
            Fechar();
            return;
        }
        if (teclado[teclaAnterior].wasPressedThisFrame) Mudar(-1);
        if (teclado[teclaProxima].wasPressedThisFrame) Mudar(1);
    }

    private void Mudar(int direcao)
    {
        int nova = Mathf.Clamp(pagina + direcao, 0, total - 1);
        if (nova == pagina) return;
        pagina = nova;
        Atualizar();
    }

    private void Atualizar()
    {
        if (total == 0) return;
        EntradaDiario e = dados.entradas[pagina];

        if (textoComida != null) textoComida.text = e.nomeComida;
        if (imagemComida != null)
        {
            imagemComida.sprite = e.imagemComida;
            imagemComida.gameObject.SetActive(e.imagemComida != null);
        }
        if (textoTitulo != null) textoTitulo.text = e.titulo;
        if (textoCorpo != null) textoCorpo.text = e.texto;
        if (textoPagina != null) textoPagina.text = $"{pagina + 1}/{total}";
        if (imagem != null)
        {
            imagem.sprite = e.imagem;
            imagem.gameObject.SetActive(e.imagem != null);
        }
        if (setaEsquerda != null) setaEsquerda.SetActive(pagina > 0);
        if (setaDireita != null) setaDireita.SetActive(pagina < total - 1);
    }

    private void Fechar()
    {
        Time.timeScale = DiarioProgresso.timeScaleAnterior;
        DiarioProgresso.aberto = false;
        if (SceneManager.sceneCount > 1)
            SceneManager.UnloadSceneAsync(gameObject.scene);
    }
}