using System.Collections;
using UnityEngine;

namespace Cozinha
{
    // Este script só cuida do VISUAL: o ingrediente voando, a nuvem e a troca da comida.
    // As regras do jogo (ordem, tempo) ficam no CozinhaManager.
    public class AnimacaoCozinha : MonoBehaviour
    {
        public Transform destino;   // Ponto no centro da frigideira, para onde o ingrediente voa
        public GameObject nuvem;    // Efeito que aparece na hora da troca
        [Tooltip("Modelo vazio primeiro; depois, um modelo para cada ingrediente.")]
        public GameObject[] modelos; // Lista: modelo 0 = frigideira vazia, 1 = com o 1º ingrediente...
        [Tooltip("OPCIONAL: texto 'Misturando...' que aparece embaixo da tela enquanto a nuvem mistura. Deixe desativado na cena. Só é usado na fase que tem mistura.")]
        public GameObject avisoMistura;
        [Min(0.1f)] public float tempoMovimento = 0.6f; // Quanto tempo o ingrediente leva para voar
        [Min(0.1f)] public float tempoNuvem = 0.65f;    // Quanto tempo a nuvem fica na tela
        public float alturaDoArco = 0.35f;              // Quão alto o ingrediente sobe no caminho

        // IEnumerator = é uma "corrotina": uma função que pode pausar e continuar nos frames seguintes
        // duracaoMistura e trocarModeloNoFim são OPCIONAIS: sem passar nada, funciona como sempre funcionou
        public IEnumerator Adicionar(Ingrediente ingrediente, int proximoModelo,
                                     float duracaoMistura = 0f, bool trocarModeloNoFim = false)
        {
            Vector3 inicio = ingrediente.transform.position; // De onde o ingrediente sai
            float tempo = 0;                                  // Contador de tempo da animação

            // ---------- FASE 1: o ingrediente voa até a panela ----------
            while (tempo < tempoMovimento) // Repete a cada frame até o tempo acabar
            {
                tempo += Time.deltaTime; // Soma o tempo do frame (igual em qualquer computador)

                // Progresso de 0 a 1 (0 = começo, 1 = chegou). Clamp01 impede de passar de 1.
                float progresso = Mathf.Clamp01(tempo / tempoMovimento);

                // SmoothStep deixa o movimento suave: começa e termina devagar
                float suavizado = Mathf.SmoothStep(0, 1, progresso);

                // Lerp = vai do ponto A ao ponto B conforme o valor (0 a 1)
                // O Sin cria o arco: vai de 0 a 1 e volta a 0, então o objeto sobe e desce
                ingrediente.transform.position = Vector3.Lerp(inicio, destino.position, suavizado)
                    + Vector3.up * Mathf.Sin(progresso * Mathf.PI) * alturaDoArco;

                yield return null; // Espera o próximo frame e continua daqui
            }

            // ---------- FASE 2: a nuvem aparece e esconde a troca ----------
            if (duracaoMistura > 0f)
                yield return NuvemMistura(ingrediente, proximoModelo, duracaoMistura, trocarModeloNoFim); // Nuvem longa
            else
                yield return NuvemNormal(ingrediente, proximoModelo); // Nuvem normal, como sempre
        }

        // A nuvem NORMAL: cresce, e quando está cobrindo a panela o modelo muda.
        private IEnumerator NuvemNormal(Ingrediente ingrediente, int proximoModelo)
        {
            nuvem.transform.position = destino.position; // Coloca a nuvem na frigideira
            nuvem.transform.localScale = Vector3.zero;   // Começa com tamanho zero
            nuvem.SetActive(true);                       // Liga a nuvem
            float tempo = 0;                             // Contador de tempo
            bool trocouModelo = false;                   // Garante que a troca aconteça só uma vez

            while (tempo < tempoNuvem)
            {
                tempo += Time.deltaTime;
                float progresso = Mathf.Clamp01(tempo / tempoNuvem);

                // Com o Sin, a nuvem cresce até o máximo no meio e depois diminui
                nuvem.transform.localScale = Vector3.one * Mathf.Sin(progresso * Mathf.PI);

                // Troca o conteúdo quando a nuvem está cobrindo a panela.
                if (!trocouModelo && progresso >= 0.4f)
                {
                    ingrediente.Esconder();         // O ingrediente some
                    MostrarModelo(proximoModelo);   // Aparece a frigideira com a comida nova
                    trocouModelo = true;            // Marca como feito, para não repetir
                }
                yield return null; // Espera o próximo frame
            }
            nuvem.SetActive(false); // Terminou: desliga a nuvem
        }

