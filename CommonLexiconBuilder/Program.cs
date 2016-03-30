using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
//using Microsoft.Tts.Offline;
//using Microsoft.Tts.Offline.Core;
//using Microsoft.Tts.Offline.Utility;
//using Microsoft.Tts.Offline.Schema;

namespace CommonLexiconBuilder
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Microsoft.Tts.Offline;
    using Microsoft.Tts.Offline.Utility;
    class Program
    {
        private static int Process(Arguments arguments)
        {
            int ret = ExitCode.NoError;

            if (!arguments.Validate())
            {
                CommandLineParser.PrintUsage(arguments);
                ret = ExitCode.InvalidArgument;
            }
            else
            {
                ToMSTTSLexicon.Init(@"..\..\..\config\lexicon_template.xml");
                string[] files = Directory.GetFiles(arguments.lexiconPath);
                foreach (string item in files)
                {
                    if (item.Contains(".DTD"))
                        continue;
                    StreamReader sr = new StreamReader(item, Encoding.UTF8);
                    string lexiconContent = sr.ReadToEnd();
                    RawLexiconParserRegularEx.regexFile = arguments.RegexFile;
                    RawLexiconParserRegularEx.convertTTSFile = arguments.ConvertTTSFile;
                    RawLexiconParserRegularEx.replaceCharactorFile = arguments.ReplaceCharactorFile;
                    RawLexiconParserRegularEx.vowelFile = arguments.VowelFile;
                    //RawLexiconParserRegularEx.convertTTSFile = @"..\..\..\config\thth_converttts.txt"; //hardcode for debugging
                    RawLexiconParserRegularEx.GetGraphemePron(lexiconContent);
                    sr.Close();
                }
                ToMSTTSLexicon.Save(arguments.lexiconXml);
            }

            return ret;
        }
        static int Main(string[] args)
        {
            //         ElGRLexiconGenerate();
            //         ThTHLexiconGenerate();
            //ThTHLexiconGenerateByGegularEx();

            //ElGRLexiconGenerateByGegularEx();

            //ThTH.ExtractSyllable(@"..\..\..\..\thth\lexicon.xml", @"..\..\..\..\thth");
            //UpdateLexicon(@"..\..\..\..\thth\newOneSyllableFile.txt", @"..\..\..\..\thth\lexicon.xml");
            return ConsoleApp<Arguments>.Run(args, Process);
        }
    }
}
