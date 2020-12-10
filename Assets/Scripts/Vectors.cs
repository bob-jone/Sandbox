using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
using System;

public static class Vectors
{
    // Vector3.zero
    private static readonly Vector3 VECTOR3_ZERO = new Vector3(0.0f, 0.0f, 0.0f);

    // Float epislon
    private const float FLOAT_EPSILON = 0.00001f;

    // Float square epislon
    private const float FLOAT_SQR_EPSILON = 1e-15f;



    // Calculates the linear parameter t that produces the interpolant value within the range [a, b].
    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 ab = b - a;
        Vector3 av = value - a;
        return Mathf.Clamp01(Vector3.Dot(av, ab) / Vector3.Dot(ab, ab));
    }



    // Returns if the target Vector3 is within variance of the source Vector3.
    public static bool Near(this Vector3 a, Vector3 b, float tolerance = 0.01f)
    {
        if (tolerance == 0f)
            return (a == b);
        else
            return FastSqrMagnitude(a - b) <= (tolerance * tolerance);
    }



    // Returns if any values within a Vector3 are NaN.
    public static bool IsNan(this Vector3 source)
    {
        return (float.IsNaN(source.x) || float.IsNaN(source.y) || float.IsNaN(source.z));
    }



    // Lerp between three Vector3 values.
    public static Vector3 Lerp3(Vector3 a, Vector3 b, Vector3 c, float percent)
    {
        Vector3 r0 = Vector3.Lerp(a, b, percent);
        Vector3 r1 = Vector3.Lerp(b, c, percent);
        return Vector3.Lerp(r0, r1, percent);
    }



    // Lerp between three Vector3 values.
    public static Vector3 Lerp3(Vector3[] vectors, float percent)
    {
        if (vectors.Length < 3)
        {
            Debug.LogWarning("Vectors -> Lerp3 -> Vectors length must be 3.");
            return Vector3.zero;
        }

        return Lerp3(vectors[0], vectors[1], vectors[2], percent);
    }



    // Multiplies a Vector3 by another.
    public static Vector3 Multiply(this Vector3 src, Vector3 multiplier)
    {
        return new Vector3(src.x * multiplier.x, src.y * multiplier.y, src.z * multiplier.z);
    }



    /* Fast checks are property of:
        *  Copyright (c) 2020 Maxim Munnig Schmidt
        *
        *  Permission is hereby granted, free of charge, to any person obtaining a copy
        *  of this software and associated documentation files (the "Software"), to deal
        *  in the Software without restriction, including without limitation the rights
        *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        *  copies of the Software, and to permit persons to whom the Software is
        *  furnished to do so, subject to the following conditions:
        *
        *  The above copyright notice and this permission notice shall be included in all
        *  copies or substantial portions of the Software.
        *
        *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
        *  SOFTWARE.
        */

    // Fast Distance.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FastDistance(Vector3 a, Vector3 b)
    {
        var distx = a.x - b.x;
        var disty = a.y - b.y;
        var distz = a.z - b.z;
        return (float)Math.Sqrt(distx * distx + disty * disty + distz * distz);
    }



    // Fast SqrMagnitude.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FastSqrMagnitude(Vector3 vector)
    {
        return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
    }



    // Fast Normalize.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 FastNormalize(Vector3 value)
    {
        float mag = (float)Math.Sqrt(value.x * value.x + value.y * value.y + value.z * value.z); //Magnitude(value);
        if (mag > FLOAT_EPSILON)
        {
            Vector3 result;
            result.x = value.x / mag;
            result.y = value.y / mag;
            result.z = value.z / mag;
            return result;// value / mag;
        }
        else
            return VECTOR3_ZERO;
    }



    // Lerp between three Vector2 values.
    public static Vector2 Lerp3(Vector2 a, Vector2 b, Vector2 c, float percent)
    {
        Vector2 r0 = Vector2.Lerp(a, b, percent);
        Vector2 r1 = Vector2.Lerp(b, c, percent);
        return Vector2.Lerp(r0, r1, percent);
    }



    // Lerp between three Vector2 values.
    public static Vector2 Lerp2(Vector2[] vectors, float percent)
    {
        if (vectors.Length < 3)
        {
            Debug.LogWarning("Vectors -> Lerp3 -> Vectors length must be 3.");
            return Vector2.zero;
        }

        return Lerp3(vectors[0], vectors[1], vectors[2], percent);
    }



    // Multiplies a Vector2 by another.
    public static Vector2 Multiply(this Vector2 src, Vector2 multiplier)
    {
        return new Vector2(src.x * multiplier.x, src.y * multiplier.y);
    }
}
