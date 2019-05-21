/*
David CHATEAU for DDMM.
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 2 of the License.
*/

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DDMM.Properties;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using File = System.IO.File;

namespace DDMM
{
    public partial class DdmmForm : Form
    {
        # region Class constructor ------------------------------------------------------------------------------------

        public DdmmForm()
        {
            InitializeComponent();

            # region Variables ----------------------------------------------------------------------------------------

            // screen objects, that will store screen coordinates and contain a preview rectangle form
            _screens = new Screen[4]; // we will use cells 1 to 3 only - I don't like having "screen 2" in cell 1
            var screenColors = new[] {Color.Black, Color.Lime, Color.Blue, Color.Orange};
            for (var i = 1; i <= 3; i++)
            {
                _screens[i] = new Screen();
                _screens[i].FormS.Owner = this;
                _screens[i].FormS.BorderColor = screenColors[i];
                _screens[i].FormS.FormName = "Screen " + i;
                _screens[i].FormS.LocationChanged += CheckWhyPreviewMoved;
            }

            GetClipCursor(ref _origClipRect); // store original clipping zone
            _currentClipRect = new Rectangle(); // current clipping zone

            # endregion

            #region Mouse clipping -----------------------------------------------------------------------------------

            _clipTimer =
                new Timer(); // timer to reactivate mouse clipping periodically (because alt-tabbing between 2 other apps will deactivate clipping)
            _clipTimer.Interval = 500; // reactivate mouse clipping every 500 ms
            _clipTimer.Tick += SetClip;
            _clipTimer.Start();

            _unclipTimer =
                new Timer(); // timer to deactivate clipping when user places the cursor on the border of the screen for N milliseconds (N is configurable)
            _unclipTimer.Tick += UnClip;

            _reclipTimer = new Timer(); // timer to reactivate clipping after letting the cursor cross screen borders
            _reclipTimer.Interval = 100; // 100ms allowed to cross screen border before clipping again
            _reclipTimer.Tick += ReClip;

            _previewTimer = new Timer(); // timer to move preview screens back to their locations
            _previewTimer.Interval = 1000; // 1 sec delay allowed to prevent system overhead
            _previewTimer.Tick += PreviewRestoreAfterTimer;

            #endregion

            #region Mouse hook ---------------------------------------------------------------------------------------

            // get mouse coordinates even if app runs on background (hook)
            _mCallback = LowLevelMouseProc;
            _mhook = SetWindowsHookEx(WhMouseLl, _mCallback, GetModuleHandle(null), 0);

            #endregion

            #region Keyboard hook ------------------------------------------------------------------------------------

            // get key presses even if app runs on background (hook)
            _kCallback = LowLevelKeyboardProc;
            _khook = SetWindowsHookEx(WhKeyboardLl, _kCallback, GetModuleHandle(null), 0);

            #endregion

            #region Display hook -------------------------------------------------------------------------------------

            //// get notifications about display windows focus changes (hook)
            //d_callback = new HookProc(DisplayCtrlProc);
            //IntPtr dhook = SetWindowsHookEx(WH_CBT, d_callback, GetModuleHandle(null), 0);
            //// Look for system message of focus change to initiate reclip instead of doing it with timer and boolean vars
            // All the above - needs to be done in external DLL

            // Handle DisplaySettingsChanged event to recompute boundaries on resolution change of any kind
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

            #endregion

            #region Application exit hook ----------------------------------------------------------------------------

            Application.ApplicationExit += just_before_ApplicationExit;

            #endregion

            # region Settings -----------------------------------------------------------------------------------------

            LoadSettings(); // load previously input settings
            ApplySettings(); // apply current settings
            _handleSettingsChanges = true; // so that further changes in settings will trigger data validation

            # endregion
        }

        # endregion

        private void l_debug_Click(object sender, EventArgs e)
        {
            l_debug.Text = "";
        }

        # region Class fields -----------------------------------------------------------------------------------------

        private class Screen
        {
            public readonly FormScreen FormS; // preview form (colored rectangle)
            public int Left, Right, Top, Bottom; // coordinates
            public bool Ok; // are screen coordinates ok

