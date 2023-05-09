using System.Text;
using DnsQuery.Types;

namespace DnsQuery
{
    public static class RequestExtensions
    {
        public static byte[] ToNetwork(this Request request) 
        {
            List<byte> list = new List<byte>();
            var id = BitConverter.GetBytes(request.header.id);
            list.AddRange(id.Reverse());

            var flags = BitConverter.GetBytes(request.header.raw_1st);
            list.AddRange(flags);

            var qdcount = BitConverter.GetBytes((ushort)request.questions.Count);
            list.AddRange(qdcount.Reverse());

            var ancount = BitConverter.GetBytes(request.header.ancount);
            list.AddRange(ancount.Reverse());

            var nscount = BitConverter.GetBytes(request.header.nscount);
            list.AddRange(nscount.Reverse());

            var arcount = BitConverter.GetBytes(request.header.arcount);
            list.AddRange(arcount.Reverse());

            foreach(var item in request.questions)
            {
                var labels = item.qname.Split(".");
                foreach(var label in labels)
                {
                    var labelLength =(byte)label.Length;
                    var LabelBytes = Encoding.ASCII.GetBytes(label);

                    list.Add(labelLength);
                    list.AddRange(LabelBytes);
                }

                var qtype = BitConverter.GetBytes(item.qtype);
                var qclass = BitConverter.GetBytes(item.qclass);
                list.Add((byte)0x0);
                list.AddRange(qtype.Reverse());
                list.AddRange(qclass.Reverse());
            }

            return list.ToArray();
        }

        public static Request ToRequest(this byte[] data)
        {
            var request = new Request();
            var skip = 0;

            request.header.id = (ushort)BitConverter.ToInt16(data.Skip(skip).Take(2).Reverse().ToArray());
            request.header.raw_1st = (ushort)BitConverter.ToInt16(data.Skip(skip += 2).Take(2).Reverse().ToArray());
            request.header.qdcount = (ushort)BitConverter.ToInt16(data.Skip(skip += 2).Take(2).Reverse().ToArray());
            request.header.ancount = (ushort)BitConverter.ToInt16(data.Skip(skip += 2).Take(2).Reverse().ToArray());
            request.header.nscount = (ushort)BitConverter.ToInt16(data.Skip(skip += 2).Take(2).Reverse().ToArray());
            request.header.arcount = (ushort)BitConverter.ToInt16(data.Skip(skip += 2).Take(2).Reverse().ToArray());

            var question = new Question();

            var domainName = Name(data, skip += 2);
            question.qname = string.Join(".", domainName.Item2);
            skip = domainName.Item1 + 1;

            question.qtype = (ushort)BitConverter.ToInt16(data.Skip(skip += 2).Take(2).Reverse().ToArray());
            question.qclass = (ushort)BitConverter.ToInt16(data.Skip(skip += 2).Take(2).Reverse().ToArray());

            request.questions.Add(question);

            for (int i = 0; i < request.header.ancount; i++)
            {
                var answer = new Answer();

                var name = Name(data, skip);

                answer.aname = string.Join(".", name.Item2);
                answer.atype = (DnsType)BitConverter.ToInt16(data.Skip(skip += 2).Take(2).Reverse().ToArray());
                answer.aclass = (DnsClass)BitConverter.ToInt16(data.Skip(skip += 2).Take(2).Reverse().ToArray());
                answer.ttl = (ushort)BitConverter.ToInt32(data.Skip(skip += 4).Take(4).Reverse().ToArray());
                answer.rdlength = (ushort)BitConverter.ToInt16(data.Skip(skip += 2).Take(2).Reverse().ToArray());
                skip += 2;
                if (answer.atype == DnsType.A)
                {
                    List<string> list = new List<string>();
                    for (int j = 0; j < 4; j++)
                    {
                        list.Add(data[skip + j].ToString());
                    }

                    answer.display = string.Join(".", list.ToArray());
                }
                else if (answer.atype == DnsType.CName)
                {
                    var cname = Name(data, skip);
                    answer.display = string.Join(".", cname.Item2);
                }

                skip += answer.rdlength;

                request.answers.Add(answer);
            }

            return request;
        }

        private static bool IsOptimised(ushort value)
        {
            return (value >> 14 & 0x3) == 0x3;
        }

        private static (int, string) ReadLabel(byte[] data, int offset)
        {
            var labelLength = data[offset];
            if (labelLength == 0x0)
            {
                return (0, string.Empty);
            }

            var answerName = (ushort)BitConverter.ToInt16(data.Skip(offset).Take(2).Reverse().ToArray());
            if (IsOptimised(answerName))
            {
                offset = answerName & 0x3FFF;
                var ptrLabel = ReadLabel(data, offset);
                return (2, ptrLabel.Item2);
            }
            else
            {
                offset++;
            }

            var label = Encoding.ASCII.GetString(data.Skip(offset).Take(labelLength).ToArray());

            return (labelLength+1, label);
        }

        private static List<string> ReadOptimisedLabels(byte[] data, int offset)
        {
            List<string> labels = new List<string>();
            var response = Name(data, offset);
            return response.Item2;
        }

        private static (int, List<string>) Name(byte[] data, int offset)
        {
            var labelLength = 0x0;
            List<string> labels = new List<string>();

            do
            {
                var labelMetadata = (ushort)BitConverter.ToInt16(data.Skip(offset).Take(2).Reverse().ToArray());
                if(IsOptimised(labelMetadata))
                {
                    var optimisedLabels = ReadOptimisedLabels(data, labelMetadata & 0x3FFF);
                    labels.AddRange(optimisedLabels);
                    offset += 2;
                    labelLength = 0x0;
                }
                else
                {
                    var label = ReadLabel(data, offset);
                    labelLength = label.Item1;
                    offset += labelLength;
                    if (labelLength > 0)
                    {
                        labels.Add(label.Item2);
                    }
                }
            }
            while (labelLength != 0x0 && data[offset] != 0x0);

            return (offset, labels);
        } 


    }
}
