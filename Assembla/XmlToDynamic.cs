using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Assembla
{
    public class DynamicXml : DynamicObject
    {
        private readonly XElement _element;

        public DynamicXml(XElement element)
        {
            _element = element;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var elementName = SplitUpperCaseToString(binder.Name);
            var descendant = _element.Element(elementName);
            if(descendant == null)
            {
                result = null;
                return false;
            }
            else if(descendant.Descendants().Any())
            {
                result = new DynamicXml(descendant);
                return true;
            }
            else
            {
                
                result = descendant.Value;
                return true;
            }
        }
        
        private static Type GetTypeFromName(string name)
        {
            switch (name)
            {
                case "integer":
                    return typeof (int);
                case "boolean":
                    return typeof (bool);
                case "datetime":
                    return typeof (DateTime);
                default:
                    return typeof(string);
            }
        }

        /// <summary>
        /// Parses a camel cased or pascal cased string and returns a new
        /// string with spaces between the words in the string.
        /// </summary>
        /// <example>
        /// The string "PascalCasing" will return an array with two
        /// elements, "Pascal" and "Casing".
        /// </example>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string SplitUpperCaseToString(string source)
        {
            return string.Join("-", SplitUpperCase(source).Select(x => x.ToLower()));
        }

        /// <summary>
        /// Parses a camel cased or pascal cased string and returns an array
        /// of the words within the string.
        /// </summary>
        /// <example>
        /// The string "PascalCasing" will return an array with two
        /// elements, "Pascal" and "Casing".
        /// </example>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string[] SplitUpperCase(string source)
        {
            if (source == null)
                return new string[] { }; //Return empty array.

            if (source.Length == 0)
                return new string[] { "" };

            StringCollection words = new StringCollection();
            int wordStartIndex = 0;

            char[] letters = source.ToCharArray();
            // Skip the first letter. we don't care what case it is.
            for (int i = 1; i < letters.Length; i++)
            {
                if (char.IsUpper(letters[i]))
                {
                    //Grab everything before the current index.
                    words.Add(new String(letters, wordStartIndex, i - wordStartIndex));
                    wordStartIndex = i;
                }
            }

            //We need to have the last word.
            words.Add(new String(letters, wordStartIndex, letters.Length - wordStartIndex));

            //Copy to a string array.
            string[] wordArray = new string[words.Count];
            words.CopyTo(wordArray, 0);
            return wordArray;
        }
    }
}
