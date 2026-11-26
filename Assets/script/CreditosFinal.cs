using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Vai na cena de créditos. Espera alguns segundos e volta ao menu, zerando o jogo.
public class CreditosFinal : MonoBehaviour
{
    [Min(1)] public float duracao = 20f;      // Segundos até voltar ao menu
    public string cenaMenu = "Menu";          // Nome exato da cena do menu
    public bool permitirPular = true;         // Deixa apertar uma tecla para voltar antes
    public Key teclaPular = Key.Escape;

    private float tempo;
    private bool saindo;

    private void Start()
    {
        // Créditos: tempo normal e mouse visível (o menu precisa dele)
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (saindo) return;

        tempo += Time.deltaTime;
        bool pulou = permitirPular && Keyboard.current != null && Keyboard.current[teclaPular].wasPressedThisFrame;

        if (tempo >= duracao || pulou) VoltarAoMenu();
    }

    private void VoltarAoMenu()
    {
        saindo = true;

        // Reinicia tudo o que sobrevive à troca de cena (valores static)
        DiarioProgresso.Resetar();   // páginas do diário voltam só ao Peru
        ViagemAviao.destino = null;  // nenhuma viagem pendente
        Time.timeScale = 1f;

        SceneManager.LoadScene(cenaMenu);
    }
}