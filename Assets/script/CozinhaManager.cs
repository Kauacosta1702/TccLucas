using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Cozinha
{
    // O "cérebro" da cozinha: controla tempo, ordem dos ingredientes, cliques e fim da receita.
    public class CozinhaManager : MonoBehaviour
    {
       
        [Header("Objetos da cena")]
        public Transform jogador;               // O personagem do jogador
        public Transform mesa;                  // Objeto perto da mesa (para medir a distância)
        public CinemachineCamera cameraCozinha; // Câmera virtual que olha para a mesa
        public AnimacaoCozinha animacao;        // Script que faz as animações
        [Header("Receita")]
        public Ingrediente[] ordemIngredientes; // Lista na ordem certa da receita
        [Min(1)] public float tempoLimite = 60; // Tempo para fazer a receita, em segundos
        [Min(0.1f)] public float distanciaParaIniciar = 2.5f; // Quão perto da mesa precisa estar
        [Header("Fim da receita")]
        [Tooltip("Nome da cena de destino (o próximo país). Deixe vazio para não trocar de cena.")]
        public string cenaSeguinte;
        [Tooltip("Cena da cutscene que passa antes do destino (ex.: cutsceneAviao). Vazio = vai direto.")]
        public string cenaCutscene = "cutsceneAviao";
        [Min(0)] public float atrasoTroca = 2f; // Segundos esperando antes de trocar de cena
        [Header("Controles")]
        public Key teclaIniciar = Key.R;        // Tecla para começar (você escolhe no Inspector)
        [Header("Interface")]
        public TMP_Text textoTempo;             // Texto do cronômetro
        public TMP_Text textoMensagem;          // Texto das mensagens ("Ingrediente incorreto!")
        [Tooltip("Aviso 'Aperte R para começar a cozinhar' (opcional)")]
        public GameObject avisoIniciar;
        [Tooltip("Imagem com a ordem dos ingredientes (opcional)")]
        public GameObject imagemOrdem;

        // Cada bool diz uma situação: true = está acontecendo agora
        private bool cozinhando, terminou, animando, aguardandoInicio, trocandoCena;
        private int etapa;                                 // Em qual ingrediente da lista estamos
        private float tempoRestante, duracaoMensagem;      // Cronômetro e tempo da mensagem na tela
        private Ingrediente ingredienteEmMovimento;        // Ingrediente que está voando agora
        private Camera cameraPrincipal;                    // A Main Camera (usada para o clique)

        private void Start()
        {
            cameraPrincipal = Camera.main; // Pega a câmera com a tag MainCamera

            // Se faltar alguma referência, avisa no Console e desliga o script
            if (!ConfiguracaoValida())
            {
                Debug.LogError("Confira as referências, a ordem e os modelos da cozinha.", this);
                enabled = false; // Desliga este script
                return;          // Sai do Start
            }
            // Deixa tudo no estado inicial: sem cronômetro, sem avisos, frigideira vazia
            textoTempo.gameObject.SetActive(false);
            Mostrar(avisoIniciar, false);
            Mostrar(imagemOrdem, false);
            animacao.Cancelar();
            animacao.MostrarModelo(0);
        }

        private void Update()
        {
            // Calcula se o jogador está perto o suficiente da mesa
            bool pertoDaMesa = Vector3.Distance(jogador.position, mesa.position) <= distanciaParaIniciar;

            // Keyboard.current pode ser null se não houver teclado, por isso a checagem
            if (Keyboard.current != null)
            {
                // wasPressedThisFrame = true só no instante em que apertou a tecla
                if (Keyboard.current.escapeKey.wasPressedThisFrame) Sair();

                // Aperta a tecla de iniciar: só vale perto da mesa, e se está
                // esperando o início OU se a receita já terminou (tentar de novo)
                if (Keyboard.current[teclaIniciar].wasPressedThisFrame && pertoDaMesa &&
                    (aguardandoInicio || (cozinhando && terminou)))
                    Iniciar();
            }

            if (!cozinhando) return; // Se não está cozinhando, não precisa fazer mais nada

            // Se já terminou ou o jogo está pausado (diário aberto), também para aqui
            if (terminou || Time.timeScale == 0) return;

            //  Cronômetro
            // Time.deltaTime = tempo que passou desde o último frame
            // Mathf.Max(0, ...) impede o tempo de ficar negativo
            tempoRestante = Mathf.Max(0, tempoRestante - Time.deltaTime);
            int segundos = Mathf.CeilToInt(tempoRestante); // Arredonda para cima
            // Mostra no formato 00:00 (minutos:segundos)
            textoTempo.text = $"{segundos / 60:00}:{segundos % 60:00}";

            // Acabou o tempo
            if (tempoRestante == 0)
            {
                CancelarAdicao();
                terminou = true;
                Mensagem($"Tempo esgotado! {teclaIniciar} para tentar novamente.");
                return;
            }

            //  Some com a mensagem depois de alguns segundos 
            if (duracaoMensagem > 0)
            {
                duracaoMensagem -= Time.deltaTime;
                if (duracaoMensagem <= 0) textoMensagem.text = "";
            }

            //  Clique do mouse nos ingredientes 
            // Só aceita clique se não tiver ingrediente voando (!animando)
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !animando)
            {
                // A CinemachineCamera não renderiza nada, quem renderiza é a Main Camera (com o Brain).
                // Cria um "raio" saindo da câmera, passando pelo ponto onde o mouse está
                Ray raio = cameraPrincipal.ScreenPointToRay(Mouse.current.position.ReadValue());

                // Lança o raio (até 100 de distância). Se bater em algo, guarda em "acerto"
                if (Physics.Raycast(raio, out RaycastHit acerto, 100))
                {
                    // Procura o script Ingrediente no objeto atingido (ou no pai dele)
                    Ingrediente ingrediente = acerto.collider.GetComponentInParent<Ingrediente>();
                    if (ingrediente != null) ingrediente.Clicar(); // Se achou, avisa que foi clicado
                }
            }
        }

        // Chame quando o jogador entrar no modo da mesa (ex.: depois do diálogo).
        // Mostra o aviso "Aperte R para começar" e libera o mouse.
        // É "public" porque é chamado por OUTRO script (NPCInteraction / BancadaCozinha)
        public void AbrirMesa()
        {
            if (!enabled) return;
            aguardandoInicio = true;       // Agora espera o jogador apertar a tecla
            Mostrar(avisoIniciar, true);
            Mostrar(imagemOrdem, false);
            Cursor.lockState = CursorLockMode.None; // Destrava o mouse
            Cursor.visible = true;                  // Mostra o cursor
        }

        // Começa (ou reinicia) a receita
        public void Iniciar()
        {
            // Não começa se: está trocando de cena, script desligado, config inválida ou longe da mesa
            if (trocandoCena || !enabled || !ConfiguracaoValida() ||
                Vector3.Distance(jogador.position, mesa.position) > distanciaParaIniciar) return;

            CancelarAdicao(); // Cancela qualquer animação que estivesse rodando
            // Devolve todos os ingredientes para o lugar de origem
            foreach (Ingrediente ingrediente in ordemIngredientes) ingrediente.Restaurar();

            etapa = 0;                       // Volta ao primeiro ingrediente
            terminou = false;
            cozinhando = true;
            aguardandoInicio = false;
            tempoRestante = tempoLimite;     // Reinicia o cronômetro
            animacao.MostrarModelo(0);       // Frigideira vazia
            textoTempo.gameObject.SetActive(true);
            Mostrar(avisoIniciar, false);    // Esconde o "Aperte R"
            Mostrar(imagemOrdem, true);      // Mostra a imagem da receita
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Mensagem("");                    // Limpa mensagens antigas
        }

        // Chamado pelo Ingrediente quando é clicado. Aqui está a regra do jogo.
        public void SelecionarIngrediente(Ingrediente ingrediente)
        {
            // Ignora o clique se: não está cozinhando, já terminou, tem animação rodando,
            // o jogo está pausado, ou o ingrediente é inválido/desligado
            if (!cozinhando || terminou || animando || Time.timeScale == 0 ||
                ingrediente == null || !ingrediente.gameObject.activeInHierarchy) return;

            // Se clicou em um ingrediente que NÃO é o da vez, mostra erro por 2 segundos
            if (ingrediente != ordemIngredientes[etapa])
            {
                Mensagem("Ingrediente incorreto!", 2);
                return;
            }

            animando = true; // Bloqueia cliques até a adição terminar.
            ingredienteEmMovimento = ingrediente;
            Mensagem("");
            StartCoroutine(AdicionarIngrediente(ingrediente)); // Inicia a corrotina (roda em etapas)
        }

        // Corrotina: executa a animação e depois decide o que acontece em seguida
        private IEnumerator AdicionarIngrediente(Ingrediente ingrediente)
        {
            // Espera a animação do ingrediente terminar antes de continuar
            yield return animacao.Adicionar(ingrediente, etapa + 1);

            etapa++;                     // Vai para o próximo ingrediente
            animando = false;            // Libera os cliques de novo
            ingredienteEmMovimento = null;

            // Se chegou ao fim da lista, a receita está pronta
            if (etapa == ordemIngredientes.Length)
            {
                terminou = true;

                // Se há uma cena seguinte configurada, troca de cena
                if (!string.IsNullOrEmpty(cenaSeguinte))
                {
                    trocandoCena = true; // Bloqueia o "tentar de novo" durante a espera
                    Mensagem("Você completou a receita, agora parta para o proximo País");
                    // Espera alguns segundos (em tempo real, mesmo se o jogo estiver pausado)
                    yield return new WaitForSecondsRealtime(atrasoTroca);

                    if (!string.IsNullOrEmpty(cenaCutscene))
                    {
                        // Guarda o destino e passa pela cutscene do avião primeiro.
                        // ViagemAviao é uma classe estática: o valor sobrevive à troca de cena
                        ViagemAviao.destino = cenaSeguinte;
                        SceneManager.LoadScene(cenaCutscene);
                    }
                    else
                    {
                        SceneManager.LoadScene(cenaSeguinte); // Sem cutscene: vai direto
                    }
                }
                else
                {
                    // Última fase: não troca de cena, só deixa tentar de novo
                    Mensagem($"Prato pronto! {teclaIniciar} para cozinhar novamente.");
                }
            }
        }

        // Interrompe a animação que estiver acontecendo
        private void CancelarAdicao()
        {
            StopAllCoroutines(); // Para todas as corrotinas deste script
            if (animacao != null)
            {
                animacao.Cancelar();              // Some com a nuvem
                animacao.MostrarModelo(etapa);    // Mostra a frigideira do ponto em que estava
            }
            // Devolve o ingrediente se o tempo acabou no meio da animação.
            if (ingredienteEmMovimento != null) ingredienteEmMovimento.Restaurar();
            ingredienteEmMovimento = null;
            animando = false;
        }

        // Sai do modo cozinha: zera tudo e esconde a interface
        public void Sair()
        {
            CancelarAdicao();
            cozinhando = false;
            terminou = false;
            aguardandoInicio = false;
            // Os "!= null" evitam erro se o objeto já foi destruído (ex.: troca de cena)
            if (textoTempo != null) textoTempo.gameObject.SetActive(false);
            if (textoMensagem != null) textoMensagem.text = "";
            Mostrar(avisoIniciar, false);
            Mostrar(imagemOrdem, false);
        }

        // Escreve uma mensagem na tela. O segundo valor é opcional:
        // se não passar, a mensagem fica até mudar; se passar, some depois desses segundos
        private void Mensagem(string texto, float segundos = 0)
        {
            textoMensagem.text = texto;
            duracaoMensagem = segundos; // Zero mantém a mensagem até a próxima ação.
        }

        // Liga ou desliga um objeto, mas só se ele existir (campos opcionais podem estar vazios)
        private void Mostrar(GameObject objeto, bool mostrar)
        {
            if (objeto != null) objeto.SetActive(mostrar);
        }

        // Confere se TUDO está configurado no Inspector. Retorna true se estiver certo.
        private bool ConfiguracaoValida()
        {
            // Algum campo obrigatório está vazio, ou algum valor é inválido?
            // Obs.: "modelos" precisa ter 1 a mais que os ingredientes (o modelo vazio)
            if (jogador == null || mesa == null || cameraCozinha == null || cameraPrincipal == null ||
                animacao == null || textoTempo == null || textoMensagem == null ||
                animacao.destino == null || animacao.nuvem == null ||
                ordemIngredientes == null || ordemIngredientes.Length == 0 ||
                animacao.modelos == null || animacao.modelos.Length != ordemIngredientes.Length + 1 ||
                tempoLimite <= 0 || animacao.tempoMovimento <= 0 || animacao.tempoNuvem <= 0) return false;

            // Confere cada ingrediente da lista
            for (int i = 0; i < ordemIngredientes.Length; i++)
            {
                // Não pode estar vazio e precisa apontar para ESTE manager
                if (ordemIngredientes[i] == null || ordemIngredientes[i].cozinha != this) return false;
                // Compara com os anteriores: não pode ter o mesmo ingrediente repetido
                for (int j = 0; j < i; j++)
                    if (ordemIngredientes[i] == ordemIngredientes[j]) return false;
            }
            // Todos os modelos da frigideira precisam estar preenchidos
            foreach (GameObject modelo in animacao.modelos)
                if (modelo == null) return false;
            return true; // Passou em todos os testes
        }

        // OnDisable roda quando o objeto é desligado ou a cena fecha
        private void OnDisable()
        {
            Sair(); // Limpa tudo para não deixar nada pela metade
        }
    }
}
