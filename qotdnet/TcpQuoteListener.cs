using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace qotdnet
{
    class TcpQuoteListener : IQuoteService
    {
        private IQuoteSource quoteSource;
        private TcpListener server;

        public TcpQuoteListener()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();
        }

        public void Listen(int port, IQuoteSource quoteSource)
        {
            this.quoteSource = quoteSource;
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();

                Log.Information("Request from {RemoteEndPoint}", client.Client.RemoteEndPoint);

                Quote quote = quoteSource.GetQuote();
                byte[] quoteBytes = Encoding.ASCII.GetBytes(quote.ToString() + "\n");
                client.GetStream().Write(quoteBytes, 0, quoteBytes.Length);

                Log.Information("Sent quote {Quote} to {RemoteEndPoint}", quote, client.Client.RemoteEndPoint);

                client.Close();
            }

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                server.Stop();
                server = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TcpQuoteListener()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
