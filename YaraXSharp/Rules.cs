using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YaraXSharp
{
    public class Rules : IDisposable
    {
        internal IntPtr _pointer = IntPtr.Zero;
        internal Rules(IntPtr rulesPtr)
        {
            _pointer = rulesPtr;
        }

        public Rules()
        {
            _pointer = IntPtr.Zero;
        }

        public int Count()
        {
            return YaraX.yrx_rules_count(_pointer);
        }

        public void Import(byte[] buffer)
        {
            if (buffer.Length == 0) throw new YrxException("Import buffer length is 0.");
            YRX_RESULT result = YaraX.yrx_rules_deserialize(buffer, buffer.LongLength, out _pointer);
            if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException(result.ToString());
        }

        public byte[] Export()
        {
            IntPtr yrx_buffer_pointer;
            YRX_RESULT result = YaraX.yrx_rules_serialize(_pointer, out yrx_buffer_pointer);
            if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException(result.ToString());

            YRX_BUFFER yrx_buffer = Marshal.PtrToStructure<YRX_BUFFER>(yrx_buffer_pointer);

            byte[] buffer = new byte[(int)yrx_buffer.length];
            Marshal.Copy(yrx_buffer.data, buffer, 0, buffer.Length);
            YaraX.yrx_buffer_destroy(yrx_buffer_pointer);

            return buffer;
        }

        public void Destroy()
        {
            if (_pointer != IntPtr.Zero) YaraX.yrx_rules_destroy(_pointer);
            _pointer = IntPtr.Zero;
        }

        public void Dispose()
        {
            Destroy();
        }
    }
}
