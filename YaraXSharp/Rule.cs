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
        private delegate void YRX_TAGS_CALLBACK(IntPtr tag);

        [DllImport("yara_x_capi.dll")]
        private static extern IntPtr yrx_rule_iter_metadata(IntPtr rule, YRX_METADATA_CALLBACK callback);

        [DllImport("yara_x_capi.dll")]
        private static extern IntPtr yrx_rule_iter_tags(IntPtr rule, YRX_TAGS_CALLBACK callback);

        private IntPtr _rule;
        public Dictionary<string, object> Metadata = new Dictionary<string, object>();
        public List<string> Tags = new List<string>();
        public Rule(IntPtr rule, params YRX_SCANNER_FLAGS[] load_info)
        {
            _rule = rule;
            if (load_info.Contains(YRX_SCANNER_FLAGS.LOAD_METADATA)) GetMetadata();
            if (load_info.Contains(YRX_SCANNER_FLAGS.LOAD_TAGS)) GetTags();
        }

        private void GetMetadata()
        {
            yrx_rule_iter_metadata(_rule, MetadataCallback);
        }

        private void GetTags()
        {
            yrx_rule_iter_tags(_rule, TagsCallback);
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
                    YRX_METADATA_BYTES yrx_buffer = data.value.bytes;
                    byte[] buffer = new byte[yrx_buffer.length];
                    Marshal.Copy(yrx_buffer.data, buffer, 0, yrx_buffer.length);
                    Metadata.Add(data.identifier, buffer);
                    break;
                case YRX_METADATA_VALUE_TYPE.YRX_F64:
                    Metadata.Add(data.identifier, data.value.f64);
                    break;
                case YRX_METADATA_VALUE_TYPE.YRX_I64:
                    Metadata.Add(data.identifier, data.value.i64);
                    break;
                case YRX_METADATA_VALUE_TYPE.YRX_BOOLEAN:
                    Metadata.Add(data.identifier, data.value.boolean);
                    break;
            }
        }

        private void TagsCallback(IntPtr tag)
        {
            Tags.Add(Marshal.PtrToStringUTF8(tag));
        }
    }
}
