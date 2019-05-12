using System;
using System.Collections.Generic;
using System.Text;

namespace qotdnet
{
    interface IQuoteService : IDisposable
    {

        void Listen(int port, IQuoteSource quoteSource);
    }
}
