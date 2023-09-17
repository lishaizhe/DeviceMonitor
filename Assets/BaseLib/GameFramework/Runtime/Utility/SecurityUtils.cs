using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace GameKit.Base
{
    public static class SecurityUtils
    {
        public static string ComputeSHA1(byte[] data)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] sha1_byte = sha1.ComputeHash(data);
            string sha1_str = "";
            foreach (byte b in sha1_byte)
            {
                sha1_str += System.Convert.ToString(b, 16).PadLeft(2, '0');
            }

            return sha1_str;
        }

        public static string ComputeSHA1(string filePath)
        {
            using (FileStream fs = File.OpenRead(filePath))
            {
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                return ComputeSHA1(data);
            }
        }

        public static string ComputeMD5(byte[] data)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] md5_byte = md5.ComputeHash(data);
            string md5_str = "";
            foreach (byte b in md5_byte)
            {
                md5_str += System.Convert.ToString(b, 16).PadLeft(2, '0');
            }

            return md5_str;
        }

        public static string ComputeMD5(string filePath)
        {
            using (FileStream fs = File.OpenRead(filePath))
            {
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                return ComputeMD5(data);
            }
        }
    }
}
