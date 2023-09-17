using System;

namespace VEngine
{
    /// <summary>
    ///     Bundle 的运行时信息
    /// </summary>
    public class BundleInfo : ISerializable
    {
        /// <summary>
        ///     包含的所有 assets（id）
        /// </summary>
        public int[] assets = Utility.IntArrayEmpty;

        /// <summary>
        ///     crc 用作版本校验
        /// </summary>
        public uint crc;

        /// <summary>
        ///     依赖列表
        /// </summary>
        public int[] deps = Utility.IntArrayEmpty;

        /// <summary>
        ///     bundle 的 id
        /// </summary>
        public int id;

        /// <summary>
        ///     bundle 名字
        /// </summary>
        public string name;

        /// <summary>
        ///     字节大小
        /// </summary>
        public ulong size;

        public void Deserialize(string line)
        {
            var fields = line.Split(',');
            id = fields[0].IntValue();
            name = fields[1];
            crc = fields[2].UIntValue();
            size = fields[3].ULongValue();
            assets = fields[4].IntArrayValue("|");
            deps = fields[5].IntArrayValue("|");
        }
        
        public void Deserialize(ReadOnlySpan<char> line)
        {
// #if UNITY_EDITOR
//             string t2 = line.ToString();
// #endif
//             
            ReadOnlySpan<char> fields_0;
            ReadOnlySpan<char> fields_1;
            ReadOnlySpan<char> fields_2;
            ReadOnlySpan<char> fields_3;
            ReadOnlySpan<char> fields_4;
            ReadOnlySpan<char> fields_5;
            line.Split_to_spans(',', out fields_0, out fields_1, out fields_2, out fields_3, out fields_4, out fields_5);

            id = fields_0.ToInt();
            name = fields_1.ToString();
            crc = (uint)fields_2.ToULong();
            size = fields_3.ToULong();
            if (fields_4.IsEmpty)
            {
                assets = Utility.IntArrayEmpty;
            }
            else
            {
                assets = fields_4.Split_to_IntArray('|');
            }

            if (fields_5.IsEmpty)
            {
                deps = Utility.IntArrayEmpty;
            }
            else
            {
                deps = fields_5.Split_to_IntArray('|');
            }

        }

        public string Serialize()
        {
            return $"{id},{name},{crc},{size},{StringExtensions.Join("|", assets)},{StringExtensions.Join("|", deps)}";
        }
    }
}