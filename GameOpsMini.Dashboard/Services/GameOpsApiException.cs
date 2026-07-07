namespace GameOpsMini.Dashboard.Services;

public class GameOpsApiException : Exception
{
    public GameOpsApiException(
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
    }
}