using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using VEngine.Editor;
using EditorUtility = UnityEditor.EditorUtility;

public class Publish
{
    [MenuItem("Tools/打包")]
    public static void Build()
    {
        BuildTarget t = EditorUserBuildSettings.activeBuildTarget;
        var result = EditorUtility.DisplayDialogComplex("开始打包", $"再次确认平台是否正确,当前是{t}", "OK", "Cancle", "");
        if (result != 0)
            return;
        //1.打所有图集
        SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget, false);
        //2.开始打bundle
        BuildBundle();
        //3.开始打包
        BuildProduct();
    }
    
    //删除所有已经打过的bundle
    private static void ClearBundles()
    {
        // 所有bundle删除
        var buildPath = VEngine.Editor.EditorUtility.PlatformBuildPath;
        if (Directory.Exists(buildPath))
        {
            Directory.Delete(buildPath, true);
        }

        Directory.CreateDirectory(buildPath);
    }

    private static void BuildBundle()
    {
        var buildPath = VEngine.Editor.EditorUtility.PlatformBuildPath;
        Debug.Log("[Publish] 清空 " + buildPath);
        ClearBundles();
        
        BuildScript.BuildBundles();
        
        Debug.Log("[Publish] 生成manifest版本文件");
        BuildScript.SaveManifestVersion();
        
        Debug.Log("[Publish] CopyToStreamingAssets");
        BuildScript.CopyToStreamingAssets();
    }
    
    [MenuItem("Tools/11")]
    private static void BuildProduct()
    {
        string ProductPath = "Product";
        PlayerSettings.companyName = "YBCK";
        PlayerSettings.productName = "设备监控";
        PlayerSettings.resizableWindow = true;
        PlayerSettings.allowFullscreenSwitch = true;
        EditorUserBuildSettings.development = false;
        if (Directory.Exists(ProductPath))
            Directory.Delete(ProductPath, true);
        Directory.CreateDirectory(ProductPath);
        string[] levels = GetLevelsFromBuildSettings();
        BuildOptions options = BuildOptions.StrictMode;
        var result = BuildPipeline.BuildPlayer(levels, $"{ProductPath}/设备监控", EditorUserBuildSettings.activeBuildTarget, options);
        if (result)
            EditorUtility.DisplayDialog("打包成功", "", "");
        else
            EditorUtility.DisplayDialog("打包失败", "", "");

    }

    static string[] GetLevelsFromBuildSettings()
    {
        List<string> levels = new List<string>();
        for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
        {
            if (EditorBuildSettings.scenes[i].enabled)
                levels.Add(EditorBuildSettings.scenes[i].path);
        }

        return levels.ToArray();
    }

}
