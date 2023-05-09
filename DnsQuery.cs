using DnsQuery.Types;
using System.Net;
using System.Net.Sockets;

namespace DnsQuery
{
    public class DnsQuery
    {
        public static Request ResolveDnsName(IPAddress server, int port, string host)
        {
            UdpClient udpClient = new UdpClient(server.ToString(), port);
            
            Request request = new Request();
            request.header.id = (ushort)Random.Shared.Next();
            request.header.rd = true;
            request.questions.Add(new Question
            {
                qname = host,
                qtype = 1,
                qclass = 1
            });

            var data = request.ToNetwork();

            int sent = udpClient.Send(data);

            if (sent != data.Length)
            {
                throw new IOException("Request not sent in its entirety.");
            }

            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var dgramresponse = udpClient.Receive(ref RemoteIpEndPoint);

            return dgramresponse.ToRequest();
        }
    }
}
