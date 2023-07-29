using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System;

using Random = UnityEngine.Random;

public static class CommonExtension
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static List<T> GetShuffledList<T>(this IList<T> list)
    {
        var copiedList = new List<T>(list);
        copiedList.Shuffle();
        return copiedList;
    }

    public static IEnumerable<T> ShuffleAndReturnAsIEnumerable<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list as IEnumerable<T>;
    }

    public static void Shuffle<T>(this IList<T> list, System.Random random)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static List<T> Except<T>(this List<T> list, T element)
    {
        var tempList = new List<T>(list);
        tempList.Remove(element);
        return tempList;
    }

    public static long MegabytesToBytes(this int megabytes)
    {
        return System.Convert.ToInt64(megabytes) << 20;
    }

    public static double BytesToMegabytes(this long bytes)
    {
        return (bytes / 1024f) / 1024f;
    }

    public static bool IsValidEmail(this string email)
    {
        bool valid = Regex.IsMatch(email, @"[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?");
        return valid;
    }

    public static bool IsNullOrEmpty(this string value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static bool IsValidText(this string value)
    {
        return (string.IsNullOrEmpty(value) == false);
    }

    public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
    {
        return collection == null || collection.Count <= 0;
    }

    public static string RemoveSpecialChars(string input)
    {
        return Regex.Replace(input, @"[^0-9a-zA-Z\._]", string.Empty);
    }

    public static string ToUpperFirstCharOnly(this string input)
    {
        if (input.IsNullOrEmpty())
            return string.Empty;
        return input.First().ToString().ToUpper() + input.Substring(1).ToLower();
    }

    public static bool HasSpecialChars(this string value)
    {
        return value.Any(ch => !System.Char.IsLetterOrDigit(ch));
    }

    public static double ConvertSecondsToMilliseconds(this float seconds)
    {
        return System.TimeSpan.FromSeconds(seconds).TotalMilliseconds;
    }

    public static T ToEnum<T>(this string value) where T : struct, IConvertible
    {
        Type type = typeof(T);
        if (!type.IsEnum)
        {
            throw new ArgumentException("T must be an enumerated type");
        }
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("value is empty.");
            return default;
        }

        if (Enum.TryParse<T>(value, out T result))
        {
            return result;
        }
        else
        {
            Debug.LogError($"value {value} can't parsed to {type.Name}");
            return default;
        }
    }

    public static bool ToBool(this int value)
    {
        return value != 0;
    }

    public static int ToInt32(this uint value)
    {
        return System.Convert.ToInt32(value);
    }

    public static uint ToUInt(this int value)
    {
        return System.Convert.ToUInt32(value);
    }

    public static uint ToUInt(this string value)
    {
        return System.Convert.ToUInt32(value);
    }

    public static T ToEnum<T>(this int value)
    {
        return (T)System.Enum.ToObject(typeof(T), value);
    }

    public static bool IsNull(this object value)
    {
        return (value == null);
    }

    public static bool IsNotNull(this object value)
    {
        return (value != null);
    }

    public static int Clamp(this int value, int min, int max)
    {
        return Mathf.Clamp(value, min, max);
    }

    public static string CalculateMD5FromFile(this string filename)
    {
        if (File.Exists(filename))
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return System.BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        return string.Empty;
    }

    public static string SubStringLastest(this string text, int length)
    {
        return text.Substring(text.Length - length, length);
    }
}