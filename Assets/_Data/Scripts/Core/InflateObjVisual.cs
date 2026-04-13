using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InflateObjVisual : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

    [Header("BlendShape")]
    [SerializeField] private int blendShapeIndex = 0;
    [SerializeField] private float currentWeight = 0f;
    [SerializeField] private float maxWeight = 100f;
    public void IncreaseBlendShape(float addAmount = 5f)
    {
        if (skinnedMeshRenderer == null) return;

        currentWeight = Mathf.Clamp(currentWeight + addAmount, 0f, maxWeight);
        skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, currentWeight);
    }
    public void DecreaseBlendShape(float subAmount = 5f)
    {
        if (skinnedMeshRenderer == null) return;

        currentWeight = Mathf.Clamp(currentWeight - subAmount, 0f, maxWeight);
        skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, currentWeight);
    }
    public void ResetBlendShape()
    {
        if (skinnedMeshRenderer == null) return;

        currentWeight = 0f;
        skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, currentWeight);
    }

}
