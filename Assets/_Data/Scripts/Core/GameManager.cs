using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;


    [Header("Levels")]
    public List<LevelData> levels = new();
    public int currentLevelIndex = 0;
    public int timeLimit = 5;
    int curTimeLimit;
    public bool isStartGame = false;
    public bool isGameOver = false;

    [Header("Scene References")]
    public LevelBuilder levelBuilder;
    public WaitingAreaController waitingArea;

    [Header("Debug")]
    public bool autoLoadOnStart = true;
    [Header("Progress")]
    public LevelProgressTracker progressTracker;

    private readonly List<GarageController> runtimeGarages = new();
    private readonly List<LaneController> runtimeLanes = new();

    public LevelData CurrentLevel =>
        currentLevelIndex >= 0 && currentLevelIndex < levels.Count ? levels[currentLevelIndex] : null;
    public BusPath CurrentBusPath => levelBuilder != null ? levelBuilder.CurrentBuiltPath : null;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

    }

    private void Start()
    {
        GameGUiManager.Ins.ShowGameGUI(true);

        if (autoLoadOnStart)
            LoadLevel(currentLevelIndex);
    }
  
    public void LoadLevel(int levelIndex)
    {

        currentLevelIndex = levelIndex;
        curTimeLimit = timeLimit;
        ClearRuntimeLists();

        if (levelBuilder != null)
            levelBuilder.ClearBuiltObjects();

        if (progressTracker != null)
        {
            progressTracker.Setup(CurrentLevel);
        }

        if (CurrentLevel == null)
        {
            Debug.LogError("Khong Co levelData");
            return;
        }

        levelBuilder.Build(CurrentLevel, this);

        foreach (var garage in runtimeGarages)
        {
            garage.Begin();
        }
    }

    public void RegisterGarage(GarageController garage)
    {
        if (!runtimeGarages.Contains(garage))
            runtimeGarages.Add(garage);
    }

    public void RegisterLane(LaneController lane)
    {
        if (!runtimeLanes.Contains(lane))
            runtimeLanes.Add(lane);
    }

    public bool AnyLaneStillHasGroups()
    {
        return runtimeLanes.Any(l => l.HasAnyGroup());
    }

    public bool AnyWaitingGroup()
    {
        return waitingArea != null && waitingArea.HasAnyWaitingGroup();
    }

    public bool AnyGarageStillHasBusWork()
    {
        return runtimeGarages.Any(g => !g.IsCompletelyDone());
    }

    public bool AnyFutureGroupOfColor(ColorType color)
    {
        bool inLanes = runtimeLanes.Any(l => l.HasColorInQueue(color));
        bool inWaiting = waitingArea != null && waitingArea.HasColor(color);
        return inLanes || inWaiting;
    }

    public void CheckWinCondition()
    {
        bool allDone = !AnyLaneStillHasGroups()
                       && !AnyWaitingGroup()
                       && !AnyGarageStillHasBusWork();

        if (allDone)
        {
            Debug.Log("WIN LEVEL " + (currentLevelIndex + 1));

            GameGUiManager.Ins.winDialog.Show(true); 
        }
    }
    public void WinNextLevel()
    {
        int next = currentLevelIndex + 1;
        if (next < levels.Count)
        {
            Debug.Log("Load next level...");
            LoadLevel(next);
        }
        else
        {
            Debug.Log("Da Hoan Thanh Level .");
        }
    }
    public void ReplayLevel()
    {
        isGameOver = false;
        isStartGame = true;

        BusController[] allBuses = FindObjectsByType<BusController>(FindObjectsSortMode.None);
        for (int i = 0; i < allBuses.Length; i++)
        {
            Destroy(allBuses[i].gameObject);
        }
        PassengerUnit[] passengerUnits = FindObjectsByType<PassengerUnit>(FindObjectsSortMode.None);
        for (int i = 0; i < passengerUnits.Length; i++)
        {
            Destroy(passengerUnits[i].gameObject);
        }

        LoadLevel(currentLevelIndex);

        GameGUiManager.Ins.UpdateTimer(IntoTIme(curTimeLimit));
    }
    private void ClearRuntimeLists()
    {
        runtimeGarages.Clear();
        runtimeLanes.Clear();

        if (levelBuilder != null)
            levelBuilder.ClearBuiltObjects();
    }



    string IntoTIme(int time)
    {
        float minute = Mathf.Floor(time / 60);
        float seconds = Mathf.RoundToInt(time % 60);

        return minute.ToString("00") + ":" + seconds.ToString("00");
    }

    public IEnumerator TimeCountDown()
    {
        while (curTimeLimit > 0)
        {
            yield return new WaitForSeconds(1f);
          
            curTimeLimit--;
            if (curTimeLimit <= 0)
            {
                isGameOver = true;
                
                GameGUiManager.Ins.gameOverDialog.Show(true);

                GameGUiManager.Ins.CurDialog = GameGUiManager.Ins.gameOverDialog;
            }
            GameGUiManager.Ins.UpdateTimer(IntoTIme(curTimeLimit));
        }
    }
}