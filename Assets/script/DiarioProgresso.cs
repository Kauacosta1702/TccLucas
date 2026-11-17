using UnityEngine;
using UnityEngine.InputSystem;

// "static class" = não vai em nenhum objeto. Existe uma só, para o jogo todo.
// Os valores dela SOBREVIVEM à troca de cena, por isso serve para guardar o progresso.
public static class DiarioProgresso
{
    public static int faseMaxima = 0;       // Fase mais avançada que o jogador já alcançou (0 = Peru)
    public static bool aberto = false;      // true enquanto o diário está aberto
    public static bool bloqueado = false;   // true enquanto o jogador está no modo cozinha
    public static Key tecla = Key.D;        // Tecla do diário (o DiarioAtalho preenche com a escolhida)
    public static float timeScaleAnterior = 1f; // Velocidade do jogo antes de pausar, para restaurar depois
    public static int frameFechado = -1;    // Número do frame em que o diário fechou (evita reabrir no mesmo frame)

    // Chamado por cada fase ao começar. Só aumenta, nunca diminui.
    public static void Registrar(int fase)
    {
        if (fase > faseMaxima) faseMaxima = fase;
    }

    // Zera ao apertar Play no editor, para não herdar valores da vez anterior.
    // Este atributo faz a Unity chamar o método sozinha, quando o jogo inicia
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Resetar()
    {
        faseMaxima = 0;
        aberto = false;
        bloqueado = false;
        timeScaleAnterior = 1f;
        frameFechado = -1;
    }
}
