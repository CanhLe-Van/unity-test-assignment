using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    [Header("Prefabs")]
    public GarageController garagePrefab;
    public LaneController lanePrefab;
    public PassengerGroupController passengerGroupPrefab;
    public BusController busPrefab;

    [Header("Layout")]
    

    private readonly List<GameObject> builtObjects = new();


    private LevelLayout currentLayout;
    public LevelLayout CurrentLayout => currentLayout;

    private BusPath currentBuiltPath;
    public BusPath CurrentBuiltPath => currentBuiltPath;

    public void Build(LevelData levelData, GameManager gm)
    {
        if (levelData == null) return;

        if (levelData.layoutPrefab == null)
        {
            Debug.LogError("LevelData ch?a gán layoutPrefab.");
            return;
        }
        if (gm.waitingArea != null)
        {
            gm.waitingArea.SetupCapacity(levelData.waitingAreaCapacity);
        }

        currentLayout = Instantiate(levelData.layoutPrefab, Vector3.zero, Quaternion.identity);

        builtObjects.Add(currentLayout.gameObject);


        if (levelData.busPathPrefab != null)
        {
            currentBuiltPath = Instantiate(
                levelData.busPathPrefab,
                Vector3.zero,
                Quaternion.identity,
                currentLayout.pathRoot
            );

            builtObjects.Add(currentBuiltPath.gameObject);
        }

        if (currentBuiltPath == null)
        {
            Debug.LogError("LevelLayout ch?a gán busPath.");
        }

        // Build garages
        for (int i = 0; i < levelData.garages.Count; i++)
        {
            GarageData garageData = levelData.garages[i];

            if (garageData.spawnPointIndex < 0 ||
                garageData.spawnPointIndex >= currentLayout.garageSpawnPoints.Count)
            {
                Debug.LogError($"Garage {garageData.garageId} co spawnPointIndex khong hop le.");
                continue;
            }

            Transform spawnPoint = currentLayout.garageSpawnPoints[garageData.spawnPointIndex];

            GarageController garage = Instantiate(
                garagePrefab,
                spawnPoint.position,
                spawnPoint.rotation,
                currentLayout.garagesRoot != null ? currentLayout.garagesRoot : currentLayout.transform
            );

            builtObjects.Add(garage.gameObject);
            garage.Setup(garageData, busPrefab);
            gm.RegisterGarage(garage);
        }

        // Build lanes
        for (int i = 0; i < levelData.lanes.Count; i++)
        {
            LaneData laneData = levelData.lanes[i];

            if (laneData.spawnPointIndex < 0 ||
                laneData.spawnPointIndex >= currentLayout.laneSpawnPoints.Count)
            {
                Debug.LogError($"Lane {laneData.laneId} co spawnPointIndex khong hop le.");
                continue;
            }

            Transform spawnPoint = currentLayout.laneSpawnPoints[laneData.spawnPointIndex];

            LaneController lane = Instantiate(
                lanePrefab,
                spawnPoint.position,
                spawnPoint.rotation,
                currentLayout.lanesRoot != null ? currentLayout.lanesRoot : currentLayout.transform
            );

            builtObjects.Add(lane.gameObject);
            lane.Setup(laneData, passengerGroupPrefab);
            gm.RegisterLane(lane);
        }
    }

    public void ClearBuiltObjects()
    {
        currentLayout = null;
        currentBuiltPath = null;

        for (int i = builtObjects.Count - 1; i >= 0; i--)
        {
            if (builtObjects[i] != null)
                Destroy(builtObjects[i]);
        }
        builtObjects.Clear();
    }

  
}