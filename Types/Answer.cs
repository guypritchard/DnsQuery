namespace DnsQuery.Types
{
    public struct Answer
    {
        public string aname;
        public DnsType atype;
        public DnsClass aclass;
        public uint ttl;
        public ushort rdlength;
        public byte[] rdata;
        public string display;
    }
}
