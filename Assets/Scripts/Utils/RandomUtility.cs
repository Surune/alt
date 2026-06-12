using System.Collections.Generic;

public static class RandomUtility
{
    public static T PickRandom<T>(this List<T> list)
    {
        var length = list.Count;
        var index = UnityEngine.Random.Range(0, length);
        return list[index];
    }
}