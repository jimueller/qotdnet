using System;
using System.Collections.Generic;
using System.Text;

namespace qotdnet
{
    interface IQuoteSource
    {
        Quote GetQuote();
        List<Quote> DumpQuotes();
    }
}
