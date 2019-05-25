using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotSpatial.Data.Rasters.GdalExtension
{
    /// <summary>
    /// MarshalHelper
    /// </summary>
    public static class MarshalHelper
    {
        public static byte[] IntPtrToByteArray(IntPtr intPtr, int length)
        {
            byte[] buffer = new byte[length];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = Marshal.ReadByte(intPtr, i * 8);
            }
            return buffer;
        }
        public static short[] IntPtrToShortArray(IntPtr intPtr, int length)
        {
            short[] buffer = new short[length];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = Marshal.ReadInt16(intPtr, i * 8);
            }
            return buffer;
        }
        public static ushort[] IntPtrToUShortArray(IntPtr intPtr, int length)
        {
            ushort[] buffer = new ushort[length];
            IntPtr stringPtr = IntPtr.Zero;
            bool ret = false;
            for (int i = 0; i < length; i++)
            {
                stringPtr = Marshal.ReadIntPtr(intPtr, i * 8);
                string str = Marshal.PtrToStringAnsi(stringPtr);
                ret = ushort.TryParse(str, out ushort value);
                if (ret)
                {
                    buffer[i] = value;
                }
            }
            Marshal.FreeHGlobal(stringPtr);
            return buffer;
        }
        public static int[] IntPtrToIntArray(IntPtr intPtr, int length)
        {
            int[] buffer = new int[length];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = Marshal.ReadInt32(intPtr, i * 8);
            }
            return buffer;
        }

        public static uint[] IntPtrToUIntArray(IntPtr intPtr, int length)
        {
            uint[] buffer = new uint[length];
            IntPtr stringPtr = IntPtr.Zero;
            bool ret = false;
            for (int i = 0; i < length; i++)
            {
                stringPtr = Marshal.ReadIntPtr(intPtr, i * 32);
                string str = Marshal.PtrToStringAnsi(stringPtr);
                ret = uint.TryParse(str, out uint value);
                if (ret)
                {
                    buffer[i] = value;
                }
            }
            Marshal.FreeHGlobal(stringPtr);
            return buffer;
        }

        public static float[] IntPtrToFloatArray(IntPtr intPtr, int length)
        {
            float[] buffer = new float[length];
            IntPtr stringPtr = IntPtr.Zero;
            bool ret = false;
            for (int i = 0; i < length; i++)
            {
                stringPtr = Marshal.ReadIntPtr(intPtr, i * 32);
                string str = Marshal.PtrToStringAnsi(stringPtr);
                ret = float.TryParse(str, out float value);
                if (ret)
                {
                    buffer[i] = value;
                }
            }
            //Marshal.FreeHGlobal(stringPtr);
            return buffer;
        }
    }
}
