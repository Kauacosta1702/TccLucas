using UnityEngine;
using UnityEngine.InputSystem;

public static class DiarioProgresso
{
    public static int faseMaxima = 0;
    public static bool aberto = false;
    public static bool bloqueado = false; // true enquanto o jogador está no modo cozinha
    public static Key tecla = Key.D;
    public static float timeScaleAnterior = 1f;
    public static int frameFechado = -1;

    public static void Registrar(int fase)
    {
        if (fase > faseMaxima) faseMaxima = fase;
    }

    // Zera ao apertar Play no editor, para não herdar valores da vez anterior.
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