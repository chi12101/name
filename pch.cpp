#include "pch.h"
#include <iostream>
#include <string>
#include <vector>
#include <unordered_map>
#include <cstdint>

// -------- UTF-8 helpers --------

std::vector<uint32_t> utf8ToCodepoints(unsigned char* str) {

    int strLen = strlen((char*)str);
    std::vector<uint32_t> codepoints;
    size_t i = 0;
    while (i < strLen) {
        unsigned char c = str[i];
        uint32_t cp = 0;
        size_t extra = 0;

        if ((c & 0x80) == 0) {            // 1-byte
            cp = c;
            extra = 0;
        }
        else if ((c & 0xE0) == 0xC0) {  // 2-byte
            cp = c & 0x1F;
            extra = 1;
        }
        else if ((c & 0xF0) == 0xE0) {  // 3-byte
            cp = c & 0x0F;
            extra = 2;
        }
        else if ((c & 0xF8) == 0xF0) {  // 4-byte
            cp = c & 0x07;
            extra = 3;
        }
        else {
            // Invalid byte, skip
            ++i;
            continue;
        }

        if (i + extra >= strLen)
            break;

        for (size_t j = 0; j < extra; ++j) {
            unsigned char c2 = str[i + 1 + j];
            if ((c2 & 0xC0) != 0x80) { // not a continuation byte
                cp = 0xFFFD; // replacement
                break;
            }
            cp = (cp << 6) | (c2 & 0x3F);
        }

        codepoints.push_back(cp);
        i += 1 + extra;
    }
    return codepoints;
}

std::string codepointToUtf8(uint32_t cp) {
    std::string out;
    if (cp <= 0x7F) {
        out.push_back(static_cast<char>(cp));
    }
    else if (cp <= 0x7FF) {
        out.push_back(static_cast<char>(0xC0 | ((cp >> 6) & 0x1F)));
        out.push_back(static_cast<char>(0x80 | (cp & 0x3F)));
    }
    else if (cp <= 0xFFFF) {
        out.push_back(static_cast<char>(0xE0 | ((cp >> 12) & 0x0F)));
        out.push_back(static_cast<char>(0x80 | ((cp >> 6) & 0x3F)));
        out.push_back(static_cast<char>(0x80 | (cp & 0x3F)));
    }
    else {
        out.push_back(static_cast<char>(0xF0 | ((cp >> 18) & 0x07)));
        out.push_back(static_cast<char>(0x80 | ((cp >> 12) & 0x3F)));
        out.push_back(static_cast<char>(0x80 | ((cp >> 6) & 0x3F)));
        out.push_back(static_cast<char>(0x80 | (cp & 0x3F)));
    }
    return out;
}

std::string romanizeLetters(uint32_t cp) {
    // Letters syllables range
    const uint32_t SBase = 0xAC00;
    const uint32_t LBase = 0x1100;
    const uint32_t VBase = 0x1161;
    const uint32_t TBase = 0x11A7;
    const int LCount = 19;
    const int VCount = 21;
    const int TCount = 28;
    const int NCount = VCount * TCount;
    const int SCount = LCount * NCount;

    if (cp < SBase || cp >= SBase + SCount) {
        return "";
    }

    static const char* LTable[LCount] = {
        "k","kk","n","d","tt","r","m","b","pp",
        "s","ss","","j","jj","ch","k","t","p","h"
    };
    static const char* VTable[VCount] = {
        "a","ae","ya","yae","eo","e","yeo","ye","o",
        "wa","wae","oe","yo","u","wo","we","wi","yu","eu","yi","i"
    };
    static const char* TTable[TCount] = {
        "", "k","k","ks","n","nj","nh","t","l","lk",
        "lm","lb","ls","lt","lp","lh","m","p","ps","t",
        "t","ng","t","t","k","t","p","t"
    };

    int SIndex = cp - SBase;
    int LIndex = SIndex / NCount;
    int VIndex = (SIndex % NCount) / TCount;
    int TIndex = SIndex % TCount;

    std::string result;
    result += LTable[LIndex];
    result += VTable[VIndex];
    result += TTable[TIndex];

    return result;
}

