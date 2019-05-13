using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace qotdnet
{
    internal class JsonFileQuoteSource : IQuoteSource
    {
        List<Quote> quotes = new List<Quote>();
        private static Random rnd = new Random();

        public JsonFileQuoteSource(FileInfo file)
        {
            LoadQuotesFromFile(file);
        }


        public Quote GetQuote()
        {
            return quotes[rnd.Next(quotes.Count)];
        }

        public List<Quote> DumpQuotes()
        {
            return quotes;
        }

        private void LoadQuotesFromFile(FileInfo file)
        {
            quotes = JsonConvert.DeserializeObject<List<Quote>>(File.ReadAllText(file.FullName));
        }
    }
}
