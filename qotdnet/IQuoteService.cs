using System;
using System.Collections.Generic;
using System.Text;

namespace qotdnet
{
    internal interface IQuoteService : IDisposable
    {

        void Listen(int port, IQuoteSource quoteSource);
    }
}
