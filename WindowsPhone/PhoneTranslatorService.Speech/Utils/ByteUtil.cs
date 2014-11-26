/* 
 * ByteUtil.cs
 * 
 * Copyright (c) 2009, Michael Schwarz (http://www.schwarz-interactive.de)
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR
 * ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 * MS   09-02-10    added hex methods
 * 
 * 
 */
using System;
using System.Text;

namespace MicroTranslatorService.Speech.Utils
{
    internal static class ByteUtil
    {
        private const string HEX_CHARS = "0123456789ABCDEF";

        public static string BytesToHex(byte[] b)
        {
            string res = "";

            for (int i = 0; i < b.Length; i++)
                res += ByteToHex(b[i]);

            return res;
        }

        public static string ByteToHex(byte b)
        {
            int lowByte = b & 0x0F;
            int highByte = (b & 0xF0) >> 4;

            return new string(
                new char[] { HEX_CHARS[highByte], HEX_CHARS[lowByte] }
            );
        }
    }
}
