using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLexiconBuilder
{
    using System.IO;//for Directory and Files
    using System.Text.RegularExpressions;//for Regex

//    public delegate string ConvertToTTSFunc(string pron);
    public class RawLexiconParserRegularEx
    {
        public static string regexFile = string.Empty;
        public static string convertTTSFile = string.Empty;
        public static string replaceCharactorFile = string.Empty;
        public static string vowelFile = string.Empty;

        //for th-th
        //private static string RegexGrapheme = "\\{(.*?)\\}\\s+\\{{(.*?)\\}}";//including grapheme and pron and categories
        //private static string RegexPronunciation = "{{(.*?)}}";
        //private static string RegexTTSPron = "(.*?WB}.*?:sb\\s+WB)";//"[{}\"WB\"\":sb WB\"]";

        //for el-gr
        //private static string RegexGrapheme = "\\<ENTRYGROUP\\s+orthography=\"(.*)\"(.|\n)*?\\</ENTRYGROUP\\>";//"\\<ENTRYGROUP(.|\n)*?\\</ENTRYGROUP\\>";//including grapheme and pron and categories
        //private static string RegexPronunciation = "<PHONETIC>(.*?)</PHONETIC>";
        //public static string RegexTTSPron = "(.*?WB}.*?:sb\\s+WB)";//"[{}\"WB\"\":sb WB\"]";

        private static string RegexGrapheme = string.Empty;
        private static string RegexPronunciation = string.Empty;
        private static string RegexAttribute = string.Empty;
        private static List<string> FormatPhoneticPairs;
        private static List<string> ReplaceCharactorPairs;
        private static List<string> Vowels;

        //static ConvertToTTSFunc ConvertToTTS;
        //public static void Init(ConvertToTTSFunc func)
        //{
        //    ConvertToTTS = func;
        //}

        
        public static void GetGraphemePron(string content)
        {
            if(!Config())
            {
                return;
            }

            if(!Validatation())
            {
                return;
            }

            List<string> graphemes = new List<string>();

            MatchCollection mcGrapheme = Regex.Matches(content, RegexGrapheme, RegexOptions.Multiline);
            if (mcGrapheme.Count > 0)
            {
                int graphemeCount = 0;
                while (graphemeCount < mcGrapheme.Count)
                {
                    string wholeOrthography = mcGrapheme[graphemeCount].Value;
                    string grapheme = mcGrapheme[graphemeCount].Groups[1].ToString();
                    graphemeCount++;
                    MatchCollection mcPron = Regex.Matches(wholeOrthography, RegexPronunciation, RegexOptions.Multiline);
                    MatchCollection mcAttr = Regex.Matches(wholeOrthography, RegexAttribute, RegexOptions.Multiline);
                    if (mcAttr.Count != mcPron.Count)
                    {
                        Console.WriteLine("error: prounciation number should match the attribute number");
                    }
                    if (mcPron.Count > 0)
                    {
                        int pronCount = 0;
                        while (pronCount < mcPron.Count)
                        {
                            int pronPartCount = 1;
                            string pron = null;
                            while (pronPartCount < mcPron[pronCount].Groups.Count)
                            {
                                pron += mcPron[pronCount].Groups[pronPartCount].ToString();
                                pronPartCount++;
                            }
                            pronCount++;

                            if (grapheme != null && pron != null)
                            {
                                ToMSTTSLexicon.LexiconOrthography orthography = new ToMSTTSLexicon.LexiconOrthography();
                                List<ToMSTTSLexicon.LexiconPhonetic> phonetics = new List<ToMSTTSLexicon.LexiconPhonetic>();
                                ToMSTTSLexicon.LexiconPhonetic phonetic = new ToMSTTSLexicon.LexiconPhonetic();
                                string pronunciation = FormatPhonetic(pron);
                                orthography.Phonetics = phonetics;
                                orthography.Grapheme = grapheme;
                                phonetic.Pronunciation = pronunciation;
                                orthography.Phonetics.Add(phonetic);
                                ToMSTTSLexicon.AddLexiconItems(orthography);
                                //Console.WriteLine(line.ToString());
                            }
                            else
                            {
                                Console.WriteLine("attention: grapheme or pron is empty");
                            }
                        }
                    }
                }
            }
        }

        private static string FormatPhonetic(string pron)
        {
            string output = pron;
            //replace the symbols if there is no blank seperator(e.g. tone,syllable)
            if (ReplaceCharactorPairs.Count > 0)
            {
                foreach (string replaceChar in ReplaceCharactorPairs)
                {
                    string[] pair = replaceChar.Split('>');
                    output = output.Replace(pair[0], pair[1]);
                }
            }

            //convert to target TTS format
            if (FormatPhoneticPairs.Count > 0)
            {
                string[] phoneList = output.Trim().Split(' ');
                for (int i = 0; i < phoneList.Length; i++)
                {
                    foreach (string pairs in FormatPhoneticPairs)
                    {
                        string[] pair = pairs.Split(' ');
                        if (phoneList[i] == pair[0])
                        {
                            phoneList[i] = pair[1];
                        }
                    }
                }
                output = string.Join(" ", phoneList).Trim();
            }
            output = Regex.Replace(output, @"\s+", " ");

            //handle tone/stress position, need to know the vowels position
            string[] parts = output.Split('-');
            for (int i = 0; i < parts.Length; i++)
            {
                string[] sy = parts[i].Split(' ');
                //if parts[0] is consonant && parts[1] = 1/2/t1/t2/...t9 &&
                for (int j = 0; j < sy.Length; j++)
                {
                    switch (sy[j])
                    {
                        case "1":
                        case "2":
                        case "t1":
                        case "t2":
                        case "t3":
                        case "t4":
                        case "t5":
                        case "t6":
                        case "t7":
                        case "t8":
                        case "t9":
                            for (int k = j+1; k < sy.Length; k++)
                            {
                                if (Vowels.Contains(sy[k]))
                                {
                                    string temp = sy[j];
                                    for (int t = j; t < k; t++)
                                    {
                                        sy[t] = sy[t + 1];
                                    }
                                    sy[k] = temp;
                                    j = k;
                                    break;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                parts[i] = string.Join(" ", sy).Trim();
            }
            output = string.Join("-", parts).Trim();

            return output.TrimEnd(new char[] { ' ', '-' });
        }

        private static bool Config()
        {
            if (regexFile != null)
            {
                using (StreamReader sr = new StreamReader(regexFile, Encoding.Unicode))
                {
                    string regexContent = sr.ReadToEnd();
                    MatchCollection mcGrapheme = Regex.Matches(regexContent, "RegexGrapheme\\s+=\\s+\"(.*)\"", RegexOptions.Multiline);
                    if (mcGrapheme.Count > 1 || mcGrapheme.Count == 0)
                    {
                        Console.WriteLine("must only one regular expression for grapheme");
                        return false;
                    }
                    RegexGrapheme = mcGrapheme[0].Groups[1].ToString();

                    MatchCollection mcPron = Regex.Matches(regexContent, "RegexPronunciation\\s+=\\s+\"(.*)\"", RegexOptions.Multiline);
                    if (mcPron.Count > 1 || mcPron.Count == 0)
                    {
                        Console.WriteLine("must only one regular expression for pronunciation");
                        return false;
                    }
                    RegexPronunciation = mcPron[0].Groups[1].ToString();

                    MatchCollection mcAttr = Regex.Matches(regexContent, "RegexAttribute\\s+=\\s+\"(.*)\"", RegexOptions.Multiline);
                    if (mcAttr.Count > 1 || mcAttr.Count == 0)
                    {
                        Console.WriteLine("must only one regular expression for attributes");
                        return false;
                    }
                    RegexAttribute = mcAttr[0].Groups[1].ToString();
                }
            }

            if (convertTTSFile != null)
            {
                FormatPhoneticPairs = new List<string>();
                using (StreamReader sr = new StreamReader(convertTTSFile, Encoding.Unicode))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        FormatPhoneticPairs.Add(line);
                    }

                }
            }

            if (replaceCharactorFile != null)
            {
                ReplaceCharactorPairs = new List<string>();
                using (StreamReader sr = new StreamReader(replaceCharactorFile, Encoding.Unicode))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        ReplaceCharactorPairs.Add(line);
                    }

                }
            }

            if (vowelFile != null)
            {
                Vowels = new List<string>();
                using (StreamReader sr = new StreamReader(vowelFile, Encoding.Unicode))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        Vowels.Add(line);
                    }

                }
            }
            return true;
        }

        private static bool Validatation()
        {
            if (RegexGrapheme == null || RegexPronunciation == null)
            {
                Console.WriteLine("error: not exist the Regex Expression");
                return false;
            }
            return true;
        }
    }
}
