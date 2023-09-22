using UnityEngine;
/*
 * 备注
 * 1. 适用性：场景中存在唯一的对象，即可让该对象继承当前类;
 * 2. 如何使用：
 *      -继承时必须传递子类类型;
 *      -在任意脚本生命周期中，通过子类类型访问Instance属性;
 */

/// <summary>
/// 脚本单例类
/// </summary>
/// <typeparam name="T"></typeparam>
public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T instance;
    public static T Instance 
    {
        get 
        {
            if (instance == null)
            {
                // 在场景中查找
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    // 创建脚本对象 // 创建对象立即执行Awake
                    instance = new GameObject("Singleton Of " + typeof(T)).AddComponent<T>();
                }
                else
                {
                    instance.Init();
                }
            }
            return instance;
        }  
    }

    protected void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            instance.Init();
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init()
    { 
    
    }
}
