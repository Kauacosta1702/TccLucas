using System.Collections;
using UnityEngine;

namespace Cozinha
{
    public class AnimacaoCozinha : MonoBehaviour
    {
        public Transform destino;
        public GameObject nuvem;
        [Tooltip("Modelo vazio primeiro; depois, um modelo para cada ingrediente.")]
        public GameObject[] modelos;
        [Min(0.1f)] public float tempoMovimento = 0.6f;
        [Min(0.1f)] public float tempoNuvem = 0.65f;
        public float alturaDoArco = 0.35f;

        public IEnumerator Adicionar(Ingrediente ingrediente, int proximoModelo)
        {
            Vector3 inicio = ingrediente.transform.position;
            float tempo = 0;

            // Leva o ingrediente até a panela, fazendo um pequeno arco.
            while (tempo < tempoMovimento)
            {
                tempo += Time.deltaTime;
                float progresso = Mathf.Clamp01(tempo / tempoMovimento);
                float suavizado = Mathf.SmoothStep(0, 1, progresso);
                ingrediente.transform.position = Vector3.Lerp(inicio, destino.position, suavizado)
                    + Vector3.up * Mathf.Sin(progresso * Mathf.PI) * alturaDoArco;
                yield return null;
            }

            nuvem.transform.position = destino.position;
            nuvem.transform.localScale = Vector3.zero;
            nuvem.SetActive(true);
            tempo = 0;
            bool trocouModelo = false;

            while (tempo < tempoNuvem)
            {
                tempo += Time.deltaTime;
                float progresso = Mathf.Clamp01(tempo / tempoNuvem);
                nuvem.transform.localScale = Vector3.one * Mathf.Sin(progresso * Mathf.PI);

                // Troca o conteúdo quando a nuvem está cobrindo a panela.
                if (!trocouModelo && progresso >= 0.4f)
                {
                    ingrediente.Esconder();
                    MostrarModelo(proximoModelo);
                    trocouModelo = true;
                }
                yield return null;
            }
            Cancelar();
        }

        public void MostrarModelo(int etapa)
        {
            if (modelos == null) return;
            foreach (GameObject modelo in modelos)
                if (modelo != null) modelo.SetActive(false);
            if (etapa >= 0 && etapa < modelos.Length && modelos[etapa] != null)
                modelos[etapa].SetActive(true);
        }

        public void Cancelar()
        {
            if (nuvem != null) nuvem.SetActive(false);
        }
    }
}
