using UnityEngine;

public class BusVisual : InflateObjVisual
{
    [Header("Reference")]
    [SerializeField] private SkinnedMeshRenderer renderers; 

    private BusController bus;

    private void Awake()
    {
        ResetBlendShape();
    }
    public void Setup(BusController owner)
    {
        bus = owner;
    }

    public void ApplyColor(ColorType color)
    {
        if (renderers == null) return;

        Material[] mats = renderers.materials;
        mats[1] = ColorLibrary.GetMaterial(color);
        renderers.materials = mats;
    }

   
}