using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System.IO;
using GameKit.Base;
using System.Text;
using AssetBundles;
using GameFramework;

public class ShaderManager : SingletonBehaviour<ShaderManager>
{
    private Dictionary<string, Shader> shaderDic = new Dictionary<string, Shader>();
    private const string shaderPath = "Assets/Shelter/LFShader.bundle";
    private bool isInitialize = false;
    private Shader defaultShader;

    // shader映射？
    private Dictionary<string, string> mapping = new Dictionary<string, string>();

    public ShaderManager()
    {
        mapping.Add("Unlit/Texture", "Standard");
    }

    public Shader Find(string shaderName)
    {
        Shader outShader = null;
        if(mapping.ContainsKey(shaderName))
        {
            shaderName = mapping[shaderName];
        }
        
        // 如果是编辑器模式，是在编辑器下运行bundle模式；
        if ((PlatformUtils.IsEditor() && !PlatformUtils.IsSimulateAssetBundleInEditor()) 
            || !shaderDic.TryGetValue(shaderName, out outShader))
        {
            // 那么这里没找到，就表示是内置shader;
            outShader = Shader.Find(shaderName);
        }
        if (outShader != null)
        {
            return outShader;
        }
#if !UNITY_EDITOR
            Log.Error("not find shader : {0}", shaderName);
#endif
        //默认的材质如果存在的话
        if(shaderDic.TryGetValue("Standard",out outShader))
        {
            return outShader;
        }


        return defaultShader;
    }

    /// <summary>
    /// 从bundle中预加载shader
    /// </summary>
    public void Initialize(System.Action onCompleted)
    {
        if (isInitialize)
        {
            return;
        }
        isInitialize = true;

        if (defaultShader == null)
        {
            defaultShader = Shader.Find("Unlit/Texture");
        }

        //编辑器模式下并且模拟ab包时，不加载lfshader.bundle
        if (PlatformUtils.IsEditor() && PlatformUtils.IsSimulateAssetBundleInEditor())
        {
            onCompleted?.Invoke();
            return;
        }

        // 加载所有shader
        ResourceManager.Instance.LoadAssetAsync<UnityEngine.Object>(shaderPath, null, 
            (key, asset, err)=>
        {
            PreInitState.LogEvent ("LoadShaderFileSuccess");
            
            OnLoadShaderComplete(key, asset, err);
            
            PreInitState.LogEvent ("LoadShaderCompleteNew");

            onCompleted?.Invoke();
        },  MemeryHold.Always);

        return;
    }

    private void OnLoadShaderComplete(string key, object asset, string err)
    {
        if (!string.IsNullOrEmpty(err))
        {
            Log.Error("ShaderManager load shader error: {0}", err);
            return;
        }

        object[] obs = (object[])asset;
        if (obs == null)
        {
            Log.Error("ShaderManager load shader empty.");
            return;
        }
        ShaderVariantCollection variantCollection = null;
        for (int i = 0; i < obs.Length; i++)
        {
            var ob = obs[i];
            if (ob == null)
            {
                continue;
            }

            if (ob is ShaderVariantCollection)
            {
                 variantCollection = ob as ShaderVariantCollection;
               
            }
            else if (ob is Shader)
            {
                var shader = ob as Shader;
                if (shader == null)
                {
                    continue;
                }
                //Debug.Log("load shader===" + shader.name);
                if (!shaderDic.ContainsKey(shader.name))
                {
                    shaderDic.Add(shader.name, shader);
                }

            }
        }
        // 尝试条件编译下shader 
        if(PlatformUtils.IsAndroidPlatform())
        {
            if(HotConfig.AndroidShaderCompile)
            {
                if (variantCollection != null && !variantCollection.isWarmedUp)
                    variantCollection.WarmUp();
                else
                {
                    if (variantCollection == null)
                        Log.Error("load asset shader variant colleciton null error ");
                }
            }
        }
        else // ios 等其他平台  
        {
            if (variantCollection != null && !variantCollection.isWarmedUp)
                variantCollection.WarmUp();
            else
            {
                if (variantCollection == null)
                    Log.Error("load asset shader variant colleciton null error ");
            }
        }
    }

    public void UseEditorShader(ref Shader shader)
    {
        if (PlatformUtils.IsSimulateAssetBundleInEditor())
            return;
        if (shader == null)
            return;
        var shaderName = shader.name;
        var newShader = Find(shaderName);
        if (newShader != null)
            shader = newShader;
    }
    
    public void UseEditorShader(Material material)
    {
        if (PlatformUtils.IsSimulateAssetBundleInEditor())
            return;
        if (material == null || material.shader == null)
            return;
        var shaderName = material.shader.name;
        var newShader = Find(shaderName);
        if (newShader != null)
            material.shader = newShader;
    }
    
   


}


