using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathExt
{
    public static float Approach(float a, float b, float c)
    {
        float result;

        if (a > b)
        {
            result = Mathf.Max(a - c, b);
        }
        else
        {
            result = Mathf.Min(a + c, b);
        }
        return result;
    }

    public static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

    public static bool Roll(int count)
    {
        int i = Random.Range(0, count);
        return (i == count-1);
    }

    public static bool Roll(int count, out int result)
    {
        result = Random.Range(0, count);
        return (result == count - 1);
    }

    public static bool Between(float a, float b, float c)
    {
        return a > b ? (a > b && a < c) : (a > c && a < b);
    }

    public static Vector3 Direction(Vector3 from, Vector3 to, bool normalised = true)
    {
        Vector3 dir = to - from;
        if (normalised)
            dir = dir.normalized;
        return dir;
    }

}
