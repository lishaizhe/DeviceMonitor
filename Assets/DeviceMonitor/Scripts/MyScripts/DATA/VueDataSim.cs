using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 模拟数据
/// </summary>
public class VueDataSim
{
    #region 系统设备数据模拟

    //------------------------field------------------------*/
    // db1 valve
    private static string[] valveFields = new string[] {
        "Cmd1",
        "State",
        "Reset",
        "Describle"
    };
    // db2 pump
    private static string[] pumpFields = new string[] {
        "Cmd1",
        "State",
        "Reset",
        "Describle"
    };
    // db4 ai
    private static string[] aiFields = new string[] {
        "State",
        "ValCur",
        "Describle",
        "HiSet",
        "LoSet",
        "HiEnable",
        "LoEnable"
    };

    //------------------------bz1800真空流程------------------------*/
    // valve
    private static string[] bz1800VacuumValve = new string[] {
        "BZ1800.Valve.V1",
        "BZ1800.Valve.V2",
        "BZ1800.Valve.V3",
        "BZ1800.Valve.V4",
        "BZ1800.Valve.V5",
        "BZ1800.Valve.V6",
        "BZ1800.Valve.V7",
        "BZ1800.Valve.V8"
    };
    // pump
    private static string[] bz1800VacuumPump = new string[] {
        "BZ1800.Pump.P1",
        "BZ1800.Pump.P2",
        "BZ1800.Pump.P3"
    };
    // AI
    private static string[] bz1800VacuumAI = new string[] {
        "BZ1800.AI.CW1",
        "BZ1800.AI.CW2",
        "BZ1800.AI.CW3",
        "BZ1800.AI.CW4",
        "BZ1800.AI.P1Curr",
        "BZ1800.AI.P1Freq",
        "BZ1800.AI.P2T1",
        "BZ1800.AI.P2T2",
        "BZ1800.AI.P2P",
        "BZ1800.AI.G4"
    };

    //------------------------bz1800低温流程------------------------*/
    // valve
    private static string[] bz1800CryogenicValve = new string[] {
        "BZ1800.Valve.C1",
        "BZ1800.Valve.C2"
    };
    // dvalve
    private static string[] bz1800CryogenicDValve = new string[] {
        "BZ1800.Valve.D1"
    };
    // AI
    private static string[] bz1800CryogenicAI = new string[] {
        "BZ1800.AI.TIA1",
        "BZ1800.AI.TIA2",
        "BZ1800.AI.TIA3",
        "BZ1800.AI.PIA1",
        "BZ1800.AI.PIA2",
        "BZ1800.AI.PIA3",
        "BZ1800.AI.LIA1"
    };
    // pid
    private static string[] bz1800CryogenicPID = new string[] {
        "BZ1800.PID.EHEAT1"
    };

    //------------------------bz1000真空流程------------------------*/ 
    // valve
    private static string[] bz1000VacuumValve = new string[] {
        "BZ1000.Valve.V1",
        "BZ1000.Valve.V2",
        "BZ1000.Valve.V3",
        "BZ1000.Valve.V4",
        "BZ1000.Valve.V5",
        "BZ1000.Valve.V6",
        "BZ1000.Valve.V7",
    };
    // pump
    private static string[] bz1000VacuumPump = new string[] {
        "BZ1000.Pump.P1",
        "BZ1000.Pump.P2",
        "BZ1000.Pump.P3",
        "BZ1000.Pump.P4"
    };
    // AI
    private static string[] bz1000VacuumAI = new string[] {
        "BZ1000.AI.P4Curr",
        "BZ1000.AI.P4Freq",
        "BZ1000.AI.P3T1",
        "BZ1000.AI.P3T2",
        "BZ1000.AI.P3P",
        "BZ1000.AI.G1",
        "BZ1000.AI.G2"
    };
    #endregion


    /// <summary>
    /// 测试获取模板
    /// </summary>
    /// <param name="pageName"></param>
    /// <returns></returns>
    public static string GetTemplates(string item)
    {
        var data = JsonConvert.DeserializeObject<TemplateItem>(item);
        if (data == null)
        {
            return "";
        }
        if (data.pagename == "实时数据")
        {
            var t = new List<Template>();
            // BZ1800.真空流程
            var item1 = new Template();
            item1.templateName = "BZ1800.真空流程";
            item1.templateid = "100001";
            t.Add(item1);
            // BZ1800.低温流程
            var item2 = new Template();
            item2.templateName = "BZ1800.低温流程";
            item2.templateid = "100002";
            t.Add(item2);
            // BZ1000.真空流程
            var item3 = new Template();
            item3.templateName = "BZ1000.真空流程";
            item3.templateid = "100003";
            t.Add(item3);

            return JsonConvert.SerializeObject(t.ToArray());
        }
        if (data.pagename == "实时事件")
        {
            var t = new List<Template>();
            // BZ1800
            var item1 = new Template();
            item1.templateName = "BZ1800.实时事件";
            item1.templateid = "110001";
            t.Add(item1);
            // BZ1000
            var item2 = new Template();
            item2.templateName = "BZ1000.实时事件";
            item2.templateid = "110002";
            t.Add(item2);

            return JsonConvert.SerializeObject(t.ToArray());
        }
        return "";
    }

