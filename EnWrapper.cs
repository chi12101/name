using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace testnet
{
    internal class EnWrapper
    {
        [DllImport("en.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int k2e(IntPtr inStr, IntPtr outStr, int outStrLength);

        [DllImport("en.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int k2r(IntPtr inStr, IntPtr outStr, int outStrLength);

        public static String toEn(String input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            IntPtr inPtr = Marshal.AllocHGlobal(inputBytes.Length + 1);
            Marshal.Copy(inputBytes, 0, inPtr, inputBytes.Length);

            IntPtr outPtr = Marshal.AllocHGlobal(20);

            int outLen = k2e(inPtr, outPtr, 20);

            Marshal.FreeHGlobal(inPtr);

            byte[] outArray = IntPtrToByteArray(outPtr, outLen);
            Marshal.FreeHGlobal(outPtr);

            var result = Encoding.UTF8.GetString(outArray);

            return result;
        }

        public static String toRu(String input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            IntPtr inPtr = Marshal.AllocHGlobal(inputBytes.Length + 1);
            Marshal.Copy(inputBytes, 0, inPtr, inputBytes.Length);

            IntPtr outPtr = Marshal.AllocHGlobal(40);

            int outLen = k2r(inPtr, outPtr, 20);

            Marshal.FreeHGlobal(inPtr);

            byte[] outArray = IntPtrToByteArray(outPtr, outLen * 2);
            Marshal.FreeHGlobal(outPtr);

            var result = Encoding.Unicode.GetString(outArray);

            return result;
        }

        static byte[] IntPtrToByteArray(IntPtr ptr, int length)
        {
            byte[] byteArray = new byte[length];
            Marshal.Copy(ptr, byteArray, 0, length);
            return byteArray;
        }
    }
}
