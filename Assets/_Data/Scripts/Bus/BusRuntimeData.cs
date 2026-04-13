using System;
using UnityEngine;

[Serializable]
public class BusRuntimeData
{
    public ColorType color;
    public int capacity = 32;
    public int currentPassengers = 0;

    public bool isPaused;
    public bool isReturningToGarage;
    public bool returnAfterLoop;
    public int returnExitPathIndex = -1;

}