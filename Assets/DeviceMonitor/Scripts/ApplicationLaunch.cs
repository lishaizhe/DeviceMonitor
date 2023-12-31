﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using Logger = VEngine.Logger;

/*
 *  项目启动项
 *  在这块初始化XAsset,以及开始登录界面
 */
public class ApplicationLaunch : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //开启日志输出
        Logger.Loggable = true;
        var cacheRoot = GameEntry.CacheRoot;
        GameEntry.Resource.Initialize((bool result) =>
        {
            if (result != true)
            {
                Logger.E("初始化XAsset失败");
                return;
            }
            
            Logger.I("初始化成功");
            GameEntry.UI.OpenUIForm(EntityAssets.UIBackground, "Default");
            GameEntry.UI.OpenUIForm(EntityAssets.UINavigationBar, "Top");
            // GameEntry.UI.OpenUIForm(EntityAssets.UILoading, "Default");
            // GameEntry.Resource.LoadAssetAsync<GameObject>(EntityAssets.CUBE, asset =>
            // {
            //     LoadingParam param = new LoadingParam();
            //     param.num1 = 3;
            //     param.num2 = 5;
            //     GameEntry.UI.OpenUIForm(EntityAssets.UILoading, "Default", param);
            // });
        });
    }

    // Update is called once per frame
    private float m_dt = 0.0f;
    void Update()
    {
        GameEntry.Update(Time.deltaTime);
        var width = Screen.width;
        var height = Screen.height;
        m_dt += Time.deltaTime;
        if (m_dt > 5.0f)
        {
            Debug.Log($">>>size: width: {width}  height: {height}");
            m_dt = 0.0f;
        }
    }
}
