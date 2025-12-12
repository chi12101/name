using System;
using System.Collections.Generic;
using System.Text;

namespace testnet
{
    public class Transliterator
    {
        // -------- UTF-8 helpers --------

        public static List<uint> Utf8ToCodepoints(byte[] str)
        {
            int strLen = str.Length;
            List<uint> codepoints = new List<uint>();
            int i = 0;

            while (i < strLen)
            {
                byte c = str[i];
                uint cp = 0;
                int extra = 0;

                if ((c & 0x80) == 0)  // 1-byte
                {
                    cp = c;
                    extra = 0;
                }
                else if ((c & 0xE0) == 0xC0)  // 2-byte
                {
                    cp = (uint)(c & 0x1F);
                    extra = 1;
                }
                else if ((c & 0xF0) == 0xE0)  // 3-byte
                {
                    cp = (uint)(c & 0x0F);
                    extra = 2;
                }
                else if ((c & 0xF8) == 0xF0)  // 4-byte
                {
                    cp = (uint)(c & 0x07);
                    extra = 3;
                }
                else
                {
                    // Invalid byte, skip
                    i++;
                    continue;
                }

                if (i + extra >= strLen)
                    break;

                for (int j = 0; j < extra; ++j)
                {
                    byte c2 = str[i + 1 + j];
                    if ((c2 & 0xC0) != 0x80)  // not a continuation byte
                    {
                        cp = 0xFFFD;  // replacement
                        break;
                    }                    
                    cp = (uint)((cp << 6) | (c2 & 0x3F));
                }

                codepoints.Add(cp);
                i += 1 + extra;
            }

            return codepoints;
        }

        public static string CodepointToUtf8(uint cp)
        {
            StringBuilder outStr = new StringBuilder();

            if (cp <= 0x7F)
            {
                outStr.Append((char)cp);
            }
            else if (cp <= 0x7FF)
            {
                outStr.Append((char)(0xC0 | ((cp >> 6) & 0x1F)));
                outStr.Append((char)(0x80 | (cp & 0x3F)));
            }
            else if (cp <= 0xFFFF)
            {
                outStr.Append((char)(0xE0 | ((cp >> 12) & 0x0F)));
                outStr.Append((char)(0x80 | ((cp >> 6) & 0x3F)));
                outStr.Append((char)(0x80 | (cp & 0x3F)));
            }
            else
            {
                outStr.Append((char)(0xF0 | ((cp >> 18) & 0x07)));
                outStr.Append((char)(0x80 | ((cp >> 12) & 0x3F)));
                outStr.Append((char)(0x80 | ((cp >> 6) & 0x3F)));
                outStr.Append((char)(0x80 | (cp & 0x3F)));
            }

            return outStr.ToString();
        }

        public static string RomanizeLetters(uint cp)
        {
            const uint SBase = 0xAC00;
            const uint LBase = 0x1100;
            const uint VBase = 0x1161;
            const uint TBase = 0x11A7;
            const int LCount = 19;
            const int VCount = 21;
            const int TCount = 28;
            const int NCount = VCount * TCount;
            const int SCount = LCount * NCount;

            if (cp < SBase || cp >= SBase + SCount)
            {
                return "";
            }

            string[] LTable = {
            "k", "kk", "n", "d", "tt", "r", "m", "b", "pp",
            "s", "ss", "", "j", "jj", "ch", "k", "t", "p", "h"
        };
            string[] VTable = {
            "a", "ae", "ya", "yae", "eo", "e", "yeo", "ye", "o",
            "wa", "wae", "oe", "yo", "u", "wo", "we", "wi", "yu", "eu", "yi", "i"
        };
            string[] TTable = {
            "", "k", "k", "ks", "n", "nj", "nh", "t", "l", "lk",
            "lm", "lb", "ls", "lt", "lp", "lh", "m", "p", "ps", "t",
            "t", "ng", "t", "t", "k", "t", "p", "t"
        };

            int SIndex = (int)(cp - SBase);
            int LIndex = SIndex / NCount;
            int VIndex = (SIndex % NCount) / TCount;
            int TIndex = SIndex % TCount;

            StringBuilder result = new StringBuilder();
            result.Append(LTable[LIndex]);
            result.Append(VTable[VIndex]);
            result.Append(TTable[TIndex]);

            return result.ToString();
        }

