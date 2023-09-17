
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using GameFramework;
using GameKit.Base;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using SpriteAtlasManager = GameKit.Base.SpriteAtlasManager;

/// <summary>
/// Unity 扩展。
/// </summary>
public static class UnityExtension
{

    public static void TryAddElement<T>(this Dictionary<long, List<T>> dic, long key, T t)
    {
        if (dic.ContainsKey(key))
        {
            dic[key].Add(t);
        }
        else
        {
            dic[key] = new List<T>();
            dic[key].Add(t);
        }
    }

    public static void SetOrAdd<T, TV>(this Dictionary<T, TV> dict, T key, TV value)
    {
        if (dict.ContainsKey(key))
        {
            dict[key] = value;
            return;
        }
        
        dict.Add(key, value);
    }
    
    public static List<string> ToStrList(this string str, char splitChar)
    {
        List<string> strList = new List<string>(5);
        if (!string.IsNullOrEmpty(str))
        {
            string[] strs = str.Split(new char[] { splitChar }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strs.Length; i++)
            {
                strList.Add(strs[i]);
            }
        }
        return strList;
    }
    public static List<int> ToIntList(this string str, char splitChar)
    {
        //Log.Error("ToIntList : {0}", str);
        List<int> iList = new List<int>(5);
        if (!string.IsNullOrEmpty(str))
        {
            string[] strs = str.Split(new char[] { splitChar }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strs.Length; i++)
            {
                iList.Add(StringUtils.TryParseInt(strs[i]));
            }
        }
        return iList;
    }

    public static int ToInt(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return 0;
        }

        if (str.Equals(" "))
            return 0;

        // 我们数值中会有大量的0-9这样的数值处理，所以这简单处理一下
        if (str.Length == 1)
        {
            if (str[0] >= '0' && str[0] <= '9')
            {
                return str[0] - '0';
            }
        }

