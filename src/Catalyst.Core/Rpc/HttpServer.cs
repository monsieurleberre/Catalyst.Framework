using Catalyst.Abstractions.IO.EventLoop;
using Catalyst.Abstractions.IO.Transport;
using Catalyst.Abstractions.IO.Transport.Channels;
using Catalyst.Core.IO.Transport;
using Catalyst.Core.IO.Transport.Channels;
using DotNetty.Transport.Channels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Catalyst.Core.Rpc.IO.Transport.Channels;

namespace Catalyst.Core.Rpc
{
    public class HttpServer : SocketBase, ISocket
    {
        public HttpServer(HttpRpcServerChannelFactory channelFactory, ILogger logger, IEventLoopGroupFactory eventLoopGroupFactory) : base(channelFactory, logger, eventLoopGroupFactory)
        {
        }
        
        public void Dispose()
        {
            //TODO
        }


        public override async Task StartAsync()
        {
            var observableSocket = await ChannelFactory.BuildChannel(EventLoopGroupFactory, IPAddress.Parse("127.0.0.1"), 5050);
            Channel = observableSocket.Channel;
        }
    }
}
