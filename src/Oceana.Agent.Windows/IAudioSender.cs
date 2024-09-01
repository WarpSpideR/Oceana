namespace Oceana.Agent.Windows;

/// <summary>
/// Audio sending interface.
/// </summary>
public interface IAudioSender : IDisposable
{
    /// <summary>
    /// Send audio to the remote player.
    /// </summary>
    /// <param name="payload">Data to send.</param>
    /// <returns>Task representing the result of the asynchronous operation.</returns>
    Task SendAsync(ArraySegment<byte> payload);
}
