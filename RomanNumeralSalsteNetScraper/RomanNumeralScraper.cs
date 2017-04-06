using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace RomanNumeralSalsteNetScraper
{
    public class RomanNumeralScraper
    {
        private const string URL = @"http://www.tuomas.salste.net/doc/roman/numeri-romani-1-5000.html";

        private const string LF = "\n";

        private const string CRLF = "\n\r";

        private readonly HashSet<string> tableIds = new HashSet<string>
        {
            "n1",
            "n501",
            "n1001",
            "n1501",
            "n2001",
            "n2501",
            "n3001",
            "n3501",
            "n4001"
        };

        private Dictionary<int, string> _arabicRomanNumeralMapping = new Dictionary<int, string>();

        public void StartScraping()
        {
            LoadDocumentRoot();
            SerializeMappingsAsDictionary();
        }

        private void LoadDocumentRoot()
        {
            string urlAsHtmlString;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(URL).Result;
                urlAsHtmlString = response.Content.ReadAsStringAsync().Result;
            }
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(urlAsHtmlString);
            GetMappingTables(document.DocumentNode);
        }

        private void GetMappingTables(HtmlNode root)
        {
            List<HtmlNode> pre = root.Descendants("pre").Where(node => tableIds.Any(tid => node.Id == tid)).ToList();
            List<string> numeralMappings = new List<string>();
            foreach (HtmlNode htmlNode in pre)
            {
                numeralMappings.Add(htmlNode.InnerText);
            }
            FlattenMappingsLists(numeralMappings);
        }

        private void FlattenMappingsLists(List<string> mappingsLists)
        {
            string firstFlatteningStep = String.Empty;
            foreach (string mappingsList in mappingsLists)
            {
                firstFlatteningStep += mappingsList;
            }

            List<string> oneMappingPerLine =
                firstFlatteningStep.Split(new string[] { LF }, StringSplitOptions.RemoveEmptyEntries).ToList();

            ParseMappingTables(oneMappingPerLine);
        }

        private void ParseMappingTables(List<string> singleLineMappings)
        {
            string pattern = @"^([0-9]+)(=)([A-Z]+)";
            Regex regex = new Regex(pattern);
            foreach (string singleLineMapping in singleLineMappings)
            {
                Match match = regex.Match(singleLineMapping);
                if (match.Success)
                {
                    int arabicNumeral = Convert.ToInt32(match.Groups[1].Value);
                    string romanNumeral = match.Groups[3].Value;
                    _arabicRomanNumeralMapping.Add(arabicNumeral, romanNumeral);
                }
                else
                {
                    Debug.WriteLine($"Could not match: {singleLineMapping}");
                }
            }
        }

        private void SerializeMappingsAsDictionary()
        {
            string outputPath = Path.Combine(AppContext.BaseDirectory, "ArabicRomanNumeralDictionary.JSON");
            string serializedDictionary = JsonConvert.SerializeObject(_arabicRomanNumeralMapping, Formatting.Indented);
            File.WriteAllText(outputPath, serializedDictionary, Encoding.UTF8);
        }


    }
}
