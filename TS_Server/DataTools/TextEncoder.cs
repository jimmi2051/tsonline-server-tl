using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS_Server.DataTools
{
    public static class TextEncoder
    {
        static readonly byte[] VISCII_char = {0xC5,0xE5,0xF0,0xCE,0xEE,0x9D,
								0xFB,0xB4,0xBD,0xBF,0xDF,0x80,0xD5,0xC4,0xE4,
								0x84,0xA4,0x85,0xA5,0x86,0xA6,0x06,0xE7,0x87,
								0xA7,0x81,0xA1,0x82,0xA2,0x02,0xC6,0x05,0xC7,
								0x83,0xA3,0x89,0xA9,0xCB,0xEB,0x88,0xA8,0x8A,
								0xAA,0x8B,0xAB,0x8C,0xAC,0x8D,0xAD,0x8E,0xAE,
								0x9B,0xEF,0x98,0xB8,0x9A,0xF7,0x99,0xF6,0x8F,
								0xAF,0x90,0xB0,0x91,0xB1,0x92,0xB2,0x93,0xB5,
								0x95,0xBE,0x96,0xB6,0x97,0xB7,0xB3,0xDE,0x94,
								0xFE,0x9E,0xF8,0x9C,0xFC,0xBA,0xD1,0xBB,0xD7,
								0xBC,0xD8,0xFF,0xE6,0xB9,0xF1,0x9F,0xCF,0x1E,
								0xDC,0x14,0xD6,0x19,0xDB,0xA0};
        static readonly char[] Unicode_char = {'\u0102', '\u0103', '\u0111', '\u0128', '\u0129', '\u0168',
								'\u0169', '\u01A0', '\u01A1', '\u01AF', '\u01B0', '\u1EA0', '\u1EA1', '\u1EA2', '\u1EA3',
								'\u1EA4', '\u1EA5', '\u1EA6', '\u1EA7', '\u1EA8', '\u1EA9', '\u1EAA', '\u1EAB', '\u1EAC',
								'\u1EAD', '\u1EAE', '\u1EAF', '\u1EB0', '\u1EB1', '\u1EB2', '\u1EB3', '\u1EB4', '\u1EB5',
								'\u1EB6', '\u1EB7', '\u1EB8', '\u1EB9', '\u1EBA', '\u1EBB', '\u1EBC', '\u1EBD', '\u1EBE',
								'\u1EBF', '\u1EC0', '\u1EC1', '\u1EC2', '\u1EC3', '\u1EC4', '\u1EC5', '\u1EC6', '\u1EC7',
								'\u1EC8', '\u1EC9', '\u1ECA', '\u1ECB', '\u1ECC', '\u1ECD', '\u1ECE', '\u1ECF', '\u1ED0',
								'\u1ED1', '\u1ED2', '\u1ED3', '\u1ED4', '\u1ED5', '\u1ED6', '\u1ED7', '\u1ED8', '\u1ED9',
								'\u1EDA', '\u1EDB', '\u1EDC', '\u1EDD', '\u1EDE', '\u1EDF', '\u1EE0', '\u1EE1', '\u1EE2',
								'\u1EE3', '\u1EE4', '\u1EE5', '\u1EE6', '\u1EE7', '\u1EE8', '\u1EE9', '\u1EEA', '\u1EEB',
								'\u1EEC', '\u1EED', '\u1EEE', '\u1EEF', '\u1EF0', '\u1EF1', '\u1EF2', '\u1EF3', '\u1EF4',
								'\u1EF5', '\u1EF6', '\u1EF7', '\u1EF8', '\u1EF9', '\u00D5'};
        static Dictionary<byte,char> convertTable = new Dictionary<byte,char>();

        static TextEncoder()
        {
            for (int i = 0; i < VISCII_char.Length; i++)
                convertTable.Add(VISCII_char[i], Unicode_char[i]);
        }

        public static string convertToUniCode(byte[] text, int init, int length)
        {
            char[] ret = new char[length];
            for (int i = init; i < init + length; i++)
            {
                if (convertTable.ContainsKey(text[i]))
                    ret[i] = convertTable[text[i]];
                else
                    ret[i] = (char)(text[i]);
            }

            return new string(ret);
        }
    }
}
