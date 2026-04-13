using System.Collections.Generic;
using UnityEngine;

public class BusPath : MonoBehaviour
{
    public List<Transform> pathPoints = new();
    public int stopPointIndex = 0;

    void Reset()
    {
        pathPoints.Clear();

        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("point_"))
            {
                pathPoints.Add(child);
            }
        }
    }

   
}