using DnsQuery.Types;
using Spectre.Console;
using System.Net;

internal class Program
{
    private static void Main(string[] args)
    {
        var host = AnsiConsole.Ask<string>("Specify a [green]host[/] to query:");

        var response = DnsQuery.DnsQuery.ResolveDnsName(IPAddress.Parse("8.8.8.8"), 53, host);

        if (response.header.rcode == ResponseCode.ServerFailure)
        {
            Console.WriteLine("Server error");
        } 
        else
        {
            var table = new Table();
            table.AddColumn("Type");
            table.AddColumn("Name");
            table.AddColumn("TTL");
            table.AddColumn("Data");
            table.Width = 250;
            table.Columns[0].Width = 8;

            foreach(var answer  in response.answers)
            {
                table.AddRow(answer.atype.ToString(), answer.aname, answer.ttl.ToString(), answer.display);
            }

            AnsiConsole.Write(table);
        }
    }
}