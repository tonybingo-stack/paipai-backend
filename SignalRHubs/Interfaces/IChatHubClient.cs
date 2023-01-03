namespace SignalRHubs
{
    public interface IChatHubClient
    {
        Task ReceiveMessage(string message);
    }
}
