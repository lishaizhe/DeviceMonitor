/// <summary>
/// 单例模式类
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : new()
{
    private static T t_instance;
    public static T Instance 
    { 
        get 
        {
            if (t_instance == null)
            {
                t_instance = new T();
            }
            return t_instance;
        }
    }

}
