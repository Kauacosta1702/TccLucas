using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject painelMenuPrincipal;
    [SerializeField] private GameObject painelOpcoes;

    public void Jogar()
    {
        SceneManager.LoadScene("Peru");
    }

    public void AbrirOpçoes()
    {
        painelMenuPrincipal.SetActive(false);
        painelOpcoes.SetActive(true);
    }
    public void FecharOpçoes()
    {
        painelOpcoes.SetActive(false);
        painelMenuPrincipal.SetActive(true);
    }

    public void Sair()
    {
        Application.Quit();
    }

}
