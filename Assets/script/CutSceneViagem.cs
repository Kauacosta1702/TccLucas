using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutSceneViagem : MonoBehaviour
{
    [Tooltip("Arraste a Timeline. Se ficar vazio, usa a duração fixa abaixo.")]
    public PlayableDirector timeline;
    [Min(0.5f)] public float duracaoFixa = 8f;
    [Tooltip("Cena usada se a cutscene for aberta sem destino (ex.: testando direto no editor).")]
    public string cenaPadrao;
    public bool permitirPular = true;
    public KeyCode teclaPular = KeyCode.Space;

    private bool carregando;
    private float tempo;

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        if (timeline != null)
        {
            timeline.stopped += AoTerminarTimeline;
            timeline.Play();
        }
    }

    private void Update()
    {
        if (carregando) return;

        if (permitirPular && Input.GetKeyDown(teclaPular))
        {
            Continuar();
            return;
        }

        // Sem Timeline: espera a duração fixa
        if (timeline == null)
        {
            tempo += Time.deltaTime;
            if (tempo >= duracaoFixa) Continuar();
        }
    }

    private void AoTerminarTimeline(PlayableDirector diretor)
    {
        Continuar();
    }

    private void Continuar()
    {
        if (carregando) return;
        carregando = true;

        string destino = string.IsNullOrEmpty(ViagemAviao.destino) ? cenaPadrao : ViagemAviao.destino;
        ViagemAviao.destino = null;

        if (string.IsNullOrEmpty(destino))
        {
            Debug.LogError("Cutscene sem destino! Defina a Cena Padrão ou passe pelo modo cozinha.", this);
            return;
        }
        SceneManager.LoadScene(destino);
    }

    private void OnDestroy()
    {
        if (timeline != null) timeline.stopped -= AoTerminarTimeline;
    }
}