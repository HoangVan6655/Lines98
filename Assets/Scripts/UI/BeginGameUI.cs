using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginGameUI : MonoBehaviour
{
    public void SetGameMode(string gameMode)
    {
        PlayerPrefs.SetString("_PREF_GAME_MODE", gameMode);
        GameManager.instance.ChangeGamestate(GameManager.GameState.InGame);
        gameObject.SetActive(false);
    }
}
