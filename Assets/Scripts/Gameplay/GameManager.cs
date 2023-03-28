using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public enum GameState { None, PreGame, InGame, EndGame }
    public enum HardMode { Normal, Hard, Ghost }

    public GameState gameState = GameState.EndGame;
    public HardMode hardMode = HardMode.Normal;
    public float playedTime { get; protected set; }
    public int score { get; protected set; }
    public int highScore { get; protected set; }

    //
    [Header("Game Instance")]
    public Board board;

    private bool m_IsInitialized = false;

    void Start()
    {
        if (!m_IsInitialized)
        {
            Initialize();
            if (HUDManager.instance != null)
            {
                HUDManager.instance.Initialize();
            }
            m_IsInitialized = true;
        }

        ChangeGamestate(GameState.PreGame);
    }

    protected virtual void Initialize()
    {
        highScore = PlayerPrefs.GetInt("_PREF_GAME_HIGH_SCORE", 0);
    }

    protected virtual void OnEnable()
    {
        EventDispatcher.Register(this, GameEvent.GE_ADD_SCORE, "OnAddScore");
    }

    protected virtual void OnDisable()
    {
        EventDispatcher.UnRegister(this, GameEvent.GE_ADD_SCORE);
    }

    void Update()
    {
        switch (gameState)
        {
            case GameState.InGame:
                playedTime += Time.deltaTime;
                HUDManager.instance.UpdatePlayedTime();
                break;
            default:
                break;
        }
    }

    public void OnAddScore(int score)
    {
        this.score += score;
        HUDManager.instance.UpdateScore();

        if (this.score > highScore)
        {
            highScore = this.score;
            PlayerPrefs.SetInt("_PREF_GAME_HIGH_SCORE", highScore);
            HUDManager.instance.UpdateHighScore();
        }
    }

    public void ChangeGamestate(GameState state)
    {
        if (gameState == state) return;
        EventDispatcher.Dispatch(GameEvent.GE_GAMESTATE_CHANGE, gameState, state);
        gameState = state;
        switch (gameState)
        {
            case GameState.PreGame:
                playedTime = 0;
                score = 0;
                HUDManager.instance.ShowBeginGame();
                HUDManager.instance.UpdateScore();
                HUDManager.instance.UpdateHighScore();
                break;
            case GameState.InGame:
                hardMode = GetHardMode();
                board.StartGame();
                break;
            case GameState.EndGame:
                board.EndGame();
                HUDManager.instance.ShowEndGame();
                break;
            default:
                break;
        }
    }

    public HardMode GetHardMode()
    {
        string hardModeStr = PlayerPrefs.GetString("_PREF_GAME_MODE", "normal");
        if (hardModeStr == "hard") return HardMode.Hard;
        if (hardModeStr == "ghost") return HardMode.Ghost;

        return HardMode.Normal;
    }

    public void DoBoardCommand(string command)
    {
        if (command == "undo")
            board?.RestorePreBoardState();
        else if (command == "change_dot")
            board?.ChangDot();
    }
}
