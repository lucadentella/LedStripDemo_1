using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LedStripController
{
    public partial class Form1 : Form
    {
        private static int INITIAL_PANEL_NUM = 12;
        private static string VERSION = "1.0";
        
        private List<Panel> ledPanels;
        private SerialPort serialPort;
        private Boolean connected = false;

        public Form1()
        {
            InitializeComponent();
            detectSerialPorts();
            createInitialPanels();
        }

        private void detectSerialPorts()
        {
            int portsNum = 0;
            foreach (string serialPortName in SerialPort.GetPortNames())
            {
                portsNum++;
                ToolStripMenuItem menuItem = new ToolStripMenuItem(serialPortName);
                menuItem.CheckOnClick = true;
                menuItem.Click += serialPortMenu_Click;
                serialPortToolStripMenuItem.DropDownItems.Add(menuItem);
            }
        }

        private void createInitialPanels()
        {
            ledPanels = new List<Panel>();

            for (int i = 0; i < INITIAL_PANEL_NUM; i++)
            {
                Panel panel = getNewPanel();
                ledPanelContainer.Controls.Add(panel);
                ledPanels.Add(panel);
            }

            panelNum.Value = INITIAL_PANEL_NUM;
        }

        private Panel getNewPanel()
        {
            Panel newPanel = new Panel();
            newPanel.Size = new Size(30, 30);
            newPanel.BackColor = Color.White;
            newPanel.BorderStyle = BorderStyle.FixedSingle;
            newPanel.MouseClick += ledPanel_Click;
            return newPanel;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void ledPanel_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            DialogResult result = colorDialog.ShowDialog();
            if (result == DialogResult.OK) ((Panel)sender).BackColor = colorDialog.Color;
        }

        private void serialPortMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem senderMenuItem = (ToolStripMenuItem)sender;

            // the serial port was selected
            if (senderMenuItem.Checked)
            {
                // if a connection is active, close it (was on a different port)
                if (connected) 
                {
                    serialPort.Close();
                    serialPort.Dispose();
                }

                // try to open a new connection
                serialPort = new SerialPort(senderMenuItem.Text, 57600);
                serialPort.Open();
                connected = true;
                btSend.Enabled = true;
                btOFF.Enabled = true;
            }

            // the serial port was unchecked
            else
                if (connected)
                {
                    serialPort.Close();
                    serialPort.Dispose();
                    connected = false;
                    btSend.Enabled = false;
                    btOFF.Enabled = true;
                }
        }

        private void panelNum_ValueChanged(object sender, EventArgs e)
        {
            int diff = (int)panelNum.Value - ledPanels.Count;
            
            // If the new number is greater than the number of panels,
            // add new ledPanels
            if (diff > 0)
                for (int i = 0; i < diff; i++)
                {
                    Panel panel = getNewPanel();
                    ledPanelContainer.Controls.Add(panel);
                    ledPanels.Add(panel);
                }

            // else remove the panels
            else
                for (int i = 0; i > diff; i--)
                {
                    // get the latest panel
                    Panel panel = ledPanels.ElementAt<Panel>(ledPanels.Count - 1);
                    
                    // remove it from the container and the list
                    ledPanelContainer.Controls.Remove(panel);
                    ledPanels.Remove(panel);                   
                }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("LedStripController " + VERSION + "\n\nLuca Dentella", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void btSend_Click(object sender, EventArgs e)
        {
            byte[] out_buffer = new byte[(ledPanels.Count * 3) + 1];
            int buffer_index = 0;

            for (int i = 0; i < ledPanels.Count; i++)
            {
                Panel panel = ledPanels.ElementAt<Panel>(i);
                Color panelColor = panel.BackColor;
                out_buffer[buffer_index++] = panelColor.R;
                out_buffer[buffer_index++] = panelColor.G;
                out_buffer[buffer_index++] = panelColor.B;
            }
           
            out_buffer[buffer_index++] = 0x0A;
            serialPort.Write(out_buffer, 0, buffer_index);

        }

        private void btOFF_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ledPanels.Count; i++)
            {
                Panel panel = ledPanels.ElementAt<Panel>(i);
                panel.BackColor = Color.Black;
                btSend.PerformClick();
            }
        }
    }
}
