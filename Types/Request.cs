namespace DnsQuery.Types
{
    public struct Request
    {
        public Header header;
        public List<Question> questions = new List<Question>();
        public List<Answer> answers = new List<Answer>();

        public Request()
        {
        }
    }
}
