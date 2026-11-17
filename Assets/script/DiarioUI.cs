using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Vai na Canvas da cena do diário. Mostra as páginas e cuida de virar e fechar.
public class DiarioUI : MonoBehaviour
{
    public DiarioData dados; // O arquivo com os textos e imagens de cada fase

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

    private int pagina, total; // pagina = página atual (começa em 0), total = quantas páginas estão liberadas

    private void Start()
    {
        // Quantas páginas mostrar: o menor entre as que existem e as liberadas até a fase atual
        // (o "? :" é um if curto: se há dados, calcula; senão, zero)
        total = dados != null && dados.entradas != null
            ? Mathf.Min(dados.entradas.Length, DiarioProgresso.faseMaxima + 1) : 0;
        pagina = Mathf.Max(0, total - 1); // Abre na página do país atual
        Atualizar();
    }

    private void Update()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado == null) return; // Sem teclado, não faz nada

        // Apertou a tecla do diário: fecha
        if (teclado[DiarioProgresso.tecla].wasPressedThisFrame)
        {
            Fechar();
            return;
        }
        // Setas do teclado viram a página (-1 = volta, +1 = avança)
        if (teclado[teclaAnterior].wasPressedThisFrame) Mudar(-1);
        if (teclado[teclaProxima].wasPressedThisFrame) Mudar(1);
    }

    // Muda de página, sem passar da primeira nem da última
    private void Mudar(int direcao)
    {
        // Clamp = "prende" o número entre um mínimo e um máximo
        int nova = Mathf.Clamp(pagina + direcao, 0, total - 1);
        if (nova == pagina) return; // Não mudou (já está no limite): nada a fazer
        pagina = nova;
        Atualizar(); // Redesenha a página
    }

    // Coloca na tela os dados da página atual
    private void Atualizar()
    {
        if (total == 0) return; // Nenhuma página para mostrar
        EntradaDiario e = dados.entradas[pagina]; // Pega a "ficha" da página

        // Cada "!= null" evita erro se você deixou um campo vazio no Inspector
        if (textoComida != null) textoComida.text = e.nomeComida;
        if (imagemComida != null)
        {
            imagemComida.sprite = e.imagemComida;
            // Esconde a imagem se a página não tiver uma foto
            imagemComida.gameObject.SetActive(e.imagemComida != null);
        }
        if (textoTitulo != null) textoTitulo.text = e.titulo;
        if (textoCorpo != null) textoCorpo.text = e.texto;
        // Mostra "2/5" (+1 porque a contagem interna começa em 0)
        if (textoPagina != null) textoPagina.text = $"{pagina + 1}/{total}";
        if (imagem != null)
        {
            imagem.sprite = e.imagem;
            imagem.gameObject.SetActive(e.imagem != null);
        }
        // Seta da esquerda só aparece se há página anterior; a da direita, se há próxima
        if (setaEsquerda != null) setaEsquerda.SetActive(pagina > 0);
        if (setaDireita != null) setaDireita.SetActive(pagina < total - 1);
    }

    // Fecha o diário e devolve o jogo ao normal
    private void Fechar()
    {
        Time.timeScale = DiarioProgresso.timeScaleAnterior; // Despausa o jogo
        DiarioProgresso.aberto = false;
        // Marca o frame em que fechou, para o DiarioAtalho não reabrir no mesmo instante
        DiarioProgresso.frameFechado = Time.frameCount;
        // Só descarrega se há mais de uma cena aberta (evita erro ao testar a cena sozinha)
        if (SceneManager.sceneCount > 1)
            SceneManager.UnloadSceneAsync(gameObject.scene); // Fecha só a cena do diário
    }
}
