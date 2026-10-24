using UnityEngine;

namespace Cozinha
{
    public class Ingrediente : MonoBehaviour
    {
        public CozinhaManager cozinha;
        private Vector3 posicaoInicial;

        private void Awake()
        {
            posicaoInicial = transform.localPosition;
        }

        public void Clicar()
        {
            if (cozinha != null) cozinha.SelecionarIngrediente(this);
        }

        public void Esconder()
        {
            gameObject.SetActive(false);
        }

        public void Restaurar()
        {
            transform.localPosition = posicaoInicial;
            gameObject.SetActive(true);
        }
    }
}
