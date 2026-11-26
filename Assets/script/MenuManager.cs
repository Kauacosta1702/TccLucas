
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject painelMenuPrincipal;
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private Slider barraVolumeGeral;
    [SerializeField] private Slider barraMusica;
    [SerializeField] private Slider barraEfeitos;
    [SerializeField] private AudioMixer mixerJogo;

    private void Start()
    {
        barraVolumeGeral.SetValueWithoutNotify(1f);
        barraMusica.SetValueWithoutNotify(1f);
        barraEfeitos.SetValueWithoutNotify(1f);

        AlterarVolumeGeral(1f);
        AlterarVolumeMusica(1f);
    }

    public void Jogar()
    {
        SceneManager.LoadScene("Cutscene Inicial");
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

    public void AlterarVolumeGeral(float valor)
    {
        AudioListener.volume = valor;
    }

    public void AlterarVolumeMusica(float valor)
    {
        if (mixerJogo != null)
        {
            float volumeDB = Mathf.Log10(Mathf.Max(valor, 0.0001f)) * 20f;
            mixerJogo.SetFloat("VolumeMusica", volumeDB);
        }
    }

    public void AlterarVolumeEfeitos(float valor)
    {
    }

    public void RestaurarPadroes()
    {
        barraVolumeGeral.SetValueWithoutNotify(1f);
        barraMusica.SetValueWithoutNotify(1f);
        barraEfeitos.SetValueWithoutNotify(1f);

        AlterarVolumeGeral(1f);
        AlterarVolumeMusica(1f);
        AlterarVolumeEfeitos(1f);
    }
}