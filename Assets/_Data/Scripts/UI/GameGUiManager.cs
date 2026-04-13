using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameGUiManager : MonoBehaviour
{
    public static GameGUiManager Ins;

    public GameObject homeGui;
    public GameObject gameGui;

    public Diglog gameOverDialog;
    public Diglog winDialog;

  //  public Image fireRateFilled;
    public Text timerText;
    public TextMeshPro waitingAreaText;
    public TextMeshPro currentPeopleText;

   // public Text killCountingText;

    Diglog m_curDialog;

    public Diglog CurDialog { get => m_curDialog; set => m_curDialog = value; }

    public void Awake()
    {
        if (Ins == null) Ins = this;
        else Destroy(gameObject);
    }

    public void ShowGameGUI(bool isShow)
    {
        if (homeGui)
            homeGui.SetActive(isShow);
        if (gameGui)
            gameGui.SetActive(!isShow);
    }

    public void UpdateTimer(string time)
    {
        if (timerText)
            timerText.text = time;
    }
    public void UpdateWaitingAreaText(int index)
    {
        if (waitingAreaText)
            waitingAreaText.text = index.ToString();
    }
    public void CurrentPeopleText(int index)
    {
        if (currentPeopleText)
            currentPeopleText.text = index.ToString();
    }
    public void UpdaterKilledCounting(int killed)
    {
       // if (killCountingText)
       //     killCountingText.text = "x" + killed.ToString();
    }
        
    public void UpdateFireRate(float rate)
    {
       // if (fireRateFilled)
       //     fireRateFilled.fillAmount = rate;
    }

    public void PlayGame()
    {
        gameOverDialog.Show(false);

        ShowGameGUI(false);

        GameManager.Instance.isStartGame = true;

        StartCoroutine(GameManager.Instance.TimeCountDown());

    }

    public void NextGame()
    {
        winDialog.Show(false);
        GameManager.Instance.WinNextLevel();
    }
    public void ReplayGame()
    {
        gameOverDialog.Show(false);
        GameManager.Instance.ReplayLevel();
    }
}
