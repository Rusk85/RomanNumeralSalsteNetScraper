using System;

namespace RomanNumeralSalsteNetScraper.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            RunScraper();
        }

        private static void RunScraper()
        {
            RomanNumeralScraper scraper = new RomanNumeralScraper();
            scraper.StartScraping();
        }
    }
}