using System;
using UnityEngine;

[Serializable]
public struct DialogueLine
{
    public string speaker;
    public Sprite portrait;
    [TextArea] public string text;
}
