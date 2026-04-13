using System.Collections;
using UnityEngine;


public class PassengerUnit : MonoBehaviour
{
    [Header("Visual")]
    public Renderer visualRenderer;

    [Header("Move")]
    public float moveSpeed = 6f;

    [Header("Steering")]
    public float separationRadius = 0.3f;
    public float separationForce = 2f;

    private Vector3 targetCenter;
    private Vector2 boundMin;
    private Vector2 boundMax;

    private bool moving;
    private Coroutine currentRoutine;

    public void Setup(ColorType color)
    {
        ApplyColor(color);
    }

    private IEnumerator SteeringRoutine()
    {
        moving = true;

        while (true)
        {
            Vector3 pos = transform.position;

            Vector3 seekDir = (targetCenter - pos);
            seekDir.y = 0f;
            Vector3 seek = seekDir.normalized;

            Vector3 separation = Vector3.zero;

            Collider[] hits = Physics.OverlapSphere(pos, separationRadius);
            foreach (var h in hits)
            {
                if (h.transform == transform) continue;

                PassengerUnit other = h.GetComponent<PassengerUnit>();
                if (other == null) continue;

                Vector3 diff = pos - other.transform.position;
                float dist = diff.magnitude;

                if (dist > 0.001f)
                {
                    separation += diff.normalized / dist;
                }
            }

            separation *= separationForce;

            Vector3 moveDir = seek + separation;
            moveDir.y = 0f;

            if (moveDir.sqrMagnitude > 0.001f)
            {
                transform.position += moveDir.normalized * moveSpeed * Time.deltaTime;
            }

            Vector3 clamped = transform.position;
            clamped.x = Mathf.Clamp(clamped.x, targetCenter.x + boundMin.x, targetCenter.x + boundMax.x);
            clamped.z = Mathf.Clamp(clamped.z, targetCenter.z + boundMin.y, targetCenter.z + boundMax.y);
            transform.position = clamped;

            yield return null;
        }
    }

    private void ApplyColor(ColorType color)
    {
        if (visualRenderer == null) return;
        visualRenderer.material = ColorLibrary.GetMaterial(color);
    }

    public void MoveTo(Vector3 targetPos)
    {
        StopCurrent();
        currentRoutine = StartCoroutine(MoveDirect(targetPos));
    }

    private IEnumerator MoveDirect(Vector3 targetPos)
    {
        moving = true;

        while (Vector3.Distance(transform.position, targetPos) > 0.03f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPos;
        moving = false;
    }

    public void MoveToWaitingArea(Vector3 center, Vector2 minBound, Vector2 maxBound)
    {
        StopCurrent();

        targetCenter = center;
        boundMin = minBound;
        boundMax = maxBound;

        currentRoutine = StartCoroutine(SteeringRoutine());
    }

    // Đi tới 1 điểm trước, rồi mới bắt đầu steering trong waiting area
    public void MoveToThenWaitingArea(Vector3 firstPoint, Vector3 center, Vector2 minBound, Vector2 maxBound)
    {
        StopCurrent();
        currentRoutine = StartCoroutine(MoveToThenWaitingAreaRoutine(firstPoint, center, minBound, maxBound));
    }

    private IEnumerator MoveToThenWaitingAreaRoutine(Vector3 firstPoint, Vector3 center, Vector2 minBound, Vector2 maxBound)
    {
        // Bước 1: đi thẳng tới điểm đầu tiên
        yield return MoveDirect(firstPoint);

        // Bước 2: sau khi tới rồi mới bắt đầu steering
        targetCenter = center;
        boundMin = minBound;
        boundMax = maxBound;

        currentRoutine = StartCoroutine(SteeringRoutine());
    }

    // Đi tới 1 điểm trước, rồi đứng yên luôn
    public void MoveToThenStop(Vector3 firstPoint)
    {
        StopCurrent();
        currentRoutine = StartCoroutine(MoveToThenStopRoutine(firstPoint));
    }

    private IEnumerator MoveToThenStopRoutine(Vector3 firstPoint)
    {
        yield return MoveDirect(firstPoint);
        moving = false;
        currentRoutine = null;
    }

    private void StopCurrent()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        moving = false;
    }
}