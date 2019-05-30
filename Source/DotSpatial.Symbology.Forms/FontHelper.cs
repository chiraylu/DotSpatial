using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotSpatial.Symbology.Forms
{
    public struct FontRange
    {
        public ushort Low;
        public ushort High;
    }
    public static class FontHelper
    {
        [DllImport("gdi32.dll")]
        public static extern uint GetFontUnicodeRanges(IntPtr hdc, IntPtr lpgs);

        [DllImport("gdi32.dll")]
        public extern static IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        
        public static List<FontRange> GetUnicodeRangesForFont(Font font)
        {
            List<FontRange> fontRanges = new List<FontRange>();
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                IntPtr hdc = g.GetHdc();
                IntPtr hFont = font.ToHfont();
                IntPtr old = SelectObject(hdc, hFont);
                uint size = GetFontUnicodeRanges(hdc, IntPtr.Zero);
                IntPtr glyphSet = Marshal.AllocHGlobal((int)size);
                GetFontUnicodeRanges(hdc, glyphSet);
                int count = Marshal.ReadInt32(glyphSet, 12);
                for (int i = 0; i < count; i++)
                {
                    FontRange range = new FontRange();
                    range.Low = (ushort)Marshal.ReadInt16(glyphSet, 16 + i * 4);
                    range.High = (ushort)(range.Low + Marshal.ReadInt16(glyphSet, 18 + i * 4) - 1);
                    fontRanges.Add(range);
                }
                SelectObject(hdc, old);
                Marshal.FreeHGlobal(glyphSet);
                g.ReleaseHdc(hdc);
            }
            return fontRanges;
        }

        public static bool CheckIfCharInFont(char character, Font font, List<FontRange> ranges)
        {
            ushort intval = Convert.ToUInt16(character);
            bool isCharacterPresent = false;
            foreach (FontRange range in ranges)
            {
                if (intval >= range.Low && intval <= range.High)
                {
                    isCharacterPresent = true;
                    break;
                }
            }
            return isCharacterPresent;
        }
    }
}
