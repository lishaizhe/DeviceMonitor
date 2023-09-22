/// <summary>
/// UI类型(主界面|弹出窗口)
/// </summary>
public enum UITpye
{
    Main,
    Sub,
}

/// <summary>
/// 场景类型，现阶段不做处理
/// </summary>
public enum ScenesType
{
    None,
    Login
}

/// <summary>
/// 打开界面动画类型
/// </summary>
public enum WindowAnimationType
{
    None,
    Right2Left,
    Left2Right,
    Fade
}

public enum WindowLoadType
{
    FromPrefab,
    FromGameObject
}