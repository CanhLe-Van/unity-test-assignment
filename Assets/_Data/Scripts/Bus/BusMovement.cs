using System.Collections.Generic;
using UnityEngine;

public class BusMovement : MonoBehaviour
{
    [Header("Movement")]
    public List<Transform> pathPoints = new();
    public int stopPointIndex = 0;
    public float moveSpeed = 4f;

    private BusController bus;
    private int currentPathIndex;

    public void Setup(BusController owner, BusPath path)
    {
        bus = owner;
        pathPoints = path.pathPoints;
        stopPointIndex = path.stopPointIndex;
        currentPathIndex = GetClosestPathIndex();
    }

    public void MoveOnLoopPath()
    {
        if (pathPoints == null || pathPoints.Count == 0) return;

        Transform target = pathPoints[currentPathIndex];
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        Vector3 dir = target.position - transform.position;
        if (dir.sqrMagnitude > 0.001f)
            transform.forward = dir.normalized;

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            int reachedIndex = currentPathIndex;

            if (bus.runtime.returnAfterLoop &&
                currentPathIndex == bus.runtime.returnExitPathIndex)
            {
                bus.ReturnToGarage();
                return;
            }

            WaitingAreaController waitingArea = GameManager.Instance.waitingArea;
            bool shouldStop =
                reachedIndex == stopPointIndex &&
                !bus.runtime.returnAfterLoop &&
                bus.HasSpace() &&
                waitingArea != null &&
                waitingArea.HasColor(bus.runtime.color);

            if (shouldStop)
            {
                StartCoroutine(GetComponent<BusBoarding>().HandleStop());
            }

            currentPathIndex = (currentPathIndex + 1) % pathPoints.Count;
        }
    }

    public void MoveToGarage()
    {
        if (bus.OwnerGarage == null || bus.OwnerGarage.garageReturnPoint == null) return;

        Transform target = bus.OwnerGarage.garageReturnPoint;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        Vector3 dir = target.position - transform.position;
        if (dir.sqrMagnitude > 0.001f)
            transform.forward = dir.normalized;

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            bus.OwnerGarage.OnBusReturnedAndFinished(bus);
            Destroy(gameObject);
        }
    }

    public int FindClosestPathIndexToGarage()
    {
        if (bus.OwnerGarage == null || bus.OwnerGarage.garageReturnPoint == null)
            return -1;

        if (pathPoints == null || pathPoints.Count == 0)
            return -1;

        Vector3 garagePos = bus.OwnerGarage.garageReturnPoint.position;

        int bestIndex = -1;
        float bestDist = float.MaxValue;

        for (int i = 0; i < pathPoints.Count; i++)
        {
            if (pathPoints[i] == null) continue;

            float d = Vector3.Distance(pathPoints[i].position, garagePos);
            if (d < bestDist)
            {
                bestDist = d;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private int GetClosestPathIndex()
    {
        if (pathPoints == null || pathPoints.Count == 0) return 0;

        int bestIndex = 0;
        float bestDist = float.MaxValue;

        for (int i = 0; i < pathPoints.Count; i++)
        {
            float d = Vector3.Distance(transform.position, pathPoints[i].position);
            if (d < bestDist)
            {
                bestDist = d;
                bestIndex = i;
            }
        }

        return bestIndex;
    }
}