    private static Dictionary<string, List<TemplateData>> deviceInfo
        = new Dictionary<string, List<TemplateData>>();
    private static Dictionary<string, Dictionary<string, int>> deviceIndex
        = new Dictionary<string, Dictionary<string, int>>();
    public static string GetTemplateDatas(string item)
    {
        var data = JsonConvert.DeserializeObject<TemplateDataItem>(item);
        if (data == null)
        {
            return "";
        }

        // bz1800真空流程
        if (data.templateid == "100001")
        {
            if (!deviceInfo.ContainsKey(data.templateid))
            {
                var index = 0;
                var itemInfo = new List<TemplateData>();
                var itemIndex = new Dictionary<string, int>();

                // 生成阀门模拟信息
                foreach (string device in bz1800VacuumValve)
                {
                    foreach (var feild in valveFields)
                    {
                        var t = new TemplateData
                        {
                            eqid = System.Guid.NewGuid().ToString("N"),
                            name = device
                        };
                        t.fieldid = System.Guid.NewGuid().ToString("N");
                        t.fieldName = feild;

                        itemInfo.Add(t);
                        itemIndex.Add(t.name + t.fieldName, index);

                        index++;
                    }
                }

                //生成泵模拟信息
                foreach (string device in bz1800VacuumPump)
                {
                    foreach (var feild in pumpFields)
                    {
                        var t = new TemplateData
                        {
                            eqid = System.Guid.NewGuid().ToString("N"),
                            name = device
                        };
                        t.fieldid = System.Guid.NewGuid().ToString("N");
                        t.fieldName = feild;

                        itemInfo.Add(t);
                        itemIndex.Add(t.name + t.fieldName, index);

                        index++;
                    }
                }

                //生成AI模拟信息
                foreach (string device in bz1800VacuumAI)
                {
                    foreach (var feild in aiFields)
                    {
                        var t = new TemplateData
                        {
                            eqid = System.Guid.NewGuid().ToString("N"),
                            name = device
                        };
                        t.fieldid = System.Guid.NewGuid().ToString("N");
                        t.fieldName = feild;

                        itemInfo.Add(t);
                        itemIndex.Add(t.name + t.fieldName, index);

                        index++;
                    }
                }

                deviceInfo.Add(data.templateid, itemInfo);
                deviceIndex.Add(data.templateid, itemIndex);
                return JsonConvert.SerializeObject(deviceInfo[data.templateid].ToArray());
            }
            else
            {
                return JsonConvert.SerializeObject(deviceInfo[data.templateid].ToArray());
            }
        }

        // bz1800低温流程
        if (data.templateid == "100002")
        {
            if (!deviceInfo.ContainsKey(data.templateid))
            {
                var index = 0;
                var itemInfo = new List<TemplateData>();
                var itemIndex = new Dictionary<string, int>();

                // 生成阀门模拟信息
                foreach (string device in bz1800CryogenicValve)
                {
                    foreach (var feild in valveFields)
                    {
                        var t = new TemplateData
                        {
                            eqid = System.Guid.NewGuid().ToString("N"),
                            name = device
                        };
                        t.fieldid = System.Guid.NewGuid().ToString("N");
                        t.fieldName = feild;

                        itemInfo.Add(t);
                        itemIndex.Add(t.name + t.fieldName, index);

                        index++;
                    }
                }

                //生成AI模拟信息
                foreach (string device in bz1800CryogenicAI)
                {
                    foreach (var feild in aiFields)
                    {
                        var t = new TemplateData
                        {
                            eqid = System.Guid.NewGuid().ToString("N"),
                            name = device
                        };
                        t.fieldid = System.Guid.NewGuid().ToString("N");
                        t.fieldName = feild;

                        itemInfo.Add(t);
                        itemIndex.Add(t.name + t.fieldName, index);

                        index++;
                    }
                }

                deviceInfo.Add(data.templateid, itemInfo);
                deviceIndex.Add(data.templateid, itemIndex);
                return JsonConvert.SerializeObject(deviceInfo[data.templateid].ToArray());
            }
            else
            {
                return JsonConvert.SerializeObject(deviceInfo[data.templateid].ToArray());
            }
        }

        // bz1000真空流程
        if (data.templateid == "100003")
        {
            if (!deviceInfo.ContainsKey(data.templateid))
            {
                var index = 0;
                var itemInfo = new List<TemplateData>();
                var itemIndex = new Dictionary<string, int>();

                // 生成阀门模拟信息
                foreach (string device in bz1000VacuumValve)
                {
                    foreach (var feild in valveFields)
                    {
                        var t = new TemplateData
                        {
                            eqid = System.Guid.NewGuid().ToString("N"),
                            name = device
                        };
                        t.fieldid = System.Guid.NewGuid().ToString("N");
                        t.fieldName = feild;

                        itemInfo.Add(t);
                        itemIndex.Add(t.name + t.fieldName, index);

                        index++;
                    }
                }

                //生成泵模拟信息
                foreach (string device in bz1000VacuumPump)
                {
                    foreach (var feild in pumpFields)
                    {
                        var t = new TemplateData
                        {
                            eqid = System.Guid.NewGuid().ToString("N"),
                            name = device
                        };
                        t.fieldid = System.Guid.NewGuid().ToString("N");
                        t.fieldName = feild;

                        itemInfo.Add(t);
                        itemIndex.Add(t.name + t.fieldName, index);

                        index++;
                    }
                }

                //生成AI模拟信息
                foreach (string device in bz1000VacuumAI)
                {
                    foreach (var feild in aiFields)
                    {
                        var t = new TemplateData
                        {
                            eqid = System.Guid.NewGuid().ToString("N"),
                            name = device
                        };
                        t.fieldid = System.Guid.NewGuid().ToString("N");
                        t.fieldName = feild;

                        itemInfo.Add(t);
                        itemIndex.Add(t.name + t.fieldName, index);

                        index++;
                    }
                }

                deviceInfo.Add(data.templateid, itemInfo);
                deviceIndex.Add(data.templateid, itemIndex);
                return JsonConvert.SerializeObject(deviceInfo[data.templateid].ToArray());
            }
            else
            {
                return JsonConvert.SerializeObject(deviceInfo[data.templateid].ToArray());
            }
        }

        return "";
    }

