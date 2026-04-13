using UnityEngine;
using UnityEngine.UI;

public class LevelProgressTracker : MonoBehaviour
{
    public Image fillImage;
    public int TotalBuses { get; private set; }

    public int CompletedBuses { get; private set; }

    public float Progress01
    {
        get
        {
            if (TotalBuses <= 0) return 0f;
            return (float)CompletedBuses / TotalBuses;
        }
    }

    public void Setup(LevelData levelData)
    {
        TotalBuses = CountTotalBuses(levelData);
        CompletedBuses = 0;

        Debug.Log($"[LevelProgressTracker] Setup | TotalBuses = {TotalBuses}");

        UpdateUI();
    }

    public void RegisterCompletedBus(BusController bus)
    {
        if (bus == null) return;

        CompletedBuses++;

        if (CompletedBuses > TotalBuses)
            CompletedBuses = TotalBuses;

        Debug.Log($"[LevelProgressTracker] Completed bus: {CompletedBuses}/{TotalBuses}");

        UpdateUI();
    }

    private int CountTotalBuses(LevelData levelData)
    {
        if (levelData == null) return 0;

        int total = 0;

        if (levelData.garages == null) return 0;

        for (int i = 0; i < levelData.garages.Count; i++)
        {
            GarageData garage = levelData.garages[i];

            if (garage == null || garage.buses == null) continue;

            total += garage.buses.Count;
        }

        return total;
    }
 

    private void UpdateUI()
    {
        if (fillImage == null) return;

        fillImage.fillAmount = Progress01;
    }

}