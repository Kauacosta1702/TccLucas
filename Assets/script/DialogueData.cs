using UnityEngine;

// Uma fala do diálogo. [System.Serializable] deixa o Inspector mostrar como uma "ficha".
[System.Serializable]
public class DialogueLine
{
    public string characterName; // Quem está falando
    [TextArea(2, 5)]             // Caixa de texto maior no Inspector
    public string text;          // O que a pessoa fala
    public Sprite portrait;      // Retrato (opcional)
}

// ScriptableObject = arquivo de dados na pasta do projeto (não precisa estar em objeto da cena)
// Cria no menu: Create > Dialogue > Dialogue Data. Um arquivo para cada conversa.
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines; // Todas as falas, na ordem
}