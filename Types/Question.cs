namespace DnsQuery.Types
{
    public struct Question
    {
        public string qname;
        public ushort qtype;
        public ushort qclass;
    }
}
