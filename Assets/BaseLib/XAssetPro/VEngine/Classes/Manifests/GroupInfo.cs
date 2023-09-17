using System;

namespace VEngine
{
    /// <summary>
    ///     Group 的运行时信息
    /// </summary>
    public class GroupInfo : ISerializable
    {
        public int[] bundles = Utility.IntArrayEmpty;
        public string name;

        public void Deserialize(string line)
        {
            var fields = line.Split(',');
            name = fields[0];
            bundles = fields[1].IntArrayValue("|");
        }
        
        public void Deserialize(ReadOnlySpan<char> line)
        {
            ReadOnlySpan<char> fields_0;
            ReadOnlySpan<char> fields_1;
            line.Split_to_spanspan(',', out fields_0, out fields_1);

            name = fields_0.ToString();

            if (fields_1.IsEmpty)
            {
                bundles = Utility.IntArrayEmpty;
            }
            else
            {
                bundles = fields_1.Split_to_IntArray('|');
            }
        }

        public string Serialize()
        {
            return $"{name},{StringExtensions.Join("|", bundles)}";
        }
    }
}