        // A nuvem de MISTURA: cresce rápido, FICA na tela durante todo o tempo da mistura
        // (com a mensagem "Misturando...") e depois diminui.
        // trocarModeloNoFim = false: a comida muda logo que a nuvem cobre a panela.
        // trocarModeloNoFim = true:  a comida só muda no FIM da mistura (a nuvem ainda cobre a troca).
        private IEnumerator NuvemMistura(Ingrediente ingrediente, int proximoModelo,
                                         float duracao, bool trocarModeloNoFim)
        {
            nuvem.transform.position = destino.position;
            nuvem.transform.localScale = Vector3.zero;
            nuvem.SetActive(true);
            Misturar(true); // Mostra a mensagem "Misturando..."

            float entrada = tempoNuvem * 0.5f;        // Tempo para a nuvem crescer (e para diminuir no fim)
            duracao = Mathf.Max(duracao, tempoNuvem); // Garante tempo suficiente para crescer e diminuir
            // Momento em que a comida muda: logo depois de crescer, ou logo antes de diminuir
            float momentoTroca = trocarModeloNoFim ? duracao - entrada : entrada;

            float tempo = 0;
            bool escondeu = false;  // O ingrediente já sumiu?
            bool trocou = false;    // A comida já mudou?

            while (tempo < duracao)
            {
                tempo += Time.deltaTime;

                float escala;
                if (tempo < entrada)
                    escala = Mathf.SmoothStep(0, 1, tempo / entrada);                     // Crescendo
                else if (tempo > duracao - entrada)
                    escala = Mathf.SmoothStep(0, 1, (duracao - tempo) / entrada);         // Diminuindo
                else
                    escala = 1f;                                                          // Cheia, cobrindo a panela
                nuvem.transform.localScale = Vector3.one * Mathf.Clamp01(escala);

                // O ingrediente some assim que a nuvem cobre a panela
                if (!escondeu && tempo >= entrada)
                {
                    ingrediente.Esconder();
                    escondeu = true;
                }
                // A comida muda no momento escolhido
                if (!trocou && tempo >= momentoTroca)
                {
                    MostrarModelo(proximoModelo);
                    trocou = true;
                }
                yield return null;
            }
            nuvem.SetActive(false);
            Misturar(false); // Esconde a mensagem
        }

        // Liga ou desliga a mensagem "Misturando...". Se não houver, não faz nada.
        public void Misturar(bool ligado)
        {
            if (avisoMistura != null) avisoMistura.SetActive(ligado);
        }

        // Mostra só o modelo da etapa atual e esconde todos os outros
        public void MostrarModelo(int etapa)
        {
            if (modelos == null) return; // Se a lista não existe, não faz nada

            // foreach = passa por cada item da lista, um de cada vez
            foreach (GameObject modelo in modelos)
                if (modelo != null) modelo.SetActive(false); // Desliga todos

            // Confere se o número da etapa existe na lista, para não dar erro
            if (etapa >= 0 && etapa < modelos.Length && modelos[etapa] != null)
                modelos[etapa].SetActive(true); // Liga só o da etapa
        }

        // Desliga a nuvem e a mensagem de mistura (ao terminar ou quando a animação é cancelada)
        public void Cancelar()
        {
            if (nuvem != null) nuvem.SetActive(false);
            Misturar(false);
        }
    }
}