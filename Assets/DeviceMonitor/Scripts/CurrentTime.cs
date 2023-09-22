using System;
using UnityEngine;
using UnityEngine.UI;

public class CurrentTime : MonoBehaviour
{

    private Text _currrentTimeText;
    private int hour;
    private int minute;
    private int second;
    private int year;
    private int month;
    private int day;
    private string week;

    // Use this for initialization
    void Start()
    {
        _currrentTimeText = GetComponent<Text>();

    }

    // Update is called once per frame
    void Update()
    {
        //获取当前时间
        hour = DateTime.Now.Hour;
        minute = DateTime.Now.Minute;
        second = DateTime.Now.Second;
        year = DateTime.Now.Year;
        month = DateTime.Now.Month;
        day = DateTime.Now.Day;
       // week = DateTime.Now.DayOfWeek;

        //格式化显示当前时间
        _currrentTimeText.text = string.Format("{0:D2}年{1:D2}月{2:D2}日" + "|"+"{3:D2}:{4:D2}:{5:D2}", year, month, day, hour, minute, second);
       // _currrentTimeText.text = string.Format("{0:D2}年{1:D2}月{2:D2} " , year, month, day)+ "|"+ week;

#if UNITY_EDITOR
       // Debug.Log("W now " + System.DateTime.Now);     //当前时间（年月日时分秒）
       // Debug.Log("W utc " + System.DateTime.UtcNow);  //当前时间（年月日时分秒）
#endif
    }
}