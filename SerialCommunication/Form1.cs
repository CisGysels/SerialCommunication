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

        public Form1()
        {
            InitializeComponent();

            // Initialize serial port object and set default timeouts (ms)
            serialPortArduino = new SerialPort();
            serialPortArduino.ReadTimeout = 1000;
            serialPortArduino.WriteTimeout = 1000;
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
    }
}
