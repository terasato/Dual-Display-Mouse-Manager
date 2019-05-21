/*
David CHATEAU for DDMM.
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 2 of the License.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using IWshRuntimeLibrary;

namespace DDMM
{
    public partial class DDMM_Form : Form
    {
        # region Class fields -----------------------------------------------------------------------------------------

        private class Screen
        {
            public bool ok;                          // are screen coordinates ok
            public int Left, Right, Top, Bottom;     // coordinates
            public Form_screen form_s;               // preview form (colored rectangle)
            public Screen() { form_s = new Form_screen(); }
        };
        private Screen[] Screens;

        private IntPtr mhook, khook;                 // Hook pointers for mouse and keyboard hooks

        private bool HandleSettingsChanges = false;  // will be set to true after loading settings
        private bool UpdatingPreview = false;        // set to true when updating preview rectangles, because sometimes desktop managers throw them around

        private Rectangle OrigClipRect;              // original clipping zone (used to restore previous state)
        private Rectangle CurrentClipRect;           // current clipping zone (used to reactivate clipping periodically)
        private Rectangle DummyClipRect;             // dummy Rectangle variable to pass to ClipCursor(), because it seems that Clipcursor() needs a "top-left-right-bottom" rectangle instead of c# standard "top-left-width-height"
        private bool ActivateProgram = false;        // is the program (mouse management) activated
        private bool ActiveClip = false;             // is the cursor currently clipped
        private bool CanClip = true;                 // is it allowed to activate clipping (it is not allowed when holding ctrl key, and not allowed during 100ms when we release cursor)
        private bool UseAutoBounds = false;          // Uses automatic screen boundary control
        private bool UsePreview = false;             // Uses preview screens for region selection

        private Color tb_color_ok = Color.White;     // textboxes color when input values are correct / incorrect: white / grey
        private Color tb_color_nok = System.Drawing.SystemColors.Control;

        private bool Method_Delay;                   // do we use "release cursor after delay" feature
        private bool Method_CtrlKey;                 // do we use "release cursor on ctrl key" feature
        private int  UnClipDelay;                    // cursor release delay
        private bool UseMouseJump;                   // do we use "mouse teleport" feature on pressing Ctrl + ~

        private const string StartWithWindowsRegPath // path to registry entry for automatic startup
            = @"Software\Microsoft\Windows\CurrentVersion\Run";

        private string StartMenuShortcutPath =       // path to start menu shortcut
            Path.Combine(Path.Combine(Path.Combine(  // concatenate "start menu", "programs", "DDMM", lnk file
              Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs"), "DDMM"), "ddmm.lnk");

        private Timer clipTimer;    // sets clipping again every second or so
        private Timer UnclipTimer;  // removes clipping after N milliseconds on screen border
        private Timer ReclipTimer;  // sets clipping again after N milliseconds of letting the cursor cross border
        private Timer PreviewTimer; // updates preview screens when moved, delayed to allow less overhead on system changes

        # endregion

        # region Class constructor ------------------------------------------------------------------------------------

        public DDMM_Form()
        {
            InitializeComponent();

            # region Variables ----------------------------------------------------------------------------------------

            // screen objects, that will store screen coordinates and contain a preview rectangle form
            Screens = new Screen[4]; // we will use cells 1 to 3 only - I don't like having "screen 2" in cell 1
            Color[] ScreenColors = new Color[4] { Color.Black, Color.Lime, Color.Blue, Color.Orange };
            for (int i = 1; i <= 3; i++) 
            { 
                Screens[i] = new Screen();
                Screens[i].form_s.Owner = this; 
                Screens[i].form_s.borderColor = ScreenColors[i];
                Screens[i].form_s.FormName = "Screen " + i;
                Screens[i].form_s.LocationChanged += new EventHandler(CheckWhyPreviewMoved);
            }

            GetClipCursor(ref OrigClipRect);   // store original clipping zone
            CurrentClipRect = new Rectangle(); // current clipping zone

            # endregion

            #region Mouse clipping -----------------------------------------------------------------------------------

            clipTimer = new Timer();    // timer to reactivate mouse clipping periodically (because alt-tabbing between 2 other apps will deactivate clipping)
            clipTimer.Interval = 500;   // reactivate mouse clipping every 500 ms
            clipTimer.Tick += new EventHandler(setClip);
            clipTimer.Start();

            UnclipTimer = new Timer();  // timer to deactivate clipping when user places the cursor on the border of the screen for N milliseconds (N is configurable)
            UnclipTimer.Tick += new EventHandler(UnClip);

            ReclipTimer = new Timer();  // timer to reactivate clipping after letting the cursor cross screen borders
            ReclipTimer.Interval = 100; // 100ms allowed to cross screen border before clipping again
            ReclipTimer.Tick += new EventHandler(ReClip);

            PreviewTimer = new Timer();  // timer to move preview screens back to their locations
            PreviewTimer.Interval = 1000; // 1 sec delay allowed to prevent system overhead
            PreviewTimer.Tick += new EventHandler(PreviewRestoreAfterTimer);

            #endregion

            #region Mouse hook ---------------------------------------------------------------------------------------

            // get mouse coordinates even if app runs on background (hook)
            m_callback = new HookProc(LowLevelMouseProc);
            mhook = SetWindowsHookEx(WH_MOUSE_LL, m_callback, GetModuleHandle(null), 0);

            #endregion

            #region Keyboard hook ------------------------------------------------------------------------------------

            // get key presses even if app runs on background (hook)
            k_callback = new HookProc(LowLevelKeyboardProc);
            khook = SetWindowsHookEx(WH_KEYBOARD_LL, k_callback, GetModuleHandle(null), 0);

            #endregion

            #region Display hook -------------------------------------------------------------------------------------

            //// get notifications about display windows focus changes (hook)
            //d_callback = new HookProc(DisplayCtrlProc);
            //IntPtr dhook = SetWindowsHookEx(WH_CBT, d_callback, GetModuleHandle(null), 0);
            //// Look for system message of focus change to initiate reclip instead of doing it with timer and boolean vars
            // All the above - needs to be done in external DLL

            // Handle DisplaySettingsChanged event to recompute boundaries on resolution change of any kind
            SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);

            #endregion

            #region Application exit hook ----------------------------------------------------------------------------

            Application.ApplicationExit += new EventHandler(just_before_ApplicationExit);

            #endregion

            # region Settings -----------------------------------------------------------------------------------------

            LoadSettings();               // load previously input settings
            ApplySettings();              // apply current settings
            HandleSettingsChanges = true; // so that further changes in settings will trigger data validation
            
            # endregion
        }

        # endregion

        #region Mouse clipping ---------------------------------------------------------------------------------------

        /* Mouse clipping is lost every time user switches from an application to another.
         * But as you will read on the internet, "Global hooks are not supported in the .NET Framework", 
         * which means there is no way to get a notification when user switches from app B to app C
         * In order to keep clipping alive we'll have either to
         * - write this app in pure C for example (no)
         * - use a timer to reset clipping every second or so (yes) */
        [DllImport("user32.dll")] static extern bool ClipCursor(ref Rectangle lpRect);
        [DllImport("user32.dll")] static extern bool GetClipCursor(ref Rectangle lpRect);

        private void setClip(object sender, EventArgs eArgs) // reactivates mouse clipping
        {
            // Needs to be substituted with external DLL w/ global hook
            if (ActivateProgram && ActiveClip)
                    ClipCursor(ref DummyClipRect);
            if (UsePreview)
                DrawOnTaskBar(); // also make sure screens previews can draw on top of taskbar (must be done periodically too, since taskbar must be brought to front when switching between apps)
        }

        private void UnClip(object sender, EventArgs eArgs) // releases clipping after N milliseconds on screen border
        {
            ClipCursor(ref OrigClipRect); // clip to original zone
            ActiveClip = false;           // no active clip
            CanClip = false;              // prevent re-clipping for a while
            notifyIcon1.Icon = new Icon(GetType(), "ddmm_normal.ico"); // tray icon = no activated screen
            ReclipTimer.Start();          // prepares re-clipping with timer
        }
        
        private void ReClip(object sender, EventArgs eArgs) // ends cursor releasing period
        {
            CanClip = true;               // we dont reactivate clipping, we just flag clipping as allowed
            ReclipTimer.Stop();           // stop this timer
        }

        #endregion

        #region Hook definitions -------------------------------------------------------------------------------------

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern IntPtr GetModuleHandle(string moduleName);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, uint wParam, IntPtr lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        #endregion

        #region Mouse hook -------------------------------------------------------------------------------------------

        private HookProc m_callback;
        static int WH_MOUSE_LL = 14;
        delegate IntPtr HookProc(int nCode, uint wParam, IntPtr lParam);
        IntPtr LowLevelMouseProc(int nCode, uint wParam, IntPtr lParam)
        {
            MouseMoved(Cursor.Position.X, Cursor.Position.Y); // action: call MouseMoved with coordinates
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }
        #endregion

        #region Keyboard hook ----------------------------------------------------------------------------------------

        private HookProc k_callback;
        static int WH_KEYBOARD_LL = 13;
        static int WM_KEYDOWN = 0x100;
        static int WM_KEYUP = 0x101;
        IntPtr LowLevelKeyboardProc(int nCode, uint wParam, IntPtr lParam)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            Keys key = ((Keys)vkCode);

            if (nCode >= 0)
            {
                if (key == Keys.LControlKey)
                {
                    if (wParam == WM_KEYDOWN) CtrlKeyPressed();  // Call function for Ctrl key pressed / released
                    if (wParam == WM_KEYUP) CtrlKeyReleased();
                }

                if ((wParam == WM_KEYDOWN) && (key == Keys.D) && ((Keys.Control | Keys.Alt) == Control.ModifierKeys))
                {
                    EmergencyRestore(); // Ctrl+Alt+D: emergency restore ways of exiting mouse clipping
                }

                if (UseMouseJump && (wParam == WM_KEYDOWN) && (key == Keys.Oemtilde) && (Keys.Control == Control.ModifierKeys))
                {
                    MouseTeleport(Cursor.Position.X, Cursor.Position.Y); // Ctrl + ~ : teleports mouse to the next screen
                }
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        #endregion

        #region Display hook -----------------------------------------------------------------------------------------

        private void AutoDetectMonitors() // self-explanatory
        {

            tb_s1left.Text = System.Windows.Forms.Screen.AllScreens[0].Bounds.Left.ToString();
            tb_s1right.Text = System.Windows.Forms.Screen.AllScreens[0].Bounds.Right.ToString();
            tb_s1top.Text = System.Windows.Forms.Screen.AllScreens[0].Bounds.Top.ToString();
            tb_s1bottom.Text = System.Windows.Forms.Screen.AllScreens[0].Bounds.Bottom.ToString();

            if (System.Windows.Forms.Screen.AllScreens.Length > 1)
            {
                tb_s2left.Text = System.Windows.Forms.Screen.AllScreens[1].Bounds.Left.ToString();
                tb_s2right.Text = System.Windows.Forms.Screen.AllScreens[1].Bounds.Right.ToString();
                tb_s2top.Text = System.Windows.Forms.Screen.AllScreens[1].Bounds.Top.ToString();
                tb_s2bottom.Text = System.Windows.Forms.Screen.AllScreens[1].Bounds.Bottom.ToString();
            }
            else
            {
                tb_s2left.Text = "";
                tb_s2right.Text = "";
                tb_s2top.Text = "";
                tb_s2bottom.Text = "";
            }

            if (System.Windows.Forms.Screen.AllScreens.Length > 2)
            {
                tb_s3left.Text = System.Windows.Forms.Screen.AllScreens[2].Bounds.Left.ToString();
                tb_s3right.Text = System.Windows.Forms.Screen.AllScreens[2].Bounds.Right.ToString();
                tb_s3top.Text = System.Windows.Forms.Screen.AllScreens[2].Bounds.Top.ToString();
                tb_s3bottom.Text = System.Windows.Forms.Screen.AllScreens[2].Bounds.Bottom.ToString();
            }
            else
            {
                tb_s3left.Text = "";
                tb_s3right.Text = "";
                tb_s3top.Text = "";
                tb_s3bottom.Text = "";
            }
        }


        private void PreviewScreens() // self-explanatory
        {
            UpdatingPreview = true;
            PreviewTimer.Stop();
            for (int i = 1; i <= 3; i++)
            {
                if (UsePreview && Screens[i].ok)
                {
                    do
                    {
                        Screens[i].form_s.Left = Screens[i].Left;
                        Screens[i].form_s.Top = Screens[i].Top;
                        Screens[i].form_s.Width = Screens[i].Right - Screens[i].Left;
                        Screens[i].form_s.Height = Screens[i].Bottom - Screens[i].Top;
                        Screens[i].form_s.Invalidate();
                        Screens[i].form_s.Show();

                    } while (Screens[i].form_s.Left != Screens[i].Left);
                }
                else
                {
                    Screens[i].form_s.Hide(); // dont't show if screen coordinates are not ok
                }
            }
            UpdatingPreview = false;
        }

        void CheckWhyPreviewMoved(object sender, EventArgs e)
        {
            if (!UpdatingPreview)
                PreviewTimer.Start();
        }

        void PreviewRestoreAfterTimer(object sender, EventArgs e)
        {
            PreviewTimer.Stop();
            PreviewScreens();
        }
        


        //private HookProc d_callback;
        //static int WH_CBT = 5;
        //static uint WM_SETFOCUS = 0x7;
        //IntPtr DisplayCtrlProc(int nCode, uint wParam, IntPtr lParam)
        //{
        //    if ((nCode >= 0) && (wParam == WM_SETFOCUS))
        //    {
        //        l_mousepos.Text = "Window Focus changed";
        //        // To check and fill the code for capturing window focus changes for any window
        //    }            
        //    return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        //}
        // All the above - needs to be done in external DLL

        void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            if (UseAutoBounds)
            {
                HandleSettingsChanges = false; // so that changing settings won't trigger data validation events
                ClearSettings();
                AutoDetectMonitors();                
                ApplySettings();
                HandleSettingsChanges = true;

                // change the tray icon according to the new screen position, using clip
                ActiveClip = false;           // no active clip
                CanClip = true;
                MouseMoved(Cursor.Position.X, Cursor.Position.Y);

                // disable clipping if only one screen
                ActiveClip = false;
                if (System.Windows.Forms.Screen.AllScreens.Length > 1)
                    CanClip = true;

                PreviewScreens();
            }
        }
        #endregion

        # region Draw over taskbar ------------------------------------------------------------------------------------

        // Allow preview rectangles to draw over the taskbar
        const uint SWP_NOSIZE = 0x0001; 
        const uint SWP_NOMOVE = 0x0002; 
        const uint SWP_NOACTIVATE = 0x0010; 
        [DllImport("user32.dll")] 
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags); 
        private void DrawOnTaskBar() 
        {
            for (int i=1; i<=3; i++)
                SetWindowPos(Screens[i].form_s.Handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE); 
        }
        # endregion

        # region Settings ---------------------------------------------------------------------------------------------

        private void LoadSettings() // loads settings in file
        {
            // general options
            cb_activate.Checked          = Properties.Settings.Default.ActivateProgram;
            cb_startwithwindows.Checked  = IsAutoStartEnabled();            // not a setting. read registry to see if autostartup is configured.
            cb_startmenushortcut.Checked = IsStartMenuShortcutPresent();    // not a setting. search for shortcut file.
            cb_hidetrayicon.Checked      = Properties.Settings.Default.HideTrayIcon;

            cb_autobounds.Checked = Properties.Settings.Default.AutoBounds;

            // screen coordinates
            tb_s1left.Text    = Properties.Settings.Default.Screen1Left;
            tb_s1right.Text   = Properties.Settings.Default.Screen1Right;
            tb_s1top.Text     = Properties.Settings.Default.Screen1Top;
            tb_s1bottom.Text  = Properties.Settings.Default.Screen1Bottom;
            tb_s2left.Text    = Properties.Settings.Default.Screen2Left;
            tb_s2right.Text   = Properties.Settings.Default.Screen2Right;
            tb_s2top.Text     = Properties.Settings.Default.Screen2Top;
            tb_s2bottom.Text  = Properties.Settings.Default.Screen2Bottom;
            tb_s3left.Text    = Properties.Settings.Default.Screen3Left;
            tb_s3right.Text   = Properties.Settings.Default.Screen3Right;
            tb_s3top.Text     = Properties.Settings.Default.Screen3Top;
            tb_s3bottom.Text  = Properties.Settings.Default.Screen3Bottom;

            if ( (tb_s1left.Text == "") && (tb_s1right.Text == "") && (tb_s1top.Text == "") && (tb_s1bottom.Text == "")
              && (tb_s2left.Text == "") && (tb_s2right.Text == "") && (tb_s2top.Text == "") && (tb_s2bottom.Text == "")
              && (tb_s3left.Text == "") && (tb_s3right.Text == "") && (tb_s3top.Text == "") && (tb_s3bottom.Text == "") )
            {
                AutoDetectMonitors();
            }

            // screen border crossing method
            tb_unclipdelay.Text              = Properties.Settings.Default.Delay_UnClip;
            cb_allowcrossingctrlkey.Checked  = Properties.Settings.Default.Method_CtrlKey;
            cb_allowcrossingdelay.Checked    = Properties.Settings.Default.Method_Delay;

            // use mouse jump between regions by pressing Ctrl + ~
            cb_mousejump.Checked             = Properties.Settings.Default.UseMouseJump;
            cb_preview.Checked               = Properties.Settings.Default.UsePreview;

        }

        private void ApplySettings() // processes settings changes (after loading or modifying)
        {
            // general options
            ActivateProgram = cb_activate.Checked;
            if (ActivateProgram)
            {
                CanClip = true;               // allow clipping
            }
            else // program deactivated
            {
                ClipCursor(ref OrigClipRect); // clip to original zone
                ActiveClip = false;           // no active clip
                CanClip = false;              // prevent re-clipping
                notifyIcon1.Icon = new Icon(GetType(), "ddmm_normal.ico"); // tray icon = white inactive icon
            }

            // show/hide systray icon
            notifyIcon1.Visible = ! cb_hidetrayicon.Checked;

            UseAutoBounds = cb_autobounds.Checked;

            if (UseAutoBounds)
            {
                tb_s1left.Enabled = false;
                tb_s1right.Enabled = false;
                tb_s1top.Enabled = false;
                tb_s1bottom.Enabled = false;
                tb_s2left.Enabled = false;
                tb_s2right.Enabled = false;
                tb_s2top.Enabled = false;
                tb_s2bottom.Enabled = false;
                tb_s3left.Enabled = false;
                tb_s3right.Enabled = false;
                tb_s3top.Enabled = false;
                tb_s3bottom.Enabled = false;
                l_mousepos.Text = "";
            }
            else
            {
                tb_s1left.Enabled = true;
                tb_s1right.Enabled = true;
                tb_s1top.Enabled = true;
                tb_s1bottom.Enabled = true;
                tb_s2left.Enabled = true;
                tb_s2right.Enabled = true;
                tb_s2top.Enabled = true;
                tb_s2bottom.Enabled = true;
                tb_s3left.Enabled = true;
                tb_s3right.Enabled = true;
                tb_s3top.Enabled = true;
                tb_s3bottom.Enabled = true;
            }

            // screens
            for (int i = 1; i <= 3; i++) Screens[i].ok = true;

            try { Screens[1].Left = Convert.ToInt32(tb_s1left.Text); }
            catch (Exception) { Screens[1].ok = false; }
            try { Screens[1].Right = Convert.ToInt32(tb_s1right.Text); }
            catch (Exception) { Screens[1].ok = false; }
            try { Screens[1].Top = Convert.ToInt32(tb_s1top.Text); }
            catch (Exception) { Screens[1].ok = false; }
            try { Screens[1].Bottom = Convert.ToInt32(tb_s1bottom.Text); }
            catch (Exception) { Screens[1].ok = false; }
            try { Screens[2].Left = Convert.ToInt32(tb_s2left.Text); }
            catch (Exception) { Screens[2].ok = false; }
            try { Screens[2].Right = Convert.ToInt32(tb_s2right.Text); }
            catch (Exception) { Screens[2].ok = false; }
            try { Screens[2].Top = Convert.ToInt32(tb_s2top.Text); }
            catch (Exception) { Screens[2].ok = false; }
            try { Screens[2].Bottom = Convert.ToInt32(tb_s2bottom.Text); }
            catch (Exception) { Screens[2].ok = false; }
            try { Screens[3].Left = Convert.ToInt32(tb_s3left.Text); }
            catch (Exception) { Screens[3].ok = false; }
            try { Screens[3].Right = Convert.ToInt32(tb_s3right.Text); }
            catch (Exception) { Screens[3].ok = false; }
            try { Screens[3].Top = Convert.ToInt32(tb_s3top.Text); }
            catch (Exception) { Screens[3].ok = false; }
            try { Screens[3].Bottom = Convert.ToInt32(tb_s3bottom.Text); }
            catch (Exception) { Screens[3].ok = false; }

            for (int i = 1; i <= 3; i++)
            {
                if (Screens[i].Right < Screens[i].Left) Screens[i].ok = false;
                if (Screens[i].Bottom < Screens[i].Top) Screens[i].ok = false;
            }

            // different textboxes colors if data is correct or incorrect
            tb_s1left.BackColor = tb_s1right.BackColor = tb_s1top.BackColor = tb_s1bottom.BackColor = (Screens[1].ok ? tb_color_ok : tb_color_nok);
            tb_s2left.BackColor = tb_s2right.BackColor = tb_s2top.BackColor = tb_s2bottom.BackColor = (Screens[2].ok ? tb_color_ok : tb_color_nok);
            tb_s3left.BackColor = tb_s3right.BackColor = tb_s3top.BackColor = tb_s3bottom.BackColor = (Screens[3].ok ? tb_color_ok : tb_color_nok);


            // window state
            if (Properties.Settings.Default.Window_Minimized)
            {
                Hide(); // hidden window
            }
            else
            {
                WindowState = FormWindowState.Normal; // visible window
                Show();
                TopMost = true;
                Focus();
                Focus(); // useful double call (!)

            }

            // screen border crossing method

            Method_Delay   = cb_allowcrossingdelay.Checked;
            Method_CtrlKey = cb_allowcrossingctrlkey.Checked;

            UseMouseJump   = cb_mousejump.Checked;

            try
            { 
                UnClipDelay = Convert.ToInt32(tb_unclipdelay.Text);
                UnclipTimer.Interval = UnClipDelay;
                tb_unclipdelay.BackColor = tb_color_ok;
            } 
            catch (Exception) 
            { 
                UnClipDelay = 0;
                Method_Delay = false; // deactivate method if delay is incorrect
                tb_unclipdelay.BackColor = tb_color_nok;
            }
            
        }

        private void SaveSettings() // saves settings (to xml file)
        {
            
            Properties.Settings.Default.ActivateProgram = cb_activate.Checked;

            Properties.Settings.Default.AutoBounds      = cb_autobounds.Checked;

            if (UseAutoBounds) // No need to save any coordinates if AutoBounds are used, they must be empty
            {
                ClearSettings();
            }
            else
            {
                Properties.Settings.Default.Screen1Left   = tb_s1left.Text;
                Properties.Settings.Default.Screen1Right  = tb_s1right.Text;
                Properties.Settings.Default.Screen1Top    = tb_s1top.Text;
                Properties.Settings.Default.Screen1Bottom = tb_s1bottom.Text;
                Properties.Settings.Default.Screen2Left   = tb_s2left.Text;
                Properties.Settings.Default.Screen2Right  = tb_s2right.Text;
                Properties.Settings.Default.Screen2Top    = tb_s2top.Text;
                Properties.Settings.Default.Screen2Bottom = tb_s2bottom.Text;
                Properties.Settings.Default.Screen3Left   = tb_s3left.Text;
                Properties.Settings.Default.Screen3Right  = tb_s3right.Text;
                Properties.Settings.Default.Screen3Top    = tb_s3top.Text;
                Properties.Settings.Default.Screen3Bottom = tb_s3bottom.Text;
            }

            Properties.Settings.Default.Method_CtrlKey  = cb_allowcrossingctrlkey.Checked;
            Properties.Settings.Default.Method_Delay    = cb_allowcrossingdelay.Checked;
            Properties.Settings.Default.Delay_UnClip    = tb_unclipdelay.Text;
            Properties.Settings.Default.UsePreview      = cb_preview.Checked;

            Properties.Settings.Default.UseMouseJump    = cb_mousejump.Checked;

            Properties.Settings.Default.Save();
        }

        private void bt_autodetectscreens_Click(object sender, EventArgs e) // auto detect multiple screens coordinates
        {
            HandleSettingsChanges = false; // so that changing settings won't trigger data validation events
            ClearSettings();
            AutoDetectMonitors();
            ApplySettings();
            HandleSettingsChanges = true;

            PreviewScreens();

            // now that clipping region changed, unlock cursor from previous settings and let clip again
            ClipCursor(ref OrigClipRect); // clip to original zone
            ActiveClip = false;           // no active clip
            CanClip = true;               // allow re-clipping
        }


        private void ClearSettings() // clears settings
        {
            Properties.Settings.Default.Screen1Left   = "";
            Properties.Settings.Default.Screen1Right  = "";
            Properties.Settings.Default.Screen1Top    = "";
            Properties.Settings.Default.Screen1Bottom = "";
            Properties.Settings.Default.Screen2Left   = "";
            Properties.Settings.Default.Screen2Right  = "";
            Properties.Settings.Default.Screen2Top    = "";
            Properties.Settings.Default.Screen2Bottom = "";
            Properties.Settings.Default.Screen3Left   = "";
            Properties.Settings.Default.Screen3Right  = "";
            Properties.Settings.Default.Screen3Top    = "";
            Properties.Settings.Default.Screen3Bottom = "";
        }

        private void ResetSettings() // simulates first run (ctrl-R tweak)
        {
            HandleSettingsChanges = false;
            Properties.Settings.Default.Window_Minimized = false;
            ClearSettings();
            ApplySettings();
            SaveSettings();
            HandleSettingsChanges = true;

            // now that clipping region changed, unlock cursor from previous settings and let clip again
            ClipCursor(ref OrigClipRect); // clip to original zone
            ActiveClip = false;           // no active clip
            CanClip = true;               // allow re-clipping
        }

        private void OptionsChanged(object sender, EventArgs e) // triggered on controls textChange event
        {
            if (HandleSettingsChanges) // do not handle event if change was made by loading settings
            {
                if (cb_autobounds.Checked)
                    AutoDetectMonitors();
                ApplySettings();
                UsePreview = cb_preview.Checked;
                PreviewScreens();
            }
        }

        private void bt_save_Click(object sender, EventArgs e) // "save and close" button
        {
            Properties.Settings.Default.Window_Minimized = true;
            SaveSettings();
            UsePreview = false;
            PreviewScreens();
            Hide();
        }

        private void DDMM_Form_Shown(object sender, EventArgs e)
        {
            ApplySettings(); // must apply settings on form shown (and not on form load) or it won't work (why...?)
            PreviewScreens();
        }

        private void DDMM_Form_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Control) && (e.KeyCode == Keys.R)) // ctrl-R tweak: simulate first run
            {
                ResetSettings();
                PreviewScreens();
            }
        }

        private void EmergencyRestore() // restores standard settings in case cursor is stuck away from settings window and tray icon
        {
            if ((!Method_CtrlKey) && (!Method_Delay)) // if both are unchecked
            {
                cb_allowcrossingdelay.Checked = true; // restore default values
                tb_unclipdelay.Text = "150";
            }
        }

        # endregion

        # region Autostart --------------------------------------------------------------------------------------------

        private void SetAutoStart() // sets auto-start with windows
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(StartWithWindowsRegPath);
            key.SetValue("ddmm", Application.ExecutablePath);
        }

        private bool IsAutoStartEnabled() // gets whether aotostart is on
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(StartWithWindowsRegPath);
            if (key == null) return false;
            string value = (string)key.GetValue("ddmm");
            if (value == null) return false;
            return (value == Application.ExecutablePath);
        }

        private void UnSetAutoStart() // disables autostart
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(StartWithWindowsRegPath);
            key.DeleteValue("ddmm");
        }

        private void cb_startwithwindows_CheckedChanged(object sender, EventArgs e) // triggered on autostart checkbox clicked
        {
            if (cb_startwithwindows.Checked) SetAutoStart(); else UnSetAutoStart();
        }

        # endregion

        # region Start Menu Shortcut ----------------------------------------------------------------------------------

        private void SetStartMenuShortcut() // create start menu shortcut
        {
            try {
                Directory.CreateDirectory(System.IO.Directory.GetParent(StartMenuShortcutPath).ToString()); // create shortcut directory if needed
                // http://www.geekpedia.com/tutorial125_Create-shortcuts-with-a-.NET-application.html
                WshShellClass WshShell = new WshShellClass();
                IWshRuntimeLibrary.IWshShortcut MyShortcut;
                MyShortcut = (IWshRuntimeLibrary.IWshShortcut)WshShell.CreateShortcut(StartMenuShortcutPath);
                MyShortcut.TargetPath = Application.ExecutablePath;
                MyShortcut.Description = "Dual Display Mouse Manager";
                MyShortcut.Save();
              } 
            catch (Exception) 
            {
                cb_startmenushortcut.Checked = false; // failed :-(
            }
        }

        private bool IsStartMenuShortcutPresent() // returns whether start menu shortcut exists
        {
            return System.IO.File.Exists(StartMenuShortcutPath);
        }

        private void UnSetStartMenuShortcut() // removes start menu shortcut
        {
            System.IO.Directory.Delete(System.IO.Directory.GetParent(StartMenuShortcutPath).ToString(), true);
        }

        private void cb_startmenushortcut_CheckedChanged(object sender, EventArgs e) // triggered on start menu checkbox clicked
        {
            if (cb_startmenushortcut.Checked) SetStartMenuShortcut(); else UnSetStartMenuShortcut();
        }

        # endregion

        # region Mouse management -------------------------------------------------------------------------------------

        private void MouseMoved(int X, int Y) // called by mouse hook to handle mouse movements
        {
            // Should modify the following line to happen only on visible form, reducing CPU usage
            if (UsePreview)
                l_mousepos.Text = "Mouse: x=" + X.ToString() + ", y=" + Y.ToString();

            if (ActivateProgram) // do something about mouse (clipping or unclipping) only if program is activated
            {

                // activate clipping
                if ((!ActiveClip) && (CanClip)) // if no current clip and if clipping is allowed
                {
                    int i = 1;
                    while (i <= 3)
                    {
                        if (Screens[i].ok)
                        {
                            CurrentClipRect = new Rectangle(Screens[i].Left, Screens[i].Top, Screens[i].Right - Screens[i].Left, Screens[i].Bottom - Screens[i].Top);
                            if (CurrentClipRect.Contains(X, Y)) // if cursor in one of the screens -> clip to that screen
                            {
                                DummyClipRect = new Rectangle(Screens[i].Left, Screens[i].Top, Screens[i].Right, Screens[i].Bottom); // dummy rectangle with width=right_bound and height=bottom_bound because clipcursor wants a rectangle with left-top-right-bottom and not left-top-width-height
                                ClipCursor(ref DummyClipRect);  // clip the cursor
                                ActiveClip = true;              // flag clipping as active
                                notifyIcon1.Icon = new Icon(GetType(), "ddmm_screen" + ((i == 1) ? "1" : "2") + ".ico"); // change icon (2 icons for different screens)
                                CanClip = false;                // no need to allow new clipping
                                break;                          // skip other screens
                            }
                        }
                        i++;
                    }
                }

                // processes "cursor on the edge of screen" event (unclipping)
                if (ActiveClip && Method_Delay) // if mouse is clipped and crossing method is "after delay on the border of the screen"
                {
                    if ((CurrentClipRect.Left == X) || (CurrentClipRect.Right == X + 1) || (CurrentClipRect.Top == Y) || (CurrentClipRect.Bottom == Y + 1)) // cursor on border
                        UnclipTimer.Start(); // start unclipping timer: we will unclip if timer reaches tick
                    else
                        UnclipTimer.Stop();  // if mouse gets away from border: stop timer
                }

            }

        }

        private void MouseTeleport(int X, int Y) // called by keyboard hook to handle mouse teleport
        {
            ActivateProgram = false;
            // now that clipping region changed, unlock cursor from previous settings and let clip again
            ClipCursor(ref OrigClipRect); // clip to original zone
            CanClip = false;                // no need to allow new clipping
            ActiveClip = false;           // no active clip

            int i = 1;            
            while (i <= 3)
            {
                if (Screens[i].ok)
                {
                    CurrentClipRect = new Rectangle(Screens[i].Left, Screens[i].Top, Screens[i].Right - Screens[i].Left, Screens[i].Bottom - Screens[i].Top);
                    if (CurrentClipRect.Contains(X, Y)) // if cursor in one of the screens ->
                    {
                        int j = (i<3 ? i+1 : 1); // select the next screen
                        int newX, newY;
                        while (!Screens[j].ok) j = (++j > 3 ? 1 : j); // if not valid - find the next valid screen
                        newX = (X - Screens[i].Left) * (Screens[j].Right  - Screens[j].Left) / (Screens[i].Right  - Screens[i].Left); // Count relative X for new screen
                        newY = (Y - Screens[i].Top)  * (Screens[j].Bottom - Screens[j].Top)  / (Screens[i].Bottom - Screens[i].Top);  // Count relative Y for new screen
                        Cursor.Position = new Point(Screens[j].Left + newX, Screens[j].Top + newY); // move the cursor
                        CurrentClipRect = new Rectangle(Screens[j].Left, Screens[j].Top, Screens[j].Right - Screens[j].Left, Screens[j].Bottom - Screens[j].Top);
                        break; // skip other screens
                    }
                }
                i++;
            }

            CanClip = true;
            ActivateProgram = cb_activate.Checked;
            MouseMoved(Cursor.Position.X, Cursor.Position.Y); // complete mouse movement, switch icons, etc
        }

        # endregion

        # region Keyboard management ----------------------------------------------------------------------------------

        private void CtrlKeyPressed()
        {
            if (ActivateProgram) // ctrl key pressed: do something only if program activated
            {
                if (Method_CtrlKey && ActiveClip) // if mouse clipped and crossing method is "ctrl key pressed"
                {
                    ClipCursor(ref OrigClipRect); // unclip (restore original clipping zone)
                    ActiveClip = false;           // flag clipping as inactive
                    CanClip = false;              // prevent new clipping while ctrl is hold
                    notifyIcon1.Icon = new Icon(GetType(), "ddmm_normal.ico");
                }
            }
        }

        private void CtrlKeyReleased()
        {
            if (ActivateProgram)
            {
                if (Method_CtrlKey)
                {
                    CanClip = true;               // flag new clipping as allowed when ctrl key is released
                }
            }
        }

        # endregion

        # region Main form and previews show / hide -------------------------------------------------------------------

        private void DDMM_Form_Deactivate(object sender, EventArgs e) // on form lost focus
        {
            // Keeping just in case
        }

        private void DDMM_Form_Activated(object sender, EventArgs e) // main form shown
        {
            // Keeping the procedure just in case
        }

        # endregion

        # region systray icon -----------------------------------------------------------------------------------------

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e) // systray icon click
        {
            if (e.Button.Equals(MouseButtons.Left))
                RestoreWindow(); // show main window
        }

        private void RestoreWindow()
        {
            Properties.Settings.Default.Window_Minimized = false;
            Properties.Settings.Default.Save(); // save restored default windows state
            
            //            Hide(); // bring to front by hiding+showing?
            Show();
            WindowState = FormWindowState.Normal;
            TopMost = true;
            UsePreview = cb_preview.Checked;
            PreviewScreens();
            Focus();
            Focus();
        }

        // single instance app: 
        // http://www.codeproject.com/KB/cs/CSSIApp.aspx
        // http://social.msdn.microsoft.com/Forums/en-US/csharpgeneral/thread/a5bcfc8a-bf69-4bbc-923d-f30f9ecf5f64
        // se code in program.cs

        public void NotifyNewInstance() // called on second program instance
        {
            cb_hidetrayicon.Checked = false; // restore tray icon on 2nd program instance
            RestoreWindow();                 // and restore window
        }

        private void cm_Restore_Click(object sender, EventArgs e) // systray menu restore main window
        {
            RestoreWindow();
        }

        private void cm_Exit_Click(object sender, EventArgs e) // systray menu exit
        {
            Application.Exit();
        }

        
        # endregion

        # region other controls ---------------------------------------------------------------------------------------

        private void lk_link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo(e.Link.LinkData.ToString());
            Process.Start(sInfo);
        }

        private void DDMM_Form_Load(object sender, EventArgs e)
        {
            lk_link.Links.Remove(lk_link.Links[0]);
            lk_link.Links.Add(0, lk_link.Text.Length, "http://ddmm.sf.net/");
        }

        private void DDMM_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();

                UsePreview = false;

                Properties.Settings.Default.Window_Minimized = true;
                Properties.Settings.Default.Save(); // save default windows state
            }
        }

        private void just_before_ApplicationExit(object sender, EventArgs e) // processing on application exit event
        {
            // restore original cursor clipping state
            ClipCursor(ref OrigClipRect);
            // unregister all the hooks
            for (int i = 1; i <= 3; i++)
                Screens[i].form_s.LocationChanged -= new EventHandler(CheckWhyPreviewMoved);
            SystemEvents.DisplaySettingsChanged -= new EventHandler(SystemEvents_DisplaySettingsChanged);
            PreviewTimer.Tick -= new EventHandler(PreviewRestoreAfterTimer);
            UnhookWindowsHookEx(mhook); // remove global mouse hook
            UnhookWindowsHookEx(khook); // remove global keyboard hook
        }

        # endregion

        private void l_debug_Click(object sender, EventArgs e)
        {
            l_debug.Text = "";
        }
    }

}
