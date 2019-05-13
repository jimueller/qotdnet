using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace qotdnet
{
    sealed class UdpQuoteListener : IQuoteService
    {
        private IQuoteSource _quoteSource;
        private UdpClient _server;

        public UdpQuoteListener()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();
        }

        public void Listen(int port, IQuoteSource quoteSource)
        {
            _quoteSource = quoteSource;
            _server = new UdpClient(port);
           
            while (true)
            {
                var remoteEP = new IPEndPoint(IPAddress.Any, port);
                _server.Receive(ref remoteEP);

                Log.Information("Request from {RemoteEndPoint}", remoteEP);

                Quote quote = quoteSource.GetQuote();
                byte[] quoteBytes = Encoding.ASCII.GetBytes(quote + "\n");
                _server.Send(quoteBytes,quoteBytes.Length, remoteEP);

                Log.Information("Sent quote {Quote} to {RemoteEndPoint}", quote, remoteEP);
            }

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                _server.Dispose();
                _server = null;

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
