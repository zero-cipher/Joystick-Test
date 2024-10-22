using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Joystick_Test
{
    class Joystick
    {


        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);




        public const int ERROR_DEVICE_NOT_CONNECTED = 1167;



        /// <summary>
        /// User index definitions
        /// </summary>
        public const int XUSER_MAX_COUNT = 4;         // XInput API는 최대 4개의 Controller 연결을 지원한다.
        public const int XUSER_INDEX_ANY = 0x000000FF;



        private int[] PrevStatus = new int[XUSER_MAX_COUNT];            // -1 : initialize, 0 : disconnect, 1 : connect
        private int[] PrevPacketNumbers = new int[XUSER_MAX_COUNT];
        private UInt16[] PrevButton = new UInt16[XUSER_MAX_COUNT];


        /// <summary>
        /// Device types available in XINPUT_CAPABILITIES
        /// Device subtypes available in XINPUT_CAPABILITIES
        /// </summary>
        struct XInputDeviceType
        {
            public static int GAMEPAD = 0x01;
            public static int WHEEL = 0x02;
            public static int ARCADE_STICK = 0x03;
            public static int FLIGHT_SICK = 0x04;
            public static int DANCE_PAD = 0x05;
            public static int GUITAR = 0x06;
            public static int DRUM_KIT = 0x08;
        }

        /// <summary>
        /// Constants for gamepad buttons
        /// </summary>
        struct XInputButton
        {
            public static UInt16 UP = 0x0001;
            public static UInt16 DOWN = 0x0002;
            public static UInt16 LEFT = 0x0004;
            public static UInt16 RIGHT = 0x0008;
            public static UInt16 START = 0x0010;
            public static UInt16 BACK = 0x0020;
            public static UInt16 LEFT_THUMB = 0x0040;
            public static UInt16 RIGHT_THUMB = 0x0080;
            public static UInt16 LEFT_SHOULDER = 0x0100;
            public static UInt16 RIGHT_SHOULDER = 0x0200;
            public static UInt16 A = 0x1000;
            public static UInt16 B = 0x2000;
            public static UInt16 X = 0x4000;
            public static UInt16 Y = 0x8000;
        }

        /// <summary>
        /// Gamepad thresholds
        /// </summary>
        struct XInputThresholds
        {
            public static int LEFT_THUMB_DEADZONE = 7849;
            public static int RIGHT_THUMB_DEADZONE = 8689;
            public static int TRIGGER_THRESHOLD = 30;
        }

        /// <summary>
        /// Codes returned for the gamepad keystroke
        /// </summary>
        struct XInputKeyStroke
        {
            public static int VK_PAD_A = 0x5800;
            public static int VK_PAD_B = 0x5801;
            public static int VK_PAD_X = 0x5802;
            public static int VK_PAD_Y = 0x5803;
            public static int VK_PAD_RSHOULDER = 0x5804;
            public static int VK_PAD_LSHOULDER = 0x5805;
            public static int VK_PAD_LTRIGGER = 0x5806;
            public static int VK_PAD_RTRIGGER = 0x5807;

            public static int VK_PAD_DPAD_UP = 0x5810;
            public static int VK_PAD_DPAD_DOWN = 0x5811;
            public static int VK_PAD_DPAD_LEFT = 0x5812;
            public static int VK_PAD_DPAD_RIGHT = 0x5813;
            public static int VK_PAD_START = 0x5814;
            public static int VK_PAD_BACK = 0x5815;
            public static int VK_PAD_LTHUMB_PRESS = 0x5816;
            public static int VK_PAD_RTHUMB_PRESS = 0x5817;

            public static int VK_PAD_LTHUMB_UP = 0x5820;
            public static int VK_PAD_LTHUMB_DOWN = 0x5821;
            public static int VK_PAD_LTHUMB_RIGHT = 0x5822;
            public static int VK_PAD_LTHUMB_LEFT = 0x5823;
            public static int VK_PAD_LTHUMB_UPLEFT = 0x5824;
            public static int VK_PAD_LTHUMB_UPRIGHT = 0x5825;
            public static int VK_PAD_LTHUMB_DOWNRIGHT = 0x5826;
            public static int VK_PAD_LTHUMB_DOWNLEFT = 0x5827;

            public static int VK_PAD_RTHUMB_UP = 0x5830;
            public static int VK_PAD_RTHUMB_DOWN = 0x5831;
            public static int VK_PAD_RTHUMB_RIGHT = 0x5832;
            public static int VK_PAD_RTHUMB_LEFT = 0x5833;
            public static int VK_PAD_RTHUMB_UPLEFT = 0x5834;
            public static int VK_PAD_RTHUMB_UPRIGHT = 0x5835;
            public static int VK_PAD_RTHUMB_DOWNRIGHT = 0x5836;
            public static int VK_PAD_RTHUMB_DOWNLEFT = 0x5837;
        }

        /// <summary>
        /// Flags used in XINPUT_KEYSTROKE
        /// </summary>
        struct XInputKeyStrokeFlag
        {
            public static int XINPUT_KEYSTROKE_KEYDOWN = 0x0001;
            public static int XINPUT_KEYSTROKE_KEYUP = 0x0002;
            public static int XINPUT_KEYSTROKE_REPEAT = 0x0004;
        }


        /// <summary>
        /// Structures used by XInput APIs
        /// </summary>
        #region XINPUT_API_STRUCTURE

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_GAMEPAD
        {
            public UInt16 wButtons;
            public byte bLeftTrigger;
            public byte bRightTrigger;
            public Int16 sThumbLX;
            public Int16 sThumbLY;
            public Int16 sThumbRX;
            public Int16 sThumbRY;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_STATE
        {
            public UInt32 dwPacketNumber;
            public XINPUT_GAMEPAD Gamepad;
        }

        public struct XINPUT_VIBRATION
        {
            public UInt16 wLeftMotorSpeed;
            public UInt16 wRightMotorSpeed;
        }

        public struct XINPUT_CAPABILITIES
        {
            public byte Type;
            public byte SubType;
            public Int16 Flags;
            public XINPUT_GAMEPAD Gamepad;
            public XINPUT_VIBRATION Vibration;
        }

        public struct XINPUT_KEYSTROKE
        {
            public Int16 VirtualKey;
            public Int16 Unicode;
            public Int16 Flags;
            public byte UserIndex;
            public byte HidCode;
        }
        #endregion


        /// <summary>
        /// XInput APIs
        /// </summary>
        #region XINPUT_APIS

        [DllImport("XInput1_4.dll")]
        public static extern int XInputGetState(int dwUserIndex, ref XINPUT_STATE pState);


        [DllImport("XInput1_4.dll")]
        public static extern int XInputSetState(int dwUserIndex, XINPUT_VIBRATION pVibration);

        [DllImport("XInput1_4.dll")]
        public static extern int XInputGetCapabilities(int dwUserIndex, int dwFlags, XINPUT_CAPABILITIES pCapabilities);

        [DllImport("XInput1_4.dll")]
        public static extern int XInputGetCapabilities(int dwUserIndex, int dwFlags, ref XINPUT_CAPABILITIES pCapabilities);

        [DllImport("XInput1_4.dll")]
        public static extern int XInputEnable(bool enable);

        [DllImport("XInput1_4.dll")]
        public static extern int XInputGetKeystroke(int dwUserIndex, int dwReserved, ref XINPUT_KEYSTROKE pKeystroke);

        #endregion





        private Thread thread = null;



        public Joystick()
        {
            for (int i = 0; i < XUSER_MAX_COUNT; i++)
            {
                this.PrevStatus[i] = -1;
            }
        }




        public void StartMonitoring()
        {
            thread = new Thread(GamepadMonitor);
            thread.IsBackground = true;

            thread.Start();
        }




        private void GamepadMonitor()
        {

            while(true)
            {

                for(int i = 0; i < XUSER_MAX_COUNT; i++)
                {
                    CheckingGamepad(i);
                }



                Thread.Sleep(1);
            }
        }


        private void CheckingGamepad(int index)
        {

            XINPUT_STATE state = new XINPUT_STATE();
            int result = XInputGetState(index, ref state);
            if (result == 0)
            {
                // connected event
                if (this.PrevStatus[index] != 1)
                {
                    logger.Debug(String.Format("[{0}] device not connected.", index));

                    if (OnConnected != null)
                        OnConnected(this, index);

                    this.PrevStatus[index] = 1;
                }



                logger.Debug("index = " + index + ", packet number = " + state.dwPacketNumber);
                if (this.PrevPacketNumbers[index] != state.dwPacketNumber)
                {
                    logger.Debug(String.Format("[{0}] Gamepad state changed.", index));


                    if (IsButtonDown(index, state.Gamepad.wButtons, XInputButton.UP))
                    {
                        if (OnButtonDown != null)
                            OnButtonDown(this, index, XInputButton.UP);
                    }
                    if (IsButtonDown(index, state.Gamepad.wButtons, XInputButton.DOWN))
                    {
                        if (OnButtonDown != null)
                            OnButtonDown(this, index, XInputButton.DOWN);
                    }
                    if (IsButtonDown(index, state.Gamepad.wButtons, XInputButton.LEFT))
                    {
                        if (OnButtonDown != null)
                            OnButtonDown(this, index, XInputButton.LEFT);
                    }
                    if (IsButtonDown(index, state.Gamepad.wButtons, XInputButton.RIGHT))
                    {
                        if (OnButtonDown != null)
                            OnButtonDown(this, index, XInputButton.RIGHT);
                    }
                    if (IsButtonDown(index, state.Gamepad.wButtons, XInputButton.START))
                    {
                        if (OnButtonDown != null)
                            OnButtonDown(this, index, XInputButton.START);
                    }
                    if (IsButtonDown(index, state.Gamepad.wButtons, XInputButton.BACK))
                    {
                        if (OnButtonDown != null)
                            OnButtonDown(this, index, XInputButton.BACK);
                    }
                    if (IsButtonDown(index, state.Gamepad.wButtons, XInputButton.LEFT_THUMB))
                    {
                        if (OnButtonDown != null)
                            OnButtonDown(this, index, XInputButton.LEFT_THUMB);
                    }
                    if (IsButtonDown(index, state.Gamepad.wButtons, XInputButton.RIGHT_THUMB))
                    {
                        if (OnButtonDown != null)
                            OnButtonDown(this, index, XInputButton.RIGHT_THUMB);
                    }
                    if (IsButtonDown(index, state.Gamepad.wButtons, XInputButton.LEFT_SHOULDER))
                    {
                        if (OnButtonDown != null)
                            OnButtonDown(this, index, XInputButton.LEFT_SHOULDER);
                    }
                    if (IsButtonDown(index, state.Gamepad.wButtons, XInputButton.RIGHT_SHOULDER))
                    {
                        if (OnButtonDown != null)
                            OnButtonDown(this, index, XInputButton.RIGHT_SHOULDER);
                    }
                    if (IsButtonDown(index, state.Gamepad.wButtons, XInputButton.A))
                    {
                        if (OnButtonDown != null)
                            OnButtonDown(this, index, XInputButton.A);
                    }
                    if (IsButtonDown(index, state.Gamepad.wButtons, XInputButton.B))
                    {
                        if (OnButtonDown != null)
                            OnButtonDown(this, index, XInputButton.B);
                    }
                    if (IsButtonDown(index, state.Gamepad.wButtons, XInputButton.X))
                    {
                        if (OnButtonDown != null)
                            OnButtonDown(this, index, XInputButton.X);
                    }
                    if (IsButtonDown(index, state.Gamepad.wButtons, XInputButton.Y))
                    {
                        if (OnButtonDown != null)
                            OnButtonDown(this, index, XInputButton.Y);
                    }
                }
            }
            else
            {
                if (result == ERROR_DEVICE_NOT_CONNECTED)
                {
                    // disconnected event
                    if (this.PrevStatus[index] != 0)
                    {
                        logger.Debug(String.Format("[{0}] device not connected.", index));

                        if (OnDisconnected != null)
                            OnDisconnected(this, index);

                        this.PrevStatus[index] = 0;
                    }
                }
                else
                {
                    logger.Debug(String.Format("[{0}] XInputGetState error returned. error code = 0x{1}.", index, result.ToString("X8")));
                }
            }
        }


        private bool IsButtonDown(int index, UInt16 buttonStatus, UInt16 button)
        {
            if ((buttonStatus & button) == button)
            {
                if ((this.PrevButton[index] & button) == 0)
                {
                    logger.Debug(String.Format("[{0}] {1} button down.", index, ButtonName(button)));
                    return true;
                }
            }

            return false;
        }



        public string ButtonName(UInt16 button)
        {
            string result = "";

            if (button == XInputButton.UP)
                result = "Up";
            else if (button == XInputButton.DOWN)
                result = "Down";
            else if (button == XInputButton.LEFT)
                result = "Left";
            else if (button == XInputButton.RIGHT)
                result = "Right";
            else if (button == XInputButton.START)
                result = "Start";
            else if (button == XInputButton.BACK)
                result = "Back";
            else if (button == XInputButton.LEFT_THUMB)
                result = "Left Thumb";
            else if (button == XInputButton.RIGHT_THUMB)
                result = "Right Thumb";
            else if (button == XInputButton.LEFT_SHOULDER)
                result = "Left Shoulder";
            else if (button == XInputButton.RIGHT_SHOULDER)
                result = "Right Shoulder";
            else if (button == XInputButton.A)
                result = "A";
            else if (button == XInputButton.B)
                result = "B";
            else if (button == XInputButton.X)
                result = "X";
            else if (button == XInputButton.Y)
                result = "Y";
            else
                result = "Unknown";

            return result;
        }

        public delegate void ConnectedEventHandler(object sender, int index);
        public ConnectedEventHandler OnConnected;

        public delegate void DisconnectedEventHandler(object sender, int index);
        public DisconnectedEventHandler OnDisconnected;

        public delegate void ButtonDownHandler(object sender, int index, UInt16 button);
        public ButtonDownHandler OnButtonDown;


    }
}