std::wstring translitLanguageToRussian(uint32_t cp) {
    //  syllables range
    const uint32_t SBase = 0xAC00;
    const uint32_t LBase = 0x1100;
    const uint32_t VBase = 0x1161;
    const uint32_t TBase = 0x11A7;
    const int LCount = 19;
    const int VCount = 21;
    const int TCount = 28;
    const int NCount = VCount * TCount;
    const int SCount = LCount * NCount;

    if (cp < SBase || cp >= SBase + SCount) {
        return L"";
    }

    // Initial consonants (L) → Russian
    static const wchar_t* LTable[LCount] = {
        L"к",   //  k
        L"кк",  //  kk
        L"н",   //  n
        L"д",   //  d
        L"тт",  //  tt
        L"р",   //  r
        L"м",   //  m
        L"б",   //  b
        L"пп",  //  pp
        L"с",   //  s
        L"сс",  //  ss
        L"",    // (silent at beginning)
        L"ч",   //  j
        L"чч",  //  jj
        L"чх",  //  ch
        L"г",   //  g
        L"т",   //  t
        L"п",   //  p
        L"х"    //  h
    };

    // Vowels (V) → Russian
    static const wchar_t* VTable[VCount] = {
        L"а",   //  a
        L"э",   //  ae
        L"я",   //  ya
        L"е",   //  yae
        L"о",   //  eo
        L"э",   //  e
        L"ё",   //  yeo
        L"е",   //  ye
        L"о",   //  o
        L"ва",  //  wa
        L"вэ",  //  wae
        L"ве",  //  oe
        L"ё",   //  yo
        L"у",   //  u
        L"во",  //  wo
        L"ве",  //  we
        L"ви",  //  wi
        L"ю",   //  yu
        L"ы",   //  eu
        L"и",   //  yi
        L"и"    //  i
    };

    // Final consonants (T / ) → Russian (simplified)
    static const wchar_t* TTable[TCount] = {
        L"",    // (none)
        L"к",   // 
        L"к",   // 
        L"кс",  // 
        L"н",   // 
        L"ндж", // 
        L"нх",  // 
        L"т",   // 
        L"ль",  // 
        L"льк", // 
        L"льм", // 
        L"льб", // 
        L"льс", // 
        L"льт", // 
        L"льп", // 
        L"льх", // 
        L"м",   // 
        L"п",   // 
        L"пс",  // 
        L"т",   // 
        L"т",   // 
        L"н",   //  (ng, approximated as н)
        L"т",   // 
        L"т",   // 
        L"к",   // 
        L"т",   // 
        L"п",   // 
        L"т"    // 
    };

    int SIndex = cp - SBase;
    int LIndex = SIndex / NCount;
    int VIndex = (SIndex % NCount) / TCount;
    int TIndex = SIndex % TCount;

    std::wstring result;
    result += LTable[LIndex];
    result += VTable[VIndex];
    result += TTable[TIndex];

    return result;
}

// -------- Dispatcher --------

std::string romanizeCodepoint(uint32_t cp) {
    // Try Letters
    std::string ko = romanizeLetters(cp);
    if (!ko.empty()) return ko;

    // Fallback: return original character
    return codepointToUtf8(cp);
}

std::wstring romanizeCodepointRu(uint32_t cp) {
    // Try Language → Russian transliteration
    std::wstring ko = translitLanguageToRussian(cp);
    if (!ko.empty()) return ko;

    return ko;
    // Fallback: return original character (for non-)
    //return codepointToUtf8(cp);
}

extern "C" __declspec(dllexport) int WINAPI k2e(char* pszKStr, char* outEStr, int outBufLen)
{     
    auto cps = utf8ToCodepoints((unsigned char*)pszKStr);
    std::string out;
    unsigned char prevChar = 0;
    for (uint32_t cp : cps) {
        std::string tempOut = romanizeCodepoint(cp);
        if (tempOut.size() >= 1)
        {
            unsigned char lower = static_cast<unsigned char>(tempOut[0]);
            if (lower >= 'a' && lower <= 'z') {
                lower = lower - 32;
            }
            if (prevChar != 0)
            {
                if (lower == 'K')
                {
                    if (prevChar == 'K')
                        lower = 'G';
                    else if (prevChar == 'G')
                        lower = 'K';
                }
            }
            tempOut[0] = lower;
            prevChar = lower;
        }
        out += tempOut;
    }
    const char* charArray = out.c_str();
    strcpy_s(outEStr, outBufLen, charArray);
    return out.size();
}

extern "C" __declspec(dllexport) int WINAPI k2r(char* pszKStr, wchar_t* outRStr, int outBufLen)
{
    auto cps = utf8ToCodepoints((unsigned char*)pszKStr);
    std::wstring out;    
    for (uint32_t cp : cps) {
        std::wstring tempOut = romanizeCodepointRu(cp);
        if (tempOut.size() >= 1)
        {
            wchar_t lower = static_cast<wchar_t>(tempOut[0]);
            
            if (lower >= L'а' && lower <= L'я') {
                lower = lower - (L'а' - L'А');
            }

            tempOut[0] = lower;    
        }
        out += tempOut;
    }
    const wchar_t* charArray = out.c_str();
    wcscpy_s(outRStr, outBufLen / 2, charArray);    
    return out.size();
}

