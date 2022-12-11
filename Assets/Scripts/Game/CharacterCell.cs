using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Character Cell", menuName = "Character Cell")]
public class CharacterCell : ScriptableObject
{
    public string CharacterName;
    public Sprite CharacterSprite;
    public float Scale = 1;
}
