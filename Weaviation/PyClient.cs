using System.Threading.Tasks;
using Grpc.Net.Client;
using GrpcWeaviationClient;

namespace Weaviation
{
    public class PyClient
    {
        private string _host;
        public PyClient(string host)
        {
            _host = host;
        }
        //Rewrite SendMessage to use the Messager client
        public async Task<int> SendMessage(EmbeddedMessage message)
        {
            using var channel = GrpcChannel.ForAddress(_host);
            var client = new Messager.MessagerClient(channel);
            var reply = await client.SendMessageAsync(
                new TextMessage { 
                    Text = message.Text, 
                    ThreadId = message.ThreadId, 
                    UserId = message.UserId, 
                    ContextId = message.ContextId });
            return reply;
        }
    }
  
}