        int i = 0;
        if (int.TryParse(str, out i) == false)
        {
            if (Log.Write)
            {
                Log.Error("ToInt error!!!! str: {0}", str);
            }
        }
        return i;
    }


    public static int ToInt(this object obj)
    {
        if (obj is string)
        {
            return ToInt((string)obj);
        }

        int i = 0;
        try
        {
            //int.TryParse(obj.ToString(), out i);
            //return i;
            i = Convert.ToInt32(obj);
        }
        catch (Exception e)
        {
            if (Log.Write)
            {
                Log.Error("ToInt exception !!!! try ToString");
            }

            // FIXME: 有些obj不能直接ToInt32，必须要ToString之后再转
            // 理论来说，这属于一个调用问题，但是这里也得做一个兼容，毕竟这个接口的参数是object
            return ToInt(obj.ToString());
        }
        return i;
    }

    // 这个函数用来实现ReadOnlySpan.ToInt，因为int.Parse在后面版本才支持ReadOnlySpan，而我们用的是.Net 2.0
    // 但是为了这个函数去升级CLR，又显得臃肿，所以这里自己特殊处理一下；这个转化只支持10进制。
    // 目前这个代码支持前端有空格，但不支持数字中间有空格的情况。
    public static int ToInt(this ReadOnlySpan<char> str)
    {
        int sign = 1, Base = 0, i = 0;

        // if whitespaces then ignore.
        while (str[i] == ' ')
        {
            i++;
        }

        // sign of number
        if (str[i] == '-' || str[i] == '+')
        {
            sign = 1 - 2 * (str[i++] == '-' ? 1 : 0);
        }

        // checking for valid input
        while (
            i < str.Length
            && str[i] >= '0'
            && str[i] <= '9')
        {
            // handling overflow test case
            if (Base > int.MaxValue / 10 || (Base == int.MaxValue / 10 && str[i] - '0' > 7))
            {
                if (sign == 1)
                    return int.MaxValue;
                else
                    return int.MinValue;
            }
            Base = 10 * Base + (str[i++] - '0');
        }

        return Base * sign;
    }

    public static float ToFloat(this string value)
    {
        return ToSingle(value);
    }

    // 为了直接从ReadOnlySpan -> float!
    public static float ToFloat(this ReadOnlySpan<char> str)
    {
        float f = (float)Strtod_CSharp.strtod(str);

#if UNITY_EDITOR && !FINAL_RELEASE
        float ttt = str.ToString().ToFloat();
        if (!Mathf.Approximately(f, ttt))
        {
            Log.Error("BUGBUGBUG! ToFloat() not same!");
        }
#endif

        return f;
    }

    public static float ToFloat(this object value)
    {
        return ToSingle(value);
    }
    
    // ReadOnlySpan => ToULong
    public static ulong ToULong(this ReadOnlySpan<char> str)
    {
        ulong u = Strtoul_CSharp.strtoul(str);
        
// #if UNITY_EDITOR
//         ulong ttt = Convert.ToUInt64(str.ToString());
//         if (ttt != u)
//         {
//             Log.Error("BUGBUGBUG! ToULong() not same!");
//         }
// #endif
        return u;
    }

    public static bool IsZero(this float value)
    {
        return value > -0.0000001 && value < 0.0000001;
    }

    public static long ToLong(this string str)
    {
        long i = 0;
        if (str.Contains("."))
        {
            List <string> strVec = new List<string>();
            StringUtils.SplitString(str, '.', ref strVec);
            long.TryParse(strVec[0], out i);
        }
        else
        {
            long.TryParse(str, out i);
        }

        return i;
    }

    /// <summary>
    /// 将整型转换成原来表格中的字符串
    /// </summary>
    /// <param name="value">整型数据</param>
    /// <returns></returns>
    public static string ToTString(this int value)
    {
        return IsTEmpty(value) ? "" : value.ToString();
    }

    /// <summary>
    /// 将浮点类型值转换成原表格中的字符串
    /// </summary>
    /// <param name="value">浮点类型值</param>
    /// <returns></returns>
    public static string ToTString(this float value)
    {
        return IsTEmpty(value) ? "" : value.ToString();
    }

    /// <summary>
    /// 将表格中的整型转换成整型(主要是默认值MinValue转换为0)
    /// </summary>
    /// <param name="value">整型数据</param>
    /// <returns></returns>
    public static int ToTInt(this int value, int defaultValue = 0)
    {
        return IsTEmpty(value) ? defaultValue : value;
    }

    /// <summary>
    /// 将表中的浮点类型值转换成浮点类型
    /// </summary>
    /// <param name="value">浮点类型值</param>
    /// <returns></returns>
    public static float ToTFloat(this float value, float defaultValue = 0.0f)
    {
        return IsTEmpty(value) ? defaultValue : value;
    }

    /// <summary>
    /// 判断整型值在表中是否为空
    /// </summary>
    /// <param name="value">整型数据</param>
    /// <returns></returns>
    public static bool IsTEmpty(this int value)
    {
        return value == int.MinValue;
    }

    /// <summary>
    /// 判断浮点值在表中是否为空
    /// </summary>
    /// <param name="value">浮点类型值</param>
    /// <returns></returns>
    public static bool IsTEmpty(this float value)
    {
        return value < float.MinValue + 0.0000001;
    }

    /// <summary>
    /// 获取active的子节点数量
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static int GetActiveChildCount(this Transform t)
    {
        var count = 0;
        foreach(Transform child in t)
        {
            if(child.gameObject.activeSelf) count++;
        }

        return count;
    }

    /// <summary>
    /// 取 <see cref="UnityEngine.Vector3" /> 的 (x, y, z) 转换为 <see cref="UnityEngine.Vector2" /> 的 (x, z)。
    /// </summary>
    /// <param name="vector3">要转换的 Vector3。</param>
    /// <returns>转换后的 Vector2。</returns>
    public static Vector2 ToVector2(this Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.z);
    }

    /// <summary>
    /// 取 <see cref="UnityEngine.Vector2" /> 的 (x, y) 转换为 <see cref="UnityEngine.Vector3" /> 的 (x, 0, y)。
    /// </summary>
    /// <param name="vector2">要转换的 Vector2。</param>
    /// <returns>转换后的 Vector3。</returns>
    public static Vector3 ToVector3(this Vector2 vector2)
    {
        return new Vector3(vector2.x, 0f, vector2.y);
    }

    /// <summary>
    /// 取 <see cref="UnityEngine.Vector2" /> 的 (x, y) 和给定参数 y 转换为 <see cref="UnityEngine.Vector3" /> 的 (x, 参数 y, y)。
    /// </summary>
    /// <param name="vector2">要转换的 Vector2。</param>
    /// <param name="y">Vector3 的 y 值。</param>
    /// <returns>转换后的 Vector3。</returns>
    public static Vector3 ToVector3(this Vector2 vector2, float y)
    {
        return new Vector3(vector2.x, y, vector2.y);
    }
    
    /// <summary>
    /// 数字转换成罗马数字形式
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string ToRoman(this int number)
    {
        if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
        if (number < 1) return string.Empty;            
        if (number >= 1000) return "M" + (number - 1000).ToRoman();
        if (number >= 900) return "CM" + (number - 900).ToRoman(); 
        if (number >= 500) return "D" + (number - 500).ToRoman();
        if (number >= 400) return "CD" + (number - 400).ToRoman();
        if (number >= 100) return "C" + (number - 100).ToRoman();            
        if (number >= 90) return "XC" + (number - 90).ToRoman();
        if (number >= 50) return "L" + (number - 50).ToRoman();
        if (number >= 40) return "XL" + (number - 40).ToRoman();
        if (number >= 10) return "X" + (number - 10).ToRoman();
        if (number >= 9) return "IX" + (number - 9).ToRoman();
        if (number >= 5) return "V" + (number - 5).ToRoman();
        if (number >= 4) return "IV" + (number - 4).ToRoman();
        if (number >= 1) return "I" + (number - 1).ToRoman();
        throw new ArgumentOutOfRangeException("something bad happened");
    }

    

    #region GameObject

    public static void Destroy(this GameObject go)
    {
        if (go != null)
        {
            GameObject.Destroy(go);
        }
    }
    /// <summary>
    /// 获取或增加组件。
    /// </summary>
    /// <typeparam name="T">要获取或增加的组件。</typeparam>
    /// <param name="gameObject">目标对象。</param>
    /// <returns>获取或增加的组件。</returns>
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }

        return component;
    }

    /// <summary>
    /// 获取或增加组件。
    /// </summary>
    /// <param name="gameObject">目标对象。</param>
    /// <param name="type">要获取或增加的组件类型。</param>
    /// <returns>获取或增加的组件。</returns>
    public static Component GetOrAddComponent(this GameObject gameObject, Type type)
    {
        Component component = gameObject.GetComponent(type);
        if (component == null)
        {
            component = gameObject.AddComponent(type);
        }

        return component;
    }

    static List<Transform> TransformList = new List<Transform>();
    /// <summary>
    /// 递归设置游戏对象的层次。
    /// </summary>
    /// <param name="gameObject"><see cref="UnityEngine.GameObject" /> 对象。</param>
    /// <param name="layer">目标层次的编号。</param>
    public static void SetLayerRecursively(this GameObject gameObject, int layer)
    {
        TransformList.Clear();
        gameObject.GetComponentsInChildren<Transform>(true, TransformList);
        for (int i = 0; i < TransformList.Count; i++)
        {
            TransformList[i].gameObject.layer = layer;
        }
    }
    

    /// <summary>
    /// 获取 GameObject 是否在场景中。
    /// </summary>
    /// <param name="gameObject">目标对象。</param>
    /// <returns>GameObject 是否在场景中。</returns>
    /// <remarks>若返回 true，表明此 GameObject 是一个场景中的实例对象；若返回 false，表明此 GameObject 是一个 Prefab。</remarks>
    public static bool InScene(this GameObject gameObject)
    {
        return gameObject.scene.name != null;
    }

    public static GameObject Instantiate(this GameObject go)
    {
        if (go != null)
        {
            return GameObject.Instantiate(go);
        }
        return go;
    }

    public static GameObject Instantiate (this GameObject go, Transform parent)
    {
        if (go != null)
        {
            return GameObject.Instantiate (go, parent);
        }
        
        Debug.LogError ("GameObject is null ---> name: " + go.name);
        return null;
    }
    
    
    //供lua检查gameObject是否valid
    public static bool IsNull(this UnityEngine.Object o)
    {
        return o == null;
    }
    
    /// <summary>
    /// Add by Darcy
    /// 实例化一个 Obj 返回其组件 T
    /// </summary>
    /// <param name="source"></param>
    /// <param name="parent">父物体</param>
    /// <param name="forceActive"></param>
    /// <typeparam name="T">要获取的组件</typeparam>
    /// <returns></returns>
    public static T Create<T> (this GameObject source, Transform parent, bool forceActive = false) where T : Component
    {
        return CreateObj<T> (source, forceActive, parent);
    }
    
    /// <summary>
    /// Add by Darcy
    /// 实例化一个 Obj 返回其组件 T
    /// </summary>
    /// <param name="source"></param>
    /// <param name="forceActive"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Create<T> (this GameObject source,bool forceActive = false) where T : Component
    {
        return CreateObj<T> (source, forceActive);
    }

    private static T CreateObj<T> (GameObject source, bool forceActive, Transform parent = null) where T : Component
    {
        GameObject obj = null;
        if (parent == null)
        {
            source.Instantiate ();
        }
        else
        {
            obj = source.Instantiate (parent);
        }
        if (obj == null)
        {
            Debug.LogError ("Instantiate Fail --> obj name: " + source.name);
            return null;
        }
        
        var component = obj.GetComponent<T> ();
        if (component == null)
        {
            Debug.LogError ($"No component: {typeof (T)} --> obj name: {source.name}");
            obj.Destroy ();
            return null;
        }
        
        if (forceActive)
            obj.SetActive (true);

        return component;
    }

    /// <summary>
    /// 无需 Null check 的情况下不要调用该接口 GameObject == null 有额外的性能消耗
    /// 扩展SetActive, 提升性能
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="active"></param>
    public static void TrySetActive(this GameObject obj, bool active)
    {
        if (active)
        {
            if (obj != null && !obj.activeSelf)
                obj.SetActive(true);
        }
        else
        {
            if (obj != null && obj.activeSelf)
                obj.SetActive(false);
        }
    }

    /// <summary>
    /// 无脑 null check 会带来额外性能消耗
    /// 所以不进行 null check
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="active"></param>
    public static void SetActiveEx (this GameObject obj, bool active)
    {
        if (obj && obj.activeSelf != active)
        {
            obj.SetActive (active);
        }
    }

    // 编辑器模式下只能使用DestroyImmediate
    // 而运行模式使用DestroyImmediate又会有隐患。。。所以这个函数出现了
    public static void DestroyEx(this GameObject obj)
    {
        if (Application.isPlaying)
        {
            GameObject.Destroy(obj);
        }
        else
        {
            GameObject.DestroyImmediate(obj, false);
        }
    }

    #endregion


    #region Transform
    public static void ForceRebuildLayoutImmediate(this RectTransform parent)
    {
        if (parent == null)
        {
            Log.Error("ForceRebuildLayoutImmediate Error, RectTransform is null????????");
            return;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
    }

    /// <summary>
    /// 设置绝对位置的 x 坐标。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="newValue">x 坐标值。</param>
    public static void SetPositionX(this Transform transform, float newValue)
    {
        Vector3 v = transform.position;
        v.x = newValue;
        transform.position = v;
    }

    /// <summary>
    /// 设置绝对位置的 y 坐标。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="newValue">y 坐标值。</param>
    public static void SetPositionY(this Transform transform, float newValue)
    {
        Vector3 v = transform.position;
        v.y = newValue;
        transform.position = v;
    }

    /// <summary>
    /// 设置绝对位置的 z 坐标。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="newValue">z 坐标值。</param>
    public static void SetPositionZ(this Transform transform, float newValue)
    {
        Vector3 v = transform.position;
        v.z = newValue;
        transform.position = v;
    }

    /// <summary>
    /// 增加绝对位置的 x 坐标。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="deltaValue">x 坐标值增量。</param>
    public static void AddPositionX(this Transform transform, float deltaValue)
    {
        Vector3 v = transform.position;
        v.x += deltaValue;
        transform.position = v;
    }

    /// <summary>
    /// 增加绝对位置的 y 坐标。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="deltaValue">y 坐标值增量。</param>
    public static void AddPositionY(this Transform transform, float deltaValue)
    {
        Vector3 v = transform.position;
        v.y += deltaValue;
        transform.position = v;
    }

    /// <summary>
    /// 增加绝对位置的 z 坐标。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="deltaValue">z 坐标值增量。</param>
    public static void AddPositionZ(this Transform transform, float deltaValue)
    {
        Vector3 v = transform.position;
        v.z += deltaValue;
        transform.position = v;
    }

    /// <summary>
    /// 设置相对位置的 x 坐标。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="newValue">x 坐标值。</param>
    public static void SetLocalPositionX(this Transform transform, float newValue)
    {
        Vector3 v = transform.localPosition;
        v.x = newValue;
        transform.localPosition = v;
    }

    /// <summary>
    /// 设置相对位置的 y 坐标。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="newValue">y 坐标值。</param>
    public static void SetLocalPositionY(this Transform transform, float newValue)
    {
        Vector3 v = transform.localPosition;
        v.y = newValue;
        transform.localPosition = v;
    }

    /// <summary>
    /// 设置相对位置的 z 坐标。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="newValue">z 坐标值。</param>
    public static void SetLocalPositionZ(this Transform transform, float newValue)
    {
        Vector3 v = transform.localPosition;
        v.z = newValue;
        transform.localPosition = v;
    }

    /// <summary>
    /// 增加相对位置的 x 坐标。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="deltaValue">x 坐标值。</param>
    public static void AddLocalPositionX(this Transform transform, float deltaValue)
    {
        Vector3 v = transform.localPosition;
        v.x += deltaValue;
        transform.localPosition = v;
    }

    /// <summary>
    /// 增加相对位置的 y 坐标。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="deltaValue">y 坐标值。</param>
    public static void AddLocalPositionY(this Transform transform, float deltaValue)
    {
        Vector3 v = transform.localPosition;
        v.y += deltaValue;
        transform.localPosition = v;
    }

    /// <summary>
    /// 增加相对位置的 z 坐标。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="deltaValue">z 坐标值。</param>
    public static void AddLocalPositionZ(this Transform transform, float deltaValue)
    {
        Vector3 v = transform.localPosition;
        v.z += deltaValue;
        transform.localPosition = v;
    }

    /// <summary>
    /// 设置尺寸
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="newValue"></param>
    public static void SetLocalScale(this Transform transform, Vector3 newValue)
    {
        transform.localScale = newValue;
    }

    /// <summary>
    /// 设置相对尺寸的 x 分量。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="newValue">x 分量值。</param>
    public static void SetLocalScaleX(this Transform transform, float newValue)
    {
        Vector3 v = transform.localScale;
        v.x = newValue;
        transform.localScale = v;
    }

    /// <summary>
    /// 设置相对尺寸的 y 分量。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="newValue">y 分量值。</param>
    public static void SetLocalScaleY(this Transform transform, float newValue)
    {
        Vector3 v = transform.localScale;
        v.y = newValue;
        transform.localScale = v;
    }

    /// <summary>
    /// 设置相对尺寸的 z 分量。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="newValue">z 分量值。</param>
    public static void SetLocalScaleZ(this Transform transform, float newValue)
    {
        Vector3 v = transform.localScale;
        v.z = newValue;
        transform.localScale = v;
    }

    /// <summary>
    /// 增加相对尺寸的 x 分量。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="deltaValue">x 分量增量。</param>
    public static void AddLocalScaleX(this Transform transform, float deltaValue)
    {
        Vector3 v = transform.localScale;
        v.x += deltaValue;
        transform.localScale = v;
    }

    /// <summary>
    /// 增加相对尺寸的 y 分量。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="deltaValue">y 分量增量。</param>
    public static void AddLocalScaleY(this Transform transform, float deltaValue)
    {
        Vector3 v = transform.localScale;
        v.y += deltaValue;
        transform.localScale = v;
    }

    /// <summary>
    /// 增加相对尺寸的 z 分量。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="deltaValue">z 分量增量。</param>
    public static void AddLocalScaleZ(this Transform transform, float deltaValue)
    {
        Vector3 v = transform.localScale;
        v.z += deltaValue;
        transform.localScale = v;
    }

    /// <summary>
    /// 二维空间下使 <see cref="UnityEngine.Transform" /> 指向指向目标点的算法，使用世界坐标。
    /// </summary>
    /// <param name="transform"><see cref="UnityEngine.Transform" /> 对象。</param>
    /// <param name="lookAtPoint2D">要朝向的二维坐标点。</param>
    /// <remarks>假定其 forward 向量为 <see cref="UnityEngine.Vector3.up" />。</remarks>
    public static void LookAt2D(this Transform transform, Vector2 lookAtPoint2D)
    {
        Vector3 vector = lookAtPoint2D.ToVector3() - transform.position;
        vector.y = 0f;

        if (vector.magnitude > 0f)
        {
            transform.rotation = Quaternion.LookRotation(vector.normalized, Vector3.up);
        }
    }

    /// <summary>
    /// 查找该物体在层级面板中的路径
    /// </summary>
    private static StringBuilder s_PathStringBuilder;
    public static string GetPath(this Transform transform)
    {
        string resultPath = null;
        if (transform == null) return resultPath;
        if (s_PathStringBuilder == null)
        {
            s_PathStringBuilder = new StringBuilder(transform.name);
        }
        else
        {
            s_PathStringBuilder.Clear();
            s_PathStringBuilder.Append(transform.name);
        }
        Transform parent = transform.parent;
        string conStr = "/";
        while (parent != null)
        {
            s_PathStringBuilder.Insert(0, parent.name + conStr);
            parent = parent.parent;
        }
        resultPath = s_PathStringBuilder.ToString();
        return resultPath;
    }

    

    #endregion Transform
    
    
    public static float ToSingle(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return 0;
        }

        float i = 0f;
        try
        {
            i = Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            Log.Error("string convert to single error : ", e);
        }
        return i;
    }
    
    public static float ToSingle(object value)
    {
        if (value is string str)
        {
            return ToFloat(str);
        }

        float i;
        try
        {
            i = Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            Log.Error("object convert to single error : ", e);
            i = ToSingle(value.ToString());
        }
        return i;
    }
}
