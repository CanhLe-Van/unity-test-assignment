using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class WaitingAreaController : MonoBehaviour
{
    [Header("Layout")]
    public Transform frontAnchor;
    public Vector3 stackDirection = Vector3.right;
    public float groupSpacing = 0.4f;

    [Header("Capacity")]
    [SerializeField] private int capacity = 180;
    [SerializeField] private int currentPeople = 0;

    [Header("Waiting Area")]
    public Transform centerPoint;
    public Vector2 boundMin = new Vector2(-2f, -5f);
    public Vector2 boundMax = new Vector2(2f, 5f);

    [Header("Visual")]
    public InflateObjVisual inflateObjVisual;

    [SerializeField] private TextMeshPro textPassenger;
    [SerializeField] private TextMeshPro textCapacity;

    private readonly List<PassengerGroupController> waitingGroups = new();


    public void SetupCapacity(int value)
    {
        capacity = value;
        GameGUiManager.Ins.UpdateWaitingAreaText(capacity);

        currentPeople = 0;
        GameGUiManager.Ins.CurrentPeopleText(currentPeople);
    }

    public int CurrentPeople => currentPeople;
    public int Capacity => capacity;

    public bool CanAcceptGroup(PassengerGroupController group)
    {
        if (group == null) return false;
        return currentPeople + group.Count <= capacity;
    }
    public int SpaceLeft()
    {
        return Mathf.Max(0, capacity - currentPeople);
    }
    public bool CheckInflateVisua()
    {
        return currentPeople <= capacity - 10;
    }
    public int GetAcceptableCount(PassengerGroupController group)
    {
        if (group == null) return 0;
        return Mathf.Min(group.Count, SpaceLeft());
    }

    public void AcceptGroup(PassengerGroupController group)
    {
        if (group == null) return;

        waitingGroups.Add(group);
        currentPeople += group.Count;

        GameGUiManager.Ins.CurrentPeopleText(currentPeople);
        if (!CheckInflateVisua())
            this.inflateObjVisual.IncreaseBlendShape(40f);

        RearrangeWaitingVisual();

        group.MoveUnitsToWaitingArea(centerPoint.position, boundMin, boundMax);
    }

    public bool HasAnyWaitingGroup()
    {
        waitingGroups.RemoveAll(x => x == null);
        return waitingGroups.Count > 0;
    }

    public bool HasColor(ColorType color)
    {
        waitingGroups.RemoveAll(x => x == null);

        foreach (var g in waitingGroups)
        {
            if (g != null && g.Color == color && g.Count > 0)
                return true;
        }

        return false;
    }

    public int BoardToBus(BusController bus)
    {
        waitingGroups.RemoveAll(x => x == null);

        int totalBoarded = 0;
        int space = bus.SpaceLeft();

        for (int i = 0; i < waitingGroups.Count; i++)
        {
            PassengerGroupController group = waitingGroups[i];
            if (group == null) continue;
            if (group.Color != bus.Color) continue;
            if (space <= 0) break;

            int taken = group.TakePeople(space);

            totalBoarded += taken;
            space -= taken;
            currentPeople -= taken;
            GameGUiManager.Ins.CurrentPeopleText(currentPeople);
        }

        waitingGroups.RemoveAll(x => x == null || x.Count <= 0);
        RearrangeWaitingVisual();

        GameManager.Instance.CheckWinCondition();
        return totalBoarded;
    }

    public List<PassengerUnit> ExtractUnitsForBus(BusController bus, int maxAmount)
    {
        waitingGroups.RemoveAll(x => x == null);

        List<PassengerUnit> result = new();
        int need = maxAmount;

        for (int i = 0; i < waitingGroups.Count; i++)
        {
            PassengerGroupController group = waitingGroups[i];
            if (group == null) continue;
            if (group.Color != bus.Color) continue;
            if (need <= 0) break;

            List<PassengerUnit> extracted = group.ExtractUnits(need);

            result.AddRange(extracted);
            need -= extracted.Count;
            currentPeople -= extracted.Count;

            GameGUiManager.Ins.CurrentPeopleText(currentPeople);
            if (CheckInflateVisua())
                this.inflateObjVisual.DecreaseBlendShape(40f);
        }

        waitingGroups.RemoveAll(x => x == null || x.Count <= 0);
        RearrangeWaitingVisual();

        return result;
    }

    private void RearrangeWaitingVisual()
    {
        waitingGroups.RemoveAll(x => x == null);

        Vector3 currentPos = frontAnchor.position;

        for (int i = 0; i < waitingGroups.Count; i++)
        {
            PassengerGroupController group = waitingGroups[i];
            group.MoveToWaitingSlot(currentPos);

        }
    }

    ///tach group di chuyens
    public int AcceptPartialGroup(PassengerGroupController sourceGroup)
    {
        if (sourceGroup == null) return 0;

        int canTake = Mathf.Min(sourceGroup.Count, SpaceLeft());
        if (canTake <= 0) return 0;
        Transform waitingGroupParent = frontAnchor;
        PassengerGroupController newGroup = sourceGroup.SplitToNewGroup(canTake, waitingGroupParent);
        if (newGroup == null) return 0;

        waitingGroups.Add(newGroup);
        currentPeople += newGroup.Count;

        GameGUiManager.Ins.CurrentPeopleText(currentPeople);
        RearrangeWaitingVisual();
        return newGroup.Count;
    }

    private void OnDrawGizmosSelected()
    {
        if (centerPoint == null) return;

        float minX = centerPoint.position.x + boundMin.x;
        float maxX = centerPoint.position.x + boundMax.x;
        float minZ = centerPoint.position.z + boundMin.y;
        float maxZ = centerPoint.position.z + boundMax.y;

        Vector3 boxCenter = new Vector3(
            (minX + maxX) * 0.5f,
            centerPoint.position.y,
            (minZ + maxZ) * 0.5f
        );

        Vector3 size = new Vector3(
            maxX - minX,
            0.1f,
            maxZ - minZ
        );

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boxCenter, size);
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