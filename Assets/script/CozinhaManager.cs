using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cozinha
{
    public class CozinhaManager : MonoBehaviour
    {
        [Header("Objetos da cena")]
        public Transform jogador;
        public Transform mesa;
        public CinemachineCamera cameraCozinha;
        public AnimacaoCozinha animacao;
        [Header("Receita")]
        public Ingrediente[] ordemIngredientes;
        [Min(1)] public float tempoLimite = 60;
        [Min(0.1f)] public float distanciaParaIniciar = 2.5f;
        [Header("Controles")]
        public Key teclaIniciar = Key.R;
        [Header("Interface")]
        public TMP_Text textoTempo;
        public TMP_Text textoMensagem;
        [Tooltip("Aviso 'Aperte R para começar a cozinhar' (opcional)")]
        public GameObject avisoIniciar;
        [Tooltip("Imagem com a ordem dos ingredientes (opcional)")]
        public GameObject imagemOrdem;

        private bool cozinhando, terminou, animando, aguardandoInicio;
        private int etapa;
        private float tempoRestante, duracaoMensagem;
        private Ingrediente ingredienteEmMovimento;
        private Camera cameraPrincipal;

        private void Start()
        {
            cameraPrincipal = Camera.main;

            if (!ConfiguracaoValida())
            {
                Debug.LogError("Confira as referências, a ordem e os modelos da cozinha.", this);
                enabled = false;
                return;
            }
            textoTempo.gameObject.SetActive(false);
            Mostrar(avisoIniciar, false);
            Mostrar(imagemOrdem, false);
            animacao.Cancelar();
            animacao.MostrarModelo(0);
        }

        private void Update()
        {
            bool pertoDaMesa = Vector3.Distance(jogador.position, mesa.position) <= distanciaParaIniciar;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame) Sair();
                if (Keyboard.current[teclaIniciar].wasPressedThisFrame && pertoDaMesa &&
                    (aguardandoInicio || (cozinhando && terminou)))
                    Iniciar();
            }
            if (!cozinhando)
            {
                textoMensagem.text = "";
                return;
            }
            if (terminou || Time.timeScale == 0) return;

            tempoRestante = Mathf.Max(0, tempoRestante - Time.deltaTime);
            int segundos = Mathf.CeilToInt(tempoRestante);
            textoTempo.text = $"{segundos / 60:00}:{segundos % 60:00}";
            if (tempoRestante == 0)
            {
                CancelarAdicao();
                terminou = true;
                Mensagem($"Tempo esgotado! {teclaIniciar} para tentar novamente.");
                return;
            }
            if (duracaoMensagem > 0)
            {
                duracaoMensagem -= Time.deltaTime;
                if (duracaoMensagem <= 0) textoMensagem.text = "";
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !animando)
            {
                // A CinemachineCamera não renderiza nada, quem renderiza é a Main Camera (com o Brain).
                Ray raio = cameraPrincipal.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(raio, out RaycastHit acerto, 100))
                {
                    Ingrediente ingrediente = acerto.collider.GetComponentInParent<Ingrediente>();
                    if (ingrediente != null) ingrediente.Clicar();
                }
            }
        }

        // Chame quando o jogador entrar no modo da mesa (ex.: depois do diálogo).
        // Mostra o aviso "Aperte R para começar" e libera o mouse.
        public void AbrirMesa()
        {
            if (!enabled) return;
            aguardandoInicio = true;
            Mostrar(avisoIniciar, true);
            Mostrar(imagemOrdem, false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Iniciar()
        {
            if (!enabled || !ConfiguracaoValida() ||
                Vector3.Distance(jogador.position, mesa.position) > distanciaParaIniciar) return;
            CancelarAdicao();
            foreach (Ingrediente ingrediente in ordemIngredientes) ingrediente.Restaurar();
            etapa = 0;
            terminou = false;
            cozinhando = true;
            aguardandoInicio = false;
            tempoRestante = tempoLimite;
            animacao.MostrarModelo(0);
            textoTempo.gameObject.SetActive(true);
            Mostrar(avisoIniciar, false);
            Mostrar(imagemOrdem, true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Mensagem("");
        }

        public void SelecionarIngrediente(Ingrediente ingrediente)
        {
            if (!cozinhando || terminou || animando || Time.timeScale == 0 ||
                ingrediente == null || !ingrediente.gameObject.activeInHierarchy) return;
            if (ingrediente != ordemIngredientes[etapa])
            {
                Mensagem("Ingrediente incorreto!", 2);
                return;
            }
            animando = true; // Bloqueia cliques até a adição terminar.
            ingredienteEmMovimento = ingrediente;
            Mensagem("");
            StartCoroutine(AdicionarIngrediente(ingrediente));
        }

        private IEnumerator AdicionarIngrediente(Ingrediente ingrediente)
        {
            yield return animacao.Adicionar(ingrediente, etapa + 1);
            etapa++;
            animando = false;
            ingredienteEmMovimento = null;
            if (etapa == ordemIngredientes.Length)
            {
                terminou = true;
                Mensagem($"Prato pronto! {teclaIniciar} para cozinhar novamente.");
            }
        }

        private void CancelarAdicao()
        {
            StopAllCoroutines();
            if (animacao != null)
            {
                animacao.Cancelar();
                animacao.MostrarModelo(etapa);
            }
            // Devolve o ingrediente se o tempo acabou no meio da animação.
            if (ingredienteEmMovimento != null) ingredienteEmMovimento.Restaurar();
            ingredienteEmMovimento = null;
            animando = false;
        }

        public void Sair()
        {
            CancelarAdicao();
            cozinhando = false;
            terminou = false;
            aguardandoInicio = false;
            if (textoTempo != null) textoTempo.gameObject.SetActive(false);
            if (textoMensagem != null) textoMensagem.text = "";
            Mostrar(avisoIniciar, false);
            Mostrar(imagemOrdem, false);
        }

        private void Mensagem(string texto, float segundos = 0)
        {
            textoMensagem.text = texto;
            duracaoMensagem = segundos; // Zero mantém a mensagem até a próxima ação.
        }

        private void Mostrar(GameObject objeto, bool mostrar)
        {
            if (objeto != null) objeto.SetActive(mostrar);
        }

        private bool ConfiguracaoValida()
        {
            if (jogador == null || mesa == null || cameraCozinha == null || cameraPrincipal == null ||
                animacao == null || textoTempo == null || textoMensagem == null ||
                animacao.destino == null || animacao.nuvem == null ||
                ordemIngredientes == null || ordemIngredientes.Length == 0 ||
                animacao.modelos == null || animacao.modelos.Length != ordemIngredientes.Length + 1 ||
                tempoLimite <= 0 || animacao.tempoMovimento <= 0 || animacao.tempoNuvem <= 0) return false;
            for (int i = 0; i < ordemIngredientes.Length; i++)
            {
                if (ordemIngredientes[i] == null || ordemIngredientes[i].cozinha != this) return false;
                for (int j = 0; j < i; j++)
                    if (ordemIngredientes[i] == ordemIngredientes[j]) return false;
            }
            foreach (GameObject modelo in animacao.modelos)
                if (modelo == null) return false;
            return true;
        }

        private void OnDisable()
        {
            Sair();
        }
    }
}