using System.Security.Cryptography;
using System;
using UnityGameFramework.Runtime;
using System.Collections.Generic;
using System.Linq;
using GameFramework;

public static class StringUtils
{
    static private string[] s_num_string = new string[]{
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
        "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
        "20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
        "30",
    };

    static private string[] s_roman_level = new string[]{
        "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X",
        "XI", "XII", "XIII", "XIV", "XV", "XVI", "XVII", "XVIII", "XIX", "XX",
        "XXI", "XXII", "XXIII", "XXIV", "XXV", "XXVI", "XXVII", "XXVIII", "XXIX", "XXX"
    };

    static private string[] s_k_params = new string[]{
        "k0",
        "k1", "k2", "k3", "k4", "k5", "k6", "k7", "k8", "k9", "k10",
        "k11", "k12", "k13", "k14", "k15", "k16", "k17", "k18", "k19", "k20",
    };

    static private Dictionary<int, string> intValueToString = new Dictionary<int, string>(1000);
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }
    public static string FixNewLine(this string str)
    {
        return str.Replace("\\n", "\n");
    }
    public static string IntToString(int variable)
    {
        if (variable >= 0 && variable < s_num_string.Length)
        {
            return s_num_string[variable];
        }

        if (variable == -1)
        {
            return "-1";
        }

        if(!intValueToString.TryGetValue(variable,out var result))
        {
            intValueToString.Add(variable, result = variable.ToString());
        }
        return result;
    }
    public static string GetMD5(string msg)
    {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);
        byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
        md5.Clear();

        string destString = "";
        for (int i = 0; i < md5Data.Length; i++)
        {
            destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
        }
        destString = destString.PadLeft(32, '0');
        return destString;
    }

    public static string GetFileNameNoExtension(this string path, char separator = '.')
    {
        if (path.IsNullOrEmpty())
        {
            return "";
        }
        return path.Substring(0, path.LastIndexOf(separator));
    }
    public static string GetFileName(this string path, char separator = '/')
    {
        if (path.IsNullOrEmpty())
        {
            return "";
        }
        return path.Substring(path.LastIndexOf(separator) + 1);
    }
    public static string GetExtensionName(this string path)
    {
        return System.IO.Path.GetExtension(path);
    }
    public static string GetUrlName(this string url)
    {
        // string str = StringUtils.GetFileName("https://gslls.im30app.com/gameservice/getserverlist.php");
        // str = StringUtils.GetFileNameNoExtension(str); str = getserverlist
        // https://gslls.im30app.com/gameservice/getserverlist.php
        string str = StringUtils.GetFileName(url);
        str = StringUtils.GetFileNameNoExtension(str);
        return str;
    }
    public static string GetDirectoryName(this string fileName)
    {
        if (fileName.IsNullOrEmpty() || !fileName.Contains("/"))
        {
            return "";
        }
        return fileName.Substring(0, fileName.LastIndexOf("/"));
    }
    //毛乐朗要求超过10k才显示k,每三位用逗号分隔
    public static string GetFormattedInt(int value)
    {
        //string unit = "";
        var kVal = (float)value / 1000f;
        var mVal = (float)value / 1000000f;
        if (mVal >= 1f)
        {
            var newmVal = Math.Floor(mVal * 10) / 10f;
            return newmVal.ToString("0.#") + "M";
        }
        if (kVal >= 10f)
        {
            var newkVal = Math.Floor(kVal * 10) / 10f;
            return newkVal.ToString("0.#") + "K";
        }
        return S2Sec(value.ToString(""));
    }

    public static string NumberFormatted (this int value)
    {
        return GetFormattedInt (value);
    }

    public static string NumberFormatted (this string value)
    {
        if (string.IsNullOrEmpty (value))
            return value;
        int _;
        return int.TryParse (value, out _) ? GetFormattedInt (_) : value;
    }

    public static string GetFormattedLong(long value)
    {
        //string unit = "";
        var kVal = (float)value / 1000f;
        var mVal = (float)value / 1000000f;
        if (mVal >= 1f)
        {
            var newmVal = Math.Floor(mVal * 10) / 10f;
            return newmVal.ToString("0.#") + "M";
        }
        if (kVal >= 10f)
        {
            var newkVal = Math.Floor(kVal * 10) / 10f;
            return newkVal.ToString("0.#") + "K";
        }
        return S2Sec(value.ToString(""));
    }
    
    public static string GetFormattedLongNoFloor(long value)
    {
        //string unit = "";
        var kVal = (float)value / 1000f;
        var mVal = (float)value / 1000000f;
        if (mVal >= 1f)
        {
            double newmVal = 0;
            newmVal = Math.Round(mVal * 10) / 10f;
            //var newmVal = Math.Floor(mVal * 10) / 10f;
            return newmVal.ToString("0.#") + "M";
        }
        if (kVal >= 10f)
        {
            double newkVal = 0;
            newkVal = Math.Round(kVal * 10) / 10f; 
            //var newkVal = Math.Floor(kVal * 10) / 10f;
            return newkVal.ToString("0.#") + "K";
        }
        return S2Sec(value.ToString(""));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isForceS2Sec">是否显示纯数字，否则大于1000会显示k或者M</param>
    /// <returns></returns>
    public static string GetFormattedForNew(long value, bool isForceS2Sec)
    {
        if (isForceS2Sec)
        {
            return S2Sec(value.ToString(""));
        }
        else
        {
            return GetFormattedLong(value);
        }
    }

    public static string GetFormattedStr(double value)
    {
        var kVal = value / 1000f;
        var mVal = value / 1000000f;
        if (mVal >= 1f)
        {
            var newmVal = Math.Floor(mVal * 10) / 10f;
            return newmVal.ToString("0.#") + "M";
        }
        if (kVal >= 10f)
        {
            var newkVal = Math.Floor(kVal * 10) / 10f;
            return newkVal.ToString("0.#") + "K";
        }
        return value.ToString("0.#");
    }

    /// <summary>
    /// 生成随机字符串
    /// </summary>
    /// <param name="_charCount">生成的字符数</param>
    /// <returns></returns>
    private static int rep = 0;
    public static string GenerateRandomStr(int _codeCount)
    {
        string str = string.Empty;
        long num2 = DateTime.Now.Ticks + rep;
        rep++;
        Random random = new Random(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> rep)));
        for (int i = 0; i < _codeCount; i++)
        {
            int num = random.Next();
            str = str + ((char)(0x30 + ((ushort)(num % 10)))).ToString();
        }
        return str;
    }

    public static string FormatDBString(string str)
    {
        return "'" + str + "'";
    }

    public static string ConvertToRomanFromInt(int lv)
    {
        if (lv < 1 || lv > 30)
            return "";
        else
            return s_roman_level[lv - 1];

    }

    // 获取k1, k2, k3, ... 这样的字符串
    public static string GetKxParam(int k)
    {
        if (k >= 0 && k < s_k_params.Length)
        {
            return s_k_params[k];
        }

        return "k" + IntToString(k);
    }

    public static void SplitString(string str, char key, ref List<string> list)
    {
        list = str.Split(key).ToList();
    }

    public static string[] SplitString(string str, char key)
    {
        return str?.Split(key);
    }
    
    public static string FormatStringMaxLength(string str, int maxLen = 18)
    {
        int strLen = str.Length;
        if (strLen > maxLen)
        {
            return str.Substring(0, maxLen);
        }
        return str;
    }

    /// <summary>
    /// string to section 逗号分段
    /// </summary>
    /// <returns>The s.</returns>
    /// <param name="rawStr">Raw string.</param>
    public static string S2Sec(string rawStr)
    {
        if (rawStr == null || rawStr == "")
            return "";

        if (rawStr.Length <= 3)
            return rawStr;

        string retStr = rawStr;
        int curPos = rawStr.Length;
        while (curPos > 3)
        {
            curPos -= 3;
            retStr = retStr.Insert(curPos, ",");
        }

        return retStr;
    }

    public static string FloatStringToSec (string v)
    {
        if (v.IsNullOrEmpty ())
            return string.Empty;

        var curPos = v.Length;
        if (v.Contains ('.'))
        {
            var index = v.IndexOf ('.');
            curPos = index;
        }

        while (curPos > 3)
        {
            curPos -= 3;
            v      =  v.Insert (curPos, ",");
        }

        return v;
    }

    public static string FloatStringToSection (this string v)
    { 
        return FloatStringToSec (v);
    }

    public static string FloatFormat (float v, bool isFloat)
    {
        return isFloat ? v.ToString ("N") : S2Sec ((int)v);
    }

    public static string FloatFormatF (this float v, bool isFloat = true)
    {
        return FloatFormat (v, isFloat);
    }

    /// <summary>
    /// string to section 逗号分段
    /// </summary>
    /// <returns>The s.</returns>
    /// <param name="rawNum">Raw int.</param>
    public static string S2Sec(int rawNum)
    {
        return S2Sec(rawNum.ToString());
    }

    public static int TryParseInt(string str)
    {
        if (int.TryParse(str, out int ret))
        {
            return ret;
        }

        return 0;

        //if (string.IsNullOrEmpty(str))
        //{
        //    return 0;
        //}
        
        //return int.Parse(str);
    }

    public static float TryParseFloat(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return 0;
        }
        
        return UnityExtension.ToSingle(str);
    }

    public static string RedPointMax(int num)
    {
        return (num < 99 ? num : 99).ToTString();
    }

    public static bool IsStrAllEnterKey(string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            var c = str[i];
            if (c != '\n' && c != '\r')
            {
                return false;
            }
        }

        return true;
    }
    
}