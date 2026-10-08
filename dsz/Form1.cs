using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using NAudio.Wave;

namespace dsz
{
    public partial class Form1 : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x84) // WM_NCHITTEST
            {
                if (m.Result == (IntPtr)1) // HTCLIENT
                {
                    int x = (short)(m.LParam.ToInt32() & 0xFFFF);
                    int y = (short)((m.LParam.ToInt32() >> 16) & 0xFFFF);
                    Point clientPoint = this.PointToClient(new Point(x, y));

                    // Se il mouse è sopra il touch screen (panel1), permettiamo il click normale!
                    if (panel1.Bounds.Contains(clientPoint))
                    {
                        // Lasciamo che il sistema processi normalmente il click (HTCLIENT)
                        m.Result = (IntPtr)1;
                    }
                    else
                    {
                        // Altrimenti lo trattiamo come la barra del titolo per permettere il drag
                        m.Result = (IntPtr)2; // HTCAPTION
                    }
                }
            }
        }

        private LibretroWrapper.retro_environment_t envCb;
        private LibretroWrapper.retro_video_refresh_t videoCb;
        private LibretroWrapper.retro_audio_sample_t audioCb;
        private LibretroWrapper.retro_audio_sample_batch_t audioBatchCb;
        private LibretroWrapper.retro_input_poll_t inputPollCb;
        private LibretroWrapper.retro_input_state_t inputStateCb;
        private Thread emuThread;
        private bool isRunning;
        private object logLock = new object();
        private WasapiOut waveOut;
        private BufferedWaveProvider waveProvider;
        private VolumeSlider volSlider;

        public static System.Collections.Generic.List<LibretroWrapper.retro_memory_descriptor> MemoryDescriptors = new System.Collections.Generic.List<LibretroWrapper.retro_memory_descriptor>();

        public int BgDimAlpha = 0;
        public int FfSpeedMultiplier = 0; // 0 = Max speed
        public bool ShowFps = false;
        
        private Label label1;
        private Label label2;
        private System.Windows.Forms.Timer fpsTimer;
        private int frameCount = 0;

        public void UpdateFpsVisibility()
        {
            if (label1 != null && label2 != null)
            {
                label1.Visible = ShowFps;
                label2.Visible = ShowFps;
            }
        }

        private void FpsTimer_Tick(object sender, EventArgs e)
        {
            int currentFps = System.Threading.Interlocked.Exchange(ref frameCount, 0);
            if (ShowFps && label1 != null && label2 != null)
            {
                label1.Text = currentFps.ToString() + " FPS";
                label2.Text = currentFps.ToString() + " FPS";
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (BgDimAlpha > 0)
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(BgDimAlpha, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
            }
        }

        private byte[] screenBuffer = new byte[256 * 384 * 4];
        private Bitmap screenBmp = new Bitmap(256, 384, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

        private void Form1_Load(object sender, EventArgs e)
        {
            // Applica i bordi arrotondati e rimuove il quadrato bianco di sfondo
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 55, 55));

            closeButton.Click += closeButton_Click;

            // Initialize FPS labels
            label1 = new Label();
            label1.AutoSize = true;
            label1.ForeColor = Color.White;
            label1.BackColor = Color.FromArgb(64, 64, 64);
            label1.Font = new Font("Consolas", 9F, FontStyle.Bold);
            label1.Location = new Point(5, 5);
            label1.Visible = false;
            label1.Text = "0 FPS";
            panel1.Controls.Add(label1);

            label2 = new Label();
            label2.AutoSize = true;
            label2.ForeColor = Color.White;
            label2.BackColor = Color.FromArgb(64, 64, 64);
            label2.Font = new Font("Consolas", 9F, FontStyle.Bold);
            label2.Location = new Point(5, 5);
            label2.Visible = false;
            label2.Text = "0 FPS";
            panel2.Controls.Add(label2);

            fpsTimer = new System.Windows.Forms.Timer();
            fpsTimer.Interval = 1000;
            fpsTimer.Tick += FpsTimer_Tick;
            fpsTimer.Start();

            volSlider = new VolumeSlider();
            volSlider.Location = new Point(panel3.Left - 65, panel3.Bottom + 5);
            volSlider.Size = new Size(100, 20);

            string configPath = "config.ini";
            float savedVol = 1.0f;
            if (System.IO.File.Exists(configPath))
            {
                float.TryParse(System.IO.File.ReadAllText(configPath), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out savedVol);
            }
            volSlider.Volume = savedVol;

            volSlider.VolumeChanged += (s, ev) =>
            {
                System.IO.File.WriteAllText(configPath, volSlider.Volume.ToString(System.Globalization.CultureInfo.InvariantCulture));
            };

            this.Controls.Add(volSlider);
            volSlider.BringToFront();

            Control p3 = this.Controls["panel3"];
            if (p3 != null)
            {
                p3.Cursor = Cursors.Hand;
                p3.Click += panel3_Click;
            }

            if (gearIcon != null)
            {
                gearIcon.Click += (s, ev) =>
                {
                    using (SettingsForm sf = new SettingsForm(this))
                    {
                        sf.ShowDialog(this);
                    }
                };
            }

            Control p4 = this.Controls["panel4"];
            if (p4 != null)
            {
                p4.Cursor = Cursors.Hand;
                p4.Click += panel4_Click;
                if (p4.BackgroundImage != null)
                {
                    imgFfNormal = new Bitmap(p4.BackgroundImage);
                    imgFfGreen = TintImageGreen(imgFfNormal);
                }
            }

            string romPath = @"C:\Users\matti\source\repos\dsz\dsz\Resources\ChinatownWars.nds";
            if (System.IO.File.Exists("rom_path.txt"))
            {
                romPath = System.IO.File.ReadAllText("rom_path.txt");
            }

            typeof(Panel).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, panel1, new object[] { true });
            typeof(Panel).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, panel2, new object[] { true });

            panel2.Paint += (s, ev) =>
            {
                ev.Graphics.DrawImage(screenBmp, new Rectangle(0, 0, panel2.Width, panel2.Height), new Rectangle(0, 0, 256, 192), GraphicsUnit.Pixel);
            };

            panel1.Paint += (s, ev) =>
            {
                ev.Graphics.DrawImage(screenBmp, new Rectangle(0, 0, panel1.Width, panel1.Height), new Rectangle(0, 192, 256, 192), GraphicsUnit.Pixel);
                if (isTouchPressed)
                {
                    using (Pen p = new Pen(Color.Red, 3))
                    {
                        ev.Graphics.DrawEllipse(p, rawTouchX - 10, rawTouchY - 10, 20, 20);
                    }
                }
            };

            StartEmulator(romPath);
        }

        private void panel3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Nintendo DS ROMs (*.nds)|*.nds|All files (*.*)|*.*";
                ofd.Title = "Seleziona una ROM Nintendo DS";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllText("rom_path.txt", ofd.FileName);

                    if (isRunning)
                    {
                        isRunning = false;
                        if (emuThread != null && emuThread.IsAlive)
                        {
                            emuThread.Join(3000); // Aspetta che il thread si fermi
                        }
                    }

                    StartEmulator(ofd.FileName);
                }
            }
        }

        private Bitmap imgFfNormal;
        private Bitmap imgFfGreen;

        private Bitmap TintImageGreen(Bitmap source)
        {
            Bitmap bmp = new Bitmap(source.Width, source.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                System.Drawing.Imaging.ColorMatrix matrix = new System.Drawing.Imaging.ColorMatrix(new float[][]
                {
                    new float[] { 0, 0, 0, 0, 0 },
                    new float[] { 0, 1, 0, 0, 0 },
                    new float[] { 0, 0, 0, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 0, 0, 0, 0, 1 }
                });
                System.Drawing.Imaging.ImageAttributes attributes = new System.Drawing.Imaging.ImageAttributes();
                attributes.SetColorMatrix(matrix);
                g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
            }
            return bmp;
        }

        private bool isFastForwarding = false;

        private void panel4_Click(object sender, EventArgs e)
        {
            isFastForwarding = !isFastForwarding;
            Control p4 = this.Controls["panel4"];
            if (p4 != null && imgFfNormal != null && imgFfGreen != null)
            {
                p4.BackgroundImage = isFastForwarding ? imgFfGreen : imgFfNormal;
            }
            if (!isFastForwarding && waveProvider != null)
            {
                waveProvider.ClearBuffer();
            }
        }

        private string sysDir;
        private IntPtr sysDirPtr = IntPtr.Zero;
        private IntPtr saveDirPtr = IntPtr.Zero;

        private bool firstFrameLogged = false;

        private volatile bool isTouchPressed = false;
        private short touchX = 0;
        private short touchY = 0;
        private int rawTouchX = 0;
        private int rawTouchY = 0;

        private const uint RETRO_DEVICE_POINTER = 6;
        private const uint RETRO_DEVICE_ID_POINTER_X = 0;
        private const uint RETRO_DEVICE_ID_POINTER_Y = 1;
        private const uint RETRO_DEVICE_ID_POINTER_PRESSED = 2;

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_STATE
        {
            public uint dwPacketNumber;
            public XINPUT_GAMEPAD Gamepad;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_GAMEPAD
        {
            public ushort wButtons;
            public byte bLeftTrigger;
            public byte bRightTrigger;
            public short sThumbLX;
            public short sThumbLY;
            public short sThumbRX;
            public short sThumbRY;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_variable
        {
            public IntPtr key;
            public IntPtr value;
        }

        [DllImport("xinput1_4.dll")]
        public static extern int XInputGetState(int dwUserIndex, out XINPUT_STATE pState);

        private XINPUT_STATE xState;
        private bool isGamepadConnected = false;

        private void StartEmulator(string romPath)
        {
            panel1.MouseDown += (s, e) => { isTouchPressed = true; UpdateTouch(e.X, e.Y); };
            panel1.MouseMove += (s, e) => { if (isTouchPressed) UpdateTouch(e.X, e.Y); };
            panel1.MouseUp += (s, e) => { isTouchPressed = false; };

            System.IO.File.WriteAllText("debug_emu.txt", "Starting emulator...\n");
            sysDir = AppDomain.CurrentDomain.BaseDirectory;
            if (sysDirPtr != IntPtr.Zero) Marshal.FreeHGlobal(sysDirPtr);
            sysDirPtr = Marshal.StringToHGlobalAnsi(sysDir);

            string saveDir = System.IO.Path.GetDirectoryName(romPath);
            if (saveDirPtr != IntPtr.Zero) Marshal.FreeHGlobal(saveDirPtr);
            saveDirPtr = Marshal.StringToHGlobalAnsi(saveDir);

            envCb = (cmd, data) =>
            {
                if (cmd == 9) // RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY
                {
                    Marshal.WriteIntPtr(data, sysDirPtr);
                    return true;
                }
                if (cmd == 31) // RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY
                {
                    Marshal.WriteIntPtr(data, saveDirPtr);
                    return true;
                }
                if (cmd == 10) // RETRO_ENVIRONMENT_SET_PIXEL_FORMAT
                {
                    int format = Marshal.ReadInt32(data);
                    if (format == 1) return true; // XRGB8888
                }
                if (cmd == 15) // RETRO_ENVIRONMENT_GET_VARIABLE
                {
                    var rv = (retro_variable)Marshal.PtrToStructure(data, typeof(retro_variable));
                    string key = Marshal.PtrToStringAnsi(rv.key);

                    if (key == "melonds_touch_mode")
                    {
                        rv.value = Marshal.StringToHGlobalAnsi("Touch");
                        Marshal.StructureToPtr(rv, data, false);
                        return true;
                    }

                    return false;
                }
                
                if (cmd == 51) // RETRO_ENVIRONMENT_SET_MEMORY_MAPS
                {
                    var map = (LibretroWrapper.retro_memory_map)Marshal.PtrToStructure(data, typeof(LibretroWrapper.retro_memory_map));
                    MemoryDescriptors.Clear();
                    
                    int descSize = Marshal.SizeOf(typeof(LibretroWrapper.retro_memory_descriptor));
                    for (int i = 0; i < map.num_descriptors; i++)
                    {
                        IntPtr descPtr = new IntPtr(map.descriptors.ToInt64() + i * descSize);
                        var desc = (LibretroWrapper.retro_memory_descriptor)Marshal.PtrToStructure(descPtr, typeof(LibretroWrapper.retro_memory_descriptor));
                        MemoryDescriptors.Add(desc);
                        
                        // string space = desc.addrspace != IntPtr.Zero ? Marshal.PtrToStringAnsi(desc.addrspace) : "unknown";
                    }
                    return true;
                }

                return false;
            };

            long lastDrawTime = 0;
            videoCb = (data, width, height, pitch) =>
            {
                if (!firstFrameLogged)
                {
                    firstFrameLogged = true;
                }

                if (data == IntPtr.Zero || width != 256 || height < 384) return;

                try
                {
                    lock (screenBuffer)
                    {
                        Marshal.Copy(data, screenBuffer, 0, screenBuffer.Length);
                    }

                    long now = DateTime.UtcNow.Ticks;
                    if (now - lastDrawTime > 160000) // 16ms = 60fps cap per UI
                    {
                        lastDrawTime = now;
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            var bmpData = screenBmp.LockBits(new Rectangle(0, 0, 256, 384), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                            lock (screenBuffer)
                            {
                                Marshal.Copy(screenBuffer, 0, bmpData.Scan0, screenBuffer.Length);
                            }
                            screenBmp.UnlockBits(bmpData);

                            panel1.Invalidate();
                            panel2.Invalidate();
                        });
                    }
                }
                catch (Exception ex)
                {
                    if (!firstFrameLogged) System.IO.File.AppendAllText("debug_emu.txt", $"Exception in videoCb: {ex.Message}\n");
                }
            };

            byte[] singleSampleBuf = new byte[4];
            audioCb = (l, r) =>
            {
                if (waveProvider != null)
                {
                    if (!isRunning) return;

                    float vol = volSlider != null ? volSlider.Volume : 1.0f;
                    short newL = (short)(l * vol);
                    short newR = (short)(r * vol);

                    singleSampleBuf[0] = (byte)(newL & 0xFF);
                    singleSampleBuf[1] = (byte)((newL >> 8) & 0xFF);
                    singleSampleBuf[2] = (byte)(newR & 0xFF);
                    singleSampleBuf[3] = (byte)((newR >> 8) & 0xFF);
                    waveProvider.AddSamples(singleSampleBuf, 0, 4);
                }
            };

            byte[] batchBuf = new byte[8192];
            audioBatchCb = (data, frames) =>
            {
                if (waveProvider != null)
                {
                    int bytesToCopy = (int)frames * 4;
                    if (!isRunning) return 0;

                    if (bytesToCopy > batchBuf.Length)
                    {
                        batchBuf = new byte[bytesToCopy];
                    }

                    Marshal.Copy(data, batchBuf, 0, bytesToCopy);

                    float vol = volSlider != null ? volSlider.Volume : 1.0f;
                    if (vol < 0.99f)
                    {
                        for (int i = 0; i < bytesToCopy; i += 2)
                        {
                            short sample = (short)(batchBuf[i] | (batchBuf[i + 1] << 8));
                            sample = (short)(sample * vol);
                            batchBuf[i] = (byte)(sample & 0xFF);
                            batchBuf[i + 1] = (byte)((sample >> 8) & 0xFF);
                        }
                    }

                    waveProvider.AddSamples(batchBuf, 0, bytesToCopy);
                }
                return frames;
            };
            inputPollCb = () =>
            {
                try
                {
                    isGamepadConnected = (XInputGetState(0, out xState) == 0);
                }
                catch (Exception ex)
                {
                    lock (logLock) { System.IO.File.AppendAllText("debug_emu.txt", $"Exception in inputPollCb: {ex.Message}\n"); }
                }
            };
            var loggedInputs = new HashSet<string>();

            inputStateCb = (port, device, index, id) =>
            {
                try
                {
                    string logKey = $"{port}_{device}_{index}_{id}";
                    if (!loggedInputs.Contains(logKey))
                    {
                        loggedInputs.Add(logKey);
                        lock (logLock) { System.IO.File.AppendAllText("debug_emu.txt", $"poll port {port} dev {device} idx {index} id {id}\n"); }
                    }

                    if (port == 0)
                    {
                        if (device == RETRO_DEVICE_POINTER)
                        {
                            if (id == RETRO_DEVICE_ID_POINTER_PRESSED)
                            {
                                return (short)(isTouchPressed ? 1 : 0);
                            }
                            if (id == RETRO_DEVICE_ID_POINTER_X) return touchX;
                            if (id == RETRO_DEVICE_ID_POINTER_Y) return touchY;
                        }
                        else if (device == 1) // RETRO_DEVICE_JOYPAD
                        {
                            if (isGamepadConnected)
                            {
                                ushort btn = xState.Gamepad.wButtons;
                                switch (id)
                                {
                                    case 0: return (short)((btn & 0x1000) != 0 ? 1 : 0); // DS B <- Xbox A
                                    case 1: return (short)((btn & 0x4000) != 0 ? 1 : 0); // DS Y <- Xbox X
                                    case 2: return (short)((btn & 0x0020) != 0 ? 1 : 0); // DS SELECT <- Xbox Back
                                    case 3: return (short)((btn & 0x0010) != 0 ? 1 : 0); // DS START <- Xbox Start
                                    case 4: return (short)((btn & 0x0001) != 0 ? 1 : 0); // DS UP
                                    case 5: return (short)((btn & 0x0002) != 0 ? 1 : 0); // DS DOWN
                                    case 6: return (short)((btn & 0x0004) != 0 ? 1 : 0); // DS LEFT
                                    case 7: return (short)((btn & 0x0008) != 0 ? 1 : 0); // DS RIGHT
                                    case 8: return (short)((btn & 0x2000) != 0 ? 1 : 0); // DS A <- Xbox B
                                    case 9: return (short)((btn & 0x8000) != 0 ? 1 : 0); // DS X <- Xbox Y
                                    case 10: return (short)((btn & 0x0100) != 0 ? 1 : 0); // DS L <- Xbox LB
                                    case 11: return (short)((btn & 0x0200) != 0 ? 1 : 0); // DS R <- Xbox RB
                                }
                            }
                        }
                    }

                    return 0;
                }
                catch (Exception ex)
                {
                    lock (logLock) { System.IO.File.AppendAllText("debug_emu.txt", $"Exception in inputStateCb: {ex.Message}\n"); }
                    return 0;
                }
            };

            isRunning = true;
            emuThread = new Thread(() => RunEmulator(romPath));
            emuThread.Start();
        }

        private void RunEmulator(string romPath)
        {
            LibretroWrapper.retro_set_environment(envCb);
            LibretroWrapper.retro_set_video_refresh(videoCb);
            LibretroWrapper.retro_set_audio_sample(audioCb);
            LibretroWrapper.retro_set_audio_sample_batch(audioBatchCb);
            LibretroWrapper.retro_set_input_poll(inputPollCb);
            LibretroWrapper.retro_set_input_state(inputStateCb);

            // Core deve sapere cosa è connesso alla porta 0 (1 = JOYPAD)
            LibretroWrapper.retro_set_controller_port_device(0, 1);

            LibretroWrapper.retro_init();

            byte[] romBytes = System.IO.File.ReadAllBytes(romPath);
            IntPtr romPtr = Marshal.AllocHGlobal(romBytes.Length);
            Marshal.Copy(romBytes, 0, romPtr, romBytes.Length);

            var gameInfo = new LibretroWrapper.retro_game_info
            {
                path = romPath,
                data = romPtr,
                size = (uint)romBytes.Length,
                meta = ""
            };

            if (LibretroWrapper.retro_load_game(ref gameInfo))
            {
                System.IO.File.AppendAllText("debug_emu.txt", "retro_load_game returned true. Starting loop.\n");

                string savePath = System.IO.Path.ChangeExtension(romPath, ".sav");
                uint saveSize = LibretroWrapper.retro_get_memory_size(LibretroWrapper.RETRO_MEMORY_SAVE_RAM);
                IntPtr savePtr = LibretroWrapper.retro_get_memory_data(LibretroWrapper.RETRO_MEMORY_SAVE_RAM);
                System.IO.File.AppendAllText("debug_emu.txt", $"Load SaveRAM: size={saveSize}, ptr={savePtr}, exists={System.IO.File.Exists(savePath)}\n");

                if (saveSize > 0 && savePtr != IntPtr.Zero && System.IO.File.Exists(savePath))
                {
                    byte[] saveData = System.IO.File.ReadAllBytes(savePath);
                    int copyLen = Math.Min(saveData.Length, (int)saveSize);
                    Marshal.Copy(saveData, 0, savePtr, copyLen);
                    System.IO.File.AppendAllText("debug_emu.txt", $"Successfully loaded {copyLen} bytes of save data.\n");
                }

                LibretroWrapper.retro_system_av_info avInfo;
                LibretroWrapper.retro_get_system_av_info(out avInfo);

                waveOut = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 50);
                int sampleRate = avInfo.timing.sample_rate > 0 ? (int)avInfo.timing.sample_rate : 44100;
                waveProvider = new BufferedWaveProvider(new WaveFormat(sampleRate, 16, 2));
                waveProvider.DiscardOnBufferOverflow = true;
                waveOut.Init(waveProvider);

                // Pre-buffer di ~320ms (20 frames) per evitare gracchi se l'UI lagga
                for (int i = 0; i < 20; i++)
                {
                    LibretroWrapper.retro_run();
                }

                waveOut.Play();

                int maxBufferedBytes = waveProvider.WaveFormat.AverageBytesPerSecond / 4; // Target ~250ms

                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                long lastFfTicks = sw.ElapsedTicks;
                long ticksPerSecond = System.Diagnostics.Stopwatch.Frequency;

                while (isRunning)
                {
                    LibretroWrapper.retro_run();
                    System.Threading.Interlocked.Increment(ref frameCount);

                    if (isFastForwarding)
                    {
                        if (FfSpeedMultiplier > 0)
                        {
                            long targetTicksPerFrame = ticksPerSecond / (60 * FfSpeedMultiplier);
                            long currentTicks = sw.ElapsedTicks;
                            long elapsed = currentTicks - lastFfTicks;
                            
                            if (elapsed > targetTicksPerFrame * 10)
                            {
                                lastFfTicks = currentTicks;
                                elapsed = 0;
                            }

                            if (elapsed < targetTicksPerFrame)
                            {
                                // Non possiamo usare Thread.Sleep(1) qui perché su Windows tipicamente 
                                // attende 15ms abbassando gli FPS a 60-70. Facciamo uno spin wait.
                                while (sw.ElapsedTicks - lastFfTicks < targetTicksPerFrame) { }
                            }
                            lastFfTicks = sw.ElapsedTicks;
                        }
                    }
                    else
                    {
                        while (!isFastForwarding && isRunning && waveProvider != null && waveProvider.BufferedBytes > maxBufferedBytes)
                        {
                            Thread.Sleep(1);
                        }
                    }
                }

                waveOut.Stop();
                waveOut.Dispose();

                uint finalSaveSize = LibretroWrapper.retro_get_memory_size(LibretroWrapper.RETRO_MEMORY_SAVE_RAM);
                IntPtr finalSavePtr = LibretroWrapper.retro_get_memory_data(LibretroWrapper.RETRO_MEMORY_SAVE_RAM);

                if (finalSaveSize > 0 && finalSavePtr != IntPtr.Zero)
                {
                    byte[] saveData = new byte[finalSaveSize];
                    Marshal.Copy(finalSavePtr, saveData, 0, (int)finalSaveSize);
                    System.IO.File.WriteAllBytes(savePath, saveData);
                    System.IO.File.AppendAllText("debug_emu.txt", $"Saved {finalSaveSize} bytes to {savePath}\n");
                }
                else
                {
                    System.IO.File.AppendAllText("debug_emu.txt", $"Could not save: finalSaveSize={finalSaveSize}, finalSavePtr={finalSavePtr}\n");
                }

                LibretroWrapper.retro_unload_game();
            }
            else
            {
                System.IO.File.AppendAllText("debug_emu.txt", "retro_load_game returned false!\n");
                this.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show("Errore nel caricamento della ROM. Potrebbero mancare i file BIOS (bios7.bin, bios9.bin, firmware.bin).", "Errore Emulator");
                });
            }

            LibretroWrapper.retro_deinit();
            Marshal.FreeHGlobal(romPtr);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Vuoi davvero chiudere l'app?", "Conferma chiusura", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                isRunning = false;
                if (emuThread != null && emuThread.IsAlive) emuThread.Join(3000);

                Application.Exit();
                Environment.Exit(0);
            }
        }

        private void UpdateTouch(int x, int y)
        {
            rawTouchX = x;
            rawTouchY = y;
            // normX e normY vanno da 0.0 a 1.0 all'interno di panel1
            float normX = (float)x / panel1.Width;
            float normY = (float)y / panel1.Height;

            if (normX < 0) normX = 0; if (normX > 1) normX = 1;
            if (normY < 0) normY = 0; if (normY > 1) normY = 1;

            // X va da -32767 a +32767
            touchX = (short)((normX * 65534.0f) - 32767.0f);

            // Y va da 0 a +32767 (perché panel1 rappresenta la metà inferiore dello schermo intero)
            touchY = (short)(normY * 32767.0f);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false;
            if (emuThread != null && emuThread.IsAlive) emuThread.Join(3000);
            LibretroWrapper.retro_deinit();

            Application.Exit();
            Environment.Exit(0);
        }

        private void debugToolsPanel_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Gli strumenti di debug avanzati (OAM, VRAM, Palette) sono stati rimossi in quanto non supportati dal core melonDS attuale.", "Debug Tools non disponibili", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void closeButton_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
