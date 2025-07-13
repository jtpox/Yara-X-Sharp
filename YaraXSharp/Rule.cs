using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YaraXSharp
{
    public class Rule
    {
        private delegate void YRX_METADATA_CALLBACK(IntPtr metadata);

        [DllImport("yara_x_capi.dll")]
        private static extern IntPtr yrx_rule_iter_metadata(IntPtr rule, YRX_METADATA_CALLBACK callback);

        private IntPtr _rule;
        public Dictionary<string, string> Metadata = new Dictionary<string, string>();
        public Rule(IntPtr rule)
        {
            _rule = rule;
        }

        public void GetMetadata()
        {
            yrx_rule_iter_metadata(_rule, MetadataCallback);
        }

        private void MetadataCallback(IntPtr metadata)
        {
            var data = Marshal.PtrToStructure<YRX_METADATA>(metadata);
            switch (data.value_type)
            {
                case YRX_METADATA_VALUE_TYPE.YRX_STRING:
                    Metadata.Add(data.identifier, Marshal.PtrToStringUTF8(data.value.String));
                    break;
                case YRX_METADATA_VALUE_TYPE.YRX_BYTES:
                    Metadata.Add(data.identifier, ""); // TODO
                    break;
                case YRX_METADATA_VALUE_TYPE.YRX_F64:
                    Metadata.Add(data.identifier, $"{data.value.f64}");
                    break;
                case YRX_METADATA_VALUE_TYPE.YRX_I64:
                    Metadata.Add(data.identifier, $"{data.value.i64}");
                    break;
                case YRX_METADATA_VALUE_TYPE.YRX_BOOLEAN:
                    Metadata.Add(data.identifier, $"{data.value.boolean}");
                    break;
            }
        }
    }
}
