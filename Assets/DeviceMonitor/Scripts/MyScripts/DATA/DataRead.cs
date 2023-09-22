using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;


/// <summary>
/// 从web端读取数据
/// </summary>
public class DataRead : MonoBehaviour
{
    //[DllImport("__Internal")]
    //private static extern string GetDevicesData();

    //// value components list
    //List<TextMeshProUGUI> allValueList = new List<TextMeshProUGUI>();
    //// update time
    //private float timeUpdate = 1.0f;
    //private float timeSum = 0.0f;

    ////
    //private string[] dataRead;
    //private void Start()
    //{
    //    GetValueObj();
    //}

    //private void Update()
    //{
    //    timeSum += Time.deltaTime;
    //    if (timeSum >= timeUpdate)
    //    {
    //        timeSum = 0.0f;
    //        // 更新数据
    //        int index = 0;
    //        dataRead = GetDevicesData().Split(",");
    //        foreach (var item in allValueList)
    //        {
    //            if (index < dataRead.Length)
    //                item.text = dataRead[index];
    //            index++;
    //        }
    //    }
    //}
    ///// <summary>
    ///// 获得值对象
    ///// </summary>
    //private void GetValueObj()
    //{
    //    var all = GetComponentsInChildren<TextMeshProUGUI>();
    //    foreach (var item in all)
    //    {
    //        if (item.gameObject.name == "value")
    //        {
    //            allValueList.Add(item.GetComponent<TextMeshProUGUI>());
    //        }
    //    }
    //}
}
