using CarControlApp.Communication;
using CarControlApp.Utilities;
using NetMQ;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarControlApp
{
    public partial class MainForm : Form
    {
        private ZeroMQCommunicator communicator;
        private Timer connectionTimer;
        private bool isConnected = false;

        // File path on the PC
        private const string TASK_HISTORY_FILE = "task_history.log";

        private string lastSavedJsonContent = "";

        public MainForm()
        {
            InitializeComponent();
            MainWindowSetup();
            InitializeApp();
        }
        #region Main Form Design

        private void MainWindowSetup()
        {
            // Make window fullscreen and non-resizable
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = false;

            // Prevent resizing
            this.MaximumSize = Screen.PrimaryScreen.Bounds.Size;
            this.MinimumSize = Screen.PrimaryScreen.Bounds.Size;

            // Setup layout after form is loaded
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LocationSetup();
        }

        private void LocationSetup()
        {
            // Position title and buttons on form
            int margin = 20;
            lblTitle.Location = new Point(margin, margin);
            btnExit.Location = new Point(this.ClientSize.Width - 140, margin);

            // Setup 4 panels in 2x2 grid
            SetupMainGroupBoxes();
        }

        private void SetupMainGroupBoxes()
        {
            int margin = 20;
            int headerHeight = 80; // Space for title and button at top

            // Calculate available width and height for grid panels
            int availableWidth = this.ClientSize.Width - (3 * margin);
            int availableHeight = this.ClientSize.Height - headerHeight - (3 * margin);

            int panelWidth = availableWidth / 2;
            int panelHeight = availableHeight / 2;

            // Top row
            grpConnection.Location = new Point(margin, headerHeight);
            grpConnection.Size = new Size(panelWidth, panelHeight);

            grpCommandText.Location = new Point(margin * 2 + panelWidth, headerHeight);
            grpCommandText.Size = new Size(panelWidth, panelHeight);

            // Bottom row
            grpMessages.Location = new Point(margin, headerHeight + panelHeight + margin);
            grpMessages.Size = new Size(panelWidth, panelHeight);

            grpCarTasks.Location = new Point(margin * 2 + panelWidth, headerHeight + panelHeight + margin);
            grpCarTasks.Size = new Size(panelWidth, panelHeight);

            // Setup textboxes and special layouts
            SetupTextBoxes();
        }

        private void SetupTextBoxes()
        {
            // Set textbox sizes to fill the groupbox content area with margins
            int textBoxMargin = 10;
            int titleHeight = 20;

            // GroupBox1 - simple layout with connection controls
            SetupGroupBoxTopLeftControls();

            // GroupBox3 - has nested GroupBoxes
            SetupGroupBoxBottomLeftControls();

            // GroupBox4 - has nested GroupBoxes
            SetupGroupBoxBottomRightControls();

            // Other groupboxes use standard layout
            foreach (var (gb, txt) in new[] {
                (grpCommandText, txtCommandText)
            })
            {
                txt.Size = new Size(
                    gb.Width - (textBoxMargin * 2),
                    gb.Height - titleHeight - textBoxMargin * 2
                );
                txt.Location = new Point(textBoxMargin, titleHeight + textBoxMargin);
            }
        }

        private void SetupGroupBoxTopLeftControls()
        {
            // Position start button on the right side
            btnStart.Location = new Point(grpConnection.Width - 320, 34);

            // Connection textbox takes remaining width
            txtIPAddress.Size = new Size(grpConnection.Width - 500, 35);
            txtIPAddress.Location = new Point(140, 35);
        }

        private void SetupGroupBoxBottomLeftControls()
        {
            int margin = 10;
            int availableWidth = grpMessages.Width - (margin * 2);
            int availableHeight = grpMessages.Height - 40; // Space for title + margin

            // Two nested GroupBoxes side by side, each taking half the width
            int areaWidth = (availableWidth - 10) / 2; // 10px gap between areas

            // Left GroupBox (grpDebugLog)
            grpDebugLog.Location = new Point(margin, 30);
            grpDebugLog.Size = new Size(areaWidth, availableHeight);

            // Right GroupBox (grpCarStatus)
            grpCarStatus.Location = new Point(margin + areaWidth + 10, 30);
            grpCarStatus.Size = new Size(areaWidth, availableHeight);

            // Setup controls inside nested GroupBoxes
            SetupNestedGroupBoxBottomLeftControls();
        }

        private void SetupGroupBoxBottomRightControls()
        {
            int margin = 10;
            int availableWidth = grpMessages.Width - (margin * 2);
            int availableHeight = grpMessages.Height - 40; // Space for title + margin

            // Two nested GroupBoxes side by side, each taking half the width
            int areaWidth = (availableWidth - 10) / 2; // 10px gap between areas

            // Left GroupBox (grpCommands)
            grpCommands.Location = new Point(margin, 30);
            grpCommands.Size = new Size(areaWidth, availableHeight);

            // Right GroupBox (grpTaskHistory)
            grpTaskHistory.Location = new Point(margin + areaWidth + 10, 30);
            grpTaskHistory.Size = new Size(areaWidth, availableHeight);

            // Setup controls inside nested GroupBoxes
            SetupNestedGroupBoxBottomRightControls();
        }

        private void SetupNestedGroupBoxBottomLeftControls()
        {
            // Connection GroupBox controls
            btnClearDebugLog.Location = new Point(grpDebugLog.Width - 85, 0);
            txtDebugLog.Size = new Size(
                grpDebugLog.Width - 18,
                grpDebugLog.Height - 45
            );
            txtDebugLog.Location = new Point(8, 35);

            // Status GroupBox controls  
            btnClearCarStatus.Location = new Point(grpCarStatus.Width - 85, 0);
            txtCarStatus.Size = new Size(
                grpCarStatus.Width - 18,
                grpCarStatus.Height - 45
            );
            txtCarStatus.Location = new Point(8, 35);

        }

        private void SetupNestedGroupBoxBottomRightControls()
        {
            // Connection GroupBox controls
            txtCommands.Size = new Size(
                grpCommands.Width - 18,
                grpCommands.Height - 45
            );
            txtCommands.Location = new Point(8, 35);

            // Status GroupBox controls  
            btnClearTaskHistory.Location = new Point(grpTaskHistory.Width - 85, 0);
            txtTaskHistory.Size = new Size(
                grpTaskHistory.Width - 18,
                grpTaskHistory.Height - 45
            );
            txtTaskHistory.Location = new Point(8, 35);
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Communication and File Operations

        private void InitializeApp()
        {
            // Setup communicator
            communicator = new ZeroMQCommunicator();
            communicator.OnMessage += (msg) => UIHelper.AppendLog(txtDebugLog, msg);
            communicator.OnFileUpdate += OnFileUpdate;
            communicator.OnApiStatus += OnApiStatusUpdate;
            communicator.OnUserStatus += OnUserStatusUpdate;
            communicator.OnCarStatus += OnCarStatusUpdate;

            // Setup connection timer (only for connection health check)
            connectionTimer = new Timer();
            connectionTimer.Interval = 10000; // Check every 10 seconds
            connectionTimer.Tick += async (s, e) => await CheckConnection();

            UIHelper.AppendLog(txtDebugLog, "Uygulama başlatıldı.");

            // Load task history on startup
            LoadTaskHistory();
        }

        // Write the last command to task history log file
        private void SaveTaskToHistory(string jsonContent)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

                string formattedJson = FormatJsonForHistory(jsonContent);

                string logEntry = $"\n[{timestamp}]\n{formattedJson}\n{new string('-', 30)}\n";

                File.AppendAllText(TASK_HISTORY_FILE, logEntry, System.Text.Encoding.UTF8);

                LoadTaskHistory();

                UIHelper.AppendLog(txtDebugLog, "Gerçekleştirilen son görev görev geçmişine kaydedildi.");
            }
            catch (Exception ex)
            {
                UIHelper.AppendLog(txtDebugLog, $"Gerçekleştirilen son görev görev geçmişine kaydedilemedi: {ex.Message}");
            }
        }

        // Load the task history log file content to txtTaskHistory
        private void LoadTaskHistory()
        {
            try
            {
                if (File.Exists(TASK_HISTORY_FILE))
                {
                    string historyContent = File.ReadAllText(TASK_HISTORY_FILE, System.Text.Encoding.UTF8);

                    if (txtTaskHistory.InvokeRequired)
                    {
                        txtTaskHistory.Invoke(new Action(() =>
                        {
                            txtTaskHistory.Text = historyContent;
                            txtTaskHistory.SelectionStart = txtTaskHistory.Text.Length;
                            txtTaskHistory.ScrollToCaret();
                        }));
                    }
                    else
                    {
                        txtTaskHistory.Text = historyContent;
                        txtTaskHistory.SelectionStart = txtTaskHistory.Text.Length;
                        txtTaskHistory.ScrollToCaret();
                    }

                    UIHelper.AppendLog(txtDebugLog, "Görev geçmişi yüklendi.");
                }
                else
                {
                    if (txtTaskHistory.InvokeRequired)
                    {
                        txtTaskHistory.Invoke(new Action(() => txtTaskHistory.Text = "Henüz kaydedilmiş görev yok..."));
                    }
                    else
                    {
                        txtTaskHistory.Text = "Henüz kaydedilmiş görev yok...";
                    }
                }
            }
            catch (Exception ex)
            {
                UIHelper.AppendLog(txtDebugLog, $"Görev geçmişi yüklenemedi: {ex.Message}");
                if (txtTaskHistory.InvokeRequired)
                {
                    txtTaskHistory.Invoke(new Action(() => txtTaskHistory.Text = $"Görev geçmişi yüklenemedi: {ex.Message}"));
                }
                else
                {
                    txtTaskHistory.Text = $"Görev geçmişi yüklenemedi: {ex.Message}";
                }
            }
        }

        // Clear txtTaskHistory
        private void btnClearTaskHistory_Click(object sender, EventArgs e)
        {
            try
            {
                File.Delete(TASK_HISTORY_FILE);
                txtTaskHistory.Text = "Görev geçmişi temizlendi. Henüz kaydedilmiş görev yok...";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Görev geçmişi temizlenemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Format task history content display
        private string FormatJsonForHistory(string jsonContent)
        {
            try
            {
                // Parse and reformat JSON with proper indentation
                JArray commands = JArray.Parse(jsonContent);
                return JsonConvert.SerializeObject(commands, Formatting.Indented);
            }
            catch
            {
                // If parsing fails, return original content
                return jsonContent;
            }
        }

        // Start button click event
        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                await ConnectToPi();
            }
            else
            {
                DisconnectFromPi();
            }
        }

        private async Task ConnectToPi()
        {
            string ip = txtIPAddress.Text.Trim();

            if (!UIHelper.IsValidIP(ip))
            {
                MessageBox.Show("Lütfen geçerli bir IP adresi giriniz!", "Geçersiz IP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UIHelper.SetButtonState(btnStart, Color.GreenYellow, false, "...");
            UIHelper.SetLabelStatus(lblConnectionStatus, "Bağlantı kuruluyor...", Color.Orange);

            bool success = await communicator.Connect(ip);

            if (success)
            {
                isConnected = true;
                UIHelper.SetButtonState(btnStart, Color.Orange, true, "Durdur");
                UIHelper.SetLabelStatus(lblConnectionStatus, $"{ip} adresi ile bağlantı kuruldu.", Color.MediumSeaGreen);
                UIHelper.SetControlState(txtIPAddress, false);
                connectionTimer.Start();

                await StartApiService();
            }
            else
            {
                UIHelper.SetButtonState(btnStart, Color.MediumSeaGreen, true, "Başlat");
                UIHelper.SetLabelStatus(lblConnectionStatus, "Bağlantı kurulamadı", Color.Red);
                MessageBox.Show("Araç ile bağlantı kurulamadı!", "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task StartApiService()
        {
            try
            {
                UIHelper.SetLabelStatus(lblApiStatus, "Sistem başlatılıyor...", Color.Orange);
                UIHelper.SetLabelStatus(lblUserStatus, "Sistem başlatılıyor...", Color.Orange);

                string response = await communicator.SendStartCommand();
            }
            catch (Exception ex)
            {
                UIHelper.AppendLog(txtDebugLog, $"Sistem başlatılamadı: {ex.Message}");
                UIHelper.SetLabelStatus(lblApiStatus, "Sistem başlatılamadı.", Color.Red);
            }
        }

        private void DisconnectFromPi()
        {
            connectionTimer.Stop();
            if (isConnected)
            {
                try
                {
                    // Try to send disconnect command to stop all car processes
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var socket = new NetMQ.Sockets.RequestSocket();
                            socket.Connect($"tcp://{txtIPAddress.Text.Trim()}:5565");
                            socket.SendFrame("DISCONNECT");

                            if (socket.TryReceiveFrameString(TimeSpan.FromSeconds(2), out string response))
                            {
                                UIHelper.AppendLog(txtDebugLog, "Sistem durduruldu.");
                            }
                            socket.Dispose();
                        }
                        catch
                        {
                            // Ignore errors, disconnect anyway
                        }
                    });
                }
                catch
                {
                    // Ignore errors, disconnect anyway
                }
            }
            communicator.Disconnect();
            isConnected = false;

            UIHelper.SetButtonState(btnStart, Color.MediumSeaGreen, true, "Başlat");
            UIHelper.SetLabelStatus(lblConnectionStatus, "Bağlantı kesildi.", Color.Gray);
            UIHelper.SetLabelStatus(lblApiStatus, "Sistem kapatıldı.", Color.Gray);
            UIHelper.SetLabelStatus(lblUserStatus, "Sistem kapatıldı.", Color.Gray);
            UIHelper.SetControlState(txtIPAddress, true);

            ShowJsonContent("");
            ShowTxtContent("");
            UIHelper.AppendLog(txtDebugLog, "Araç bağlantısı kesildi.");
        }

        private async Task CheckConnection()
        {
            if (isConnected)
            {
                bool alive = await communicator.TestConnection();
                if (!alive)
                {
                    UIHelper.SetLabelStatus(lblConnectionStatus, "Bağlantı kesildi!", Color.Red);
                    DisconnectFromPi();
                }
            }
        }

        private void OnFileUpdate(string fileType, string content)
        {
            // This method is called when Pi sends file update notification
            try
            {
                if (fileType == "JSON")
                {
                    string formattedContent = FormatJsonCommands(content);
                    ShowJsonContent(formattedContent);

                    if (content != lastSavedJsonContent && !string.IsNullOrWhiteSpace(content) && !IsEmptyJsonArray(content))
                    {
                        SaveTaskToHistory(content);
                        lastSavedJsonContent = content;
                    }
                }
                else if (fileType == "TXT")
                {
                    ShowTxtContent(content);
                }
            }
            catch (Exception ex)
            {
                UIHelper.AppendLog(txtDebugLog, $"Yeni komutlar/komut metni alınamadı: {ex.Message}");
            }
        }

        private bool IsEmptyJsonArray(string jsonContent)
        {
            try
            {
                JArray commands = JArray.Parse(jsonContent);
                return commands.Count == 0;
            }
            catch
            {
                return false;
            }
        }

        private string FormatJsonCommands(string jsonContent)
        {
            try
            {
                JArray commands;

                // Try to parse the content as JSON
                try
                {
                    commands = JArray.Parse(jsonContent);
                }
                catch
                {
                    // If direct parsing fails, it might be wrapped in a result object
                    var wrapper = JObject.Parse(jsonContent);
                    if (wrapper["data"] != null)
                    {
                        commands = JArray.Parse(wrapper["data"].ToString());
                    }
                    else
                    {
                        throw new Exception("Hatalı komut formatı!");
                    }
                }

                string output = "";

                foreach (var item in commands)
                {
                    string komut = item["komut"]?.ToString();
                    string kosul = item["kosul"]?.ToString();

                    if (!string.IsNullOrEmpty(komut))
                        output += $"Komut: {komut}\r\n";

                    if (!string.IsNullOrEmpty(kosul))
                        output += $"Koşul: {kosul}\r\n";

                    output += "\r\n";
                }

                return string.IsNullOrEmpty(output) ? "Komut bulunamadı" : output;
            }
            catch (Exception ex)
            {
                // If all parsing fails, show both error and original content for debugging
                return $"JSON işlenemiyor: {ex.Message}\r\n\r\n--- Orijinal İçerik ---\r\n{jsonContent}";
            }
        }

        private void OnApiStatusUpdate(string status, string message)
        {
            // Handle API status updates
            try
            {
                Color apiStatusColor = Color.Gray;
                string apiMessage = message;
                bool userFlag = false;
                bool apiFlag = false;

                switch (status)
                {
                    case "READY":
                        apiStatusColor = Color.Green;
                        userFlag = true;
                        apiFlag = true;
                        break;
                    case "BUSY":
                        apiStatusColor = Color.Purple;
                        apiFlag = true;
                        break;
                    case "PROCESSING":
                        apiStatusColor = Color.Orange;
                        apiFlag = true;
                        break;
                    case "LISTENING":
                        apiStatusColor = Color.Blue;
                        apiFlag = true;
                        break;
                    case "USER_PROCESSING":
                        apiStatusColor = Color.Orange;
                        userFlag = true;
                        break;
                    case "USER_WAITING":
                        apiStatusColor = Color.Purple;
                        userFlag = true;
                        break;
                    case "ERROR":
                        apiStatusColor = Color.Red;
                        apiFlag = true;
                        break;
                    case "STOPPED":
                        apiStatusColor = Color.Gray;
                        userFlag = true;
                        apiFlag = true;
                        break;
                    default:
                        apiStatusColor = Color.Gray;
                        apiFlag = true;
                        break;
                }

                if (userFlag == true)
                {
                    UIHelper.SetLabelStatus(lblUserStatus, apiMessage, apiStatusColor);
                }
                if (apiFlag == true)
                {
                    UIHelper.SetLabelStatus(lblApiStatus, apiMessage, apiStatusColor);
                }
                if (status == "LOG")
                {
                    UIHelper.AppendLog(txtDebugLog, apiMessage);
                }
                
            }
            catch (Exception ex)
            {
                UIHelper.AppendLog(txtDebugLog, $"Sistem bildirimleri alınamadı: {ex.Message}");
            }
        }

        private void OnUserStatusUpdate(string user, bool recognized)
        {
            try
            {
                Color statusColor = Color.Gray;
                string displayMessage;

                if (recognized)
                {
                    statusColor = Color.MediumSeaGreen;
                    displayMessage = $"Kullanıcı doğrulandı: {user}";
                }
                else
                {
                    statusColor = Color.Red;
                    displayMessage = "Kullanıcı doğrulanamadı.";
                }

                UIHelper.SetLabelStatus(lblUserStatus, displayMessage, statusColor);
            }
            catch (Exception ex)
            {
                UIHelper.AppendLog(txtDebugLog, $"Sistem bildirimleri alınamadı: {ex.Message}");
            }
        }

        private void OnCarStatusUpdate(string action, string details, int speed, double distance, string timestamp)
        {
            try
            {
                string statusMessage = $"{action}";
                if (!string.IsNullOrEmpty(details))
                    statusMessage += $" - {details}";

                UIHelper.AppendLog(txtCarStatus, statusMessage);
            }
            catch (Exception ex)
            {
                UIHelper.AppendLog(txtDebugLog, $"Araç bildirimleri alınamadı: {ex.Message}");
            }
        }

        private void ShowJsonContent(string content)
        {
            if (txtCommands.InvokeRequired)
            {
                txtCommands.Invoke(new Action(() => txtCommands.Text = content));
            }
            else
            {
                txtCommands.Text = content;
            }
        }

        private void ShowTxtContent(string content)
        {
            if (txtCommandText.InvokeRequired)
            {
                txtCommandText.Invoke(new Action(() => txtCommandText.Text = content));
            }
            else
            {
                txtCommandText.Text = content;
            }
        }

        private void btnClearDebugLog_Click(object sender, EventArgs e)
        {
            txtDebugLog.Clear();
            UIHelper.AppendLog(txtDebugLog, "Sistem mesajları temizlendi.");
        }

        private void btnClearCarStatus_Click(object sender, EventArgs e)
        {
            txtCarStatus.Clear();
            UIHelper.AppendLog(txtCarStatus, "Araç mesajları temizlendi.");
        }

        private void txtIPAddress_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow digits, dots, and control characters
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            connectionTimer?.Stop();
            connectionTimer?.Dispose();
            communicator?.Dispose();
            Application.Exit();
        }
        #endregion
    }
}