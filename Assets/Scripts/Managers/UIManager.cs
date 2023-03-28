using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    // score
    [HideInInspector] public int currentScore;
    [SerializeField] TMP_Text scoreText;
    //best score
    public int bestScore;
    [SerializeField] TMP_Text bestScoreText;
    //timer
    float _timer;
    [SerializeField] TMP_Text timerText;

    [SerializeField] TMP_Text displayFinalScoreText;
    //ball display
    [SerializeField] Image[] ballImages;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        UpdateAndDisplayScore(0);
    }
    void UpdateTimer()
    {
        _timer += Time.deltaTime;
        string hours = Mathf.RoundToInt(_timer / 3600).ToString("00");
        string minutes = Mathf.RoundToInt(_timer / 60).ToString("00");
        string second = Mathf.RoundToInt((_timer) % 60).ToString("00");
        timerText.text = hours+":"+minutes+":"+second;
    }
    private void Update()
    {
        UpdateTimer();
    }
    private void Start()
    {
        if (PlayerPrefs.HasKey("bestScore"))
        {
            bestScore = PlayerPrefs.GetInt("bestScore");
        }
        else
        {
            bestScore = 0;
        }
        DisplayBestScore();
    }

    private void UpdateAndDisplayScore(int score)// update score whenever the balls explode
    {
        currentScore += score;
        scoreText.text = currentScore.ToString();
    }

    private void DisplayBestScore()//display best score when the game start
    {
        bestScoreText.text = bestScore.ToString();
    }
    public void UpdateBestScore(int newbestScore)//update best score when the player lost
    {
        Debug.Log("Lose");
        if (newbestScore > bestScore)
        {
            PlayerPrefs.SetInt("bestScore", newbestScore);
        }
    }

    
    public void UpdateLostMenu(int _finalScore)// just default value to using delegate OnLosingGame
    {
        StartCoroutine(DisplayFinalScore(_finalScore));

    }
    IEnumerator DisplayFinalScore(int _score)
    {
        int count = 1;
        while(count<=_score)
        {
            displayFinalScoreText.text = count.ToString();
            count++;
            yield return new WaitForSecondsRealtime(0.05f);
        }
    }
}