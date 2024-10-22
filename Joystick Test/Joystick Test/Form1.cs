using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Joystick_Test
{
    public partial class Form1 : Form
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private Joystick joystick = new Joystick();

        private Label[] StatusText;
        private Label[] ButtonText;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            logger.Debug("Joystick test start.");

            this.StatusText = new Label[] { lbl_Status1, lbl_Status2, lbl_Status3, lbl_Status4 };
            this.ButtonText = new Label[] { lbl_Button1, lbl_Button2, lbl_Button3, lbl_Button4 };


            // set event
            joystick.OnConnected += new Joystick.ConnectedEventHandler(JoystickConnected);
            joystick.OnDisconnected += new Joystick.DisconnectedEventHandler(JoystickDisconnected);
            joystick.OnButtonDown += new Joystick.ButtonDownHandler(JoystickButtonDown);

            // start monitoring
            joystick.StartMonitoring();
        }


        private void JoystickButtonDown(object sender, int index, ushort button)
        {

            try
            {
                this.Invoke(new Action(() =>
                {
                    this.ButtonText[index].Text = joystick.ButtonName(button);
                }));

            }
            catch (Exception)
            {

            }
        }

        private void JoystickConnected(object sender, int index)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    this.StatusText[index].Text = "Connected";
                }));

            }
            catch (Exception)
            {

            }
        }

        private void JoystickDisconnected(object sender, int index)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    this.StatusText[index].Text = "Disconnected";
                }));

            }
            catch (Exception)
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

    }
}
