using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLexiconBuilder
{
    using Microsoft.Tts.Offline;
    using Microsoft.Tts.Offline.Core;
    using Microsoft.Tts.Offline.Utility;
    using Microsoft.Tts.Offline.Schema;
    using System.Diagnostics;//assert
    using System.Collections.Generic;//List
    class ToMSTTSLexicon
    {
        private static Lexicon MSTTSLexicon;

        public struct LexiconOrthography
        {
            public string Grapheme;
            public List<LexiconPhonetic> Phonetics;
            public bool IsABB;
        }
        public struct LexiconPhonetic
        {
            public string Pronunciation;
            public string Lemma;
            public string POS;
            public List<LexiconAttribution> Attributions;
            public List<string> Domains;
            public string ABB;
        }

        public struct LexiconAttribution
        {
            public string Attribution;
            public string Category;
        }

        public static string[] POSMap =
        {
            "NOM_COU",
            "NOM_CIT",
            "NOM_STR",
            "NOM_GEO",
            "NOM_ORG",
            "NOM_BRN",
            "NOM_TOU",
            "NOM_DAT",
            "NOM_PNM",
            "NOM_PNM_M",
            "NOM_PNM_F",
            "NOM_FNM",
            "NOM_FNM_M",
            "NOM_FNM_F",
            "NOM_LNM",
            "NOM_LNM_M",
            "NOM_LNM_F",
            "NOM_UPN",
            "VER_infinitive",
            "VER_participle",
            "VER_gerund",
            "ADJ_qualitative",
            "ADJ_relative",
            "ADJ_possessive",
            "ADJ_participle",
            "DET_possessive",
            "DET_demonstrative",
            "DET_indefinite",
            "DET_definite",
            "DET_partitive",
            "DET_exclamative",
            "DET_relative",
            "DET_intensifying",
            "DET_interrogative",
            "DET_quantifying",
            "PRO_personal",
            "PRO_demonstrative",
            "PRO_reflexive",
            "PRO_indefinite",
            "PRO_definite",
            "PRO_interrogative",
            "PRO_reciprocal",
            "PRO_relative",
            "PRO_possessive",
            "PRO_quantifying",
            "ART_definite",
            "ART_indefinite",
            "ADV_time",
            "ADV_place",
            "ADV_participle",
            "ADV_manner",
            "CON_coordinating",
            "CON_subordinating",
            "ADP_simple",
            "ADP_articulated",
            "NUM_cardinal",
            "NUM_ordinal",
            "NUM_multiplicative",
            "NUM_collective",
            "NUM_percentage",
            "NUM_time"
        };
        //       private static string ttsLexiconPath = @"D:\Narrator\delivery\lexiconGenerate\lexicon.xml";
        public static void Init(string ttsLexiconPath)
        {
            //load template lexicon.xml
            Lexicon.ContentControler controler = new Lexicon.ContentControler();
            controler.IsCaseSensitive = true;
            controler.IsHistoryCheckingMode = true;
            MSTTSLexicon = new Lexicon();
            MSTTSLexicon.Load(ttsLexiconPath, controler);
            MSTTSLexicon.Language = Language.TrTR;//hardcode first since current no langid in Microsoft.Tts.Offline
            MSTTSLexicon.Encoding = Encoding.Unicode;
        }

        public static void Save(string path)
        {
            MSTTSLexicon.Save(path);
        }

        
        //todo: will introduce the real validation code from current TTS solution
        private static void ValidatePron(string pron)
        {
            if (pron.Contains("  ")) //just in case there are some multiple space 
            {
                Console.WriteLine("Warning: validate pron with multiple space");
            }
            if (pron.Trim(' ') != pron)
            {
                Console.WriteLine("Warning: there are space at the begining or end");
            }
        }

        public static void AddLexiconItems(LexiconOrthography orthography/*string grapheme, string pron, string pos, string[] attr, string abb*/)
        {
            LexicalItem lexWord = new LexicalItem();
            lexWord.Grapheme = orthography.Grapheme;
            if (!MSTTSLexicon.Items.ContainsKey(lexWord.Grapheme)) //new grapheme
            {
                foreach (LexiconPhonetic phoneticItem in orthography.Phonetics)
                {
                    ValidatePron(phoneticItem.Pronunciation);
                    LexiconPronunciation targetPronunciation = AddPronunciation(phoneticItem);
                    lexWord.Pronunciations.Add(targetPronunciation);
                }
                lexWord.Status = Lexicon.LexiconStatus.Original;
                MSTTSLexicon.Items.Add(lexWord.Grapheme, lexWord);
            }
            else
            {
                foreach (LexiconPhonetic phoneticItem in orthography.Phonetics)
                {
                    ValidatePron(phoneticItem.Pronunciation);
                    LexiconPronunciation targetPronunciation = AddPronunciation(phoneticItem);
                    if (!isPronExist(MSTTSLexicon.Items[lexWord.Grapheme], phoneticItem.Pronunciation)) // new pron
                    {
                        MSTTSLexicon.Items[lexWord.Grapheme].Pronunciations.Add(targetPronunciation);
                    }
                }
            }
        }
        /*
        public static void CombineSamePronunciation(LexiconOrthography orthography)
        {
            foreach (LexiconPhonetic phoneticItem in orthography.Phonetics)
            {
                
            }
        }
        */
        public static bool isPronExist(LexicalItem lexWord, string expectedPron)
        {
            bool ret = false;

            foreach (LexiconPronunciation pron in lexWord.Pronunciations)
            {
                string pureLexiconPron = pron.Symbolic;
                if (pron.Status != Lexicon.LexiconStatus.Deleted && pureLexiconPron.Equals(expectedPron))
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }

        
        private static LexiconPronunciation AddPronunciation(LexiconPhonetic phonetic)
        {
            // Add pronunciation
            if(phonetic.Pronunciation == null)
            {
                Console.WriteLine("Error: no pronunciation");
                Trace.Assert(false, "error:no pronunciation");
            }

            LexiconPronunciation pronunciation = new LexiconPronunciation();
            pronunciation.Symbolic = phonetic.Pronunciation;

            // Add Properties
            LexiconItemProperty itemProperty = new LexiconItemProperty();
            pronunciation.Properties.Add(itemProperty);

            // Add POS
            if(phonetic.POS != null)
            {
                PosItem pos = new PosItem(phonetic.POS);
                itemProperty.PartOfSpeech = pos;
            }
            else
            {
                PosItem pos = new PosItem("unknown");
                itemProperty.PartOfSpeech = pos;
            }

            // Add domain tag
            if(phonetic.Domains != null)
            {
                foreach (string domain in phonetic.Domains)
                {
                    if(domain != null)
                    {
                        DomainItem domainItem = new DomainItem(domain);
                        itemProperty.Domains.Add(domainItem.Value, domainItem);
                    }
                    else
                    {
                        Console.WriteLine("Warning: pronunciation {0} domain is empty", phonetic.Pronunciation);
                    }
                }
            }
            else
            {
                DomainItem domainItem = new DomainItem("general");
                itemProperty.Domains.Add(domainItem.Value, domainItem);
            }
            
            
            if(phonetic.Attributions != null)
            {
                foreach (LexiconAttribution attrItem in phonetic.Attributions)
                {
                    if (attrItem.Attribution != null && attrItem.Category != null)
                    {
                        List<AttributeItem> attributeList = new List<AttributeItem>();
                        attributeList.Add(new AttributeItem(attrItem.Category, attrItem.Attribution));
                        itemProperty.AttributeSet.Add(attrItem.Category, attributeList);
                    }
                    else
                    {
                        Console.WriteLine("Warning: pronunciation {0} Attribution is empty", phonetic.Pronunciation);
                    }
                }
            }
            

            if (phonetic.ABB != null)
            {
                List<AttributeItem> mAttributeList = new List<AttributeItem>();
                mAttributeList.Add(new AttributeItem("NON_POS_LABEL", phonetic.ABB));
                itemProperty.AttributeSet.Add("NON_POS_LABEL", mAttributeList);
            }

            return pronunciation;
        }
    }
}
