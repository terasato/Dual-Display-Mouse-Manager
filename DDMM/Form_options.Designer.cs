namespace DDMM
{
    partial class DDMM_Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DDMM_Form));
            this.gb_screenboundaries = new System.Windows.Forms.GroupBox();
            this.cb_preview = new System.Windows.Forms.CheckBox();
            this.cb_autobounds = new System.Windows.Forms.CheckBox();
            this.bt_autodetectscreens = new System.Windows.Forms.Button();
            this.l_mousepos = new System.Windows.Forms.Label();
            this.tb_s3bottom = new System.Windows.Forms.TextBox();
            this.tb_s3top = new System.Windows.Forms.TextBox();
            this.tb_s3right = new System.Windows.Forms.TextBox();
            this.tb_s3left = new System.Windows.Forms.TextBox();
            this.l_screen3 = new System.Windows.Forms.Label();
            this.l_sbsep3 = new System.Windows.Forms.Label();
            this.tb_s2bottom = new System.Windows.Forms.TextBox();
            this.tb_s2top = new System.Windows.Forms.TextBox();
            this.tb_s2right = new System.Windows.Forms.TextBox();
            this.tb_s2left = new System.Windows.Forms.TextBox();
            this.l_screen2 = new System.Windows.Forms.Label();
            this.l_sbsep2 = new System.Windows.Forms.Label();
            this.l_sbsep1 = new System.Windows.Forms.Label();
            this.l_bottom = new System.Windows.Forms.Label();
            this.l_top = new System.Windows.Forms.Label();
            this.l_right = new System.Windows.Forms.Label();
            this.l_left = new System.Windows.Forms.Label();
            this.tb_s1bottom = new System.Windows.Forms.TextBox();
            this.tb_s1top = new System.Windows.Forms.TextBox();
            this.tb_s1right = new System.Windows.Forms.TextBox();
            this.tb_s1left = new System.Windows.Forms.TextBox();
            this.l_screen1 = new System.Windows.Forms.Label();
            this.gb_general = new System.Windows.Forms.GroupBox();
            this.cb_startmenushortcut = new System.Windows.Forms.CheckBox();
            this.cb_hidetrayicon = new System.Windows.Forms.CheckBox();
            this.cb_startwithwindows = new System.Windows.Forms.CheckBox();
            this.cb_activate = new System.Windows.Forms.CheckBox();
            this.gb_method = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.l_ms = new System.Windows.Forms.Label();
            this.tb_unclipdelay = new System.Windows.Forms.TextBox();
            this.cb_allowcrossingdelay = new System.Windows.Forms.CheckBox();
            this.cb_allowcrossingctrlkey = new System.Windows.Forms.CheckBox();
            this.bt_save = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cm_Restore = new System.Windows.Forms.ToolStripMenuItem();
            this.cm_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cb_mousejump = new System.Windows.Forms.CheckBox();
            this.l_debug = new System.Windows.Forms.Label();
            this.gb_screenboundaries.SuspendLayout();
            this.gb_general.SuspendLayout();
            this.gb_method.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gb_screenboundaries
            // 
            this.gb_screenboundaries.Controls.Add(this.cb_preview);
            this.gb_screenboundaries.Controls.Add(this.cb_autobounds);
            this.gb_screenboundaries.Controls.Add(this.bt_autodetectscreens);
            this.gb_screenboundaries.Controls.Add(this.l_mousepos);
            this.gb_screenboundaries.Controls.Add(this.tb_s3bottom);
            this.gb_screenboundaries.Controls.Add(this.tb_s3top);
            this.gb_screenboundaries.Controls.Add(this.tb_s3right);
            this.gb_screenboundaries.Controls.Add(this.tb_s3left);
            this.gb_screenboundaries.Controls.Add(this.l_screen3);
            this.gb_screenboundaries.Controls.Add(this.l_sbsep3);
            this.gb_screenboundaries.Controls.Add(this.tb_s2bottom);
            this.gb_screenboundaries.Controls.Add(this.tb_s2top);
            this.gb_screenboundaries.Controls.Add(this.tb_s2right);
            this.gb_screenboundaries.Controls.Add(this.tb_s2left);
            this.gb_screenboundaries.Controls.Add(this.l_screen2);
            this.gb_screenboundaries.Controls.Add(this.l_sbsep2);
            this.gb_screenboundaries.Controls.Add(this.l_sbsep1);
            this.gb_screenboundaries.Controls.Add(this.l_bottom);
            this.gb_screenboundaries.Controls.Add(this.l_top);
            this.gb_screenboundaries.Controls.Add(this.l_right);
            this.gb_screenboundaries.Controls.Add(this.l_left);
            this.gb_screenboundaries.Controls.Add(this.tb_s1bottom);
            this.gb_screenboundaries.Controls.Add(this.tb_s1top);
            this.gb_screenboundaries.Controls.Add(this.tb_s1right);
            this.gb_screenboundaries.Controls.Add(this.tb_s1left);
            this.gb_screenboundaries.Controls.Add(this.l_screen1);
            this.gb_screenboundaries.Location = new System.Drawing.Point(352, 11);
            this.gb_screenboundaries.Name = "gb_screenboundaries";
            this.gb_screenboundaries.Size = new System.Drawing.Size(260, 190);
            this.gb_screenboundaries.TabIndex = 1;
            this.gb_screenboundaries.TabStop = false;
            this.gb_screenboundaries.Text = "Screens boundaries";
            // 
            // cb_preview
            // 
            this.cb_preview.AutoSize = true;
            this.cb_preview.Location = new System.Drawing.Point(169, 28);
            this.cb_preview.Name = "cb_preview";
            this.cb_preview.Size = new System.Drawing.Size(87, 16);
            this.cb_preview.TabIndex = 0;
            this.cb_preview.Text = "Use preview";
            this.cb_preview.UseVisualStyleBackColor = true;
            this.cb_preview.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // cb_autobounds
            // 
            this.cb_autobounds.AutoSize = true;
            this.cb_autobounds.Location = new System.Drawing.Point(16, 28);
            this.cb_autobounds.Name = "cb_autobounds";
            this.cb_autobounds.Size = new System.Drawing.Size(115, 16);
            this.cb_autobounds.TabIndex = 0;
            this.cb_autobounds.Text = "Automatic control";
            this.cb_autobounds.UseVisualStyleBackColor = true;
            this.cb_autobounds.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // bt_autodetectscreens
            // 
            this.bt_autodetectscreens.Location = new System.Drawing.Point(175, 158);
            this.bt_autodetectscreens.Name = "bt_autodetectscreens";
            this.bt_autodetectscreens.Size = new System.Drawing.Size(75, 21);
            this.bt_autodetectscreens.TabIndex = 26;
            this.bt_autodetectscreens.Text = "Detect";
            this.bt_autodetectscreens.UseVisualStyleBackColor = true;
            this.bt_autodetectscreens.Click += new System.EventHandler(this.bt_autodetectscreens_Click);
            // 
            // l_mousepos
            // 
            this.l_mousepos.AutoSize = true;
            this.l_mousepos.Location = new System.Drawing.Point(17, 162);
            this.l_mousepos.Name = "l_mousepos";
            this.l_mousepos.Size = new System.Drawing.Size(33, 12);
            this.l_mousepos.TabIndex = 24;
            this.l_mousepos.Text = "       ";
            // 
            // tb_s3bottom
            // 
            this.tb_s3bottom.Location = new System.Drawing.Point(209, 129);
            this.tb_s3bottom.Name = "tb_s3bottom";
            this.tb_s3bottom.Size = new System.Drawing.Size(40, 19);
            this.tb_s3bottom.TabIndex = 22;
            this.tb_s3bottom.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // tb_s3top
            // 
            this.tb_s3top.Location = new System.Drawing.Point(163, 129);
            this.tb_s3top.Name = "tb_s3top";
            this.tb_s3top.Size = new System.Drawing.Size(40, 19);
            this.tb_s3top.TabIndex = 21;
            this.tb_s3top.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // tb_s3right
            // 
            this.tb_s3right.Location = new System.Drawing.Point(117, 129);
            this.tb_s3right.Name = "tb_s3right";
            this.tb_s3right.Size = new System.Drawing.Size(40, 19);
            this.tb_s3right.TabIndex = 20;
            this.tb_s3right.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // tb_s3left
            // 
            this.tb_s3left.BackColor = System.Drawing.Color.White;
            this.tb_s3left.Location = new System.Drawing.Point(71, 129);
            this.tb_s3left.Name = "tb_s3left";
            this.tb_s3left.Size = new System.Drawing.Size(40, 19);
            this.tb_s3left.TabIndex = 19;
            this.tb_s3left.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // l_screen3
            // 
            this.l_screen3.AutoSize = true;
            this.l_screen3.Location = new System.Drawing.Point(17, 132);
            this.l_screen3.Name = "l_screen3";
            this.l_screen3.Size = new System.Drawing.Size(50, 12);
            this.l_screen3.TabIndex = 18;
            this.l_screen3.Text = "Screen 3";
            // 
            // l_sbsep3
            // 
            this.l_sbsep3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.l_sbsep3.Location = new System.Drawing.Point(16, 123);
            this.l_sbsep3.Name = "l_sbsep3";
            this.l_sbsep3.Size = new System.Drawing.Size(238, 2);
            this.l_sbsep3.TabIndex = 17;
            // 
            // tb_s2bottom
            // 
            this.tb_s2bottom.Location = new System.Drawing.Point(209, 100);
            this.tb_s2bottom.Name = "tb_s2bottom";
            this.tb_s2bottom.Size = new System.Drawing.Size(40, 19);
            this.tb_s2bottom.TabIndex = 16;
            this.tb_s2bottom.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // tb_s2top
            // 
            this.tb_s2top.Location = new System.Drawing.Point(163, 100);
            this.tb_s2top.Name = "tb_s2top";
            this.tb_s2top.Size = new System.Drawing.Size(40, 19);
            this.tb_s2top.TabIndex = 15;
            this.tb_s2top.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // tb_s2right
            // 
            this.tb_s2right.Location = new System.Drawing.Point(117, 100);
            this.tb_s2right.Name = "tb_s2right";
            this.tb_s2right.Size = new System.Drawing.Size(40, 19);
            this.tb_s2right.TabIndex = 14;
            this.tb_s2right.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // tb_s2left
            // 
            this.tb_s2left.Location = new System.Drawing.Point(71, 100);
            this.tb_s2left.Name = "tb_s2left";
            this.tb_s2left.Size = new System.Drawing.Size(40, 19);
            this.tb_s2left.TabIndex = 13;
            this.tb_s2left.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // l_screen2
            // 
            this.l_screen2.AutoSize = true;
            this.l_screen2.Location = new System.Drawing.Point(17, 102);
            this.l_screen2.Name = "l_screen2";
            this.l_screen2.Size = new System.Drawing.Size(50, 12);
            this.l_screen2.TabIndex = 12;
            this.l_screen2.Text = "Screen 2";
            // 
            // l_sbsep2
            // 
            this.l_sbsep2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.l_sbsep2.Location = new System.Drawing.Point(16, 93);
            this.l_sbsep2.Name = "l_sbsep2";
            this.l_sbsep2.Size = new System.Drawing.Size(238, 2);
            this.l_sbsep2.TabIndex = 11;
            // 
            // l_sbsep1
            // 
            this.l_sbsep1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.l_sbsep1.Location = new System.Drawing.Point(16, 64);
            this.l_sbsep1.Name = "l_sbsep1";
            this.l_sbsep1.Size = new System.Drawing.Size(238, 2);
            this.l_sbsep1.TabIndex = 9;
            // 
            // l_bottom
            // 
            this.l_bottom.AutoSize = true;
            this.l_bottom.Location = new System.Drawing.Point(210, 48);
            this.l_bottom.Name = "l_bottom";
            this.l_bottom.Size = new System.Drawing.Size(42, 12);
            this.l_bottom.TabIndex = 8;
            this.l_bottom.Text = "Bottom";
            // 
            // l_top
            // 
            this.l_top.AutoSize = true;
            this.l_top.Location = new System.Drawing.Point(170, 48);
            this.l_top.Name = "l_top";
            this.l_top.Size = new System.Drawing.Size(24, 12);
            this.l_top.TabIndex = 7;
            this.l_top.Text = "Top";
            // 
            // l_right
            // 
            this.l_right.AutoSize = true;
            this.l_right.Location = new System.Drawing.Point(121, 48);
            this.l_right.Name = "l_right";
            this.l_right.Size = new System.Drawing.Size(32, 12);
            this.l_right.TabIndex = 6;
            this.l_right.Text = "Right";
            // 
            // l_left
            // 
            this.l_left.AutoSize = true;
            this.l_left.Location = new System.Drawing.Point(78, 48);
            this.l_left.Name = "l_left";
            this.l_left.Size = new System.Drawing.Size(25, 12);
            this.l_left.TabIndex = 5;
            this.l_left.Text = "Left";
            // 
            // tb_s1bottom
            // 
            this.tb_s1bottom.Location = new System.Drawing.Point(209, 70);
            this.tb_s1bottom.Name = "tb_s1bottom";
            this.tb_s1bottom.Size = new System.Drawing.Size(40, 19);
            this.tb_s1bottom.TabIndex = 4;
            this.tb_s1bottom.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // tb_s1top
            // 
            this.tb_s1top.Location = new System.Drawing.Point(163, 70);
            this.tb_s1top.Name = "tb_s1top";
            this.tb_s1top.Size = new System.Drawing.Size(40, 19);
            this.tb_s1top.TabIndex = 3;
            this.tb_s1top.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // tb_s1right
            // 
            this.tb_s1right.Location = new System.Drawing.Point(117, 70);
            this.tb_s1right.Name = "tb_s1right";
            this.tb_s1right.Size = new System.Drawing.Size(40, 19);
            this.tb_s1right.TabIndex = 2;
            this.tb_s1right.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // tb_s1left
            // 
            this.tb_s1left.BackColor = System.Drawing.SystemColors.Window;
            this.tb_s1left.Location = new System.Drawing.Point(71, 70);
            this.tb_s1left.Name = "tb_s1left";
            this.tb_s1left.Size = new System.Drawing.Size(40, 19);
            this.tb_s1left.TabIndex = 1;
            this.tb_s1left.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // l_screen1
            // 
            this.l_screen1.AutoSize = true;
            this.l_screen1.Location = new System.Drawing.Point(17, 73);
            this.l_screen1.Name = "l_screen1";
            this.l_screen1.Size = new System.Drawing.Size(50, 12);
            this.l_screen1.TabIndex = 0;
            this.l_screen1.Text = "Screen 1";
            // 
            // gb_general
            // 
            this.gb_general.Controls.Add(this.cb_startmenushortcut);
            this.gb_general.Controls.Add(this.cb_hidetrayicon);
            this.gb_general.Controls.Add(this.cb_startwithwindows);
            this.gb_general.Controls.Add(this.cb_activate);
            this.gb_general.Location = new System.Drawing.Point(12, 11);
            this.gb_general.Name = "gb_general";
            this.gb_general.Size = new System.Drawing.Size(334, 131);
            this.gb_general.TabIndex = 2;
            this.gb_general.TabStop = false;
            this.gb_general.Text = "General";
            // 
            // cb_startmenushortcut
            // 
            this.cb_startmenushortcut.AutoSize = true;
            this.cb_startmenushortcut.Location = new System.Drawing.Point(16, 72);
            this.cb_startmenushortcut.Name = "cb_startmenushortcut";
            this.cb_startmenushortcut.Size = new System.Drawing.Size(170, 16);
            this.cb_startmenushortcut.TabIndex = 31;
            this.cb_startmenushortcut.Text = "Place shortcut in start menu";
            this.cb_startmenushortcut.UseVisualStyleBackColor = true;
            this.cb_startmenushortcut.CheckedChanged += new System.EventHandler(this.cb_startmenushortcut_CheckedChanged);
            // 
            // cb_hidetrayicon
            // 
            this.cb_hidetrayicon.AutoSize = true;
            this.cb_hidetrayicon.Location = new System.Drawing.Point(16, 94);
            this.cb_hidetrayicon.Name = "cb_hidetrayicon";
            this.cb_hidetrayicon.Size = new System.Drawing.Size(265, 16);
            this.cb_hidetrayicon.TabIndex = 28;
            this.cb_hidetrayicon.Text = "Hide tray icon ( run program twice to restore! )";
            this.cb_hidetrayicon.UseVisualStyleBackColor = true;
            this.cb_hidetrayicon.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // cb_startwithwindows
            // 
            this.cb_startwithwindows.AutoSize = true;
            this.cb_startwithwindows.Location = new System.Drawing.Point(16, 50);
            this.cb_startwithwindows.Name = "cb_startwithwindows";
            this.cb_startwithwindows.Size = new System.Drawing.Size(121, 16);
            this.cb_startwithwindows.TabIndex = 27;
            this.cb_startwithwindows.Text = "Start with windows";
            this.cb_startwithwindows.UseVisualStyleBackColor = true;
            this.cb_startwithwindows.CheckedChanged += new System.EventHandler(this.cb_startwithwindows_CheckedChanged);
            // 
            // cb_activate
            // 
            this.cb_activate.AutoSize = true;
            this.cb_activate.Location = new System.Drawing.Point(16, 28);
            this.cb_activate.Name = "cb_activate";
            this.cb_activate.Size = new System.Drawing.Size(172, 16);
            this.cb_activate.TabIndex = 0;
            this.cb_activate.Text = "Activate mouse management";
            this.cb_activate.UseVisualStyleBackColor = true;
            this.cb_activate.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // gb_method
            // 
            this.gb_method.Controls.Add(this.label1);
            this.gb_method.Controls.Add(this.l_ms);
            this.gb_method.Controls.Add(this.tb_unclipdelay);
            this.gb_method.Controls.Add(this.cb_allowcrossingdelay);
            this.gb_method.Controls.Add(this.cb_allowcrossingctrlkey);
            this.gb_method.Location = new System.Drawing.Point(12, 148);
            this.gb_method.Name = "gb_method";
            this.gb_method.Size = new System.Drawing.Size(334, 118);
            this.gb_method.TabIndex = 3;
            this.gb_method.TabStop = false;
            this.gb_method.Text = "Method";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(273, 12);
            this.label1.TabIndex = 30;
            this.label1.Text = "( if you uncheck both and get stuck, hit Ctrl+Alt+D )";
            // 
            // l_ms
            // 
            this.l_ms.AutoSize = true;
            this.l_ms.Location = new System.Drawing.Point(264, 50);
            this.l_ms.Name = "l_ms";
            this.l_ms.Size = new System.Drawing.Size(20, 12);
            this.l_ms.TabIndex = 3;
            this.l_ms.Text = "ms";
            // 
            // tb_unclipdelay
            // 
            this.tb_unclipdelay.Location = new System.Drawing.Point(205, 47);
            this.tb_unclipdelay.Name = "tb_unclipdelay";
            this.tb_unclipdelay.Size = new System.Drawing.Size(53, 19);
            this.tb_unclipdelay.TabIndex = 2;
            this.tb_unclipdelay.TextChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // cb_allowcrossingdelay
            // 
            this.cb_allowcrossingdelay.AutoSize = true;
            this.cb_allowcrossingdelay.Location = new System.Drawing.Point(12, 50);
            this.cb_allowcrossingdelay.Name = "cb_allowcrossingdelay";
            this.cb_allowcrossingdelay.Size = new System.Drawing.Size(196, 16);
            this.cb_allowcrossingdelay.TabIndex = 1;
            this.cb_allowcrossingdelay.Text = "Allow border crossing after delay:";
            this.cb_allowcrossingdelay.UseVisualStyleBackColor = true;
            this.cb_allowcrossingdelay.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // cb_allowcrossingctrlkey
            // 
            this.cb_allowcrossingctrlkey.AutoSize = true;
            this.cb_allowcrossingctrlkey.Location = new System.Drawing.Point(12, 29);
            this.cb_allowcrossingctrlkey.Name = "cb_allowcrossingctrlkey";
            this.cb_allowcrossingctrlkey.Size = new System.Drawing.Size(267, 16);
            this.cb_allowcrossingctrlkey.TabIndex = 0;
            this.cb_allowcrossingctrlkey.Text = "Allow border crossing when Ctrl key is pressed";
            this.cb_allowcrossingctrlkey.UseVisualStyleBackColor = true;
            this.cb_allowcrossingctrlkey.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // bt_save
            // 
            this.bt_save.Location = new System.Drawing.Point(511, 408);
            this.bt_save.Name = "bt_save";
            this.bt_save.Size = new System.Drawing.Size(101, 21);
            this.bt_save.TabIndex = 4;
            this.bt_save.Text = "Save and close";
            this.bt_save.UseVisualStyleBackColor = true;
            this.bt_save.Click += new System.EventHandler(this.bt_save_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Dual Display Mouse Manager";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cm_Restore,
            this.cm_Exit});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.ShowImageMargin = false;
            this.contextMenuStrip1.ShowItemToolTips = false;
            this.contextMenuStrip1.Size = new System.Drawing.Size(82, 48);
            // 
            // cm_Restore
            // 
            this.cm_Restore.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.cm_Restore.Name = "cm_Restore";
            this.cm_Restore.ShowShortcutKeys = false;
            this.cm_Restore.Size = new System.Drawing.Size(81, 22);
            this.cm_Restore.Text = "Restore";
            this.cm_Restore.Click += new System.EventHandler(this.cm_Restore_Click);
            // 
            // cm_Exit
            // 
            this.cm_Exit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.cm_Exit.Name = "cm_Exit";
            this.cm_Exit.ShowShortcutKeys = false;
            this.cm_Exit.Size = new System.Drawing.Size(81, 22);
            this.cm_Exit.Text = "Exit";
            this.cm_Exit.Click += new System.EventHandler(this.cm_Exit_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cb_mousejump);
            this.groupBox1.Location = new System.Drawing.Point(352, 207);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 59);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mouse control";
            // 
            // cb_mousejump
            // 
            this.cb_mousejump.AutoSize = true;
            this.cb_mousejump.Location = new System.Drawing.Point(15, 29);
            this.cb_mousejump.Name = "cb_mousejump";
            this.cb_mousejump.Size = new System.Drawing.Size(225, 16);
            this.cb_mousejump.TabIndex = 0;
            this.cb_mousejump.Text = "Allow mouse region jump using Ctrl + ~";
            this.cb_mousejump.UseVisualStyleBackColor = true;
            this.cb_mousejump.CheckedChanged += new System.EventHandler(this.OptionsChanged);
            // 
            // l_debug
            // 
            this.l_debug.AutoSize = true;
            this.l_debug.Location = new System.Drawing.Point(115, 275);
            this.l_debug.Name = "l_debug";
            this.l_debug.Size = new System.Drawing.Size(89, 12);
            this.l_debug.TabIndex = 24;
            this.l_debug.Text = "                     ";
            this.l_debug.Click += new System.EventHandler(this.l_debug_Click);
            // 
            // DDMM_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.gb_general);
            this.Controls.Add(this.gb_method);
            this.Controls.Add(this.bt_save);
            this.Controls.Add(this.l_debug);
            this.Controls.Add(this.gb_screenboundaries);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DDMM_Form";
            this.ShowInTaskbar = false;
            this.Text = "Dual Display Mouse Manager v1.1";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Activated += new System.EventHandler(this.DDMM_Form_Activated);
            this.Deactivate += new System.EventHandler(this.DDMM_Form_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DDMM_Form_FormClosing);
            this.Load += new System.EventHandler(this.DDMM_Form_Load);
            this.Shown += new System.EventHandler(this.DDMM_Form_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DDMM_Form_KeyDown);
            this.gb_screenboundaries.ResumeLayout(false);
            this.gb_screenboundaries.PerformLayout();
            this.gb_general.ResumeLayout(false);
            this.gb_general.PerformLayout();
            this.gb_method.ResumeLayout(false);
            this.gb_method.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gb_screenboundaries;
        private System.Windows.Forms.Label l_bottom;
        private System.Windows.Forms.Label l_top;
        private System.Windows.Forms.Label l_right;
        private System.Windows.Forms.Label l_left;
        private System.Windows.Forms.TextBox tb_s1bottom;
        private System.Windows.Forms.TextBox tb_s1top;
        private System.Windows.Forms.TextBox tb_s1right;
        private System.Windows.Forms.TextBox tb_s1left;
        private System.Windows.Forms.Label l_screen1;
        private System.Windows.Forms.Label l_sbsep1;
        private System.Windows.Forms.Label l_sbsep2;
        private System.Windows.Forms.TextBox tb_s2bottom;
        private System.Windows.Forms.TextBox tb_s2top;
        private System.Windows.Forms.TextBox tb_s2right;
        private System.Windows.Forms.TextBox tb_s2left;
        private System.Windows.Forms.Label l_screen2;
        private System.Windows.Forms.TextBox tb_s3bottom;
        private System.Windows.Forms.TextBox tb_s3top;
        private System.Windows.Forms.TextBox tb_s3right;
        private System.Windows.Forms.TextBox tb_s3left;
        private System.Windows.Forms.Label l_screen3;
        private System.Windows.Forms.Label l_sbsep3;
        private System.Windows.Forms.Label l_mousepos;
        private System.Windows.Forms.GroupBox gb_general;
        private System.Windows.Forms.CheckBox cb_activate;
        private System.Windows.Forms.CheckBox cb_startwithwindows;
        private System.Windows.Forms.CheckBox cb_hidetrayicon;
        private System.Windows.Forms.GroupBox gb_method;
        private System.Windows.Forms.CheckBox cb_allowcrossingctrlkey;
        private System.Windows.Forms.Label l_ms;
        private System.Windows.Forms.TextBox tb_unclipdelay;
        private System.Windows.Forms.CheckBox cb_allowcrossingdelay;
        private System.Windows.Forms.Button bt_save;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem cm_Exit;
        private System.Windows.Forms.ToolStripMenuItem cm_Restore;
        private System.Windows.Forms.Button bt_autodetectscreens;
        private System.Windows.Forms.CheckBox cb_startmenushortcut;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cb_mousejump;
        private System.Windows.Forms.CheckBox cb_autobounds;
        private System.Windows.Forms.CheckBox cb_preview;
        private System.Windows.Forms.Label l_debug;
    }
}

