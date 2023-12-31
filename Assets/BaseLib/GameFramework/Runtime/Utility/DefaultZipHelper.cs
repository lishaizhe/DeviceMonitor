﻿//

using GameFramework;
using System.IO;
using System.IO.Compression;
using GZipStream = System.IO.Compression.GZipStream;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 压缩解压缩辅助器。
    /// </summary>
    public class DefaultZipHelper : Utility.Zip.IZipHelper
    {
        /// <summary>
        /// 压缩数据。
        /// </summary>
        /// <param name="bytes">要压缩的数据。</param>
        /// <returns>压缩后的数据。</returns>
        public byte[] Compress(byte[] bytes)
        {
            if (bytes == null || bytes.Length <= 0)
            {
                return bytes;
            }

            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream();
                using (GZipStream gZipOutputStream = new GZipStream(memoryStream,CompressionLevel.Fastest))
                {
                    gZipOutputStream.Write(bytes, 0, bytes.Length);
                }

                return memoryStream.ToArray();
            }
            finally
            {
                if (memoryStream != null)
                {
                    memoryStream.Dispose();
                    memoryStream = null;
                }
            }
        }

        /// <summary>
        /// 解压缩数据。
        /// </summary>
        /// <param name="bytes">要解压缩的数据。</param>
        /// <returns>解压缩后的数据。</returns>
        public byte[] Decompress(byte[] bytes)
        {
            if (bytes == null || bytes.Length <= 0)
            {
                return bytes;
            }

            MemoryStream decompressedStream = null;
            MemoryStream memoryStream = null;
            try
            {
                decompressedStream = new MemoryStream();
                memoryStream = new MemoryStream(bytes);
                using (GZipStream gZipInputStream = new GZipStream(memoryStream,CompressionLevel.Fastest))
                {
                    memoryStream = null;
                    int bytesRead = 0;
                    byte[] clip = new byte[0x1000];
                    while ((bytesRead = gZipInputStream.Read(clip, 0, clip.Length)) != 0)
                    {
                        decompressedStream.Write(clip, 0, bytesRead);
                    }
                }

                return decompressedStream.ToArray();
            }
            finally
            {
                if (decompressedStream != null)
                {
                    decompressedStream.Dispose();
                    decompressedStream = null;
                }

                if (memoryStream != null)
                {
                    memoryStream.Dispose();
                    memoryStream = null;
                }
            }
        }
    }
}
