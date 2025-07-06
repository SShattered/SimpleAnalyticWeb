using SimpleAnalyticApp.Models;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleAnalyticApp.Utils
{
    public class TcpClientHandler
    {
        private TcpClient? client;
        private Stream? stream;

        private readonly ConcurrentDictionary<string, byte[]> frames = new();
        private readonly ConcurrentDictionary<string, int> detections = new();
        private ConcurrentDictionary<string, TaskCompletionSource<string>> pendingMessages;
        private readonly SemaphoreSlim writeLock = new SemaphoreSlim(1, 1);

        public event Action<string, string>? OnResponseReceived;

        public TcpClientHandler()
        {
            pendingMessages = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
            Debug.WriteLine("TcpCommHandler");
        }

        public async Task Connect(string ip, int port)
        {
            client = new TcpClient();
            await client.ConnectAsync(ip, port);
            stream = client.GetStream();
            Listen();
        }

        public bool IsConnected()
        {
            try
            {
                if (client == null || !client.Connected)
                    return false;

                if (client.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buffer = new byte[1];
                    return !(client.Client.Receive(buffer, SocketFlags.Peek) == 0);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Disconnect()
        {
            try
            {
                if (client.Connected)
                {
                    stream?.Close();
                    client.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error disconnecting: {ex.Message}");
            }
            return false;
        }

        public void Listen()
        {
            Task.Run(async () =>
            {
                var lengthBuffer = new byte[4];
                while (client.Connected)
                {
                    try
                    {
                        await stream.ReadExactlyAsync(lengthBuffer, 0, 4);
                        int length = BitConverter.ToInt32(lengthBuffer.Reverse().ToArray(), 0); // big-endian

                        var frameBuffer = new byte[length];
                        await stream.ReadExactlyAsync(frameBuffer, 0, frameBuffer.Length);

                        var data = ASCIIEncoding.UTF8.GetString(frameBuffer, 0, frameBuffer.Length);
                        //Debug.WriteLine($"{data}");
                        var messageModel = JsonSerializer.Deserialize<MessageModel<string>>(data);

                        if(pendingMessages.TryRemove(messageModel.MessageId, out var tcs))
                        {
                            tcs.SetResult(data);
                        }
                        else
                        {
                            if(messageModel.Instruction == "Frame")
                            {
                                frames[messageModel.MessageId] = Convert.FromBase64String(messageModel.Message);
                            }else if(messageModel.Instruction == "Detections")
                            {
                                detections[messageModel.MessageId] = Convert.ToInt32(messageModel.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                        break;
                    }
                }
            });
        }

        public async Task<string> SendMessage(MessageModel<object> message)
        {
            var json = JsonSerializer.Serialize(message);
            Debug.WriteLine(json);
            var tcs = new TaskCompletionSource<string>();
            pendingMessages[message.MessageId] = tcs;
            WriteString(json);

            return await tcs.Task;
        }

        private async void WriteString(string message)
        {
            byte[] msgBuffer = Encoding.UTF8.GetBytes(message);
            byte[] lengthBuffer = BitConverter.GetBytes(msgBuffer.Length);
            if(BitConverter.IsLittleEndian)
                Array.Reverse(lengthBuffer);

            await writeLock.WaitAsync();
            try
            {
                await stream.WriteAsync(lengthBuffer, 0, lengthBuffer.Length);
                await stream.WriteAsync(msgBuffer);
                await stream.FlushAsync();
            }
            finally
            {
                writeLock.Release();
            }
        }

        public byte[] GetLatestFrame(string streamId)
        {
            frames.TryGetValue(streamId, out var frame);
            return frame;
        }

        public string GetLatestDetections(string id)
        {
            if (detections.TryGetValue(id, out var count))
            {
                return count.ToString();
            }
            return "NA";
        }
    }
}
