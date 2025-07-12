using System;
using System.Drawing;
using System.Windows.Forms;

namespace CarControlApp.Utilities
{
    public class UIHelper
    {
        // Check if the IP address is valid
        public static bool IsValidIP(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return false;

            string[] parts = ip.Split('.');
            if (parts.Length != 4) return false;

            foreach (string part in parts)
            {
                if (!int.TryParse(part, out int num) || num < 0 || num > 255)
                    return false;
            }
            return true;
        }

        // Enabled/Disabled desired component
        public static void SetControlState(Control control, bool enabled, string text = null)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new Action(() => SetControlState(control, enabled, text)));
                return;
            }

            control.Enabled = enabled;
            if (text != null) control.Text = text;
        }

        // Control buttons
        public static void SetButtonState(Control button, Color color, bool enabled, string text = null)
        {
            if (button.InvokeRequired)
            {
                button.Invoke(new Action(() => SetButtonState(button, color, enabled, text)));
                return;
            }

            button.Enabled = enabled;
            if (text != null) button.Text = text;
            button.BackColor = color;
        }

        // Control labels
        public static void SetLabelStatus(Label label, string text, Color color)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() => SetLabelStatus(label, text, color)));
                return;
            }

            label.Text = text;
            label.ForeColor = color;
        }

        // Write to desired textboxes
        public static void AppendLog(TextBox textBox, string message)
        {
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action(() => AppendLog(textBox, message)));
                return;
            }

            textBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}" + "\r\n");
            textBox.SelectionStart = textBox.Text.Length;
            textBox.ScrollToCaret();
        }
    }
}
