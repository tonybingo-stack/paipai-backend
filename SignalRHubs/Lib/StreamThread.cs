
namespace SignalRHubs.Lib
{
    public class StreamThread
    {
        public async Task CreateDnsThreadAsync(String name, String packet)
        {
            Console.WriteLine(name + "===>" + packet);
        }
    }
}
