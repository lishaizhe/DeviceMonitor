using UnityEngine;

public static class GameObjectOpt
{
    public static void RemoveAllChild(this Transform t)
    {
        int childCnt = t.childCount;
        for (int i = 0; i < childCnt; ++i)
        {
            var obj = t.GetChild(i);
            GameObject.DestroyImmediate(obj.gameObject);
        }
    }

    public static void SetLayer(this GameObject o, string layerName)
    {
        o.layer = LayerMask.NameToLayer(layerName);
        Transform[] allTrans = o.transform.GetComponentsInChildren<Transform>();
        foreach (var trans in allTrans)
        {
            trans.gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }
}
