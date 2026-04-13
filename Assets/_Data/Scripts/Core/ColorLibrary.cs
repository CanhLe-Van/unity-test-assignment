using UnityEngine;

public static class ColorLibrary
{
    public static Material GetMaterial(ColorType c)
    {
        return c switch
        {
            ColorType.Red => Resources.Load<Material>("Materials/Mat_Red"),
            ColorType.Blue => Resources.Load<Material>("Materials/Mat_Blue"),
            ColorType.Green => Resources.Load<Material>("Materials/Mat_Green"),
            ColorType.Yellow => Resources.Load<Material>("Materials/Mat_Yellow"),
            ColorType.Purple => Resources.Load<Material>("Materials/Mat_Purple"),
            ColorType.Orange => Resources.Load<Material>("Materials/Mat_Orange"),
            _ => null
        };
    }
}