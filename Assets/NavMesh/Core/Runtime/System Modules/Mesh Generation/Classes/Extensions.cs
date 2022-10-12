using UnityEngine;

public static class Extensions
{
    public static Vector2 XZ(this Vector3 vec)
    {
        return new Vector2(vec.x, vec.z);
    }
}
