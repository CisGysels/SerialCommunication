using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace SerialCommunication
{
    public partial class Form1 : Form
    {
        private SerialPort serialPortArduino;
        private System.Windows.Forms.Timer timerOefening4;

        public Form1()
        {
            InitializeComponent();

            // Initialize serial port object and set default timeouts (ms)
            serialPortArduino = new SerialPort();
            serialPortArduino.ReadTimeout = 1000;
            serialPortArduino.WriteTimeout = 1000;

            // Timer for Oefening4 (interval 1000 ms)
            timerOefening4 = new System.Windows.Forms.Timer();
            timerOefening4.Interval = 1000; // 1000 ms
            timerOefening4.Tick += timerOefening4_Tick;

            // Wire tab selection to start/stop timer
            tabControl.SelectedIndexChanged += tabControl_SelectedIndexChanged;

            // If the tab is already selected at startup, start timer
            if (tabControl.SelectedTab == tabPageOefening4)
            {
                timerOefening4.Start();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string[] portNames = SerialPort.GetPortNames().Distinct().ToArray();
                comboBoxPoort.Items.Clear();
                comboBoxPoort.Items.AddRange(portNames);
                if (comboBoxPoort.Items.Count > 0) comboBoxPoort.SelectedIndex = 0;

                comboBoxBaudrate.SelectedIndex = comboBoxBaudrate.Items.IndexOf("115200");
            }
            catch (Exception)
            { }
        }

        private void cboPoort_DropDown(object sender, EventArgs e)
        {
            try
            {
                string selected = (string)comboBoxPoort.SelectedItem;
                string[] portNames = SerialPort.GetPortNames().Distinct().ToArray();

                comboBoxPoort.Items.Clear();
                comboBoxPoort.Items.AddRange(portNames);

                comboBoxPoort.SelectedIndex = comboBoxPoort.Items.IndexOf(selected);
            }
            catch (Exception)
            {
                if (comboBoxPoort.Items.Count > 0) comboBoxPoort.SelectedIndex = 0;
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino == null)
                {
                    serialPortArduino = new SerialPort();
                    serialPortArduino.ReadTimeout = 1000;
                    serialPortArduino.WriteTimeout = 1000;
                }

                if (!serialPortArduino.IsOpen)
                {
                    // Read UI values
                    string port = comboBoxPoort.SelectedItem as string;
                    if (string.IsNullOrEmpty(port))
                    {
                        MessageBox.Show("Selecteer een poort.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    int baud = 115200;
                    if (comboBoxBaudrate.SelectedItem != null)
                    {
                        int.TryParse(comboBoxBaudrate.SelectedItem.ToString(), out baud);
                    }

                    serialPortArduino.PortName = port;
                    serialPortArduino.BaudRate = baud;

                    // DataBits
                    try
                    {
                        serialPortArduino.DataBits = (int)numericUpDownDatabits.Value;
                    }
                    catch { }

                    // Parity
                    if (radioButtonParityNone.Checked) serialPortArduino.Parity = Parity.None;
                    else if (radioButtonParityEven.Checked) serialPortArduino.Parity = Parity.Even;
                    else if (radioButtonParityOdd.Checked) serialPortArduino.Parity = Parity.Odd;
                    else if (radioButtonParityMark.Checked) serialPortArduino.Parity = Parity.Mark;
                    else if (radioButtonParitySpace.Checked) serialPortArduino.Parity = Parity.Space;

                    // StopBits
                    if (radioButtonStopbitsOne.Checked) serialPortArduino.StopBits = StopBits.One;
                    else if (radioButtonStopbitsOnePointFive.Checked) serialPortArduino.StopBits = StopBits.OnePointFive;
                    else if (radioButtonStopbitsTwo.Checked) serialPortArduino.StopBits = StopBits.Two;
                    else if (radioButtonStopbitsNone.Checked) serialPortArduino.StopBits = StopBits.None;

                    // Handshake
                    if (radioButtonHandshakeNone.Checked) serialPortArduino.Handshake = Handshake.None;
                    else if (radioButtonHandshakeRTS.Checked) serialPortArduino.Handshake = Handshake.RequestToSend;
                    else if (radioButtonHandshakeRTSXonXoff.Checked) serialPortArduino.Handshake = Handshake.RequestToSendXOnXOff;
                    else if (radioButtonHandshakeXonXoff.Checked) serialPortArduino.Handshake = Handshake.XOnXOff;

                    // RTS / DTR
                    serialPortArduino.RtsEnable = checkBoxRtsEnable.Checked;
                    serialPortArduino.DtrEnable = checkBoxDtrEnable.Checked;

                    try
                    {
                        serialPortArduino.Open();
                    }
                    catch (Exception openEx)
                    {
                        MessageBox.Show("Kan poort niet openen: " + openEx.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Ping-pong verification
                    try
                    {
                        serialPortArduino.DiscardInBuffer();
                        serialPortArduino.DiscardOutBuffer();

                        serialPortArduino.WriteLine("ping");
                        string resp = serialPortArduino.ReadLine();

                        if (!string.IsNullOrEmpty(resp) && resp.Trim().Equals("pong", StringComparison.OrdinalIgnoreCase))
                        {
                            buttonConnect.Text = "Disconnect";
                            radioButtonVerbonden.Checked = true;
                            if (labelStatus != null) labelStatus.Text = "Connected";
                        }
                        else
                        {
                            MessageBox.Show("Onverwacht antwoord van apparaat: '" + resp + "'", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            try { serialPortArduino.Close(); } catch { }
                            buttonConnect.Text = "Connect";
                            radioButtonVerbonden.Checked = false;
                            if (labelStatus != null) labelStatus.Text = "Disconnected";
                        }
                    }
                    catch (TimeoutException)
                    {
                        MessageBox.Show("Geen antwoord op ping binnen timeout.", "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        try { serialPortArduino.Close(); } catch { }
                        buttonConnect.Text = "Connect";
                        radioButtonVerbonden.Checked = false;
                        if (labelStatus != null) labelStatus.Text = "Disconnected";
                    }
                }
                else
                {
                    try
                    {
                        serialPortArduino.Close();
                    }
                    catch (Exception closeEx)
                    {
                        MessageBox.Show("Fout bij sluiten van poort: " + closeEx.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    buttonConnect.Text = "Connect";
                    radioButtonVerbonden.Checked = false;
                    if (labelStatus != null) labelStatus.Text = "Disconnected";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout met seriële poort: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkBoxDigital2_Checked(object sender, EventArgs e)
        {
            try
            {
                if (!serialPortArduino.IsOpen)
                {
                    MessageBox.Show("Seriële verbinding is niet geopend.", "Fout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    checkBoxDigital2.Checked = false;
                    return;
                }

                string command = checkBoxDigital2.Checked ? "set d2 high" : "set d2 low";
                serialPortArduino.WriteLine(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij versturen van commando: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                checkBoxDigital2.Checked = false;
            }
        }

        private void checkBoxDigital3_Checked(object sender, EventArgs e)
        {
            try
            {
                if (!serialPortArduino.IsOpen)
                {
                    MessageBox.Show("Seriële verbinding is niet geopend.", "Fout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    checkBoxDigital3.Checked = false;
                    return;
                }

                string command = checkBoxDigital3.Checked ? "set d3 high" : "set d3 low";
                serialPortArduino.WriteLine(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij versturen van commando: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                checkBoxDigital3.Checked = false;
            }
        }

        private void checkBoxDigital4_Checked(object sender, EventArgs e)
        {
            try
            {
                if (!serialPortArduino.IsOpen)
                {
                    MessageBox.Show("Seriële verbinding is niet geopend.", "Fout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    checkBoxDigital4.Checked = false;
                    return;
                }

                string command = checkBoxDigital4.Checked ? "set d4 high" : "set d4 low";
                serialPortArduino.WriteLine(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij versturen van commando: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                checkBoxDigital4.Checked = false;
            }
        }

        private void trackBarPWM9_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (!serialPortArduino.IsOpen)
                {
                    MessageBox.Show("Seriële verbinding is niet geopend.", "Fout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int pwmValue = trackBarPWM9.Value;
                string command = $"set pwm9 {pwmValue}";
                serialPortArduino.WriteLine(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij versturen van PWM9 commando: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void trackBarPWM10_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (!serialPortArduino.IsOpen)
                {
                    MessageBox.Show("Seriële verbinding is niet geopend.", "Fout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int pwmValue = trackBarPWM10.Value;
                string command = $"set pwm10 {pwmValue}";
                serialPortArduino.WriteLine(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij versturen van PWM10 commando: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void trackBarPWM11_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (!serialPortArduino.IsOpen)
                {
                    MessageBox.Show("Seriële verbinding is niet geopend.", "Fout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int pwmValue = trackBarPWM11.Value;
                string command = $"set pwm11 {pwmValue}";
                serialPortArduino.WriteLine(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij versturen van PWM11 commando: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // timer for Oefening3
                if (tabControl.SelectedTab == tabPageOefening3)
                {
                    timerOefening3.Enabled = true;
                }
                else
                {
                    timerOefening3.Enabled = false;
                }

                // timer for Oefening4
                if (tabControl.SelectedTab == tabPageOefening4)
                {
                    timerOefening4?.Start();
                }
                else
                {
                    timerOefening4?.Stop();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij tab selectie: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void timerOefening3_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!serialPortArduino.IsOpen)
                {
                    return;
                }

                // Verwijder alle voorgaande antwoorden van de Arduino
                serialPortArduino.ReadExisting();

                // Query digital5
                serialPortArduino.WriteLine("get d5");
                string responseD5 = serialPortArduino.ReadLine();
                if (!string.IsNullOrEmpty(responseD5))
                {
                    responseD5 = responseD5.Trim();
                    // Antwoord formaat: "d5: 0" of "d5: 1"
                    if (responseD5.Contains(":"))
                    {
                        string value = responseD5.Split(':')[1].Trim();
                        radioButtonDigital5.Checked = (value == "1");
                    }
                }

                // Query digital6
                serialPortArduino.WriteLine("get d6");
                string responseD6 = serialPortArduino.ReadLine();
                if (!string.IsNullOrEmpty(responseD6))
                {
                    responseD6 = responseD6.Trim();
                    if (responseD6.Contains(":"))
                    {
                        string value = responseD6.Split(':')[1].Trim();
                        radioButtonDigital6.Checked = (value == "1");
                    }
                }

                // Query digital7
                serialPortArduino.WriteLine("get d7");
                string responseD7 = serialPortArduino.ReadLine();
                if (!string.IsNullOrEmpty(responseD7))
                {
                    responseD7 = responseD7.Trim();
                    if (responseD7.Contains(":"))
                    {
                        string value = responseD7.Split(':')[1].Trim();
                        radioButtonDigital7.Checked = (value == "1");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij lezen van digitale ingangen: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void timerOefening4_Tick(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino == null || !serialPortArduino.IsOpen)
                {
                    return;
                }

                // Remove any previous incoming data
                try { serialPortArduino.ReadExisting(); } catch { }

                // Request analog0 value
                serialPortArduino.WriteLine("get a0");

                string resp = null;
                try
                {
                    resp = serialPortArduino.ReadLine();
                }
                catch (TimeoutException) { resp = null; }
                catch (Exception) { resp = null; }

                if (string.IsNullOrEmpty(resp))
                {
                    return;
                }

                // Trim to the last token (likely the numeric value)
                string value = resp.Trim();
                var tokens = value.Split(new char[] { ' ', '\t', ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length > 0) value = tokens[tokens.Length - 1].Trim();

                // Update UI thread-safely
                if (labelAnalog0.InvokeRequired)
                {
                    labelAnalog0.BeginInvoke(new Action(() => labelAnalog0.Text = value));
                }
                else
                {
                    labelAnalog0.Text = value;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in timerOefening4_Tick: " + ex.Message);
            }
        }
    }
}
