using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Transforms
{
    // Destroys all children under the specified transform.
    public static void DestroyChildren(this Transform t, bool destroyImmediately = false)
    {
        foreach (Transform child in t)
        {
            if (destroyImmediately)
                MonoBehaviour.DestroyImmediate(child.gameObject);
            else
                MonoBehaviour.Destroy(child.gameObject);
        }
    }

    // Gets components in children and optionally parent.
    public static void GetComponentsInChildren<T>(Transform parent, List<T> results, bool includeParent = true, bool includeInactive = false) where T : Component
    {
        if (!includeParent)
        {
            List<T> current = new List<T>();
            for (int i = 0; i < parent.childCount; i++)
            {
                parent.GetChild(i).GetComponentsInChildren(includeInactive, current);
                results.AddRange(current);
            }
        }
        else
        {
            parent.GetComponentsInChildren(includeInactive, results);
        }
    }

    // Returns the position of this transform.
    public static Vector3 GetPosition(this Transform t, bool localSpace)
    {
        return (localSpace) ? t.localPosition : t.position;
    }

    // Returns the rotation of this transform.
    public static Quaternion GetRotation(this Transform t, bool localSpace)
    {
        return (localSpace) ? t.localRotation : t.rotation;
    }

    // Returns the scale of this transform.
    public static Vector3 GetScale(this Transform t)
    {
        return t.localScale;
    }


    // Sets the position of this transform.
    public static void SetPosition(this Transform t, bool localSpace, Vector3 pos)
    {
        if (localSpace)
            t.localPosition = pos;
        else
            t.position = pos;
    }

    // Sets the position of this transform.
    public static void SetRotation(this Transform t, bool localSpace, Quaternion rot)
    {
        if (localSpace)
            t.localRotation = rot;
        else
            t.rotation = rot;
    }

    // Sets the position of this transform.
    public static void SetScale(this Transform t, Vector3 scale)
    {
        t.localScale = scale;
    }


}
