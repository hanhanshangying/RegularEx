using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLexiconBuilder
{
    using System;
    using System.IO;
    using Microsoft.Tts.Offline.Utility;
    class Arguments
    {
        [Argument("Lex", Description = "Input lexicon path",
           Optional = false, UsagePlaceholder = "lexiconPath")]
        private string _lexiconPath = string.Empty;

        [Argument("Regex", Description = "Input regex.txt",
           Optional = false, UsagePlaceholder = "regexFile")]
        private string _regexFile = string.Empty;

        [Argument("format", Description = "Input converttts.txt",
           Optional = false, UsagePlaceholder = "regexFile")]
        private string _convertTTSFile = string.Empty;

        [Argument("replace", Description = "Input replace.txt",
           Optional = false, UsagePlaceholder = "replaceFile")]
        private string _replaceCharactorFile = string.Empty;

        [Argument("vowel", Description = "Input vowel.txt",
          Optional = false, UsagePlaceholder = "vowelFile")]
        private string _vowelFile = string.Empty;

        [Argument("Output", Description = "Output lexicon.xml",
           Optional = false, UsagePlaceholder = "lexiconXml")]
        private string _lexiconXml = string.Empty;

        /// <summary>
        /// Gets regex Xml.
        /// </summary>
        public string RegexFile
        {
            get { return _regexFile; }
        }

        public string ConvertTTSFile
        {
            get { return _convertTTSFile; }
        }

        public string ReplaceCharactorFile
        {
            get { return _replaceCharactorFile; }
        }

        public string VowelFile
        {
            get { return _vowelFile; }
        }
        /// <summary>
        /// Gets regex Xml.
        /// </summary>
        public string lexiconPath
        {
            get { return _lexiconPath; }
        }

        /// <summary>
        /// Gets regex Xml.
        /// </summary>
        public string lexiconXml
        {
            get { return _lexiconXml; }
        }
        /// <summary>
        /// Validate command line parameter.
        /// </summary>
        /// <returns>Whether the paremeter is correct.</returns>
        public bool Validate()
        {
            bool validate = true;

            if (!File.Exists(RegexFile))
            {
                validate = false;
                Console.WriteLine("RegexXmlNotExist");
            }
            string[] files = Directory.GetFiles(lexiconPath);
            if (files.Count() <= 0)
            {
                validate = false;
                Console.WriteLine("LexiconNotExist");
            }
            return validate;
        }
    }
}
