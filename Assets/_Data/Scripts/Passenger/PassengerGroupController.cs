using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PassengerGroupController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private ColorType color;
    public ColorType Color => color;
    public int Count => units.Count;
    public IReadOnlyList<PassengerUnit> Units => units;


    [Header("Group Layout")]
    public int rows = 4;

    public float unitSpacingX = 0.2f;
    public float unitSpacingZ = 0.2f;

    [Header("References")]
    public PassengerUnit passengerUnitPrefab;

    public Transform unitsRoot;

    [Header("Move")]
    public float groupMoveSpeed = 5f;

    private LaneController ownerLane;
    private bool interactable;
    private bool moving;

    private readonly List<PassengerUnit> units = new();

    public Collider groupCollider;
    public void Setup(GroupData data, LaneController lane)
    {
        color = data.color;
        ownerLane = lane;

        gameObject.name = $"Group_{color}_{data.count}";

        SpawnUnits(data.count);
        RefreshUnitLayoutImmediate();
    }

    private void SpawnUnits(int count)
    {
        for (int i = units.Count - 1; i >= 0; i--)
        {
            if (units[i] != null)
                Destroy(units[i].gameObject);
        }
        units.Clear();

        for (int i = 0; i < count; i++)
        {
            PassengerUnit unit = Instantiate(
                passengerUnitPrefab,
                unitsRoot.position,
                Quaternion.identity,
                unitsRoot
            );

            unit.Setup(color);
            units.Add(unit);
        }
    }

    public void RefreshUnitLayoutImmediate()
    {
        int widthCount = Mathf.Min(units.Count, rows);
        float totalWidth = (widthCount - 1) * unitSpacingX;
        float startX = -totalWidth * 0.5f;

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] == null) continue;

            int column = i / rows;
            int row = i % rows;

            float x = startX + row * unitSpacingX;
            float z = column * unitSpacingZ;

            units[i].transform.localPosition = new Vector3(x, 0f, -z);
        }
    }

    public float GetLength()
    {
        int columns = GetColumnCount();

        if (units.Count <= 1 || columns <= 0)
            return 0f;

        return (columns - 1) * unitSpacingZ;
    }

    public int GetColumnCount()
    {
        return Mathf.CeilToInt((float)units.Count / rows);
    }

    public void SetInteractable(bool value)
    {
        interactable = value;
    }

    private void OnMouseDown()
    {
        if (GameManager.Instance.isStartGame == false) return;
        if (GameManager.Instance.isGameOver == true) return;

        if (!interactable) return;
        if (moving) return;
        if (ownerLane == null) return;

        ownerLane.OnFrontGroupTapped(this);
    }

    public void MoveToLanePosition(Vector3 targetPos)
    {
        StopAllCoroutines();
        StartCoroutine(MoveGroupRoutine(targetPos));
    }

    public void MoveToWaitingSlot(Vector3 targetPos)
    {
        Debug.Log($"{name} move to waiting: {targetPos}");
        StopAllCoroutines();
        StartCoroutine(MoveGroupRoutine(targetPos));
    }
    private IEnumerator MoveGroupRoutine(Vector3 targetPos)
    {
        moving = true;

        while (Vector3.Distance(transform.position, targetPos) > 0.03f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                groupMoveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPos;
        moving = false;
    }

    public int TakePeople(int amount)
    {
        int taken = Mathf.Min(amount, units.Count);

        for (int i = 0; i < taken; i++)
        {
            int lastIndex = units.Count - 1;
            PassengerUnit unit = units[lastIndex];
            units.RemoveAt(lastIndex);

            if (unit != null)
                Destroy(unit.gameObject);
        }

        RefreshUnitLayoutImmediate();

        if (units.Count == 0)
        {
            Destroy(gameObject);
        }

        return taken;
    }


    public List<PassengerUnit> ExtractUnits(int amount)
    {
        int taken = Mathf.Min(amount, units.Count);
        List<PassengerUnit> extracted = new();

        for (int i = 0; i < taken; i++)
        {
            int lastIndex = units.Count - 1;
            PassengerUnit unit = units[lastIndex];
            units.RemoveAt(lastIndex);

            if (unit != null)
            {
                unit.transform.SetParent(null);
                extracted.Add(unit);
            }
        }

        RefreshUnitLayoutImmediate();

        if (units.Count == 0)
        {
            Destroy(gameObject);
        }

        return extracted;
    }

    public void MoveUnitsToWaitingArea(Vector3 center, Vector2 boundMin, Vector2 boundMax)
    {
        StartCoroutine(MoveUnitsSequentially(center, boundMin, boundMax));
    }

    private IEnumerator MoveUnitsSequentially(Vector3 center, Vector2 boundMin, Vector2 boundMax)
    {
     
        for (int i = 0; i < units.Count; i++)
        {
            PassengerUnit unit = units[i];

            if (unit == null) continue;

            unit.MoveToThenWaitingArea(center,center, boundMin, boundMax);
            yield return new WaitForSeconds(0.01f);
        }
    }


    ///tách group
    public void SetupFromExtractedUnits(ColorType groupColor, List<PassengerUnit> extractedUnits, LaneController lane)
    {
        color = groupColor;
        ownerLane = lane;

        gameObject.name = $"Group_{color}_{extractedUnits.Count}";

        units.Clear();

        for (int i = 0; i < extractedUnits.Count; i++)
        {
            PassengerUnit unit = extractedUnits[i];
            if (unit == null) continue;

            unit.transform.SetParent(unitsRoot);
            unit.Setup(color);
            units.Add(unit);
        }

        RefreshUnitLayoutImmediate();
    }
    public PassengerGroupController SplitToNewGroup(int amount, Transform newGroupParent)
    {
        int taken = Mathf.Min(amount, units.Count);
        if (taken <= 0) return null;
        if (passengerUnitPrefab == null) return null;
        if (unitsRoot == null) return null;

        // Lấy các unit thật ra khỏi group hiện tại
        List<PassengerUnit> extractedUnits = ExtractUnits(taken);
        if (extractedUnits == null || extractedUnits.Count == 0)
            return null;

        // Tạo object group mới
        GameObject groupObj = new GameObject($"Group_{color}_{extractedUnits.Count}");
        groupObj.transform.SetParent(newGroupParent);
        groupObj.transform.position = transform.position;
        groupObj.transform.rotation = transform.rotation;

        // Gắn PassengerGroupController
        PassengerGroupController newGroup = groupObj.AddComponent<PassengerGroupController>();

        // Tạo unitsRoot mới cho group mới
        GameObject rootObj = new GameObject("UnitsRoot");
        rootObj.transform.SetParent(groupObj.transform);
        rootObj.transform.localPosition = Vector3.zero;
        rootObj.transform.localRotation = Quaternion.identity;

        // Copy config từ group cũ sang group mới
        newGroup.rows = rows;
        newGroup.unitSpacingX = unitSpacingX;
        newGroup.unitSpacingZ = unitSpacingZ;
        newGroup.passengerUnitPrefab = passengerUnitPrefab;
        newGroup.unitsRoot = rootObj.transform;
        newGroup.groupMoveSpeed = groupMoveSpeed;

        // Nhét các unit đã tách vào group mới
        newGroup.SetupFromExtractedUnits(color, extractedUnits, null);

        return newGroup;
    }

}