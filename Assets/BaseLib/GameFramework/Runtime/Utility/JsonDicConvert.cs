using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public static class JsonDicConvert
{
    public static string ObjectToJson(Dictionary<string, object> dict)
    {
        return JsonConvert.SerializeObject(dict);
    }
}
