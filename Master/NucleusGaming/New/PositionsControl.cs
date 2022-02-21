using Nucleus.Gaming;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Properties;
using SharpDX.DirectInput;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Nucleus.Coop
{
    public class PositionsControl : UserInputControl, IDynamicSized
    {
        private static PositionsControl _positionsControl;

        private bool canProceed;

        //private bool keyboardPlayer = false;

        // array of users's screens
        private UserScreen[] screens;

        // the factor to scale all screens to fit them inside the edit area
        private float scale;

        // the total bounds of all the connected monitors together
        private Rectangle totalBounds;

        private Font playerFont;
        private Font playerCustomFont;
        private Font smallTextFont;
        private Font playerTextFont;
        private Font extraSmallTextFont;

        private RectangleF screensArea;
        private RectangleF playersArea;
        private int playerSize;

        private bool dragging = false;
        private int draggingIndex = -1;
        private Point draggingOffset;
        private Point mousePos;
        private int draggingScreen = -1;
        private Rectangle draggingScreenRec;
        private Rectangle draggingScreenBounds;

        private Image gamepadImg;
        private Image genericImg;
        private Image keyboardImg;
        private Image rawKeyboardImg;
        private Image rawMouseImg;

        public bool isDisconnected;
        private int dinputPressed = -1;
        private float fontMinus;
        private float fontScale;
        private float newplayerFontSize ;
        private float newplayerCustomFontSize;
        private float newplayerTextFontSize; 
        private float newsmallTextFontSize ;
        private float newextraSmallTextFontSize;
        private int instruction_click = 0;
        private readonly IniFile ini = new IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));

        private ImageAttributes flashImageAttributes;

        private bool insideGamepadTick = false;
        public override bool CanProceed => canProceed;
        public override string Title => "Position Players";
        public override bool CanPlay => false;

        // dinput
        //private DirectInput dinput;
        //private List<Joystick> dinputJoysticks;

        // xinput
        // private List<OpenXinputController> xinputControllers;

        //private Timer gamepadTimer;
        //private Timer gamepadPollTimer;
        private System.Threading.Timer gamepadTimer;
        private System.Threading.Timer gamepadPollTimer;
        //private System.Threading.Thread pollThread;

        private int gamePadPressed = -1;

        private int testDinputPlayers = -1;// 16;
        private int testXinputPlayers = -1;// 16;
        //private TextBox instructionText;
        private PictureBox instruction;
        private PictureBox instructionImg;

        public PositionsControl()
        {
            PositionsControl._positionsControl = this;

            Initialize();
        }

        private void Initialize()
        {
            string ChoosenTheme = ini.IniReadValue("Theme", "Theme");
            IniFile theme = new IniFile(Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\theme\\" + ChoosenTheme, "theme.ini"));

            if (gamepadTimer == null)
            {
                gamepadTimer = new System.Threading.Timer(GamepadTimer_Tick, null, 0, 1000);
            }

            if (gamepadPollTimer == null)
            {
                gamepadPollTimer = new System.Threading.Timer(GamepadPollTimer_Tick, null, 0, 1001);
            }
            
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom| AnchorStyles.Left| AnchorStyles.Right;
            playerFont = new Font("Franklin Gothic Medium", 20.0f, FontStyle.Regular, GraphicsUnit.Point, 0);
            playerCustomFont = new Font("Franklin Gothic Medium", 16.0f, FontStyle.Bold, GraphicsUnit.Point, 0);
            playerTextFont = new Font("Franklin Gothic Medium", 12.0f, FontStyle.Regular, GraphicsUnit.Point, 0);
            smallTextFont = new Font("Franklin Gothic Medium", 12.0f, FontStyle.Regular, GraphicsUnit.Point, 0);
            extraSmallTextFont = new Font("Franklin Gothic Medium", 10.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
            BackColor = Color.Transparent;
            BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"gui\Theme\" + ChoosenTheme + "\\setup_screen.png"));
            BackgroundImageLayout = ImageLayout.Stretch;

            if (ini.IniReadValue("Advanced", "Font") != "")
            {
                float fontSize = 12F;
                fontSize = float.Parse(ini.IniReadValue("Advanced", "Font"));
                playerFont = new Font("Segoe UI", fontSize);
                playerCustomFont = new Font("Segoe UI", fontSize);
                playerTextFont = new Font("Segoe UI", fontSize);
                smallTextFont = new Font("Segoe UI", fontSize);
                extraSmallTextFont = new Font("Segoe UI", fontSize);

            }

            instruction = new PictureBox();//using a button cause focus issues
            instruction.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            instruction.Size = new Size(25,25);
            instruction.Location = new Point((Width-instruction.Width)-5,5);
            instruction.BackColor = Color.Transparent;
            instruction.ForeColor = Color.White;
            instruction.BackgroundImage = Resources.instruction;
            instruction.BackgroundImageLayout = ImageLayout.Stretch;
            instruction.Click += new EventHandler(this.instruction_Click);
 
            instructionImg = new PictureBox()
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right ,
                BackColor = Color.Black,
                Image = Resources.instructions,
                BackgroundImageLayout = ImageLayout.Stretch,
                Cursor = Cursors.Default,
                 //Size\Location  see => UpdateScreens() 
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false

            };
           
            this.Controls.Add(instruction);
            this.Controls.Add(instructionImg);
            gamepadImg = Resources.gamepad;
            genericImg = Resources.generic;
            keyboardImg = Resources.keyboard;
            rawMouseImg = Resources.mouse;
            rawKeyboardImg = Resources.rawkeyboard;

            //Flash image attributes
            {
                ColorMatrix colorMatrix = new ColorMatrix(new[]
                {
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 1, 0},
                    new float[] {0.4f, 0, 0, 0, 1}
                });

                flashImageAttributes = new ImageAttributes();
                flashImageAttributes.SetColorMatrix(colorMatrix);
            }
            DPIManager.Register(this);         
            DPIManager.Update(this);
            RemoveFlicker();
        }

        
        private void instruction_Click(object sender, EventArgs e)
        {          
            instruction_click++;
            instruction.BackgroundImage = Resources.instruction_close;
            instructionImg.Visible = true;

            if (instruction_click == 2)
            { 
                instructionImg.Visible = false;
                instruction.BackgroundImage = Resources.instruction;
                instruction_click = 0;
            }  
        }
        private bool appStart = false;
        public void UpdateSize(float scale)
        {
          
            if (IsDisposed)
            {
                DPIManager.Unregister(this);
                return;
            }
            if (!appStart)
            {
                if (fontScale > 1.0F || fontScale < 2.0F)
                {
                    fontMinus = 0.3f;
                }
                if (fontScale >= 2.0F)
                {
                    fontMinus = 0.8f;
                }

                newplayerFontSize = (playerFont.Size - fontMinus);
                newplayerCustomFontSize = (playerCustomFont.Size - fontMinus);
                newplayerTextFontSize = (playerTextFont.Size - fontMinus);
                newsmallTextFontSize = (smallTextFont.Size - fontMinus);
                newextraSmallTextFontSize = (extraSmallTextFont.Size - fontMinus);

                playerFont = new Font("Franklin Gothic Medium", newplayerFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);
                playerCustomFont = new Font("Franklin Gothic Medium", newplayerCustomFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);
                playerTextFont = new Font("Franklin Gothic Medium", newplayerTextFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);
                smallTextFont = new Font("Franklin Gothic Medium", newsmallTextFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);
                extraSmallTextFont = new Font("Franklin Gothic Medium", newextraSmallTextFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);
                float instructionW = instruction.Width * scale;
                float instructionH = instruction.Height * scale;
                instruction.Width = (int)instructionW;
                instruction.Height = (int)instructionH;
                instruction.Location = new Point((Width - instruction.Width) - 5, 5);
                appStart = true;
            }
        }
        public static void InvalidateFlash()
        {
            _positionsControl?.Invalidate();
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            //if (dinput != null)
            //{
            //    dinput.Dispose();
            //    dinput = null;
            //}

            if (profile != null && profile.PlayerData != null)
            {
                List<PlayerInfo> data = profile.PlayerData;

                foreach (PlayerInfo player in data)
                {
                    if (player.DInputJoystick != null)
                    {
                        player.DInputJoystick.Dispose();
                    }
                }
            }
        }

        public override void Ended()
        {
            base.Ended();

            List<PlayerInfo> data = profile.PlayerData;
            foreach (PlayerInfo player in data)
            {
                if (player.DInputJoystick != null)
                {
                    player.DInputJoystick.Dispose();
                }
            }

            //if (dinput != null)
            //{
            //    dinput.Dispose();
            //    dinput = null;
            //}

            //gamepadTimer.Enabled = false;
            //gamepadPollTimer.Enabled = false;
            gamepadTimer.Dispose();
            gamepadPollTimer.Dispose();
        }


        private void GamepadPollTimer_Tick(object state)/* object sender, EventArgs e)*/
        {
            gamePadPressed = -1;
            try
            {
                List<PlayerInfo> data = profile.PlayerData;
                foreach (PlayerInfo player in data)
                {
                    if (!player.IsKeyboardPlayer && !player.IsRawKeyboard && !player.IsRawMouse)
                    {
                        Invoke(new Action(() => PollGamepad(player)));
                    }
                }
            }
            catch (Exception)
            {
                //UpdatePlayers();
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(UpdatePlayers));
                    Invoke(new MethodInvoker(Refresh));
                    return;
                }
                gamePadPressed = -1;
            }

        }

        private void PollGamepad(PlayerInfo player)
        {

            gamePadPressed = -1;

            try
            {
                if (player.DInputJoystick.IsDisposed)
                {
                    //player.DInputJoystick.Dispose();
                    return;
                }

                //if (player.DInputJoystick.Poll().IsFailure)
                //{
                //    //player.DInputJoystick.Dispose();
                //    return;
                //}

                JoystickState state = player.DInputJoystick.GetCurrentState();

                bool[] buttonsPressed = state.Buttons;

                bool btnPressed = false;
                for (int b = 0; b < buttonsPressed.Length; b++)
                {
                    if (buttonsPressed[b])
                    {
                        btnPressed = true;
                        if (player.IsDInput)
                        {
                            dinputPressed = player.GamepadId;
                        }
                        gamePadPressed = player.GamepadId;
                        Refresh();
                        break;
                    }
                }
                if (player.IsDInput && !btnPressed && dinputPressed == player.GamepadId)
                {
                    dinputPressed = -1;
                    Invalidate();
                }
            }
            catch (Exception)
            {
                UpdatePlayers();
                Refresh();
                gamePadPressed = -1;
            }
        }


        private void GamepadTimer_Tick(object state)/* object sender, EventArgs e)*/
        {
            if (insideGamepadTick)
            {
                return;
            }

            insideGamepadTick = true;


            gamePadPressed = -1;
            if (profile == null)
            {
                insideGamepadTick = false;
                return;
            }

            List<PlayerInfo> data = profile.PlayerData;
            //List<string> instanceIds = new List<string>();
            bool changed = false;

            GenericGameInfo g = game.Game;

            DirectInput dinput = new DirectInput();

            // Using OpenXinput with more than 4 players means we can use more than 4 xinput controllers

            if (g.Hook.DInputEnabled || g.Hook.XInputReroute || g.ProtoInput.DinputDeviceHook)
            {

                IList<DeviceInstance> devices = dinput.GetDevices(DeviceClass.GameControl /*SlimDX.DirectInput.DeviceType.Gamepad*/, DeviceEnumerationFlags.AllDevices);

                // first search for disconnected gamepads
                for (int j = 0; j < data.Count; j++)
                {
                    PlayerInfo p = data[j];
                    if (!p.IsDInput || p.IsFake)
                    {
                        continue;
                    }

                    //if (!p.DInputJoystick.Acquire().IsFailure)
                    //{
                    //    if (!p.DInputJoystick.Poll().IsFailure)
                    //    {
                    //        JoystickState state = p.DInputJoystick.GetCurrentState();
                    //    }
                    //}

                    bool foundGamepad = false;
                    for (int i = 0; i < devices.Count; i++)
                    {
                        //DeviceInstance device = devices[i];
                        if (devices[i].InstanceGuid == p.GamepadGuid && i == data[j].GamepadId)
                        {
                            foundGamepad = true;
                            break;
                        }
                    }

                    if (!foundGamepad)
                    {
                        data[j].DInputJoystick.Unacquire();
                        changed = true;
                        data.RemoveAt(j);
                        j--;
                        isDisconnected = true;
                    }
                }

                for (int i = 0; i < devices.Count; i++)
                {
                    //DeviceInstance device = devices[i];
                    bool already = false;

                    // see if this gamepad is already on a player
                    for (int j = 0; j < data.Count; j++)
                    {
                        PlayerInfo p = data[j];
                        if (p.GamepadGuid == devices[i].InstanceGuid)
                        {
                            already = true;
                            break;
                        }
                    }

                    if (already)
                    {
                        continue;
                    }

                    changed = true;

                    // new gamepad
                    PlayerInfo player = new PlayerInfo
                    {
                        DInputJoystick = new Joystick(dinput, devices[i].InstanceGuid)
                    };

                    if (player.DInputJoystick.Properties.InterfacePath.ToUpper().Contains("IG_") && !g.Hook.XInputReroute && g.Hook.XInputEnabled)
                    {
                        continue;
                    }
                    player.GamepadProductGuid = devices[i].ProductGuid;
                    player.GamepadGuid = devices[i].InstanceGuid;
                    //instanceIds.Add(device.InstanceGuid.ToString());
                    player.GamepadName = devices[i].InstanceName;
                    player.IsDInput = true;
                    player.GamepadId = i;
                    string hid = player.DInputJoystick.Properties.InterfacePath;
                    player.RawHID = hid;
                    int start = hid.IndexOf("hid#");
                    int end = hid.LastIndexOf("#{");
                    string fhid = hid.Substring(start, end - start).Replace('#', '\\').ToUpper();
                    player.HIDDeviceID = fhid;
                    //if (ini.IniReadValue("ControllerMapping", "Player_"+ (i + 1)) != "")
                    //{
                    //    player.Nickname = ini.IniReadValue("ControllerMapping", "Player_" + (i + 1));
                    //}
                    player.DInputJoystick.Acquire();

                    data.Add(player);
                }

            }

            if ((g.Hook.XInputEnabled && !g.Hook.XInputReroute && !g.ProtoInput.DinputDeviceHook) || g.ProtoInput.XinputHook)
            {
                // XInput is only really enabled inside Nucleus Coop when
                // we have 4 or less players, else we need to force DirectInput to grab everything

                // foreach (var xinput in xinputControllers)
                //  xinput.openXinput = g.ProtoInput.UseOpenXinput;

                for (int j = 0; j < data.Count; j++)
                {
                    PlayerInfo p = data[j];
                    if (p.IsXInput && !p.IsFake)
                    {
                        // var c = xinputControllers[p.GamepadId];
                        OpenXinputController c = new OpenXinputController(g.ProtoInput.UseOpenXinput, p.GamepadId);
                        if (!c.IsConnected)
                        {
                            data[j].DInputJoystick.Unacquire();
                            changed = true;
                            data.RemoveAt(j);
                            j--;
                            isDisconnected = true;
                        }
                    }
                }

                IList<DeviceInstance> devices = dinput.GetDevices(
                SharpDX.DirectInput.DeviceType.Gamepad /*SlimDX.DirectInput.DeviceType.Gamepad*//*DeviceClass.GameController*/, DeviceEnumerationFlags.AttachedOnly);

                int cOffset = 0;
                int numControllers = g.ProtoInput.UseOpenXinput ? 32 : 4;
                for (int i = 0; i < numControllers; i++)
                {
                    OpenXinputController c = new OpenXinputController(g.ProtoInput.UseOpenXinput, i);

                    bool already = false;

                    if (!c.IsConnected)
                    {
                        cOffset++;
                    }
                    else
                    {
                        // see if this gamepad is already on a player
                        foreach (PlayerInfo p in data)
                        {
                            if (p.IsXInput && p.GamepadId == i)
                            {
                                State s = c.GetState();
                                int newmask = (int)s.Gamepad.Buttons;
                                if (p.GamepadMask != newmask)
                                {
                                    changed = true;
                                    p.GamepadMask = newmask;
                                }


                                already = true;
                                break;
                            }
                        }

                        if (already)
                        {
                            continue;
                        }

                        changed = true;

                        PlayerInfo player = new PlayerInfo();

                        for (int x = 0; x < devices.Count; x++)
                        {
                            DeviceInstance device = devices[x];
                            //if(!instanceIds.Contains(device.InstanceGuid.ToString()))
                            if ((x + cOffset) == i)
                            {
                                //instanceIds.Add(device.InstanceGuid.ToString());
                                player.GamepadGuid = device.InstanceGuid;
                                player.GamepadProductGuid = device.ProductGuid;
                                player.GamepadName = device.InstanceName;
                                player.DInputJoystick = new Joystick(dinput, device.InstanceGuid);
                                string hid = player.DInputJoystick.Properties.InterfacePath;
                                player.RawHID = hid;
                                int start = hid.IndexOf("hid#");
                                int end = hid.LastIndexOf("#{");
                                string fhid = hid.Substring(start, end - start).Replace('#', '\\').ToUpper();
                                player.HIDDeviceID = fhid;
                                //if (ini.IniReadValue("ControllerMapping", "Player_" + (i + 1)) != "")
                                //{
                                //    player.Nickname = ini.IniReadValue("ControllerMapping", "Player_" + (i + 1));
                                //}

                                player.DInputJoystick.Acquire();

                                //dinput.Dispose();
                                break;
                            }
                        }

                        // new gamepad
                        player.IsXInput = true;
                        player.GamepadId = i;
                        data.Add(player);
                    }
                }
            }

            if (changed)
            {
                //UpdatePlayers();
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(UpdatePlayers));
                    Invoke(new MethodInvoker(Refresh));
                    insideGamepadTick = false;
                    return;
                }
            }

            dinput?.Dispose();
            insideGamepadTick = false;
        }

        private void AddPlayer(int i, float playerWidth, float playerHeight, float offset)
        {
            Rectangle r = RectangleUtil.Float(50 + ((playerWidth + offset) * i), 100, playerWidth, playerHeight);
            PlayerInfo player = new PlayerInfo
            {
                EditBounds = r
            };
            profile.PlayerData.Add(player);
        }

       // private bool setupPrimary = false;
        private void UpdateScreens()
        {          
            if (screens == null)
            {
                screens = ScreensUtil.AllScreens();
                totalBounds = RectangleUtil.Union(screens);
            }
            else
            {
                UserScreen[] newScreens = ScreensUtil.AllScreens();
                Rectangle newBounds = RectangleUtil.Union(newScreens);
                if (newBounds.Equals(totalBounds))
                {
                    return;
                }

                //Display[] scr = ScreensUtil.AllScreensParams();
              
                // screens got updated, need to reflect in our window
                screens = newScreens;
                totalBounds = newBounds;

                // remove all players screens
                List<PlayerInfo> playerData = profile.PlayerData;
                if (playerData != null)
                {
                    for (int i = 0; i < playerData.Count; i++)
                    {
                        PlayerInfo player = playerData[i];
                        player.EditBounds = GetDefaultBounds(draggingIndex);
                        player.ScreenIndex = -1;
                    }
                }
            }

            if (screens.Length > 1)
            {
                screensArea = new RectangleF(5,this.Height/2.3f, Width - 20, Height /2.1f);
            }
            else
            {
                screensArea = new RectangleF(10, 50 + Height * 0.2f + 10, Width - 20, Height * 0.5f);
            }

            //if (totalBounds.Width > totalBounds.Height)
            //{
            // horizontal monitor setup
            
            scale = screensArea.Width / (float)totalBounds.Width;
            if (totalBounds.Height * scale > screensArea.Height)
            {
                scale = screensArea.Height / (float)totalBounds.Height;
            }
            
            //}
            //else
            //{
            //    // vertical monitor setup
            //    scale = screensArea.Height / (float)totalBounds.Height;
            //    if (totalBounds.Width * scale > screensArea.Width)
            //    {
            //        scale = screensArea.Width / (float)totalBounds.Width;
            //    }
            //}

            Rectangle scaledBounds = RectangleUtil.Scale(totalBounds, scale);
            scaledBounds.X = (int)screensArea.X;
            scaledBounds.Y = (int)screensArea.Y;
            //scaledBounds = RectangleUtil.Center(scaledBounds, RectangleUtil.Float(0, this.Height * 0.25f, this.Width, this.Height * 0.7f));

            int minY = 0;
            for (int i = 0; i < screens.Length; i++)
            {
                UserScreen screen = screens[i];

                Rectangle bounds = RectangleUtil.Scale(screen.MonitorBounds, scale);
               // Console.WriteLine(bounds);
               // float boundW = scaledBounds.Width * 1.30f;
                //float boundH = scaledBounds.Height * 1.30f;

                Rectangle uiBounds = new Rectangle(bounds.X, bounds.Y + scaledBounds.Y,bounds.Width, bounds.Height);
                screen.UIBounds = uiBounds;

                minY = Math.Min(minY, uiBounds.X);
            }

            // remove negative monitors
            minY = -minY;
            for (int i = 0; i < screens.Length; i++)
            {
                UserScreen screen = screens[i];

                Rectangle uiBounds = screen.UIBounds;

                uiBounds.X += minY + scaledBounds.X;
                screen.UIBounds = uiBounds;
                screen.SwapTypeBounds = RectangleUtil.Float(uiBounds.X, uiBounds.Y, uiBounds.Width * 0.1f, uiBounds.Width * 0.1f);
            }

            float instWidth = screensArea.Width / 1.77f;
            instructionImg.Location = new Point((int)screensArea.X, (int)screensArea.Y/4);
            instructionImg.Size = new Size((int)screensArea.Width, (int)instWidth);

            UpdatePlayers();
        }
       
        public override void Initialize(UserGameInfo game, GameProfile profile)
        {
            base.Initialize(game, profile);

            //gamepadTimer.Enabled = true;
            //gamepadPollTimer.Enabled = true;

            if (gamepadTimer == null)
            {
                gamepadTimer = new System.Threading.Timer(GamepadTimer_Tick, null, 0, 1000);
            }

            if (gamepadPollTimer == null)
            {
                gamepadPollTimer = new System.Threading.Timer(GamepadPollTimer_Tick, null, 0, 1001);
            }

            UpdatePlayers();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            SuspendLayout();
            totalBounds = Rectangle.Empty;
            UpdatePlayers();
            ResumeLayout();
            Invalidate();
        }

        private void UpdatePlayers()
        {
            if (profile == null)
            {
                return;
            }

            List<PlayerInfo> playerData = profile.PlayerData;
            canProceed = playerData.Count(c => c.ScreenIndex != -1) >= 1;
            if (playerData.Count == 0)
            {
                if (game.Game.SupportsKeyboard)
                {
                    // add keyboard data
                    PlayerInfo kbPlayer = new PlayerInfo
                    {
                        IsKeyboardPlayer = true,
                        GamepadId = 99
                    };
                    playerData.Add(kbPlayer);
                    //keyboardPlayer = true;
                }

                //Raw mice/keyboards
                if (game.Game.SupportsMultipleKeyboardsAndMice)
                {
                    playerData.AddRange(RawInputManager.GetDeviceInputInfos());
                }

                // make fake data if needed
                if (testDinputPlayers != -1)
                {
                    for (int i = 0; i < testDinputPlayers; i++)
                    {
                        // new gamepad
                        PlayerInfo player = new PlayerInfo
                        {
                            GamepadGuid = new Guid(),
                            GamepadName = "Player",
                            IsDInput = true,
                            IsFake = true
                        };
                        playerData.Add(player);
                    }
                }

                if (testXinputPlayers != -1)
                {
                    for (int i = 0; i < testXinputPlayers; i++)
                    {
                        PlayerInfo player = new PlayerInfo
                        {
                            GamepadGuid = new Guid(),
                            GamepadName = "XPlayer",
                            IsXInput = true,
                            GamepadId = i,
                            IsFake = true
                        };
                        playerData.Add(player);
                    }
                }
            }
            UpdateScreens();

            float playersWidth = Width * 0.65f;

            float playerCount = playerData.Count;
            float playerWidth = playersWidth * 0.9f;
            float playerHeight = Height * 0.2f;
            playersArea = new RectangleF(10, 27, playersWidth, playerHeight);

            float playersAreaArea = playersArea.Width * playersArea.Height;
            float maxArea = playersAreaArea / playerCount;
            playerSize = (int)Math.Round(Math.Sqrt(maxArea) - 0.5); // force the round down
            // see if the size can fit it or we need to make some further adjustments
            int horizontal = (int)Math.Round((playersWidth / playerSize) - 0.5);
            int vertical = (int)Math.Round((playerHeight / playerSize) - 0.5);
            int total = vertical * horizontal;
            if (total < playerCount)
            {
                int newVertical = vertical + 1;
                playerCustomFont = new Font("Franklin Gothic Medium", newplayerCustomFontSize*0.8f, FontStyle.Regular, GraphicsUnit.Point, 0);
                playerSize = (int)Math.Round(((playerHeight/1.2f) / newVertical));
            }


            for (int i = 0; i < playerData.Count; i++)
            {
                PlayerInfo info = playerData[i];

                if (info.ScreenIndex == -1)
                {
                    info.EditBounds = GetDefaultBounds(i);
                    info.SourceEditBounds = info.EditBounds;
                }
            }

            CanPlayUpdated(canProceed, false);
            Invalidate();
        }

        private bool GetScreenDivisionBounds(UserScreenType screenType, int index, out Rectangle? monitorBounds, out Rectangle? editorBounds, Rectangle bounds, Rectangle ebounds)
        {
            monitorBounds = null;
            editorBounds = null;

            bool Regular(int width, int height, out Rectangle? _monitorBounds, out Rectangle? _editorBounds)
            {
                int y = index % height;
                int x = (index - y) / height;

                int halfw = (int)(bounds.Width / (float)width);
                int halfh = (int)(bounds.Height / (float)height);

                _monitorBounds = new Rectangle(bounds.X + (halfw * x), bounds.Y + (halfh * y), halfw, halfh);
                int halfwe = (int)(ebounds.Width / (float)width);
                int halfhe = (int)(ebounds.Height / (float)height);
                _editorBounds = new Rectangle(ebounds.X + (halfwe * x), ebounds.Y + (halfhe * y), halfwe, halfhe);

                return true;
            }

            switch (screenType)
            {
                case UserScreenType.FullScreen:
                    {
                        if (index >= 1)
                        {
                            return false;
                        }

                        monitorBounds = bounds;
                        editorBounds = ebounds;
                        return true;
                    }
                case UserScreenType.DualHorizontal:
                    {
                        if (index >= 2)
                        {
                            return false;
                        }

                        return Regular(1, 2, out monitorBounds, out editorBounds);
                    }
                case UserScreenType.DualVertical:
                    {
                        if (index >= 2)
                        {
                            return false;
                        }

                        return Regular(2, 1, out monitorBounds, out editorBounds);
                    }
                case UserScreenType.FourPlayers:
                    {
                        if (index >= 4)
                        {
                            return false;
                        }

                        return Regular(2, 2, out monitorBounds, out editorBounds);
                    }
                case UserScreenType.SixPlayers:
                    {
                        if (index >= 6)
                        {
                            return false;
                        }

                        return Regular(3, 2, out monitorBounds, out editorBounds);
                    }
                case UserScreenType.EightPlayers:
                    {
                        if (index >= 8)
                        {
                            return false;
                        }

                        return Regular(4, 2, out monitorBounds, out editorBounds);
                    }
                case UserScreenType.SixteenPlayers:
                    {
                        if (index >= 16)
                        {
                            return false;
                        }

                        return Regular(4, 4, out monitorBounds, out editorBounds);
                    }
                case UserScreenType.Custom:
                    {
                        //TODO: needs to be cached
                        int horLines = int.Parse(ini.IniReadValue("CustomLayout", "HorizontalLines"));
                        int verLines = int.Parse(ini.IniReadValue("CustomLayout", "VerticalLines"));
                        int maxPlayers = int.Parse(ini.IniReadValue("CustomLayout", "MaxPlayers"));

                        //if (horLines == 0)
                        //    horLines = 1;
                        //if (verLines == 0)
                        //    verLines = 1;
                        horLines++;
                        verLines++;

                        if (index >= maxPlayers)
                        {
                            return false;
                        }

                        return Regular(verLines, horLines, out monitorBounds, out editorBounds);
                    }
            }

            return false;
        }

        private bool GetFreeSpace(int screenIndex, out Rectangle? editorBounds, out Rectangle? monitorBounds, PlayerInfo playerToInsert)
        {
            editorBounds = null;
            monitorBounds = null;

            List<PlayerInfo> players = profile.PlayerData;
            UserScreen screen = screens[screenIndex];
            Rectangle bounds = screen.MonitorBounds;
            Rectangle ebounds = screen.UIBounds;

            int index = -1;
            while (GetScreenDivisionBounds(screen.Type, ++index, out Rectangle? divMonitorBounds, out Rectangle? divEditorBounds, bounds, ebounds))
            {
                IEnumerable<PlayerInfo> playersInDiv = players.Where(
                    x => x.ScreenIndex == screenIndex && (x.MonitorBounds == divMonitorBounds.Value || (x.MonitorBounds.Width == divMonitorBounds.Value.Width * 2 && x.MonitorBounds.Y == divMonitorBounds.Value.Y && x.MonitorBounds.X == divMonitorBounds.Value.X) || (x.MonitorBounds.Height == divMonitorBounds.Value.Height * 2 && x.MonitorBounds.X == divMonitorBounds.Value.X && x.MonitorBounds.Y == divMonitorBounds.Value.Y)));

                //Raw mice/keyboards can go as pairs, nothing else can.
                if (
                    playerToInsert.IsRawMouse ? playersInDiv.All(x => x.IsRawKeyboard) :
                    playerToInsert.IsRawKeyboard ? playersInDiv.All(x => x.IsRawMouse) :

                    !playersInDiv.Any())
                {
                    monitorBounds = divMonitorBounds;
                    editorBounds = divEditorBounds;
                    return true;
                }
            }

            return false;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (dragging)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                // first count how many gamepads we have
                bool changed = false;
                for (int i = 0; i < screens.Length; i++)
                {
                    UserScreen screen = screens[i];
                    if (screen.UIBounds.Contains(e.Location) && !screen.SwapTypeBounds.Contains(e.Location))
                    {
                        List<PlayerInfo> playerData = profile.PlayerData;

                        // add all possible players!
                        for (int j = 0; j < playerData.Count; j++)
                        {
                            PlayerInfo player = playerData[j];

                            bool hasFreeSpace = GetFreeSpace(i, out Rectangle? editor, out Rectangle? monitor, player);

                            if (hasFreeSpace)
                            {
                                if (player.ScreenIndex == -1)
                                {
                                    player.Owner = screens[i];
                                    player.ScreenIndex = i;
                                    player.MonitorBounds = monitor.Value;
                                    player.EditBounds = editor.Value;
                                    changed = true;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        break;
                    }
                }

                if (changed)
                {
                    UpdatePlayers();
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            List<PlayerInfo> players = profile.PlayerData;

            if (dragging)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                for (int i = 0; i < screens.Length; i++)
                {
                    UserScreen screen = screens[i];
                    if (screen.SwapTypeBounds.Contains(e.Location))
                    {
                        if (screen.Type == UserScreenType.Custom)
                        {
                            screen.Type = 0;
                        }
                        else
                        {
                            screen.Type++;
                        }
                        // invalidate all players inside screen
                        for (int j = 0; j < players.Count; j++)
                        {
                            // return to default position
                            PlayerInfo p = players[j];
                            if (p.ScreenIndex == i)
                            {
                                p.EditBounds = GetDefaultBounds(j);
                                p.ScreenIndex = -1;
                            }
                        }

                        Invalidate();
                        return;
                    }
                }

                for (int i = 0; i < players.Count; i++)
                {
                    Rectangle r = players[i].EditBounds;
                    if (r.Contains(e.Location))
                    {
                        dragging = true;
                        draggingIndex = i;
                        draggingOffset = new Point(r.X - e.X, r.Y - e.Y);
                        Rectangle newBounds = GetDefaultBounds(draggingIndex);
                        profile.PlayerData[draggingIndex].EditBounds = newBounds;

                        if (draggingOffset.X < -newBounds.Width ||
                            draggingOffset.Y < -newBounds.Height)
                        {
                            draggingOffset = new Point(0, 0);
                        }

                        break;
                    }
                }
            }
            else if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                for (int i = 0; i < screens.Length; i++)
                {
                    UserScreen screen = screens[i];
                    if (screen.SwapTypeBounds.Contains(e.Location))
                    {
                        if (screen.Type == UserScreenType.FullScreen)
                        {
                            screen.Type = UserScreenType.Custom;
                        }
                        else
                        {
                            screen.Type--;
                        }
                        // invalidate all players inside screen
                        for (int j = 0; j < players.Count; j++)
                        {
                            // return to default position
                            PlayerInfo p = players[j];
                            if (p.ScreenIndex == i)
                            {
                                p.EditBounds = GetDefaultBounds(j);
                                p.ScreenIndex = -1;
                            }
                        }

                        Invalidate();
                        return;
                    }
                }
                // if over a player on a screen, change the type
                for (int i = 0; i < players.Count; i++)
                {

                    PlayerInfo p = players[i];
                    Rectangle r = p.EditBounds;
                    if (r.Contains(e.Location))
                    {

                        if (p.ScreenIndex != -1)
                        {


                            UserScreen screen = screens[p.ScreenIndex];

                            int verLines = 2;
                            int horLines = 2;
                            switch (screen.Type)
                            {
                                case UserScreenType.FourPlayers:
                                    {
                                        verLines = 2;
                                        horLines = 2;
                                    }
                                    break;
                                case UserScreenType.SixPlayers:
                                    {
                                        verLines = 3;
                                        horLines = 2;
                                    }
                                    break;
                                case UserScreenType.EightPlayers:
                                    {
                                        verLines = 4;
                                        horLines = 2;
                                    }
                                    break;
                                case UserScreenType.SixteenPlayers:
                                    {
                                        verLines = 4;
                                        horLines = 4;
                                    }
                                    break;
                                case UserScreenType.Custom:
                                    {
                                        horLines = int.Parse(ini.IniReadValue("CustomLayout", "HorizontalLines")) + 1;
                                        verLines = int.Parse(ini.IniReadValue("CustomLayout", "VerticalLines")) + 1;
                                    }
                                    break;
                            }

                            //int halfWidth = screen.MonitorBounds.Width / 2;
                            //int halfHeight = screen.MonitorBounds.Height / 2;
                            int halfWidth = screen.MonitorBounds.Width / verLines;
                            int halfHeight = screen.MonitorBounds.Height / horLines;

                            Rectangle bounds = p.MonitorBounds;
                            if ((int)screen.Type >= 3)
                            {
                                // check if the size is 1/4th of screen
                                if (bounds.Width == halfWidth && bounds.Height == halfHeight)
                                {
                                    bool hasLeftRightSpace = true;
                                    bool hasTopBottomSpace = true;

                                    // check if we have something left/right or top/bottom
                                    for (int j = 0; j < players.Count; j++)
                                    {
                                        if (i == j)
                                        {
                                            continue;
                                        }

                                        PlayerInfo other = players[j];
                                        //if (other.ScreenIndex == p.ScreenIndex)
                                        //{
                                        //    continue;
                                        //}

                                        //if(other.ScreenIndex == -1)
                                        //{
                                        //    continue;
                                        //}

                                        //if (other.MonitorBounds.Y == p.MonitorBounds.Y)
                                        //{
                                        //    hasLeftRightSpace = false;
                                        //}
                                        //if (other.MonitorBounds.X == p.MonitorBounds.X)
                                        //{
                                        //    hasTopBottomSpace = false;
                                        //}

                                        //if (other.EditBounds.Y <= 50)
                                        //{
                                        //    continue;
                                        //}

                                        //if(p.MonitorBounds.Y == other.MonitorBounds.Y && p.MonitorBounds.Width + other.MonitorBounds.X + other.MonitorBounds.Width > )
                                        //{
                                        //    hasLeftRightSpace = false;
                                        //}

                                        if (other.MonitorBounds == new Rectangle(0, 0, 0, 0))
                                        {
                                            continue;
                                        }

                                        //if(p.MonitorBounds == other.MonitorBounds && (p.IsRawKeyboard && !other.IsRawMouse || p.IsRawMouse && !other.IsRawKeyboard))
                                        //{
                                        //if (p.IsRawMouse)
                                        //{
                                        //    continue;
                                        //}
                                        //}

                                        if (((p.IsKeyboardPlayer && !p.IsRawKeyboard && !p.IsRawMouse) || p.IsXInput || p.IsDInput || (p.IsRawKeyboard && !other.IsRawMouse) || (p.IsRawMouse && !other.IsRawKeyboard)) && (p.MonitorBounds.Y == other.MonitorBounds.Y || (p.MonitorBounds.Y < other.MonitorBounds.Height && p.MonitorBounds.Y > other.MonitorBounds.Y)))
                                        {
                                            hasLeftRightSpace = false;
                                        }
                                        if (((p.IsKeyboardPlayer && !p.IsRawKeyboard && !p.IsRawMouse) || p.IsXInput || p.IsDInput || (p.IsRawKeyboard && !other.IsRawMouse) || (p.IsRawMouse && !other.IsRawKeyboard)) && (p.MonitorBounds.X == other.MonitorBounds.X || (p.MonitorBounds.X < other.MonitorBounds.Width && p.MonitorBounds.X > other.MonitorBounds.X)))
                                        {
                                            hasTopBottomSpace = false;
                                        }

                                        //if (p.EditBounds.Y == other.EditBounds.Y - other.EditBounds.Height || p.EditBounds.Y == other.EditBounds.Y + other.EditBounds.Height)
                                        //{
                                        //    hasLeftRightSpace = false;
                                        //}
                                        //if (p.EditBounds.X == other.EditBounds.X - other.EditBounds.Width || p.EditBounds.X == other.EditBounds.X + other.EditBounds.Width)
                                        //{
                                        //    hasTopBottomSpace = false;
                                        //}

                                        //if (other.MonitorBounds.X == screen.MonitorBounds.X + halfWidth &&
                                        //    other.MonitorBounds.Height == screen.MonitorBounds.Height)
                                        //{
                                        //    hasLeftRightSpace = false;
                                        //}
                                        //if (other.MonitorBounds.X == screen.MonitorBounds.X &&
                                        //    other.MonitorBounds.Width == screen.MonitorBounds.Width)
                                        //{
                                        //    hasTopBottomSpace = false;
                                        //}
                                    }

                                    if (hasLeftRightSpace)
                                    {
                                        Rectangle edit = p.EditBounds;

                                        if (bounds.X == screen.MonitorBounds.X + bounds.Width)
                                        {
                                            bounds.X -= bounds.Width;
                                            edit.X -= edit.Width;
                                        }

                                        bounds.Width *= verLines;
                                        edit.Width *= verLines;

                                        p.EditBounds = edit;
                                        p.MonitorBounds = bounds;

                                        Invalidate();
                                    }
                                    else if (hasTopBottomSpace)
                                    {
                                        Rectangle edit = p.EditBounds;
                                        if (bounds.Y == screen.MonitorBounds.Y + bounds.Height)
                                        {
                                            bounds.Y -= bounds.Height;
                                            edit.Y -= edit.Height;
                                        }

                                        bounds.Height *= horLines;
                                        edit.Height *= horLines;

                                        p.EditBounds = edit;
                                        p.MonitorBounds = bounds;

                                        Invalidate();
                                    }

                                }
                                else
                                {

                                    bounds.Width = screen.MonitorBounds.Width / verLines;
                                    bounds.Height = screen.MonitorBounds.Height / horLines;
                                    p.MonitorBounds = bounds;

                                    Rectangle edit = p.EditBounds;
                                    edit.Width = screen.UIBounds.Width / verLines;
                                    edit.Height = screen.UIBounds.Height / horLines;
                                    p.EditBounds = edit;

                                    Invalidate();
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            mousePos = e.Location;

            if (dragging)
            {
                List<PlayerInfo> players = profile.PlayerData;

                PlayerInfo player = players[draggingIndex];

                Rectangle p = player.EditBounds;
                if (draggingScreen == -1)
                {
                    for (int i = 0; i < screens.Length; i++)
                    {
                        UserScreen screen = screens[i];
                        Rectangle s = screen.UIBounds;
                        float pc = RectangleUtil.PcInside(p, s);

                        // bigger than 60% = major part inside this screen
                        if (pc > 0.6f)
                        {
                            float offset = s.Width * 0.05f;

                            // check if there's space available on this screen
                            List<PlayerInfo> playas = profile.PlayerData;
                            GetFreeSpace(i, out Rectangle? editor, out Rectangle? monitor, player);

                            if (editor != null)
                            {
                                draggingScreenRec = editor.Value;
                                draggingScreenBounds = monitor.Value;
                                draggingScreen = i;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    Rectangle s = screens[draggingScreen].UIBounds;
                    float pc = RectangleUtil.PcInside(p, s);
                    if (pc < 0.6f)
                    {
                        draggingScreen = -1;
                    }
                }

                p = new Rectangle(mousePos.X + draggingOffset.X, mousePos.Y + draggingOffset.Y, p.Width, p.Height);
                players[draggingIndex].EditBounds = p;

                Invalidate();
            }
        }


        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
            {
                if (dragging)
                {
                    PlayerInfo p = profile.PlayerData[draggingIndex];
                    dragging = false;

                    if (draggingScreen != -1)
                    {
                        p.Owner = screens[draggingScreen];
                        p.ScreenIndex = draggingScreen;
                        p.MonitorBounds = draggingScreenBounds;
                        p.EditBounds = draggingScreenRec;

                        draggingScreen = -1;
                    }
                    else
                    {
                        // return to default position
                        p.Owner = null;
                        p.EditBounds = GetDefaultBounds(draggingIndex);
                        p.MonitorBounds = new Rectangle(0, 0, 0, 0);
                        p.ScreenIndex = -1;
                    }

                    UpdatePlayers(); // force a player update                    

                    Invalidate();
                }
            }
        }

        private Rectangle GetDefaultBounds(int index)
        {
            float lineWidth = index * playerSize;
            float line = (float)Math.Round(((lineWidth + playerSize) / (double)playersArea.Width) - 0.5);
            int perLine = (int)Math.Round((playersArea.Width / (double)playerSize) - 0.5);

            float x = playersArea.X + (index * playerSize) - (perLine * playerSize * line);
            float y = playersArea.Y + (playerSize * line);

            return new Rectangle((int)x, (int)y, playerSize, playerSize);
        }

        public void RefreshAll()
        {
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            string ChoosenTheme = ini.IniReadValue("Theme", "Theme");
            IniFile theme = new IniFile(Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\theme\\" + ChoosenTheme, "theme.ini"));

            base.OnPaint(e);
            string[] rgb_PositionControlsFontColor = theme.IniReadValue("Colors", "PositionControlsFontColor").Split(',');
            string[] rgb_PositionScreenColor = theme.IniReadValue("Colors", "PositionScreenColor").Split(',');
            string[] rgb_PositionPlayerScreenColor = theme.IniReadValue("Colors", "PositionPlayerScreenColor").Split(',');

            SolidBrush myBrush = new SolidBrush(Color.FromArgb(Convert.ToInt32(rgb_PositionControlsFontColor[0]), Convert.ToInt32(rgb_PositionControlsFontColor[1]), Convert.ToInt32(rgb_PositionControlsFontColor[2])));
            Pen PositionScreenPen = new Pen(Color.FromArgb(Convert.ToInt32(rgb_PositionScreenColor[0]), Convert.ToInt32(rgb_PositionScreenColor[1]), Convert.ToInt32(rgb_PositionScreenColor[2])));
            Pen PositionPlayerScreenPen = new Pen(Color.FromArgb(Convert.ToInt32(rgb_PositionPlayerScreenColor[0]), Convert.ToInt32(rgb_PositionPlayerScreenColor[1]), Convert.ToInt32(rgb_PositionPlayerScreenColor[2])));
            Graphics g = e.Graphics;
            int gampadCount = 0;
#if DEBUG
            //g.FillRectangle(Brushes.Green, playersArea);
            //g.FillRectangle(Brushes.CornflowerBlue, screensArea);
            //g.FillRectangle(Brushes.CornflowerBlue, new RectangleF(0, 0, Width, Height));
#endif
            SuspendLayout();
            UpdateScreens();

            for (int i = 0; i < screens.Length; i++)
            {
                UserScreen s = screens[i];
                g.DrawRectangle(PositionScreenPen, s.UIBounds);
                g.DrawRectangle(PositionScreenPen, s.SwapTypeBounds);

                switch (s.Type)
                {
                    case UserScreenType.FullScreen:
                        g.DrawImage(Resources.fullscreen, s.SwapTypeBounds);
                        break;
                    case UserScreenType.DualHorizontal:
                        g.DrawImage(Resources.horizontal, s.SwapTypeBounds);
                        break;
                    case UserScreenType.DualVertical:
                        g.DrawImage(Resources.vertical, s.SwapTypeBounds);
                        break;
                    case UserScreenType.FourPlayers:
                        g.DrawImage(Resources._4players, s.SwapTypeBounds);
                        break;
                    case UserScreenType.SixPlayers:
                        g.DrawImage(Resources._6players, s.SwapTypeBounds);
                        break;
                    case UserScreenType.EightPlayers:
                        g.DrawImage(Resources._8players, s.SwapTypeBounds);
                        break;
                    case UserScreenType.SixteenPlayers:
                        g.DrawImage(Resources._16players, s.SwapTypeBounds);
                        break;
                    case UserScreenType.Custom:
                        g.DrawImage(Resources.customLayout, s.SwapTypeBounds);
                        break;
                }
            }

            List<PlayerInfo> players = profile.PlayerData;

            if (players.Count == 0)
            {
            }
            else
            {
                for (int i = 0; i < players.Count; i++)
                {                    
                    PlayerInfo info = players[i];

                    Brush[] colors = new Brush[]
                    {
                      Brushes.Red,
                      Brushes.DodgerBlue, Brushes.LimeGreen, Brushes.Yellow,
                      Brushes.SaddleBrown, Brushes.BlueViolet, Brushes.Aqua, Brushes.DarkOrange, Brushes.Silver,
                      Brushes.Magenta, Brushes.SpringGreen, Brushes.Indigo, Brushes.Black,
                      Brushes.White, Brushes.Bisque, Brushes.SkyBlue, Brushes.SeaGreen,
                      Brushes.Wheat, Brushes.Crimson, Brushes.Turquoise, Brushes.Chocolate,
                      Brushes.OrangeRed, Brushes.Olive, Brushes.DarkRed, Brushes.Lavender
                    };
          
                    Rectangle s = info.EditBounds;
                    g.ResetClip();
                    g.Clip = new Region(new RectangleF(s.X, s.Y, s.Width + 1, s.Height + 1));
                  
                    Rectangle gamepadRect = RectangleUtil.ScaleAndCenter(gamepadImg.Size, s);
                    
                    string str = (i + 1).ToString();
                    SizeF size = g.MeasureString(str, playerFont);
                    PointF loc = RectangleUtil.Center(size, s);

                    if (gamePadPressed == info.GamepadId)
                    {
                        var playerColor =  colors[info.GamepadId];
                        g.FillPie(playerColor, gamepadRect,100.0f,-20.0f);
                        gamePadPressed = -1;
                    }
                    else
                    {
                        g.ResetClip();
                    }

                    if (info.IsXInput)
                    {                       
                        loc.Y -= gamepadRect.Height * 0.1f;
                        var playerColor = colors[info.GamepadId];
                        gampadCount++;
                        str = Convert.ToString(gampadCount);                           

                        size = g.MeasureString(str, playerCustomFont);
                        loc = RectangleUtil.Center(size, s);
                        loc.Y -= 5;
                            
                        g.DrawString(str, playerCustomFont, playerColor, loc);
                        g.DrawImage(gamepadImg, gamepadRect);
                    }
                    else if (info.IsKeyboardPlayer && !info.IsRawKeyboard && !info.IsRawMouse)
                    {
                        g.DrawImage(keyboardImg, gamepadRect);
                    }
                    else if (info.IsRawKeyboard || info.IsRawMouse)
                    {
                        Image img = info.IsRawKeyboard ? rawKeyboardImg : rawMouseImg;

                        if (info.ShouldFlash)
                        {
                            g.DrawImage(img, gamepadRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, flashImageAttributes);
                        }
                        else
                        {
                            g.DrawImage(img, gamepadRect);
                        }
                    }
                    else
                    {
                        loc.Y -= gamepadRect.Height * 0.2f;
                        
                        var playerColor = colors[info.GamepadId];
                        gampadCount++;
                        str = Convert.ToString(gampadCount);
                        size = g.MeasureString(str, playerCustomFont);
                        loc = RectangleUtil.Center(size, s);
                        loc.Y -= 5;
                        
                        g.DrawString(str, playerCustomFont, playerColor, loc);
                      
                        g.DrawImage(genericImg, gamepadRect);
                    }

                    if (info.ScreenIndex != -1)
                    {
                        UserScreen screen = screens[info.ScreenIndex];
                        if ((s.Height + s.Y) - (screen.UIBounds.Height + screen.UIBounds.Y) == -1)
                        {
                            s.Height += 1;
                        }
                        g.Clip = new Region(new RectangleF(s.X, s.Y, s.Width + 1, s.Height + 1));
                        g.DrawRectangle(PositionPlayerScreenPen, s);
                    }

                    if (gamePadPressed != info.GamepadId)
                    {
                        g.FillEllipse(Brushes.Transparent, gamepadRect);
                    }
                }
            }
            g.ResetClip();
            if (dragging && draggingScreen != -1)
            {
                g.DrawRectangle(Pens.LimeGreen, draggingScreenRec);
            }
            if (players.Count == 0)
            {
                g.DrawString(game.Game.SupportsMultipleKeyboardsAndMice ? "Input Devices" : "No Gamepads detected", playerTextFont, myBrush, new PointF(10, 10));
            }
            else
            {
                g.DrawString( "Input Devices", playerTextFont, myBrush, new PointF(10, 10));
            }
            ResumeLayout();
        }

    }
}
