using System.Collections.Generic;
using UnityEngine;

public class LevelLayout : MonoBehaviour
{
    [Header("Spawn Points")]
    public List<Transform> garageSpawnPoints = new();
    public List<Transform> laneSpawnPoints = new();

  
    [Header("Scene Refs")]
    public Transform garagesRoot;
    public Transform lanesRoot;
    public Transform pathRoot;
}