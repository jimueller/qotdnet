using System;
using System.Collections.Generic;
using System.Text;

namespace qotdnet
{
    internal interface IQuoteSource
    {
        Quote GetQuote();
        List<Quote> DumpQuotes();
    }
}
