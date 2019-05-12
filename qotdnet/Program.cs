using CommandLine;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace qotdnet
{
    class Program
    {

        public class Options
        {
            [Option("protocol", Default = "tcp", HelpText = "'tcp' or 'udp'")]
            public string Protocol { get; set; }

            [Option("port", Default = 17, HelpText = "TCP/UDP port to service listens on")]
            public int Port { get; set; }

            [Option('i', "in", Default = ".\\quotes.json", HelpText = "Quotes file path")]
            public string QuotesFilePath { get; set; }
        }

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Starting qotdnet...");

            ProtocolType prot = ProtocolType.Tcp;
            int port = 0;
            string filePath = "";

            //parse options
            try
            {
                var result = Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    port = options.Port;

                    if (string.Equals("tcp", options.Protocol, StringComparison.OrdinalIgnoreCase) ||
                        string.IsNullOrWhiteSpace(options.Protocol))
                    {
                        prot = ProtocolType.Tcp;
                    }
                    else if (string.Equals("udp", options.Protocol, StringComparison.OrdinalIgnoreCase))
                    {
                        prot = ProtocolType.Udp;
                    }
                    else
                    {
                        throw new ArgumentException("Unknown protocol: must be 'tcp' or 'udp'");
                    }

                    filePath = options.QuotesFilePath;

                    // find file
                    if (!File.Exists(filePath))
                    {
                        throw new ArgumentException("Specified file, " + options.QuotesFilePath + " not found.");
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                System.Environment.Exit(1);
            }

            Log.Information("Protocol: {ProtocolType}", prot);
            Log.Information("Port: {int}", port);
            Log.Information("Quotes file : {string}", filePath);

            IQuoteSource qs = new JsonFileQuoteSource(new FileInfo(filePath));
            IQuoteService quoteService;

            if(ProtocolType.Tcp == prot)
            {
                quoteService = new TcpQuoteListener();
                quoteService.Listen(port, qs);
            } else
            {
                quoteService = new UdpQuoteListener();
                quoteService.Listen(port, qs);
            }
            
        }
    }
}
