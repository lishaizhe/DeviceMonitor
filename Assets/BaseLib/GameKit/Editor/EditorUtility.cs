using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace GameKit.Editor
{
    public static class EditorUtility
    {
        private static List<string> GetDefinesList(BuildTargetGroup group)
        {
            return new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
        }

        public static void SetEnabled(string symbol, bool enable)
        {
            List<string> defines = GetDefinesList(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (enable)
            {
                if (!defines.Contains(symbol))
                {
                    defines.Add(symbol);
                }
            }
            else
            {
                while (defines.Contains(symbol))
                {
                    defines.Remove(symbol);
                }
            }

            string definesString = string.Join(";", defines.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, definesString);
        }

        public static string GetDataMD5(byte[] data)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(data);
            string md5str = "";
            foreach (byte b in result)
            {
                md5str += System.Convert.ToString(b, 16).PadLeft(2, '0');
            }

            return md5str;
        }

        public static string GetFileMD5(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();

            return GetDataMD5(data);
        }

        public static long GetFileSize(string filePath)
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open);
                long len = fs.Length;
                fs.Close();
                fs.Dispose();
                return len;
            }
            catch(System.Exception ex)
            {
                throw new System.Exception("GetFileSize() fail, error" + ex.Message);
            }
        }

        public static bool ProcessCommand(string command, string argument, bool createNoWindow = false, bool errorDialog = true, bool useShellExecute = false)
        {
            System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo(command)
            {
                Arguments = argument,
                CreateNoWindow = createNoWindow,
                ErrorDialog = errorDialog,
                UseShellExecute = useShellExecute
            };

            if (start.UseShellExecute)
            {
                start.RedirectStandardOutput = false;
                start.RedirectStandardError = false;
                start.RedirectStandardInput = false;
            }
            else
            {
                start.RedirectStandardOutput = true;
                start.RedirectStandardError = true;
                start.RedirectStandardInput = true;
                start.StandardOutputEncoding = System.Text.Encoding.UTF8;
                start.StandardErrorEncoding = System.Text.Encoding.UTF8;
            }

            System.Diagnostics.Process p = System.Diagnostics.Process.Start(start);

            bool b = true;
            if (!start.UseShellExecute)
            {
                while (!p.StandardError.EndOfStream)
                {
                    Debug.LogError(p.StandardError.ReadLine());
                    b = false;
                }
                while (!p.StandardOutput.EndOfStream)
                {
                    Debug.Log(p.StandardOutput.ReadLine());
                }
                //Debug.Log(p.StandardOutput);
                //Debug.Log(p.StandardError);
            }

            p.WaitForExit();
            p.Close();

            return b;
        }

        public static void DeleteDirectory(string path)
        {
            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(path, false);
        }

        public static string GetCustomSaveFileValue(string key)
        {
            string pathInStreaming = Application.streamingAssetsPath + "/CUSTOMSAVE.txt";
            if (File.Exists(pathInStreaming) == false)
                return string.Empty;

            var content = File.ReadAllText(pathInStreaming);
            if (content.IsNullOrEmpty())
                return string.Empty;
                
            content = content.Trim('\r', '\n');
            var configs = new Dictionary<string, string>();
            string[] args = content.Split('|');
            for (int i = 0; i < args.Length; ++i)
            {
                string[] temp = args[i].Split(',');
                if (temp.Length == 2)
                {
                    //获取服务器配置键值对
                    string _key = temp[0];
                    string _val = temp[1];
                    configs[_key] = _val.Trim('\r');
                }
            }

            if (configs.ContainsKey(key))
                return configs[key];
            else
                return string.Empty;
        }
    }
}