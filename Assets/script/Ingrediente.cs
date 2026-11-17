using UnityEngine;

namespace Cozinha // Agrupa as classes da cozinha, para não misturar com outros scripts
{
    // MonoBehaviour = faz o script poder ser colocado em um objeto da cena
    // Este script vai em CADA ingrediente (cebola, tomate, tempero...)
    public class Ingrediente : MonoBehaviour
    {
        // Referência ao "cérebro" da cozinha. Você arrasta ele no Inspector.
        public CozinhaManager cozinha;

        // Guarda onde o ingrediente estava no começo, para poder voltar depois
        private Vector3 posicaoInicial;

        // Awake roda uma vez, antes de tudo, assim que o objeto nasce
        private void Awake()
        {
            // localPosition = posição em relação ao objeto pai
            posicaoInicial = transform.localPosition;
        }

        // Chamado pelo CozinhaManager quando você clica neste ingrediente
        public void Clicar()
        {
            // Só avisa o manager se a referência existir (evita erro de null)
            // "this" = este ingrediente. O manager decide se é o certo ou não.
            if (cozinha != null) cozinha.SelecionarIngrediente(this);
        }

        // Some com o ingrediente (depois que ele "caiu" na frigideira)
        public void Esconder()
        {
            gameObject.SetActive(false);
        }

        // Faz o ingrediente voltar ao lugar original e aparecer de novo
        public void Restaurar()
        {
            transform.localPosition = posicaoInicial;
            gameObject.SetActive(true);
        }
    }
}
