using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BusCurvePointGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform startPoint;
    public Transform endPoint;
    public Transform centerPoint;

    [Header("Generate")]
    [Min(1)] public int pointsToSpawn = 5;
    public bool clockwise = false;

    [Tooltip("0 = giu ban kinh theo start/end, >0 = ep ban kinh nay")]
    public float overrideRadius = 0f;

    [Tooltip("dat ten point bat dau tu so nay.  100 -> point_100, point_101...")]
    public int startNameIndex = 100;

    [Header("Parent")]
    public Transform pointsParent;

    [Header("Preview")]
    public bool drawPreview = true;
    public Color previewColor = Color.cyan;

    [ContextMenu("Generate Curve Points")]
    public void GenerateCurvePoints()
    {
        if (startPoint == null || endPoint == null || centerPoint == null)
        {
            Debug.LogWarning("trieu startPoint / endPoint / centerPoint");
            return;
        }

        Transform parent = pointsParent != null ? pointsParent : transform;

        Vector3 center = centerPoint.position;
        Vector3 startDir = (startPoint.position - center);
        Vector3 endDir = (endPoint.position - center);

        if (startDir.sqrMagnitude < 0.0001f || endDir.sqrMagnitude < 0.0001f)
        {
            Debug.LogWarning("startPoint hoac endPoint dang trung centerPoint");
            return;
        }

        float radius;
        if (overrideRadius > 0.001f)
        {
            radius = overrideRadius;
            startDir = startDir.normalized * radius;
            endDir = endDir.normalized * radius;
        }
        else
        {
            float startRadius = startDir.magnitude;
            float endRadius = endDir.magnitude;
            radius = (startRadius + endRadius) * 0.5f;
            startDir = startDir.normalized * radius;
            endDir = endDir.normalized * radius;
        }

        Vector3 axis = Vector3.up;

        float signedAngle = Vector3.SignedAngle(startDir, endDir, axis);

        if (clockwise)
        {
            if (signedAngle > 0f) signedAngle -= 360f;
        }
        else
        {
            if (signedAngle < 0f) signedAngle += 360f;
        }

        List<Transform> created = new List<Transform>();

        for (int i = 1; i <= pointsToSpawn; i++)
        {
            float t = i / (pointsToSpawn + 1f);
            float angle = signedAngle * t;

            Vector3 dir = Quaternion.AngleAxis(angle, axis) * startDir;
            Vector3 pos = center + dir;

            GameObject go = new GameObject($"point_{startNameIndex + i - 1}");
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.identity;

            created.Add(go.transform);

#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(go, "Generate Curve Points");
            EditorUtility.SetDirty(go);
#endif
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(parent);
        EditorUtility.SetDirty(this);
#endif

        Debug.Log($"created {created.Count} curve points.");
    }

    [ContextMenu("Delete Generated Curve Points By Prefix")]
    public void DeleteGeneratedCurvePointsByPrefix()
    {
        Transform parent = pointsParent != null ? pointsParent : transform;
        List<GameObject> toDelete = new List<GameObject>();

        foreach (Transform child in parent)
        {
            if (child.name.StartsWith("point_"))
            {
                string suffix = child.name.Substring("point_".Length);
                if (int.TryParse(suffix, out int n))
                {
                    if (n >= startNameIndex && n < startNameIndex + pointsToSpawn)
                    {
                        toDelete.Add(child.gameObject);
                    }
                }
            }
        }

        for (int i = 0; i < toDelete.Count; i++)
        {
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(toDelete[i]);
#else
            DestroyImmediate(toDelete[i]);
#endif
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(parent);
        EditorUtility.SetDirty(this);
#endif

        Debug.Log($"Delete  {toDelete.Count} curve points.");
    }

    private void OnDrawGizmos()
    {
        if (!drawPreview || startPoint == null || endPoint == null || centerPoint == null) return;

        Vector3 center = centerPoint.position;
        Vector3 startDir = (startPoint.position - center);
        Vector3 endDir = (endPoint.position - center);

        if (startDir.sqrMagnitude < 0.0001f || endDir.sqrMagnitude < 0.0001f) return;

        float radius;
        if (overrideRadius > 0.001f)
        {
            radius = overrideRadius;
            startDir = startDir.normalized * radius;
            endDir = endDir.normalized * radius;
        }
        else
        {
            float startRadius = startDir.magnitude;
            float endRadius = endDir.magnitude;
            radius = (startRadius + endRadius) * 0.5f;
            startDir = startDir.normalized * radius;
            endDir = endDir.normalized * radius;
        }

        float signedAngle = Vector3.SignedAngle(startDir, endDir, Vector3.up);

        if (clockwise)
        {
            if (signedAngle > 0f) signedAngle -= 360f;
        }
        else
        {
            if (signedAngle < 0f) signedAngle += 360f;
        }

        Gizmos.color = previewColor;

        Vector3 prev = center + startDir;
        const int previewSteps = 24;

        for (int i = 1; i <= previewSteps; i++)
        {
            float t = i / (float)previewSteps;
            float angle = signedAngle * t;
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * startDir;
            Vector3 next = center + dir;

            Gizmos.DrawLine(prev, next);
            prev = next;
        }

        for (int i = 1; i <= pointsToSpawn; i++)
        {
            float t = i / (pointsToSpawn + 1f);
            float angle = signedAngle * t;
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * startDir;
            Vector3 pos = center + dir;

            Gizmos.DrawSphere(pos, 0.12f);
        }
    }
}