using UnityEngine;
using UnityEngine.InputSystem;

public static class DiarioProgresso
{
    public static int faseMaxima = 0;
    public static bool aberto = false;
    public static Key tecla = Key.L;
    public static float timeScaleAnterior = 1f;

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
        timeScaleAnterior = 1f;
    }
}