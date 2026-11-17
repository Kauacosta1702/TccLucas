using System;
using UnityEngine;

// [Serializable] = permite que o Inspector mostre esta classe como uma "ficha" preenchível
// Cada EntradaDiario é UMA página do diário (uma fase/país)
[Serializable]
public class EntradaDiario
{
    public string nomeComida;      // Nome da comida (ex.: "Ceviche")
    public Sprite imagemComida;    // Foto da comida
    public string titulo;          // Título da página
    [TextArea(5, 15)] public string texto; // Texto longo (a caixa no Inspector fica maior)
    public Sprite imagem;          // Imagem da página
}

// ScriptableObject = um arquivo de DADOS que fica na pasta do projeto, sem precisar de objeto na cena
// CreateAssetMenu = cria a opção no menu: Create > Diario > Dados do Diario
[CreateAssetMenu(fileName = "DiarioData", menuName = "Diario/Dados do Diario")]
public class DiarioData : ScriptableObject
{
    [Tooltip("Na ordem do jogo: posição 0 = primeira fase (Peru) ... última = Marrocos.")]
    public EntradaDiario[] entradas; // Lista com todas as páginas, na ordem do jogo
}
