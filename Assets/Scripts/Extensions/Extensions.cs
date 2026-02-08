using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Extensions
{
    public static T Next<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == j) ? Arr[0] : Arr[j];
    }

    public static T Rand<T>(this IList<T> list) where T : struct
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static void DeleteChildren(this Transform t)
    {
        foreach(Transform child in t)
        {
            UnityEngine.Object.Destroy(child.gameObject);
        }
    }

    public static Dictionary<string, T> PartialMatch<T>(
    this Dictionary<string, T> dictionary,
    string partialKey)
    {
        // This, or use a RegEx or whatever.
        IEnumerable<string> fullMatchingKeys =
            dictionary.Keys.Where(currentKey => currentKey.Contains(partialKey));

        Dictionary<string, T> returnedValues = new Dictionary<string, T>();

        foreach (string currentKey in fullMatchingKeys)
        {
            returnedValues.Add(currentKey, dictionary[currentKey]);
        }

        return returnedValues;
    }

    public static List<string> PartialMatchKey<T>(
    this Dictionary<string, T> dictionary,
    string partialKey)
    {
        // This, or use a RegEx or whatever.
        IEnumerable<string> fullMatchingKeys =
            dictionary.Keys.Where(currentKey => currentKey.Contains(partialKey));

        List<string> returnedValues = new List<string>();

        foreach (string currentKey in fullMatchingKeys)
        {
            string saveValue = currentKey.Split("_")[1];
            returnedValues.Add(saveValue);
        }

        return returnedValues;
    }

    public static List<T> PartialMatchValue<T>(
    this Dictionary<string, T> dictionary,
    string partialKey)
    {
        // This, or use a RegEx or whatever.
        IEnumerable<string> fullMatchingKeys =
            dictionary.Keys.Where(currentKey => currentKey.Contains(partialKey));

        List<T> returnedValues = new List<T>();

        foreach (string currentKey in fullMatchingKeys)
        {
            returnedValues.Add(dictionary[currentKey]);
        }

        return returnedValues;
    }

    public static T GetComponentOnlyInChildren<T>(this Transform t)
    {
        foreach (Transform child in t)
        {
            if(child.GetComponent<T>() != null)
            {
                return child.GetComponent<T>();
            }
        }

        return default(T);
    }
}
