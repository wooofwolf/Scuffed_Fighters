using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    [SerializeField] private List<CharacterCell> _characterCells = new();
    [SerializeField] private GameObject _characterCellPrefab;
    [SerializeField] private LocalLobby _localLobby;

    private void Start()
    {
        foreach (CharacterCell c in _characterCells)
        {
            SpawnCharacterCell(c);
        }
    }

    private void SpawnCharacterCell(CharacterCell characterCell)
    {
        GameObject charCell = Instantiate(_characterCellPrefab, transform);

        Image characterImage = charCell.transform.Find("CharacterImage").GetComponent<Image>();
        TextMeshProUGUI name = charCell.transform.Find("NameImage").Find("Name").GetComponent<TextMeshProUGUI>();

        characterImage.sprite = characterCell.CharacterSprite;
        name.text = characterCell.CharacterName;

        Vector2 pixelSize = new(characterImage.sprite.texture.width, characterImage.sprite.texture.height);
        Vector2 pixelPivot = characterImage.sprite.pivot;
        Vector2 uiPivot = new(pixelPivot.x / pixelSize.x, pixelPivot.y / pixelSize.y);

        characterImage.GetComponent<RectTransform>().pivot = uiPivot;
        characterImage.GetComponent<RectTransform>().sizeDelta *= characterCell.Scale;

        Button button = charCell.GetComponent<Button>();
        button.onClick.AddListener(() => _localLobby.ChangeCharacter(characterCell.CharacterId));
    }
}
