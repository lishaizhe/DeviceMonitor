using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityGameFramework.Runtime;

public static class AtlasUtils
{
    public static Dictionary<string, Dictionary<string, Sprite>> m_cacheSprite = new Dictionary<string, Dictionary<string, Sprite>>();
       
    public static Sprite GetOrAddCacheSprite(SpriteAtlas atlas, string spriteFile)
    {
        if (atlas == null || string.IsNullOrEmpty(spriteFile))
            return null;

        Sprite sprite = null;
        if (m_cacheSprite.TryGetValue(atlas.name, out var sprites))
        {
            if (sprites.TryGetValue(spriteFile, out sprite))
                return sprite;
        }
        else
        {
            sprites = new Dictionary<string, Sprite>();
            m_cacheSprite.Add(atlas.name, sprites);
        }

        sprite = atlas.GetSprite(spriteFile);
        sprites.Add(spriteFile, sprite);
        return sprite;
    }

    public static Sprite GetCacheSprite(string spriteFile)
    {
        if (IsHaveSpriteByFileName(spriteFile,out Sprite sprite))
        {
            return sprite;
        }
        return null;
    }

    //根据图片名判断缓存中是否有sprite
    public static bool IsHaveSpriteByFileName(string spriteFile,out Sprite sprite)
    {
        sprite = null;
        if (string.IsNullOrEmpty(spriteFile))
            return false;

        //LSZ
        // var datarow = GameEntry.Table.GetDataRow<LF.SpritesDataRow>(spriteFile);
        // if (null == datarow)
        //     return false;
        // if (m_cacheSprite.TryGetValue(datarow.AtlasOrBundleName, out var sprites))
        // {
        //     if (sprites.TryGetValue(spriteFile, out sprite))
        //         return true;
        // }
        return false;
    }

    public static void ClearCacheSprite(string atlasName)
    {
        if (!string.IsNullOrEmpty(atlasName))
        {
            m_cacheSprite.Remove(atlasName);
        }
    }

    public static void ClearAllCacheSprites()
    {
        m_cacheSprite.Clear();
    }
}