        public static string TransliterateLanguageToRussian(uint cp)
        {
            const uint SBase = 0xAC00;
            const uint LBase = 0x1100;
            const uint VBase = 0x1161;
            const uint TBase = 0x11A7;
            const int LCount = 19;
            const int VCount = 21;
            const int TCount = 28;
            const int NCount = VCount * TCount;
            const int SCount = LCount * NCount;

            if (cp < SBase || cp >= SBase + SCount)
            {
                return "";
            }

            string[] LTable = {
            "к", "кк", "н", "д", "тт", "р", "м", "б", "пп",
            "с", "сс", "", "ч", "чч", "чх", "г", "т", "п", "х"
        };
            string[] VTable = {
            "а", "э", "я", "е", "о", "э", "ё", "е", "о",
            "ва", "вэ", "ве", "ё", "у", "во", "ве", "ви", "ю", "ы", "и", "и"
        };
            string[] TTable = {
            "", "к", "к", "кс", "н", "ндж", "нх", "т", "ль", "льк",
            "льм", "льб", "льс", "льт", "льп", "льх", "м", "п", "пс", "т",
            "т", "н", "т", "т", "к", "т", "п", "т"
        };

            int SIndex = (int)(cp - SBase);
            int LIndex = SIndex / NCount;
            int VIndex = (SIndex % NCount) / TCount;
            int TIndex = SIndex % TCount;

            StringBuilder result = new StringBuilder();
            result.Append(LTable[LIndex]);
            result.Append(VTable[VIndex]);
            result.Append(TTable[TIndex]);

            return result.ToString();
        }

        // -------- Dispatcher --------

        public static string RomanizeCodepoint(uint cp)
        {
            // Try Letters
            string ko = RomanizeLetters(cp);
            if (!string.IsNullOrEmpty(ko)) return ko;

            // Fallback: return original character
            return CodepointToUtf8(cp);
        }

        public static string RomanizeCodepointRu(uint cp)
        {
            // Try Language → Russian transliteration
            return TransliterateLanguageToRussian(cp);
        }

        public static int K2e(string pszKStr, StringBuilder outEStr, int outBufLen)
        {
            List<uint> cps = Utf8ToCodepoints(Encoding.UTF8.GetBytes(pszKStr));
            StringBuilder outStr = new StringBuilder();
            char prevChar = '\0';

            foreach (uint cp in cps)
            {
                string tempOut = RomanizeCodepoint(cp);
                if (tempOut.Length >= 1)
                {
                    char lower = char.ToUpper(tempOut[0]);
                    if (prevChar != '\0' && lower == 'K')
                    {
                        if (prevChar == 'K')
                            lower = 'G';
                        else if (prevChar == 'G')
                            lower = 'K';
                    }
                    tempOut = lower + tempOut.Substring(1);
                    prevChar = lower;
                }
                outStr.Append(tempOut);
            }

            string result = outStr.ToString();
            outEStr.Clear().Append(result);
            return result.Length;
        }

        public static int K2r(string pszKStr, StringBuilder outRStr, int outBufLen)
        {
            List<uint> cps = Utf8ToCodepoints(Encoding.UTF8.GetBytes(pszKStr));
            StringBuilder outStr = new StringBuilder();

            foreach (uint cp in cps)
            {
                string tempOut = RomanizeCodepointRu(cp);
                if (tempOut.Length >= 1)
                {
                    char lower = char.ToUpper(tempOut[0]);
                    tempOut = lower + tempOut.Substring(1);
                }
                outStr.Append(tempOut);
            }

            string result = outStr.ToString();
            outRStr.Clear().Append(result);
            return result.Length;
        }
    }

}
