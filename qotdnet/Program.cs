using CommandLine;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace qotdnet
{
    class Program
    {
        private List<ProtocolType> AllowedProtocols = new List<ProtocolType>()
        {
            ProtocolType.Tcp,
            ProtocolType.Udp
        };

        private const int DefaultPort = 17;
        private const ProtocolType DefaultProtocol = ProtocolType.Tcp;
        private const string DefaultQuotesFilePath = "quotes.json";


        private ProtocolType _protocol = ProtocolType.Unspecified;
        private int _port = 0;
        private string _quotesFilePath;


        private class Options
        {
            [Option("protocol", HelpText = "'tcp' or 'udp'")]
            public string Protocol { get; set; }

            [Option("port", Default = -1, HelpText = "TCP/UDP port to service listens on")]
            public int Port { get; set; }

            [Option('i', "in", HelpText = "Quotes file path")]
            public string QuotesFilePath { get; set; }
        }

        static void Main(string[] args)
        {
            Program program = new Program();

            try
            {
                program.Run(args);
                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Log.Error("Error: {Exception}", ex);
                Environment.ExitCode = 1;
            }
            finally
            {
                Environment.Exit(Environment.ExitCode);
            }
        }

        void Run(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Starting qotdnet...");

            Initialize(args);

            Log.Information("Protocol: {ProtocolType}", _protocol);
            Log.Information("Port: {int}", _port);
            Log.Information("Quotes file : {string}", _quotesFilePath);

            IQuoteSource qs = new JsonFileQuoteSource(new FileInfo(_quotesFilePath));
            IQuoteService quoteService;

            if (ProtocolType.Tcp == _protocol)
            {
                quoteService = new TcpQuoteListener();
                quoteService.Listen(_port, qs);
            }
            else
            {
                quoteService = new UdpQuoteListener();
                quoteService.Listen(_port, qs);
            }
        }

        private void Initialize(string[] args)
        {
            Log.Information("Initializing configuration...");

            Parser.Default.ParseArguments<Options>(args)
            .WithParsed(options =>
            {
                InitPort(options.Port);
                InitProtocolType(options.Protocol);
                InitFilePath(options.QuotesFilePath);
            });     
        }

        private void InitFilePath(string quotesFilePath)
        {
            string filePath = quotesFilePath;

            if (String.IsNullOrEmpty(filePath))
            {
                filePath = Environment.GetEnvironmentVariable("QOTD_FILE_PATH");
            }

            if (String.IsNullOrEmpty(filePath))
            {
                filePath = DefaultQuotesFilePath;
            }

            // Check if path or just file name
            if (!filePath.Contains(Path.DirectorySeparatorChar))
            {
                // let's assume it's just a file name, so expect it to be in pwd
                filePath = "." + Path.DirectorySeparatorChar + filePath;
            }

            // find file
            if (!File.Exists(filePath))
            {
                throw new ArgumentException("Quotes file, " + filePath + " not found.");
            }
            else
            {
                _quotesFilePath = filePath;
            }
        }

        private void InitProtocolType(string optionsProtocol)
        {
            string proto = null;
            ProtocolType protocolType = ProtocolType.Unspecified;

            if (String.IsNullOrEmpty(optionsProtocol))
            {
                Log.Information("Protocol arg not specified, checking QOTD_PROTO_TYPE env var...");
                string protoEnv = Environment.GetEnvironmentVariable("QOTD_PROTO_TYPE");

                if (!String.IsNullOrEmpty(protoEnv))
                {
                    Log.Information("Found QOTD_PROTO_TYPE: {string}.", protoEnv);
                    proto = protoEnv;
                }
            } else
            {
                proto = optionsProtocol;
            }

            if (!String.IsNullOrEmpty(proto))
            {
                if(Enum.TryParse(proto, true, out protocolType))
                {
                    if (!AllowedProtocols.Contains(protocolType))
                    {
                        throw new ArgumentException($"Specified protocol {proto} not allowed, must be TCP or UDP");
                    }
                }
                else
                {
                    throw new ArgumentException($"Unknown protocol {proto} specified, must be TCP or UDP");
                }
            }

            if(ProtocolType.Unspecified == protocolType)
            {
                Log.Information("Using default protocol {ProtocolType}", DefaultProtocol);
                protocolType = DefaultProtocol;
            }

            _protocol = protocolType;
        }

        private void InitPort(int optionsPort)
        {
            int port = -1;
            bool provided = false;

            if (optionsPort == -1)
            {
                Log.Information("Port from args not provided, reading QOTD_PORT env var...");
                string qotdEnvVar = Environment.GetEnvironmentVariable("QOTD_PORT");

                if (!String.IsNullOrEmpty(qotdEnvVar))
                {
                    if (!int.TryParse(Environment.GetEnvironmentVariable("QOTD_PORT"), out port))
                    {
                        throw new ArithmeticException("Unable to parse QOTD_PORT " + qotdEnvVar);
                    }
                    else
                    {
                        Log.Information("Found QOTD_PORT: {int}.", port);
                        provided = true;
                    }
                }
            }
            else
            {
                Log.Information("Using port {int} from args", optionsPort);
                port = optionsPort;
                provided = true;
            }

            if (!provided)
            {
                Log.Information("Using default port, {int}", DefaultPort);
                port = DefaultPort;
            }
            else if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException($"Port {port} not in allowable range {IPEndPoint.MinPort}:{IPEndPoint.MaxPort}");
            }

            _port = port;
        }
    }
}
