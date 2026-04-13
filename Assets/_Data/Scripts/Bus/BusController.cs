using TMPro;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class BusController : MonoBehaviour
{
    [Header("Runtime Data")]
    public BusRuntimeData runtime = new();

    [Header("Modules")]
    [SerializeField] private BusMovement movement;
    [SerializeField] public BusBoarding boarding;
    [SerializeField] private BusFollower follower;
    [SerializeField] public BusVisual busVisual;

    [Header("Scene")]
    [SerializeField] private TextMeshPro textPassenger;
    [SerializeField] private TextMeshPro textCapacity;
    [SerializeField] public ParticleSystem vfxPassenger;
    [SerializeField] public ParticleSystem vfxIsFull;
    public ColorType Color => runtime.color;
    public int Capacity => runtime.capacity;
    public int CurrentPassengers => runtime.currentPassengers;
    public bool IsPaused => runtime.isPaused;
    public bool IsReturningToGarage => runtime.isReturningToGarage;

    private GarageController ownerGarage;

    public GarageController OwnerGarage => ownerGarage;

    public void Setup(BusData data, GarageController garage)
    {
        ownerGarage = garage;

        runtime.color = data.color;
        runtime.capacity = data.capacity;
        runtime.currentPassengers = 0;
        runtime.isPaused = false;
        runtime.isReturningToGarage = false;
        runtime.returnAfterLoop = false;
        runtime.returnExitPathIndex = -1;

        BusPath path = GameManager.Instance.CurrentBusPath;
        if (path == null)
        {
            Debug.LogError("Không tìm thấy BusPath");
            return;
        }

        busVisual.Setup(this);
        movement.Setup(this, path);
        boarding.Setup(this);
        follower.Setup(this);

        busVisual.ApplyColor(runtime.color);
        SetTextCapacity(data.capacity);
    }

    private void Update()
    {
        if (runtime.isPaused) return;
        if (follower.ShouldWaitForFrontBus()) return;

        if (runtime.isReturningToGarage)
        {
            movement.MoveToGarage();
            return;
        }

        movement.MoveOnLoopPath();
    }

    public bool HasSpace() => runtime.currentPassengers < runtime.capacity;
    public int SpaceLeft() => Mathf.Max(0, runtime.capacity - runtime.currentPassengers);
    public bool IsFull() => runtime.currentPassengers >= runtime.capacity;

    public void ReturnToGarage()
    {
        runtime.isReturningToGarage = true;
        runtime.isPaused = false;
    }

    public void PlayFx(ParticleSystem vfx)
    {
        if (vfxPassenger == null) return;
        vfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        vfx.Play();
    }

    public void SetTextPassenger(int index)
    {
        if (textPassenger == null) return;
        textPassenger.text = index.ToString();
    }
    public void SetTextCapacity(int index)
    {
        if (textCapacity == null) return;
        textCapacity.text = index.ToString();
    }
}