            public Screen()
            {
                FormS = new FormScreen();
            }
        }

        private readonly Screen[] _screens;

        private readonly IntPtr _mhook; // Hook pointers for mouse and keyboard hooks
        private readonly IntPtr _khook; // Hook pointers for mouse and keyboard hooks

        private bool _handleSettingsChanges = false; // will be set to true after loading settings

        private bool
            _updatingPreview =
                false; // set to true when updating preview rectangles, because sometimes desktop managers throw them around

        private Rectangle _origClipRect; // original clipping zone (used to restore previous state)
        private Rectangle _currentClipRect; // current clipping zone (used to reactivate clipping periodically)

        private Rectangle
            _dummyClipRect; // dummy Rectangle variable to pass to ClipCursor(), because it seems that Clipcursor() needs a "top-left-right-bottom" rectangle instead of c# standard "top-left-width-height"

        private bool _activateProgram = false; // is the program (mouse management) activated
        private bool _activeClip = false; // is the cursor currently clipped

        private bool
            _canClip = true; // is it allowed to activate clipping (it is not allowed when holding ctrl key, and not allowed during 100ms when we release cursor)

        private bool _useAutoBounds = false; // Uses automatic screen boundary control
        private bool _usePreview = false; // Uses preview screens for region selection

        private readonly Color
            _tbColorOk = Color.White; // textboxes color when input values are correct / incorrect: white / grey

        private readonly Color _tbColorNok = SystemColors.Control;

        private bool _methodDelay; // do we use "release cursor after delay" feature
        private bool _methodCtrlKey; // do we use "release cursor on ctrl key" feature
        private int _unClipDelay; // cursor release delay
        private bool _useMouseJump; // do we use "mouse teleport" feature on pressing Ctrl + ~

        private const string StartWithWindowsRegPath // path to registry entry for automatic startup
            = @"Software\Microsoft\Windows\CurrentVersion\Run";

