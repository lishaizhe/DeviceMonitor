using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System.IO;
using GameFramework;
using UnityGameFramework.Runtime;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace GameKit.Base
{
    public class SpriteAtlasManager : SingletonBehaviour<SpriteAtlasManager>
    {
        public Sprite DefaultSprite;
#if ODIN_INSPECTOR
        [ShowInInspector, ShowIf("showOdinInfo"), DictionaryDrawerSettings(IsReadOnly = true)]
#endif
        private readonly Dictionary<string, List<System.Delegate>> m_CallbackStack = new Dictionary<string, List<System.Delegate>>();

        public override void Release()
        {
            base.Release();

            UnityEngine.U2D.SpriteAtlasManager.atlasRegistered -= OnAtlasRegistered;
            UnityEngine.U2D.SpriteAtlasManager.atlasRequested -= OnAtlasRequested;

            //LSZ
            // ResourceManager.Instance.UnloadAssetWithObject(DefaultSprite);
        }

        public void Awake()
        {
            UnityEngine.U2D.SpriteAtlasManager.atlasRegistered += OnAtlasRegistered;
            UnityEngine.U2D.SpriteAtlasManager.atlasRequested += OnAtlasRequested;
        }


        public void PreloadSpriteAtlas(string atlasName, MemeryHold memeryHold = MemeryHold.Normal, System.Action<SpriteAtlas> action = null)
        {
            //LSZ
            // RegistCallback(atlasName, action);
            // string assetBundle = GetAtlasBundleNameEx(atlasName);
            // ResourceManager.Instance.LoadAssetAsync<SpriteAtlas>(assetBundle, atlasName, (key, asset, err) =>
            // {
            //     if (Log.IsLoad())
            //         Log.Info("+++! PreloadSpriteAtlas LoadAssetAsync ok {0}, err {1}", atlasName, err);
            //
            //     if (string.IsNullOrEmpty(err))
            //     {
            //         SpriteAtlas atlas = asset as SpriteAtlas;
            //         Callback(atlasName, atlas);
            //     }
            //     else
            //     {
            //         Callback(atlasName,null);
            //     }
            // },  memeryHold);
        }

        private void LoadSpriteAtlas(string spriteName,System.Action<SpriteAtlas> action)
        {
            string assetBundle = string.Empty;
            string atlasName = string.Empty;
            RegistCallback(atlasName, action);

            //LSZ
            // ResourceManager.Instance.LoadAssetAsync<SpriteAtlas>(assetBundle, atlasName, (key, asset, err) =>
            // {
            //     if (string.IsNullOrEmpty(err))
            //     {
            //         SpriteAtlas atlas = asset as SpriteAtlas;
            //         Callback(atlasName, atlas);
            //     }
            //     else
            //     {
            //         Callback(atlasName,null);
            //     }
            // });
        }


        public void GetSpriteAsync( string spriteName,  System.Action<Sprite> callback)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                Log.ReleaseError("spriteName is null, in GetSpriteAsync function");
                callback?.Invoke(DefaultSprite);
                return;
            }
            
            LoadSpriteAtlas(spriteName,(spriteAtlas) =>
            {
                Sprite sprite = null;
                if (spriteAtlas == null)
                {
                    Log.ReleaseError($"{spriteName}没有查到所在图集, 请检查!");
                    callback?.Invoke( DefaultSprite);
                    return;
                }

                if (!string.IsNullOrEmpty(spriteName))
                {
                    spriteName = Path.GetFileNameWithoutExtension(spriteName);
                    sprite = AtlasUtils.GetOrAddCacheSprite(spriteAtlas, spriteName);
                }

                if (sprite != null)
                {
                    callback?.Invoke( sprite);
                    return;
                }

                Log.ReleaseError("sprite is null, in GetSpriteAsync function");
                callback?.Invoke( DefaultSprite);
            });

        }
        

        private void OnAtlasRegistered(SpriteAtlas sa)
        {
            //Debugger.LogWarningFormat("OnAtlasRegistered: {0}", sa.name);
        }

        private void OnAtlasRequested(string atlasName, System.Action<SpriteAtlas> action)
        {
            //Debugger.LogWarningFormat("OnAtlasRequested: {0}", atlasName);
            PreloadSpriteAtlas(atlasName,MemeryHold.Normal, action);
        }

        private void Callback(string key, SpriteAtlas spriteAtlas)
        {
            if (m_CallbackStack.TryGetValue(key, out List<System.Delegate> callbackList))
            {
                m_CallbackStack.Remove(key);
                foreach (System.Action<SpriteAtlas> callback in callbackList)
                {
                    callback?.Invoke(spriteAtlas);
                }
            }
        }

        private void RegistCallback(string key, System.Action<SpriteAtlas> callback)
        {
            if (callback != null && !string.IsNullOrEmpty(key))
            {
                if (!m_CallbackStack.ContainsKey(key))
                    m_CallbackStack.Add(key, new List<System.Delegate>());
                if (!m_CallbackStack[key].Contains(callback))
                    m_CallbackStack[key].Add(callback);
            }
        }
     
    }
}
