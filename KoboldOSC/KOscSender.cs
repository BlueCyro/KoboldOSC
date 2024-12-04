using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;
using KoboldOSC.Messages;

namespace KoboldOSC;

public class KOscSender : IDisposable
{
    public Action<object?, int>? Logger;

    public readonly IPEndPoint Endpoint;
    public bool IsOpen { get; private set; }
    

    private bool disposed;
    private ArrayPool<byte> bufferPool;
    private Socket socket;
    private CancellationTokenSource tokenSource;
    private CancellationToken token;

    [AllowNull]
    private Task sendTask;
    private BufferBlock<IKOscPacket> outgoing;


    public KOscSender(IPEndPoint ep)
    {
        Endpoint = ep;
        bufferPool = ArrayPool<byte>.Create();
        outgoing = new();


        socket = new(Endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);


        tokenSource = new();
        token = tokenSource.Token;
    }



    public void Open()
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        if (IsOpen)
            throw new InvalidOperationException("Socket is already open!");


        sendTask = Task.Run(SendLoop, token);
        IsOpen = true;
    }



    private async Task<Exception?> SendLoop()
    {
        for (;;)
        {
            if (token.IsCancellationRequested)
                break;

            try
            {
                IKOscPacket packet = await outgoing.ReceiveAsync(token);
                await SendData(packet, token);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Log($"Exception in sender loop: {ex}");
                return ex;
            }
        }
        return null;
    }



    private async Task SendData(IKOscPacket packet, CancellationToken token)
    {
        byte[] toSend = bufferPool.Rent(packet.ByteLength);
        Array.Clear(toSend);

        packet.Serialize(toSend);
        try
        {
            int bytesSent = await socket.SendToAsync(toSend.AsMemory()[..packet.ByteLength], Endpoint, token);
            // Log($"Sent byte count: {bytesSent}");
        }
        finally
        {
            packet.Dispose();
            bufferPool.Return(toSend);
        }
    }



    public bool Send(params Span<IKOscPacket> packet)
    {
        bool success = true;
        for (int i = 0; i < packet.Length; i++)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            
            if (!IsOpen)
                throw new InvalidOperationException("This OSC sender isn't open yet! Did you call 'Open()'?");

            if (!outgoing.Post(packet[i]))
            {
                success = false;
            }
        }
        return success;

    }



    public void Dispose()
    {
        if (disposed)
            return;

        GC.SuppressFinalize(this);
        disposed = true;
        IsOpen = false;

        tokenSource.Cancel();
        sendTask.Wait();
        outgoing.Complete();
        socket.Dispose();
    }

    private void Log(object? message = null, int logLevel = 0) => Logger?.Invoke(message, logLevel);
}
