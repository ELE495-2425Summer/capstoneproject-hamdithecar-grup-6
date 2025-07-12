using System;
using System.Threading.Tasks;
using CarControlApp.Utilities;
using System.Windows.Forms.Design;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CarControlApp.Communication
{
    public class ZeroMQCommunicator
    {
        private RequestSocket reqSocket;
        private SubscriberSocket subSocket;
        private bool isConnected = false;
        private string serverAddress;
        private Task subscriberTask;

        public bool IsConnected => isConnected;
        public event Action<string> OnMessage;
        public event Action<string, string> OnFileUpdate; // File updates
        public event Action<string, string> OnApiStatus; // API messages
        public event Action<string, bool> OnUserStatus; // User detection messages
        public event Action<string, string, int, double, string> OnCarStatus; // Car messages

        public ZeroMQCommunicator()
        {
            reqSocket = null;
            subSocket = null;
        }

        public async Task<bool> Connect(string ipAddress, int port = 5565)
        {
            try
            {
                // Clean up existing connections
                Disconnect();

                // Create REQ socket for commands
                reqSocket = new RequestSocket();
                reqSocket.Options.Linger = TimeSpan.FromMilliseconds(1000);
                serverAddress = $"tcp://{ipAddress}:{port}";
                reqSocket.Connect(serverAddress);

                // Create SUB socket for file updates
                subSocket = new SubscriberSocket();
                subSocket.Connect($"tcp://{ipAddress}:{port + 1}"); // Connect to Pi's IP and port
                subSocket.SubscribeToAnyTopic(); // Subscribe to all messages

                // Test connection
                if (await TestConnection())
                {
                    isConnected = true;
                    // Start file update monitoring
                    ProcessPubMessages();
                    return true;
                }
                else
                {
                    Disconnect();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Disconnect();
                return false;
            }
        }

        // Disconnect from Pi
        public void Disconnect()
        {
            isConnected = false;

            subscriberTask?.Wait(1000);

            reqSocket?.Dispose();
            reqSocket = null;

            subSocket?.Dispose();
            subSocket = null;
        }

        // Process PUB messages comes from Pi
        private void ProcessPubMessages()
        {
            subscriberTask = Task.Run(() =>
            {
                try
                {
                    while (isConnected && subSocket != null)
                    {
                        if (subSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(100), out string message))
                        {
                            try
                            {
                                var update = JsonConvert.DeserializeObject<JObject>(message);

                                if (update["type"]?.Value<string>() == "FILE_UPDATE") // File updates
                                {
                                    string fileType = update["file_type"]?.Value<string>();
                                    var result = update["result"] as JObject;

                                    if (result?["success"]?.Value<bool>() == true)
                                    {
                                        string content = "";

                                        if (fileType == "JSON")
                                        {
                                            content = result["data"]?.Value<string>() ?? "";
                                        }
                                        else
                                        {
                                            content = result["data"]?.Value<string>() ?? "";
                                        }
                                        OnFileUpdate?.Invoke(fileType, content);
                                    }
                                }
                                else if (update["type"]?.Value<string>() == "CAR_STATUS") // Car messages
                                {
                                    var data = update["data"] as JObject;
                                    if (data != null)
                                    {
                                        string action = data["action"]?.Value<string>() ?? "";
                                        string details = data["details"]?.Value<string>() ?? "";
                                        string timestamp = data["timestamp"]?.Value<string>() ?? "";
                                        int speed = data["speed"]?.Value<int>() ?? 0;
                                        double distance = data["distance"]?.Value<double>() ?? 0;

                                        OnCarStatus?.Invoke(action, details, speed, distance, timestamp);
                                    }
                                }
                                else if (update["type"]?.Value<string>() == "API_STATUS") // API messages
                                {
                                    var data = update["data"] as JObject;
                                    if (data != null)
                                    {
                                        string status = data["status"]?.Value<string>() ?? "";
                                        string statusMessage = data["message"]?.Value<string>() ?? "";

                                        OnApiStatus?.Invoke(status, statusMessage);
                                    }
                                }
                                else if (update["type"]?.Value<string>() == "VOICE_DETECTION") // Voice detection messages
                                {
                                    var data = update["data"] as JObject;
                                    if (data != null)
                                    {
                                        string user = data["user"]?.Value<string>() ?? "";
                                        bool recognized = data["recognized"]?.Value<bool>() ?? false;

                                        OnUserStatus?.Invoke(user, recognized);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log($"Mesaj alımında hata gerçekleşti: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"Komut/Komut metni güncellemesi hatası: {ex.Message}");
                }
            });
        }

        // Test connection health
        public async Task<bool> TestConnection()
        {
            try
            {
                if (reqSocket == null) return false;

                return await Task.Run(() =>
                {
                    try
                    {
                        reqSocket.SendFrame("PING");

                        bool received = reqSocket.TryReceiveFrameString(TimeSpan.FromSeconds(3), out string response);

                        if (received && response == "PONG")
                        {
                            return true;
                        }

                        Log("Araçtan cevap alınamadı.");
                        return false;
                    }
                    catch
                    {
                        return false;
                    }
                });
            }
            catch
            {
                return false;
            }
        }

        // Send start command to Pi
        public async Task<string> SendStartCommand()
        {
            if (!isConnected || reqSocket == null)
                throw new Exception("Not connected");

            try
            {
                return await Task.Run(() =>
                {
                    reqSocket.SendFrame("START");

                    if (reqSocket.TryReceiveFrameString(TimeSpan.FromSeconds(10), out string response))
                    {
                        return response;
                    }

                    throw new Exception("No response received");
                });
            }
            catch (Exception ex)
            {
                Log($"Başlat komutu başarısız: {ex.Message}");
                throw;
            }
        }

        // Write log messages to txtDebugLog
        private void Log(string message)
        {
            OnMessage?.Invoke(message);
        }

        public void Dispose()
        {
            Disconnect();
            NetMQConfig.Cleanup();
        }
    }
}
