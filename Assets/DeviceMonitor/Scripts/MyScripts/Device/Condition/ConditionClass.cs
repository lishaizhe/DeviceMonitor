using System;

/// <summary>
/// �豸������Ϣ
/// </summary>
[Serializable]
public class ConditonsDescribe
{
    public string[] ActCon1_0 = new string[8];
    public string[] ActCon1_1 = new string[8];
    public string[] ActCon2_0 = new string[8];
    public string[] ActCon2_1 = new string[8];
    public string[] LinkCon1_0 = new string[8];
    public string[] LinkCon1_1 = new string[8];
    public string[] LinkCon2_0 = new string[8];
    public string[] LinkCon2_1 = new string[8];
}

/// <summary>
/// ��������
/// </summary>
public enum ConditionsType
{
    ActCon1_0,
    ActCon1_1,
    ActCon2_0,
    ActCon2_1,
    LinkCon1_0,
    LinkCon1_1,
    LinkCon2_0,
    LinkCon2_1,
}