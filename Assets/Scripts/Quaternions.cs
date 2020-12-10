using UnityEngine;

public static class Quaternions
{
    // Returns if a rotational value matches another. This method is preferred over Equals or == since those variations allow larger
    public static bool Near(this Quaternion r, Quaternion target, float tolerance = 1f)
    {
        if (tolerance == 0f)
            tolerance = 0.01f;

        float a = Vectors.FastSqrMagnitude(r.eulerAngles - target.eulerAngles);
        return (a <= (tolerance * tolerance));
    }
}