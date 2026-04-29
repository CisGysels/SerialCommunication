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
                    // Other properties (parity, databits, stopbits, handshake, etc.) will be set elsewhere in code.

                    serialPortArduino.Open();

                    buttonConnect.Text = "Disconnect";
                    radioButtonVerbonden.Checked = true;
                    if (labelStatus != null) labelStatus.Text = "Connected";
                }
                else
                {
                    serialPortArduino.Close();

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
