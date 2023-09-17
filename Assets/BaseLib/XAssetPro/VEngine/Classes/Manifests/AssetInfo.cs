using System;

namespace VEngine
{
    /// <summary>
    ///     Asset 的运行时信息
    /// </summary>
    public class AssetInfo : ISerializable
    {
        /// <summary>
        ///     资源名字 对应的 bundle（id）
        /// </summary>
        public int bundle;

        /// <summary>
        ///     资源所有依赖的 bundle 集合（id）
        /// </summary>
        public int[] bundles = Utility.IntArrayEmpty;

        public int id;

        public void Deserialize(string line)
        {
            var fields = line.Split(',');
            id = fields[0].IntValue();
            bundle = fields[1].IntValue();
            bundles = fields[2].IntArrayValue("|");
        }
        
        public void Deserialize(ReadOnlySpan<char> line)
        {
            // #if UNITY_EDITOR
            // string t = line.ToString();
            // #endif
            
            ReadOnlySpan<char> fields_0;
            ReadOnlySpan<char> fields_1;
            ReadOnlySpan<char> fields_2;
            var fields = line.Split_to_spans(',', out fields_0, out fields_1, out fields_2);
            id = fields_0.ToInt();
            bundle = fields_1.ToInt();

            if (fields_2.IsEmpty)
            {
                bundles = Utility.IntArrayEmpty;
            }
            else
            {
                bundles = fields_2.Split_to_IntArray('|');
            }
        }

        public string Serialize()
        {
            return $"{id},{bundle},{StringExtensions.Join("|", bundles)}";
        }
    }
}