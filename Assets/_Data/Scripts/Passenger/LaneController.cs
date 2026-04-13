using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneController : MonoBehaviour
{
    public string laneId;

    [Header("Dynamic Layout")]

    public Transform frontAnchor;

    public Vector3 stackDirection = Vector3.back;

    public float groupSpacing = 0.1f;

    private readonly Queue<PassengerGroupController> groupsQueue = new();

    public void Setup(LaneData data, PassengerGroupController groupPrefab)
    {
        laneId = data.laneId;

        for (int i = 0; i < data.groups.Count; i++)
        {
            PassengerGroupController group = Instantiate(
                groupPrefab,
                frontAnchor.position,
                Quaternion.identity,
                transform
            );

            group.Setup(data.groups[i], this);
            groupsQueue.Enqueue(group);
        }

        RearrangeQueueVisual();
        RefreshInteractable();
    }

    public bool IsFrontGroup(PassengerGroupController group)
    {
        if (groupsQueue.Count == 0) return false;
        return groupsQueue.Peek() == group;
    }

    public void OnFrontGroupTapped(PassengerGroupController group)
    {
        if (!IsFrontGroup(group)) return;

        WaitingAreaController waitingArea = GameManager.Instance.waitingArea;

      //  if (!waitingArea.CheckInflateVisua())
      //      waitingArea.inflateObjVisual.InflateVisual(0.05f);

        int acceptedCount = waitingArea.GetAcceptableCount(group);

        if (acceptedCount <= 0)
        {
            Debug.Log("WaitingArea Full.");
            return;
        }
     

        if (waitingArea.CanAcceptGroup(group))
        {
            groupsQueue.Dequeue();
            waitingArea.AcceptGroup(group);
        }
        else
        {
            waitingArea.AcceptPartialGroup(group);
        }


        StartCoroutine(RearrangeDelay());

        RefreshInteractable();

        GameManager.Instance.CheckWinCondition();
    }

    private void RearrangeQueueVisual()
    {
        PassengerGroupController[] arr = groupsQueue.ToArray();
        Vector3 dir = stackDirection.normalized;
        Vector3 currentPos = frontAnchor.position;

        for (int i = 0; i < arr.Length; i++)
        {
            PassengerGroupController group = arr[i];
            if (group == null) continue;

            group.MoveToLanePosition(currentPos);

            float length = group.GetLength();

            float spacingBetweenGroups = group.unitSpacingZ;

            currentPos += dir * (length + spacingBetweenGroups) * groupSpacing;
        }
    }

    private void RefreshInteractable()
    {
        foreach (var g in groupsQueue)
            g.SetInteractable(false);

        if (groupsQueue.Count > 0)
            groupsQueue.Peek().SetInteractable(true);
    }

    public bool HasAnyGroup()
    {
        return groupsQueue.Count > 0;
    }

    public bool HasColorInQueue(ColorType color)
    {
        foreach (var g in groupsQueue)
        {
            if (g != null && g.Color == color)
                return true;
        }
        return false;
    }

    private IEnumerator RearrangeDelay()
    {
        yield return new WaitForSeconds(0.2f); 

        RearrangeQueueVisual();
    }
}