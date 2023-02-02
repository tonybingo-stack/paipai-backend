namespace SignalRHubs.Lib
{
    public class StreamThread
    {
        public async Task CreateDnsThreadAsync(String domain, String ip)
        {
            Console.WriteLine(domain + "===>" + ip);
        }
    }
}
