using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "BusGame/Level Data")]
public class LevelData : ScriptableObject
{
    public int waitingAreaCapacity = 180;

    public List<GarageData> garages = new();
    public List<LaneData> lanes = new();

    public BusPath busPathPrefab;
    public LevelLayout layoutPrefab;
}

[System.Serializable]
public class GarageData
{
    public string garageId;
    public int spawnPointIndex;
    public List<BusData> buses = new();  
}

[System.Serializable]
public class BusData
{
    public ColorType color;
    public int capacity = 32;
}

[System.Serializable]
public class LaneData
{
    public string laneId;
    public int spawnPointIndex;
    public List<GroupData> groups = new();
}

[System.Serializable]
public class GroupData
{
    public ColorType color;
    public int count;
}