        private readonly string _startMenuShortcutPath = // path to start menu shortcut
            Path.Combine(Path.Combine(Path.Combine( // concatenate "start menu", "programs", "DDMM", lnk file
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs"), "DDMM"), "ddmm.lnk");

        private readonly Timer _clipTimer; // sets clipping again every second or so
        private readonly Timer _unclipTimer; // removes clipping after N milliseconds on screen border

        private readonly Timer
            _reclipTimer; // sets clipping again after N milliseconds of letting the cursor cross border

        private readonly Timer
            _previewTimer; // updates preview screens when moved, delayed to allow less overhead on system changes

        # endregion

        #region Mouse clipping ---------------------------------------------------------------------------------------

        /* Mouse clipping is lost every time user switches from an application to another.
         * But as you will read on the internet, "Global hooks are not supported in the .NET Framework", 
         * which means there is no way to get a notification when user switches from app B to app C
         * In order to keep clipping alive we'll have either to
         * - write this app in pure C for example (no)
         * - use a timer to reset clipping every second or so (yes) */
        [DllImport("user32.dll")]
        private static extern bool ClipCursor(ref Rectangle lpRect);

        [DllImport("user32.dll")]
        private static extern bool GetClipCursor(ref Rectangle lpRect);

        private void SetClip(object sender, EventArgs eArgs) // reactivates mouse clipping
        {
            // Needs to be substituted with external DLL w/ global hook
            if (_activateProgram && _activeClip)
                ClipCursor(ref _dummyClipRect);
            if (_usePreview)
                DrawOnTaskBar(); // also make sure screens previews can draw on top of taskbar (must be done periodically too, since taskbar must be brought to front when switching between apps)
        }

        private void UnClip(object sender, EventArgs eArgs) // releases clipping after N milliseconds on screen border
        {
            ClipCursor(ref _origClipRect); // clip to original zone
            _activeClip = false; // no active clip
            _canClip = false; // prevent re-clipping for a while
            notifyIcon1.Icon = new Icon(GetType(), "ddmm_normal.ico"); // tray icon = no activated screen
            _reclipTimer.Start(); // prepares re-clipping with timer
        }

        private void ReClip(object sender, EventArgs eArgs) // ends cursor releasing period
        {
            _canClip = true; // we dont reactivate clipping, we just flag clipping as allowed
            _reclipTimer.Stop(); // stop this timer
        }

        #endregion

        #region Hook definitions -------------------------------------------------------------------------------------

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, uint wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        #endregion

        #region Mouse hook -------------------------------------------------------------------------------------------

        private readonly HookProc _mCallback;
        private static readonly int WhMouseLl = 14;

        private delegate IntPtr HookProc(int nCode, uint wParam, IntPtr lParam);

        private IntPtr LowLevelMouseProc(int nCode, uint wParam, IntPtr lParam)
        {
            MouseMoved(Cursor.Position.X, Cursor.Position.Y); // action: call MouseMoved with coordinates
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        #endregion

        #region Keyboard hook ----------------------------------------------------------------------------------------

        private readonly HookProc _kCallback;
        private static readonly int WhKeyboardLl = 13;
        private static readonly int WmKeydown = 0x100;
        private static readonly int WmKeyup = 0x101;

        private IntPtr LowLevelKeyboardProc(int nCode, uint wParam, IntPtr lParam)
        {
            var vkCode = Marshal.ReadInt32(lParam);
            var key = (Keys) vkCode;

            if (nCode >= 0)
            {
                if (key == Keys.LControlKey)
                {
                    if (wParam == WmKeydown) CtrlKeyPressed(); // Call function for Ctrl key pressed / released
                    if (wParam == WmKeyup) CtrlKeyReleased();
                }

                if (wParam == WmKeydown && key == Keys.D && (Keys.Control | Keys.Alt) == ModifierKeys)
                    EmergencyRestore(); // Ctrl+Alt+D: emergency restore ways of exiting mouse clipping

                if (_useMouseJump && wParam == WmKeydown && key == Keys.Oemtilde && Keys.Control == ModifierKeys)
                    MouseTeleport(Cursor.Position.X,
                        Cursor.Position.Y); // Ctrl + ~ : teleports mouse to the next screen
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
            _updatingPreview = true;
            _previewTimer.Stop();
            for (var i = 1; i <= 3; i++)
                if (_usePreview && _screens[i].Ok)
                    do
                    {
                        _screens[i].FormS.Left = _screens[i].Left;
                        _screens[i].FormS.Top = _screens[i].Top;
                        _screens[i].FormS.Width = _screens[i].Right - _screens[i].Left;
                        _screens[i].FormS.Height = _screens[i].Bottom - _screens[i].Top;
                        _screens[i].FormS.Invalidate();
                        _screens[i].FormS.Show();
                    } while (_screens[i].FormS.Left != _screens[i].Left);
                else
                    _screens[i].FormS.Hide(); // dont't show if screen coordinates are not ok

            _updatingPreview = false;
        }

        private void CheckWhyPreviewMoved(object sender, EventArgs e)
        {
            if (!_updatingPreview)
                _previewTimer.Start();
        }

        private void PreviewRestoreAfterTimer(object sender, EventArgs e)
        {
            _previewTimer.Stop();
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

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            if (_useAutoBounds)
            {
                _handleSettingsChanges = false; // so that changing settings won't trigger data validation events
                ClearSettings();
                AutoDetectMonitors();
                ApplySettings();
                _handleSettingsChanges = true;

                // change the tray icon according to the new screen position, using clip
                _activeClip = false; // no active clip
                _canClip = true;
                MouseMoved(Cursor.Position.X, Cursor.Position.Y);

                // disable clipping if only one screen
                _activeClip = false;
                if (System.Windows.Forms.Screen.AllScreens.Length > 1)
                    _canClip = true;

                PreviewScreens();
            }
        }

        #endregion

        # region Draw over taskbar ------------------------------------------------------------------------------------

        // Allow preview rectangles to draw over the taskbar
        private const uint SwpNosize = 0x0001;
        private const uint SwpNomove = 0x0002;
        private const uint SwpNoactivate = 0x0010;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy,
            uint uFlags);

        private void DrawOnTaskBar()
        {
            for (var i = 1; i <= 3; i++)
                SetWindowPos(_screens[i].FormS.Handle, IntPtr.Zero, 0, 0, 0, 0,
                    SwpNosize | SwpNomove | SwpNoactivate);
        }

        # endregion

        # region Settings ---------------------------------------------------------------------------------------------

        private void LoadSettings() // loads settings in file
        {
            // general options
            cb_activate.Checked = Settings.Default.ActivateProgram;
            cb_startwithwindows.Checked =
                IsAutoStartEnabled(); // not a setting. read registry to see if autostartup is configured.
            cb_startmenushortcut.Checked = IsStartMenuShortcutPresent(); // not a setting. search for shortcut file.
            cb_hidetrayicon.Checked = Settings.Default.HideTrayIcon;

            cb_autobounds.Checked = Settings.Default.AutoBounds;

            // screen coordinates
            tb_s1left.Text = Settings.Default.Screen1Left;
            tb_s1right.Text = Settings.Default.Screen1Right;
            tb_s1top.Text = Settings.Default.Screen1Top;
            tb_s1bottom.Text = Settings.Default.Screen1Bottom;
            tb_s2left.Text = Settings.Default.Screen2Left;
            tb_s2right.Text = Settings.Default.Screen2Right;
            tb_s2top.Text = Settings.Default.Screen2Top;
            tb_s2bottom.Text = Settings.Default.Screen2Bottom;
            tb_s3left.Text = Settings.Default.Screen3Left;
            tb_s3right.Text = Settings.Default.Screen3Right;
            tb_s3top.Text = Settings.Default.Screen3Top;
            tb_s3bottom.Text = Settings.Default.Screen3Bottom;

            if (tb_s1left.Text == "" && tb_s1right.Text == "" && tb_s1top.Text == "" && tb_s1bottom.Text == ""
                && tb_s2left.Text == "" && tb_s2right.Text == "" && tb_s2top.Text == "" && tb_s2bottom.Text == ""
                && tb_s3left.Text == "" && tb_s3right.Text == "" && tb_s3top.Text == "" && tb_s3bottom.Text == "")
                AutoDetectMonitors();

            // screen border crossing method
            tb_unclipdelay.Text = Settings.Default.Delay_UnClip;
            cb_allowcrossingctrlkey.Checked = Settings.Default.Method_CtrlKey;
            cb_allowcrossingdelay.Checked = Settings.Default.Method_Delay;

            // use mouse jump between regions by pressing Ctrl + ~
            cb_mousejump.Checked = Settings.Default.UseMouseJump;
            cb_preview.Checked = Settings.Default.UsePreview;
        }

        private void ApplySettings() // processes settings changes (after loading or modifying)
        {
            // general options
            _activateProgram = cb_activate.Checked;
            if (_activateProgram)
            {
                _canClip = true; // allow clipping
            }
            else // program deactivated
            {
                ClipCursor(ref _origClipRect); // clip to original zone
                _activeClip = false; // no active clip
                _canClip = false; // prevent re-clipping
                notifyIcon1.Icon = new Icon(GetType(), "ddmm_normal.ico"); // tray icon = white inactive icon
            }

            // show/hide systray icon
            notifyIcon1.Visible = !cb_hidetrayicon.Checked;

            _useAutoBounds = cb_autobounds.Checked;

            if (_useAutoBounds)
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
            for (var i = 1; i <= 3; i++) _screens[i].Ok = true;

            try
            {
                _screens[1].Left = Convert.ToInt32(tb_s1left.Text);
            }
            catch (Exception)
            {
                _screens[1].Ok = false;
            }

            try
            {
                _screens[1].Right = Convert.ToInt32(tb_s1right.Text);
            }
            catch (Exception)
            {
                _screens[1].Ok = false;
            }

            try
            {
                _screens[1].Top = Convert.ToInt32(tb_s1top.Text);
            }
            catch (Exception)
            {
                _screens[1].Ok = false;
            }

            try
            {
                _screens[1].Bottom = Convert.ToInt32(tb_s1bottom.Text);
            }
            catch (Exception)
            {
                _screens[1].Ok = false;
            }

            try
            {
                _screens[2].Left = Convert.ToInt32(tb_s2left.Text);
            }
            catch (Exception)
            {
                _screens[2].Ok = false;
            }

            try
            {
                _screens[2].Right = Convert.ToInt32(tb_s2right.Text);
            }
            catch (Exception)
            {
                _screens[2].Ok = false;
            }

            try
            {
                _screens[2].Top = Convert.ToInt32(tb_s2top.Text);
            }
            catch (Exception)
            {
                _screens[2].Ok = false;
            }

            try
            {
                _screens[2].Bottom = Convert.ToInt32(tb_s2bottom.Text);
            }
            catch (Exception)
            {
                _screens[2].Ok = false;
            }

            try
            {
                _screens[3].Left = Convert.ToInt32(tb_s3left.Text);
            }
            catch (Exception)
            {
                _screens[3].Ok = false;
            }

            try
            {
                _screens[3].Right = Convert.ToInt32(tb_s3right.Text);
            }
            catch (Exception)
            {
                _screens[3].Ok = false;
            }

            try
            {
                _screens[3].Top = Convert.ToInt32(tb_s3top.Text);
            }
            catch (Exception)
            {
                _screens[3].Ok = false;
            }

            try
            {
                _screens[3].Bottom = Convert.ToInt32(tb_s3bottom.Text);
            }
            catch (Exception)
            {
                _screens[3].Ok = false;
            }

            for (var i = 1; i <= 3; i++)
            {
                if (_screens[i].Right < _screens[i].Left) _screens[i].Ok = false;
                if (_screens[i].Bottom < _screens[i].Top) _screens[i].Ok = false;
            }

            // different textboxes colors if data is correct or incorrect
            tb_s1left.BackColor = tb_s1right.BackColor =
                tb_s1top.BackColor = tb_s1bottom.BackColor = _screens[1].Ok ? _tbColorOk : _tbColorNok;
            tb_s2left.BackColor = tb_s2right.BackColor =
                tb_s2top.BackColor = tb_s2bottom.BackColor = _screens[2].Ok ? _tbColorOk : _tbColorNok;
            tb_s3left.BackColor = tb_s3right.BackColor =
                tb_s3top.BackColor = tb_s3bottom.BackColor = _screens[3].Ok ? _tbColorOk : _tbColorNok;


            // window state
            if (Settings.Default.Window_Minimized)
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

            _methodDelay = cb_allowcrossingdelay.Checked;
            _methodCtrlKey = cb_allowcrossingctrlkey.Checked;

            _useMouseJump = cb_mousejump.Checked;

            try
            {
                _unClipDelay = Convert.ToInt32(tb_unclipdelay.Text);
                _unclipTimer.Interval = _unClipDelay;
                tb_unclipdelay.BackColor = _tbColorOk;
            }
            catch (Exception)
            {
                _unClipDelay = 0;
                _methodDelay = false; // deactivate method if delay is incorrect
                tb_unclipdelay.BackColor = _tbColorNok;
            }
        }

        private void SaveSettings() // saves settings (to xml file)
        {
            Settings.Default.ActivateProgram = cb_activate.Checked;

            Settings.Default.AutoBounds = cb_autobounds.Checked;

            if (_useAutoBounds) // No need to save any coordinates if AutoBounds are used, they must be empty
            {
                ClearSettings();
            }
            else
            {
                Settings.Default.Screen1Left = tb_s1left.Text;
                Settings.Default.Screen1Right = tb_s1right.Text;
                Settings.Default.Screen1Top = tb_s1top.Text;
                Settings.Default.Screen1Bottom = tb_s1bottom.Text;
                Settings.Default.Screen2Left = tb_s2left.Text;
                Settings.Default.Screen2Right = tb_s2right.Text;
                Settings.Default.Screen2Top = tb_s2top.Text;
                Settings.Default.Screen2Bottom = tb_s2bottom.Text;
                Settings.Default.Screen3Left = tb_s3left.Text;
                Settings.Default.Screen3Right = tb_s3right.Text;
                Settings.Default.Screen3Top = tb_s3top.Text;
                Settings.Default.Screen3Bottom = tb_s3bottom.Text;
            }

            Settings.Default.Method_CtrlKey = cb_allowcrossingctrlkey.Checked;
            Settings.Default.Method_Delay = cb_allowcrossingdelay.Checked;
            Settings.Default.Delay_UnClip = tb_unclipdelay.Text;
            Settings.Default.UsePreview = cb_preview.Checked;

            Settings.Default.UseMouseJump = cb_mousejump.Checked;

            Settings.Default.Save();
        }

        private void bt_autodetectscreens_Click(object sender, EventArgs e) // auto detect multiple screens coordinates
        {
            _handleSettingsChanges = false; // so that changing settings won't trigger data validation events
            ClearSettings();
            AutoDetectMonitors();
            ApplySettings();
            _handleSettingsChanges = true;

            PreviewScreens();

            // now that clipping region changed, unlock cursor from previous settings and let clip again
            ClipCursor(ref _origClipRect); // clip to original zone
            _activeClip = false; // no active clip
            _canClip = true; // allow re-clipping
        }


        private void ClearSettings() // clears settings
        {
            Settings.Default.Screen1Left = "";
            Settings.Default.Screen1Right = "";
            Settings.Default.Screen1Top = "";
            Settings.Default.Screen1Bottom = "";
            Settings.Default.Screen2Left = "";
            Settings.Default.Screen2Right = "";
            Settings.Default.Screen2Top = "";
            Settings.Default.Screen2Bottom = "";
            Settings.Default.Screen3Left = "";
            Settings.Default.Screen3Right = "";
            Settings.Default.Screen3Top = "";
            Settings.Default.Screen3Bottom = "";
        }

        private void ResetSettings() // simulates first run (ctrl-R tweak)
        {
            _handleSettingsChanges = false;
            Settings.Default.Window_Minimized = false;
            ClearSettings();
            ApplySettings();
            SaveSettings();
            _handleSettingsChanges = true;

            // now that clipping region changed, unlock cursor from previous settings and let clip again
            ClipCursor(ref _origClipRect); // clip to original zone
            _activeClip = false; // no active clip
            _canClip = true; // allow re-clipping
        }

        private void OptionsChanged(object sender, EventArgs e) // triggered on controls textChange event
        {
            if (_handleSettingsChanges) // do not handle event if change was made by loading settings
            {
                if (cb_autobounds.Checked)
                    AutoDetectMonitors();
                ApplySettings();
                _usePreview = cb_preview.Checked;
                PreviewScreens();
            }
        }

        private void bt_save_Click(object sender, EventArgs e) // "save and close" button
        {
            Settings.Default.Window_Minimized = true;
            SaveSettings();
            _usePreview = false;
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
            if (e.Control && e.KeyCode == Keys.R) // ctrl-R tweak: simulate first run
            {
                ResetSettings();
                PreviewScreens();
            }
        }

        private void
            EmergencyRestore() // restores standard settings in case cursor is stuck away from settings window and tray icon
        {
            if (!_methodCtrlKey && !_methodDelay) // if both are unchecked
            {
                cb_allowcrossingdelay.Checked = true; // restore default values
                tb_unclipdelay.Text = "150";
            }
        }

        # endregion

        # region Autostart --------------------------------------------------------------------------------------------

        private void SetAutoStart() // sets auto-start with windows
        {
            var key = Registry.CurrentUser.CreateSubKey(StartWithWindowsRegPath);
            key?.SetValue("ddmm", Application.ExecutablePath);
        }

        private bool IsAutoStartEnabled() // gets whether aotostart is on
        {
            var key = Registry.CurrentUser.OpenSubKey(StartWithWindowsRegPath);
            if (key == null) return false;
            var value = (string) key.GetValue("ddmm");
            if (value == null) return false;
            return value == Application.ExecutablePath;
        }

        private void UnSetAutoStart() // disables autostart
        {
            var key = Registry.CurrentUser.CreateSubKey(StartWithWindowsRegPath);
            key?.DeleteValue("ddmm");
        }

        private void
            cb_startwithwindows_CheckedChanged(object sender, EventArgs e) // triggered on autostart checkbox clicked
        {
            if (cb_startwithwindows.Checked) SetAutoStart();
            else UnSetAutoStart();
        }

        # endregion

        # region Start Menu Shortcut ----------------------------------------------------------------------------------

        private void SetStartMenuShortcut() // create start menu shortcut
        {
            try
            {
                Directory.CreateDirectory(Directory.GetParent(_startMenuShortcutPath)
                    .ToString()); // create shortcut directory if needed
                // http://www.geekpedia.com/tutorial125_Create-shortcuts-with-a-.NET-application.html
                var wshShell = new WshShellClass();
                IWshShortcut myShortcut;
                myShortcut = (IWshShortcut) wshShell.CreateShortcut(_startMenuShortcutPath);
                myShortcut.TargetPath = Application.ExecutablePath;
                myShortcut.Description = "Dual Display Mouse Manager";
                myShortcut.Save();
            }
            catch (Exception)
            {
                cb_startmenushortcut.Checked = false; // failed :-(
            }
        }

        private bool IsStartMenuShortcutPresent() // returns whether start menu shortcut exists
        {
            return File.Exists(_startMenuShortcutPath);
        }

        private void UnSetStartMenuShortcut() // removes start menu shortcut
        {
            Directory.Delete(Directory.GetParent(_startMenuShortcutPath).ToString(), true);
        }

        private void
            cb_startmenushortcut_CheckedChanged(object sender, EventArgs e) // triggered on start menu checkbox clicked
        {
            if (cb_startmenushortcut.Checked) SetStartMenuShortcut();
            else UnSetStartMenuShortcut();
        }

        # endregion

        # region Mouse management -------------------------------------------------------------------------------------

        private void MouseMoved(int x, int y) // called by mouse hook to handle mouse movements
        {
            // Should modify the following line to happen only on visible form, reducing CPU usage
            if (_usePreview)
                l_mousepos.Text = "Mouse: x=" + x + ", y=" + y;

            if (_activateProgram) // do something about mouse (clipping or unclipping) only if program is activated
            {
                // activate clipping
                if (!_activeClip && _canClip) // if no current clip and if clipping is allowed
                {
                    var i = 1;
                    while (i <= 3)
                    {
                        if (_screens[i].Ok)
                        {
                            _currentClipRect = new Rectangle(_screens[i].Left, _screens[i].Top,
                                _screens[i].Right - _screens[i].Left, _screens[i].Bottom - _screens[i].Top);
                            if (_currentClipRect.Contains(x, y)
                            ) // if cursor in one of the screens -> clip to that screen
                            {
                                _dummyClipRect = new Rectangle(_screens[i].Left, _screens[i].Top, _screens[i].Right,
                                    _screens[i]
                                        .Bottom); // dummy rectangle with width=right_bound and height=bottom_bound because clipcursor wants a rectangle with left-top-right-bottom and not left-top-width-height
                                ClipCursor(ref _dummyClipRect); // clip the cursor
                                _activeClip = true; // flag clipping as active
                                notifyIcon1.Icon =
                                    new Icon(GetType(),
                                        "ddmm_screen" + (i == 1 ? "1" : "2") +
                                        ".ico"); // change icon (2 icons for different screens)
                                _canClip = false; // no need to allow new clipping
                                break; // skip other screens
                            }
                        }

                        i++;
                    }
                }

                // processes "cursor on the edge of screen" event (unclipping)
                if (_activeClip && _methodDelay
                ) // if mouse is clipped and crossing method is "after delay on the border of the screen"
                {
                    if (_currentClipRect.Left == x || _currentClipRect.Right == x + 1 || _currentClipRect.Top == y ||
                        _currentClipRect.Bottom == y + 1) // cursor on border
                        _unclipTimer.Start(); // start unclipping timer: we will unclip if timer reaches tick
                    else
                        _unclipTimer.Stop(); // if mouse gets away from border: stop timer
                }
            }
        }

        private void MouseTeleport(int x, int y) // called by keyboard hook to handle mouse teleport
        {
            _activateProgram = false;
            // now that clipping region changed, unlock cursor from previous settings and let clip again
            ClipCursor(ref _origClipRect); // clip to original zone
            _canClip = false; // no need to allow new clipping
            _activeClip = false; // no active clip

            var i = 1;
            while (i <= 3)
            {
                if (_screens[i].Ok)
                {
                    _currentClipRect = new Rectangle(_screens[i].Left, _screens[i].Top,
                        _screens[i].Right - _screens[i].Left,
                        _screens[i].Bottom - _screens[i].Top);
                    if (_currentClipRect.Contains(x, y)) // if cursor in one of the screens ->
                    {
                        var j = i < 3 ? i + 1 : 1; // select the next screen
                        int newX, newY;
                        while (!_screens[j].Ok) j = ++j > 3 ? 1 : j; // if not valid - find the next valid screen
                        newX = (x - _screens[i].Left) * (_screens[j].Right - _screens[j].Left) /
                               (_screens[i].Right - _screens[i].Left); // Count relative X for new screen
                        newY = (y - _screens[i].Top) * (_screens[j].Bottom - _screens[j].Top) /
                               (_screens[i].Bottom - _screens[i].Top); // Count relative Y for new screen
                        Cursor.Position = new Point(_screens[j].Left + newX, _screens[j].Top + newY); // move the cursor
                        _currentClipRect = new Rectangle(_screens[j].Left, _screens[j].Top,
                            _screens[j].Right - _screens[j].Left, _screens[j].Bottom - _screens[j].Top);
                        break; // skip other screens
                    }
                }

                i++;
            }

            _canClip = true;
            _activateProgram = cb_activate.Checked;
            MouseMoved(Cursor.Position.X, Cursor.Position.Y); // complete mouse movement, switch icons, etc
        }

        # endregion

        # region Keyboard management ----------------------------------------------------------------------------------

        private void CtrlKeyPressed()
        {
            if (_activateProgram) // ctrl key pressed: do something only if program activated
                if (_methodCtrlKey && _activeClip) // if mouse clipped and crossing method is "ctrl key pressed"
                {
                    ClipCursor(ref _origClipRect); // unclip (restore original clipping zone)
                    _activeClip = false; // flag clipping as inactive
                    _canClip = false; // prevent new clipping while ctrl is hold
                    notifyIcon1.Icon = new Icon(GetType(), "ddmm_normal.ico");
                }
        }

        private void CtrlKeyReleased()
        {
            if (_activateProgram)
                if (_methodCtrlKey)
                    _canClip = true; // flag new clipping as allowed when ctrl key is released
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
            Settings.Default.Window_Minimized = false;
            Settings.Default.Save(); // save restored default windows state

            //            Hide(); // bring to front by hiding+showing?
            Show();
            WindowState = FormWindowState.Normal;
            TopMost = true;
            _usePreview = cb_preview.Checked;
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
            RestoreWindow(); // and restore window
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

        private void DDMM_Form_Load(object sender, EventArgs e)
        {
        }

        private void DDMM_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();

                _usePreview = false;

                Settings.Default.Window_Minimized = true;
                Settings.Default.Save(); // save default windows state
            }
        }

        private void just_before_ApplicationExit(object sender, EventArgs e) // processing on application exit event
        {
            // restore original cursor clipping state
            ClipCursor(ref _origClipRect);
            // unregister all the hooks
            for (var i = 1; i <= 3; i++)
                _screens[i].FormS.LocationChanged -= CheckWhyPreviewMoved;
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
            _previewTimer.Tick -= PreviewRestoreAfterTimer;
            UnhookWindowsHookEx(_mhook); // remove global mouse hook
            UnhookWindowsHookEx(_khook); // remove global keyboard hook
        }

        # endregion
    }
}