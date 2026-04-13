using UnityEngine;

public class BusFollower : MonoBehaviour
{
    [Header("Bus Following")]
    public float followCheckDistance = 3f;
    public float followRadius = 0.35f;
    public float safeGap = 1.2f;
    public LayerMask busLayer;

    private BusController bus;

    public void Setup(BusController owner)
    {
        bus = owner;
    }

    public bool ShouldWaitForFrontBus()
    {
        if (!HasBusAhead(out BusController frontBus, out float dist))
            return false;

        if (dist <= safeGap)
            return true;

        if (frontBus != null && frontBus.runtime.isPaused && dist <= followCheckDistance)
            return true;

        return false;
    }

    private bool HasBusAhead(out BusController frontBus, out float distance)
    {
        frontBus = null;
        distance = 0f;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 dir = transform.forward.normalized;

        if (Physics.SphereCast(origin, followRadius, dir, out RaycastHit hit,
            followCheckDistance, busLayer, QueryTriggerInteraction.Ignore))
        {
            BusController found = hit.collider.GetComponentInParent<BusController>();

            if (found != null && found != bus)
            {
                frontBus = found;
                distance = hit.distance;
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawLine(origin, origin + transform.forward * 3f);
    }
}