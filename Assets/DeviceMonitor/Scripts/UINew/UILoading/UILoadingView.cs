using System.Collections;
using System.Collections.Generic;
using GameFramework;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using XChartsDemo;
using Logger = VEngine.Logger;

public struct LoadingParam
{
    public int num1;
    public int num2;
}

public class UILoadingView : BaseUIForm
{
    [SerializeField] private InputField m_inputUserName;
    [SerializeField] private Text m_textInputUserName;
    [SerializeField] private InputField m_inputPassword;
    [SerializeField] private Text m_textInputPassword;
    [SerializeField] private Button m_btnLogin;
    protected internal override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        // LoadingParam param = (LoadingParam)userData;
    }

    protected internal override void OnClose(object userData)
    {
        base.OnClose(userData);
    }

    public void OnClickBtn()
    {
        string strUserName = m_textInputUserName.text;
        string strPassword = m_textInputUserName.text;
        if (string.IsNullOrEmpty(strUserName) || string.IsNullOrEmpty(strPassword))
        {
            UIUtil.ShowTips("没有设置名字或者密码");
            return;
        }

        string url = "http://121.40.254.4:9009/displaylogin";
        WWWForm form = new WWWForm();
        form.AddField("username", strUserName);
        form.AddField("password", strPassword);
        GameEntry.WebRequest.Post(url, form, (request, err, userdata) =>
        {
            if (request.isDone)
            {
                if (string.IsNullOrEmpty(request.error))
                {
                    Dictionary<string, object> dict =
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
                    if (dict == null)
                        return;
                    if (dict.TryGetValue("message", out object value))
                    {
                        Logger.I("login ok token is {0}", (string)value);
                        GameEntry.WebRequest.ClearHeader();
                        GameEntry.WebRequest.SetHeader("x-auth-token", (string)value);
                        GameEntry.WebRequest.SetHeader("Content-Type", "application/json");
                    }
                }
            }
        }, 0, 10);
    }


    public void OnClickBtn2()
    {
        List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
        Dictionary<string, string> param = new Dictionary<string, string>();
        param["id"] = "2c91b8748a6a8965018a6abb52880125";
        param["allremarks"] = "V1阀门";
        param["name"] = "BZ1800.Valve.BZ1800.Valve.V1.Cmd1";
        param["templateid"] = "2c91b8748a692a5c018a69fc0cc8047d";
        param["equipmentid"] = "2c91b8748a692a5c018a69eb155b040c";
        param["equipmentname"] = "BZ1800.Valve.V1";
        param["eqfieldlistid"] = "2c91b8748a692a5c018a69daa73703c8";
        param["eqfieldlistname"] = "Cmd1";
        list.Add(param);

        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict["infolist"] = list;
        dict["sortName"] = "addtime";
        dict["sortType"] = "desc";
        dict["pageSize"] = 20;
        dict["pageNumber"] = 1;
        string json2 = JsonDicConvert.ObjectToJson(dict);
        string url = "";
        GameEntry.WebRequest.PostRaw("http://121.40.254.4:9009/realtimedata/gettemplatedatapage", json2,
            (request, err, userdata) =>
            {
                var a = 1;
            });

    }





}
