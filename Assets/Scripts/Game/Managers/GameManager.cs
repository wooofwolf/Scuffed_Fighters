using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; internal set; }

    private Dictionary<ulong, int> _characters;

    private void Awake()
    {
        if (Instance != this && Instance != null) Destroy(Instance.gameObject);

        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void SetCharacters(Dictionary<ulong, int> characters)
    {
        _characters = characters;
    }

    public Dictionary<ulong, int> GetCharacters()
    {
        return _characters;
    }

    public int GetCharactersId(ulong clientId)
    {
        return _characters[clientId];
    }
}
