namespace DnsQuery.Types
{
    public struct Header
    {
        public ushort id;

        public ushort raw_1st;

        public Qr qr
        {
            get
            {
                return (Qr)(raw_1st >> 15 & 1);
            }
        }

        public OpCode opCode
        {
            get
            {
                return (OpCode)(raw_1st >> 11 & 15);
            }
        }

        public bool aa
        {
            get
            {
                return (raw_1st >> 10 & 1) == 1;
            }
        }

        public bool tc
        {
            get
            {
                return (raw_1st >> 9 & 1) == 1;
            }
        }

        public bool rd
        {
            get
            {
                return (raw_1st >> 8 & 1) == 1;
            }
            set
            {
                raw_1st = (ushort)(raw_1st | (value ? 1 : 0 << 8));
            }
        }

        public bool ra
        {
            get
            {
                return (raw_1st >> 7 & 1) == 1;
            }
            set
            {
                raw_1st = (ushort)(raw_1st | (value ? 1 : 0 << 7));
            }
        }

        public byte z
        {
            get
            {
                return (byte)(raw_1st >> 6 & 7);
            }
        }

        public ResponseCode rcode
        {
            get
            {
                return (ResponseCode)(raw_1st & 0xF);
            }
        }

        public ushort qdcount;
        public ushort ancount;
        public ushort nscount;
        public ushort arcount;
    }
}