    private static Dictionary<string, List<string>> deviceValue =
        new Dictionary<string, List<string>>();
    private static int _count = 0;
    public static string GetTemplateValue(string item)
    {
        var data = JsonConvert.DeserializeObject<TemplateDataItem>(item);
        if (data == null)
        {
            return "";
        }

        if (deviceInfo.ContainsKey(data.templateid))
        {
            if (!deviceValue.ContainsKey(data.templateid))
            {
                //init
                var feildValue = new List<string>();
                for (var i = 0; i < deviceInfo[data.templateid].Count; i++)
                {
                    var feild = deviceInfo[data.templateid][i];

                    switch (feild.fieldName)
                    {
                        case "Cmd1":
                            feildValue.Add("false");
                            break;
                        case "State":
                            feildValue.Add("0");
                            break;
                        case "Reset":
                            feildValue.Add("false");
                            break;
                        case "Describle":
                            feildValue.Add($"<{feild.name}>的描述");
                            break;
                        case "ValCur":
                            feildValue.Add("000.0");
                            break;
                        case "HiSet":
                            feildValue.Add("200.0");
                            break;
                        case "LoSet":
                            feildValue.Add("-200.0");
                            break;
                        case "HiEnable":
                            feildValue.Add("true");
                            break;
                        case "LoEnable":
                            feildValue.Add("true");
                            break;
                        default:
                            break;
                    }
                }
                // 添加数据
                deviceValue.Add(data.templateid, feildValue);
            }
            else
            {
                _count++;
                // 每请求10次随机生成1次数据
                if (_count > 9)
                {
                    _count = 0;
                    for (var i = 0; i < deviceInfo[data.templateid].Count; i++)
                    {
                        var feild = deviceInfo[data.templateid][i];
                        var index = 0;
                        switch (feild.fieldName)
                        {
                            case "State":
                                if (deviceIndex[data.templateid].TryGetValue(feild.name + feild.fieldName, out index))
                                {
                                    //Debug.Log(feild.name + feild.fieldName + "---" + index);
                                    deviceValue[data.templateid][index] =
                                        Random.Range(0f, 1024f).ToString("F0");
                                }
                                break;
                            case "ValCur":
                                if (deviceIndex[data.templateid].TryGetValue(feild.name + feild.fieldName, out index))
                                {
                                    deviceValue[data.templateid][index] =
                                        Random.Range(-200f, 200f).ToString("F1");
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            // 返回请求的数据
            return JsonConvert.SerializeObject(deviceValue[data.templateid].ToArray());
        }
        return "";
    }

    public static string SetTemplateValue(string item)
    {
        var data = JsonConvert.DeserializeObject<SetTemplateValues>(item);
        if (data != null)
        {
            foreach (var setInfo in data.set)
            {
                var isFind = false;
                foreach (var sysName in deviceInfo.Keys)
                {
                    foreach (var device in deviceInfo[sysName])
                    {
                        if (device.eqid == setInfo.eqid && device.fieldid == setInfo.fieldid)
                        {
                            var index = deviceIndex[sysName][device.name + device.fieldName];
                            deviceValue[sysName][index] = setInfo.value;
                            isFind = true;
                            break;
                        }
                    }
                    if (isFind) { break; }
                }
            }
            return "true";
        }
        else
        {
            return "false";
        }
    }

    private static List<TemplateEvent> bz1800Event;
    private static List<TemplateEvent> bz1000Event;

    public static string GetTemplateEvent(string item)
    {
        var data = JsonConvert.DeserializeObject<TemplateDataItem>(item);
        if (data == null)
        {
            return "";
        }

        // bz1800
        if (data.templateid == "110001")
        {
            if (bz1800Event == null)
            {
                bz1800Event = new List<TemplateEvent>();
                //1.
                var e1 = new TemplateEvent();
                e1.id = "1100011";
                e1.starttime = "2023-05-07 09:00:00";
                e1.endtime = "2023-05-07 10:00:00";
                e1.name = "BZ1800.V1";
                e1.msg = "超时故障";
                bz1800Event.Add(e1);
                //2.
                var e2 = new TemplateEvent();
                e2.id = "1100012";
                e2.starttime = "2023-05-08 09:00:00";
                e2.endtime = "2023-05-08 10:00:00";
                e2.name = "BZ1800.V2";
                e2.msg = "传感器故障";
                bz1800Event.Add(e2);
            }
            return JsonConvert.SerializeObject(bz1800Event.ToArray());
        }

        // bz1000
        if (data.templateid == "110002")
        {
            if (bz1000Event == null)
            {
                bz1000Event = new List<TemplateEvent>();
                //1.
                var e1 = new TemplateEvent();
                e1.id = "1100021";
                e1.starttime = "2023-05-07 09:00:00";
                e1.endtime = "2023-05-07 10:00:00";
                e1.name = "BZ1000.V1";
                e1.msg = "超时故障";
                bz1000Event.Add(e1);
                //2.
                var e2 = new TemplateEvent();
                e2.id = "1100022";
                e2.starttime = "2023-05-08 09:00:00";
                e2.endtime = "2023-05-08 10:00:00";
                e2.name = "BZ1000.V2";
                e2.msg = "传感器故障";
                bz1000Event.Add(e2);
            }
            return JsonConvert.SerializeObject(bz1000Event.ToArray());
        }
        return "";
    }

    public static string SetTemplateEvent(string item)
    {
        var data = JsonConvert.DeserializeObject<SetTemplateEvent>(item);
        if (data != null)
        {
            var deleteEvent = new List<TemplateEvent>();
            foreach (var id in data.set)
            {
                foreach (var e in bz1800Event)
                {
                    if (id == e.id)
                    {
                        deleteEvent.Add(e);
                    }
                }
                foreach (var e in bz1000Event)
                {
                    if (id == e.id)
                    {
                        deleteEvent.Add(e);
                    }
                }
            }
            if (deleteEvent.Count > 0)
            {
                foreach (var e in deleteEvent)
                {
                    if (bz1800Event.Contains(e)) bz1800Event.Remove(e);
                    if (bz1000Event.Contains(e)) bz1000Event.Remove(e);
                }
                return "true";
            }
            else
            {
                return "false";
            }
        }
        return "false";
    }

    public static string GetLogin(string pageName)
    {
        var item = new LoginName();
        item.name = "admin";
        return JsonConvert.SerializeObject(item);
    }

    public static string SetLogout(string pageName)
    {
        return "";
    }


    public static string GetDataFromJson(string fileName)
    {
        string jsonData;
        string filepath = Application.streamingAssetsPath + "/DataTest/" + fileName;
        using (StreamReader reader = new StreamReader(filepath))
        {
            jsonData = reader.ReadToEnd();
            reader.Close();
        }
        return jsonData;
    }
}
