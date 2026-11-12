using System;
using UnityEngine;

[Serializable]
public class EntradaDiario
{
    public string nomeComida;
    public Sprite imagemComida;
    public string titulo;
    [TextArea(5, 15)] public string texto;
    public Sprite imagem;
}

[CreateAssetMenu(fileName = "DiarioData", menuName = "Diario/Dados do Diario")]
public class DiarioData : ScriptableObject
{
    [Tooltip("Na ordem do jogo: posição 0 = primeira fase (Peru) ... última = Marrocos.")]
    public EntradaDiario[] entradas;
}