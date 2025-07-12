using System;
using System.Drawing;
using System.Windows.Forms;

namespace CarControlApp
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            //
            // Component initialization
            //

            // Title and buttons
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();

            // Top Left GroupBox
            this.grpConnection = new System.Windows.Forms.GroupBox();
            this.lblIPAddress = new System.Windows.Forms.Label();
            this.txtIPAddress = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.lblConnectionTitle = new System.Windows.Forms.Label();
            this.lblConnectionStatus = new System.Windows.Forms.Label();
            this.lblApiTitle = new System.Windows.Forms.Label();
            this.lblApiStatus = new System.Windows.Forms.Label();
            this.lblUserTitle = new System.Windows.Forms.Label();
            this.lblUserStatus = new System.Windows.Forms.Label();

            // Top Right GroupBox
            this.grpCommandText = new System.Windows.Forms.GroupBox();
            this.txtCommandText = new System.Windows.Forms.TextBox();

            // Bottom Left GroupBox
            this.grpMessages = new System.Windows.Forms.GroupBox();
            this.grpDebugLog = new System.Windows.Forms.GroupBox();
            this.txtDebugLog = new System.Windows.Forms.TextBox();
            this.btnClearDebugLog = new System.Windows.Forms.Button();
            this.grpCarStatus = new System.Windows.Forms.GroupBox();
            this.txtCarStatus = new System.Windows.Forms.TextBox();
            this.btnClearCarStatus = new System.Windows.Forms.Button();

            // Bottom Right GroupBox
            this.grpCarTasks = new System.Windows.Forms.GroupBox();
            this.grpCommands = new System.Windows.Forms.GroupBox();
            this.txtCommands = new System.Windows.Forms.TextBox();
            this.grpTaskHistory = new System.Windows.Forms.GroupBox();
            this.txtTaskHistory = new System.Windows.Forms.TextBox();
            this.btnClearTaskHistory = new System.Windows.Forms.Button();

            //
            // MainForm
            //

            this.Name = "MainForm";
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.MintCream;
            this.SuspendLayout();
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.grpConnection);
            this.Controls.Add(this.grpCommandText);
            this.Controls.Add(this.grpMessages);
            this.Controls.Add(this.grpCarTasks);
            this.ResumeLayout(false);

            //
            // Title and Buttons
            //

            // lblTitle
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "Araç Kontrol Paneli";
            this.lblTitle.AutoSize = false;
            this.lblTitle.BackColor = Color.Transparent;
            this.lblTitle.Font = new Font("Montserrat", 24F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.MidnightBlue;
            this.lblTitle.Size = new Size(600, 60);
            this.lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            this.lblTitle.Location = new Point(20, 20);

            // btnExit
            this.btnExit.Name = "btnExit";
            this.btnExit.Text = "Kapat";
            this.btnExit.BackColor = Color.DarkRed;
            this.btnExit.ForeColor = Color.MintCream;
            this.btnExit.Font = new Font("Montserrat", 12F, FontStyle.Bold);
            this.btnExit.Size = new Size(100, 40);
            this.btnExit.FlatStyle = FlatStyle.Flat;
            this.btnExit.Click += new EventHandler(this.ExitButton_Click);

            //
            // Top Left GroupBox
            //

            // grpConnection (Top Left)
            this.grpConnection.Name = "grpConnection";
            this.grpConnection.Text = "Sistem Takibi";
            this.grpConnection.BackColor = Color.MintCream;
            this.grpConnection.ForeColor = Color.MidnightBlue;
            this.grpConnection.Font = new Font("Montserrat", 15F, FontStyle.Bold);
            this.grpConnection.FlatStyle = FlatStyle.Flat;
            this.grpConnection.Controls.Add(this.txtIPAddress);
            this.grpConnection.Controls.Add(this.btnStart);
            this.grpConnection.Controls.Add(this.lblConnectionTitle);
            this.grpConnection.Controls.Add(this.lblConnectionStatus);
            this.grpConnection.Controls.Add(this.lblApiTitle);
            this.grpConnection.Controls.Add(this.lblApiStatus);
            this.grpConnection.Controls.Add(this.lblUserTitle);
            this.grpConnection.Controls.Add(this.lblUserStatus);
            this.grpConnection.Controls.Add(this.lblIPAddress);
            this.grpConnection.SuspendLayout();

            // lblIPAddress
            this.lblIPAddress.Name = "lblIPAddress";
            this.lblIPAddress.Text = "IP Adresi:";
            this.lblIPAddress.BackColor = Color.Transparent;
            this.lblIPAddress.ForeColor = Color.DarkRed;
            this.lblIPAddress.Font = new Font("Montserrat", 12F, FontStyle.Bold);
            this.lblIPAddress.TextAlign = ContentAlignment.MiddleLeft;
            this.lblIPAddress.AutoSize = false;
            this.lblIPAddress.Size = new Size(250, 25);
            this.lblIPAddress.Location = new Point(10, 38);

            // txtIPAddress
            this.txtIPAddress.Name = "txtIPAddress";
            this.txtIPAddress.BackColor = Color.MidnightBlue;
            this.txtIPAddress.ForeColor = Color.MintCream;
            this.txtIPAddress.Font = new Font("Consolas", 15F);
            this.txtIPAddress.Text = "192.168.45.7";
            this.txtIPAddress.KeyPress += new KeyPressEventHandler(this.txtIPAddress_KeyPress);

            // btnStart
            this.btnStart.Name = "btnStart";
            this.btnStart.Text = "Başlat";
            this.btnStart.BackColor = Color.MediumSeaGreen;
            this.btnStart.ForeColor = Color.MintCream;
            this.btnStart.Font = new Font("Montserrat", 10F, FontStyle.Bold);
            this.btnStart.Size = new Size(100, 32);
            this.btnStart.FlatStyle = FlatStyle.Flat;
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new EventHandler(this.btnStart_Click);

            // Status Labels - Row 1
            this.lblConnectionTitle.Name = "lblConnectionTitle";
            this.lblConnectionTitle.Text = "Bağlantı:";
            this.lblConnectionTitle.BackColor = Color.Transparent;
            this.lblConnectionTitle.ForeColor = Color.DarkRed;
            this.lblConnectionTitle.Font = new Font("Montserrat", 12F, FontStyle.Bold);
            this.lblConnectionTitle.TextAlign = ContentAlignment.MiddleLeft;
            this.lblConnectionTitle.AutoSize = false;
            this.lblConnectionTitle.Size = new Size(120, 25);
            this.lblConnectionTitle.Location = new Point(10, 85);

            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Text = "Bağlantının kurulması bekleniyor...";
            this.lblConnectionStatus.BackColor = Color.Transparent;
            this.lblConnectionStatus.ForeColor = Color.DimGray;
            this.lblConnectionStatus.Font = new Font("Montserrat", 12F, FontStyle.Bold);
            this.lblConnectionStatus.TextAlign = ContentAlignment.MiddleLeft;
            this.lblConnectionStatus.AutoSize = true;
            this.lblConnectionStatus.Location = new Point(138, 85);

            // Status Labels - Row 2
            this.lblApiTitle.Name = "lblApiTitle";
            this.lblApiTitle.Text = "Sistem:";
            this.lblApiTitle.BackColor = Color.Transparent;
            this.lblApiTitle.ForeColor = Color.DarkRed;
            this.lblApiTitle.Font = new Font("Montserrat", 12F, FontStyle.Bold);
            this.lblApiTitle.TextAlign = ContentAlignment.MiddleLeft;
            this.lblApiTitle.AutoSize = false;
            this.lblApiTitle.Size = new Size(120, 25);
            this.lblApiTitle.Location = new Point(10, 175);

            this.lblApiStatus.Name = "lblApiStatus";
            this.lblApiStatus.Text = "Sistemin başlatılması bekleniyor...";
            this.lblApiStatus.BackColor = Color.Transparent;
            this.lblApiStatus.ForeColor = Color.DimGray;
            this.lblApiStatus.Font = new Font("Montserrat", 12F, FontStyle.Bold);
            this.lblApiStatus.TextAlign = ContentAlignment.MiddleLeft;
            this.lblApiStatus.AutoSize = true;
            this.lblApiStatus.Location = new Point(138, 175);

            // Status Labels - Row 3
            this.lblUserTitle.Name = "lblUserTitle";
            this.lblUserTitle.Text = "Kullanıcı:";
            this.lblUserTitle.BackColor = Color.Transparent;
            this.lblUserTitle.ForeColor = Color.DarkRed;
            this.lblUserTitle.Font = new Font("Montserrat", 12F, FontStyle.Bold);
            this.lblUserTitle.TextAlign = ContentAlignment.MiddleLeft;
            this.lblUserTitle.AutoSize = false;
            this.lblUserTitle.Size = new Size(120, 25);
            this.lblUserTitle.Location = new Point(10, 130);

            this.lblUserStatus.Name = "lblUserStatus";
            this.lblUserStatus.Text = "Sistemin başlatılması bekleniyor...";
            this.lblUserStatus.BackColor = Color.Transparent;
            this.lblUserStatus.ForeColor = Color.DimGray;
            this.lblUserStatus.Font = new Font("Montserrat", 12F, FontStyle.Bold);
            this.lblUserStatus.TextAlign = ContentAlignment.MiddleLeft;
            this.lblUserStatus.AutoSize = true;
            this.lblUserStatus.Location = new Point(138, 130);

            //
            // Top Right GroupBox
            //

            // grpCommandText (Top Right)
            this.grpCommandText.Name = "grpCommandText";
            this.grpCommandText.Text = "Komut Metni";
            this.grpCommandText.BackColor = Color.MintCream;
            this.grpCommandText.ForeColor = Color.MidnightBlue;
            this.grpCommandText.Font = new Font("Montserrat", 15F, FontStyle.Bold);
            this.grpCommandText.FlatStyle = FlatStyle.Flat;
            this.grpCommandText.Controls.Add(this.txtCommandText);
            this.grpCommandText.SuspendLayout();

            // txtCommandText
            this.txtCommandText.Name = "txtCommandText";
            this.txtCommandText.BackColor = Color.MidnightBlue;
            this.txtCommandText.ForeColor = Color.MintCream;
            this.txtCommandText.Font = new Font("Consolas", 12F);
            this.txtCommandText.Multiline = true;
            this.txtCommandText.ReadOnly = true;
            this.txtCommandText.ScrollBars = ScrollBars.Vertical;
            this.txtCommandText.Location = new Point(10, 25);
            this.txtCommandText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            //
            // Bottom Left GroupBox
            //

            // grpMessages (Bottom Left)
            this.grpMessages.Name = "grpMessages";
            this.grpMessages.BackColor = Color.MintCream;
            this.grpMessages.ForeColor = Color.MidnightBlue;
            this.grpMessages.Font = new Font("Montserrat", 15F, FontStyle.Bold);
            this.grpMessages.FlatStyle = FlatStyle.Flat;
            this.grpMessages.Controls.Add(this.grpDebugLog);
            this.grpMessages.Controls.Add(this.grpCarStatus);
            this.grpMessages.SuspendLayout();

            // grpDebugLog (Nested inside grpMessages - Left side)
            this.grpDebugLog.Name = "grpDebugLog";
            this.grpDebugLog.Text = "Sistem Mesajları";
            this.grpDebugLog.BackColor = Color.MintCream;
            this.grpDebugLog.ForeColor = Color.MidnightBlue;
            this.grpDebugLog.Font = new Font("Montserrat", 15F, FontStyle.Bold);
            this.grpDebugLog.FlatStyle = FlatStyle.Flat;
            this.grpDebugLog.Controls.Add(this.txtDebugLog);
            this.grpDebugLog.Controls.Add(this.btnClearDebugLog);
            this.grpDebugLog.SuspendLayout();

            // txtDebugLog
            this.txtDebugLog.Name = "txtDebugLog";
            this.txtDebugLog.BackColor = Color.MidnightBlue;
            this.txtDebugLog.ForeColor = Color.MintCream;
            this.txtDebugLog.Font = new Font("Consolas", 12F);
            this.txtDebugLog.Multiline = true;
            this.txtDebugLog.ScrollBars = ScrollBars.Vertical;
            this.txtDebugLog.ReadOnly = true;
            this.txtDebugLog.Location = new Point(5, 35);
            this.txtDebugLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // btnClearDebugLog
            this.btnClearDebugLog.Name = "btnClearDebugLog";
            this.btnClearDebugLog.Text = "Temizle";
            this.btnClearDebugLog.BackColor = Color.DarkOrange;
            this.btnClearDebugLog.ForeColor = Color.MintCream;
            this.btnClearDebugLog.Size = new Size(80, 30);
            this.btnClearDebugLog.Font = new Font("Montserrat", 8F, FontStyle.Bold);
            this.btnClearDebugLog.FlatStyle = FlatStyle.Flat;
            this.btnClearDebugLog.UseVisualStyleBackColor = false;
            this.btnClearDebugLog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnClearDebugLog.Click += new EventHandler(this.btnClearDebugLog_Click);

            // grpCarStatus (Nested inside grpMessages - Right side)
            this.grpCarStatus.Name = "grpCarStatus";
            this.grpCarStatus.Text = "Araç Mesajları";
            this.grpCarStatus.BackColor = Color.MintCream;
            this.grpCarStatus.ForeColor = Color.MidnightBlue;
            this.grpCarStatus.Font = new Font("Montserrat", 15F, FontStyle.Bold);
            this.grpCarStatus.FlatStyle = FlatStyle.Flat;
            this.grpCarStatus.Controls.Add(this.txtCarStatus);
            this.grpCarStatus.Controls.Add(this.btnClearCarStatus);
            this.grpCarStatus.SuspendLayout();

            // txtCarStatus
            this.txtCarStatus.Name = "txtCarStatus";
            this.txtCarStatus.BackColor = Color.MidnightBlue;
            this.txtCarStatus.ForeColor = Color.MintCream;
            this.txtCarStatus.Font = new Font("Consolas", 12F);
            this.txtCarStatus.Multiline = true;
            this.txtCarStatus.ScrollBars = ScrollBars.Vertical;
            this.txtCarStatus.ReadOnly = true;
            this.txtCarStatus.Location = new Point(5, 35);
            this.txtCarStatus.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // btnClearCarStatus
            this.btnClearCarStatus.Name = "btnClearCarStatus";
            this.btnClearCarStatus.Text = "Temizle";
            this.btnClearCarStatus.BackColor = Color.DarkOrange;
            this.btnClearCarStatus.ForeColor = Color.MintCream;
            this.btnClearCarStatus.Size = new Size(80, 30);
            this.btnClearCarStatus.Font = new Font("Montserrat", 8F, FontStyle.Bold);
            this.btnClearCarStatus.FlatStyle = FlatStyle.Flat;
            this.btnClearCarStatus.UseVisualStyleBackColor = false;
            this.btnClearCarStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnClearCarStatus.Click += new EventHandler(this.btnClearCarStatus_Click);

            //
            // Bottom Right GroupBox
            //

            // grpCarTasks (Bottom Right)
            this.grpCarTasks.Name = "grpCarTasks";
            this.grpCarTasks.BackColor = Color.MintCream;
            this.grpCarTasks.ForeColor = Color.MidnightBlue;
            this.grpCarTasks.Font = new Font("Montserrat", 15F, FontStyle.Bold);
            this.grpCarTasks.FlatStyle = FlatStyle.Flat;
            this.grpCarTasks.Controls.Add(this.grpCommands);
            this.grpCarTasks.Controls.Add(this.grpTaskHistory);
            this.grpCarTasks.SuspendLayout();

            // grpCommands (Nested inside grpMessages - Left side)
            this.grpCommands.Name = "grpCommands";
            this.grpCommands.Text = "Komutlar";
            this.grpCommands.BackColor = Color.MintCream;
            this.grpCommands.ForeColor = Color.MidnightBlue;
            this.grpCommands.Font = new Font("Montserrat", 15F, FontStyle.Bold);
            this.grpCommands.FlatStyle = FlatStyle.Flat;
            this.grpCommands.Controls.Add(this.txtCommands);
            this.grpCommands.SuspendLayout();

            // txtCommands
            this.txtCommands.Name = "txtCommands";
            this.txtCommands.BackColor = Color.MidnightBlue;
            this.txtCommands.ForeColor = Color.MintCream;
            this.txtCommands.Font = new Font("Consolas", 12F);
            this.txtCommands.Multiline = true;
            this.txtCommands.ScrollBars = ScrollBars.Vertical;
            this.txtCommands.ReadOnly = true;
            this.txtCommands.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // grpTaskHistory (Nested inside grpMessages - Right side)
            this.grpTaskHistory.Name = "grpTaskHistory";
            this.grpTaskHistory.Text = "Görev Geçmişi";
            this.grpTaskHistory.BackColor = Color.MintCream;
            this.grpTaskHistory.ForeColor = Color.MidnightBlue;
            this.grpTaskHistory.Font = new Font("Montserrat", 15F, FontStyle.Bold);
            this.grpTaskHistory.FlatStyle = FlatStyle.Flat;
            this.grpTaskHistory.Controls.Add(this.txtTaskHistory);
            this.grpTaskHistory.Controls.Add(this.btnClearTaskHistory);
            this.grpTaskHistory.SuspendLayout();

            // txtTaskHistory
            this.txtTaskHistory.Name = "txtTaskHistory";
            this.txtTaskHistory.BackColor = Color.MidnightBlue;
            this.txtTaskHistory.ForeColor = Color.MintCream;
            this.txtTaskHistory.Font = new Font("Consolas", 12F);
            this.txtTaskHistory.Multiline = true;
            this.txtTaskHistory.ScrollBars = ScrollBars.Vertical;
            this.txtTaskHistory.ReadOnly = true;
            this.txtTaskHistory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // btnClearTaskHistory
            this.btnClearTaskHistory.Name = "btnClearTaskHistory";
            this.btnClearTaskHistory.Text = "Temizle";
            this.btnClearTaskHistory.BackColor = Color.DarkOrange;
            this.btnClearTaskHistory.ForeColor = Color.MintCream;
            this.btnClearTaskHistory.Size = new Size(80, 30);
            this.btnClearTaskHistory.Font = new Font("Montserrat", 8F, FontStyle.Bold);
            this.btnClearTaskHistory.FlatStyle = FlatStyle.Flat;
            this.btnClearTaskHistory.UseVisualStyleBackColor = false;
            this.btnClearTaskHistory.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnClearTaskHistory.Click += new EventHandler(this.btnClearTaskHistory_Click);
        }

        #endregion

        private System.Windows.Forms.GroupBox grpConnection;
        private System.Windows.Forms.GroupBox grpCommandText;
        private System.Windows.Forms.GroupBox grpMessages;
        private System.Windows.Forms.GroupBox grpCarTasks;
        private System.Windows.Forms.GroupBox grpTaskHistory;
        private System.Windows.Forms.GroupBox grpCommands;
        private System.Windows.Forms.GroupBox grpDebugLog;
        private System.Windows.Forms.GroupBox grpCarStatus;
        private System.Windows.Forms.TextBox txtCommandText;
        private System.Windows.Forms.TextBox txtCommands;
        private System.Windows.Forms.TextBox txtTaskHistory;
        private System.Windows.Forms.TextBox txtDebugLog;
        private System.Windows.Forms.TextBox txtCarStatus;
        private System.Windows.Forms.TextBox txtIPAddress;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnClearDebugLog;
        private System.Windows.Forms.Button btnClearCarStatus;
        private System.Windows.Forms.Button btnClearTaskHistory;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblConnectionTitle;
        private System.Windows.Forms.Label lblConnectionStatus;
        private System.Windows.Forms.Label lblApiTitle;
        private System.Windows.Forms.Label lblApiStatus;
        private System.Windows.Forms.Label lblUserTitle;
        private System.Windows.Forms.Label lblUserStatus;
        private System.Windows.Forms.Label lblIPAddress;
    }
}