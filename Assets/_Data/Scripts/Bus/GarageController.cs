using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GarageController : MonoBehaviour
{
    [Header("Scene refs")]
    public Transform busSpawnPoint;
    public Transform garageReturnPoint;

    [Header("Runtime")]
    public string garageId;

    private Queue<BusData> busQueue = new();
    private BusController currentBus;
    private BusController busPrefab;
    private bool started;

    [SerializeField] private int busIndex ;
    [SerializeField] private TextMeshPro textBus;
    [SerializeField] public ParticleSystem vfxIsFull;

    public void Setup(GarageData data, BusController prefab)
    {
        garageId = data.garageId;
        busPrefab = prefab;

        busQueue.Clear();
        foreach (var bus in data.buses)
        {
            busQueue.Enqueue(bus);
            busIndex = busQueue.Count;
        }
    }

    public void Begin()
    {
        if (started) return;
        started = true;
        SpawnNextBus();
    }

    public void SpawnNextBus()
    {
        if (currentBus != null) return;

        if (busQueue.Count == 0)
        {
            PlayFx();

            GameManager.Instance.CheckWinCondition();
            return;
        }

        BusData data = busQueue.Dequeue();

        currentBus = Instantiate(busPrefab, busSpawnPoint.position, busSpawnPoint.rotation);
        currentBus.Setup(data, this);

        busIndex--;
        SetTextPassenger(busIndex);
    }

    public void OnBusReturnedAndFinished(BusController bus)
    {
        if (currentBus == bus)
            currentBus = null;

        if (GameManager.Instance != null && GameManager.Instance.progressTracker != null)
        {
            GameManager.Instance.progressTracker.RegisterCompletedBus(bus);
        }

        StartCoroutine(SpawnNextDelayed());
    }

    private IEnumerator SpawnNextDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        SpawnNextBus();
    }

    public bool IsCompletelyDone()
    {
        return busQueue.Count == 0 && currentBus == null;
    }

    public void SetTextPassenger(int index)
    {
        if (textBus == null) return;
        textBus.text = index.ToString();
    }
    public void PlayFx()
    {
        if (vfxIsFull == null) return;
        vfxIsFull.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        vfxIsFull.Play();
    }
}