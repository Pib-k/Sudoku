using Sudoku_Namespace.Rendering;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Sudoku_Namespace.UI
{
    internal class SettingsForm : BaseDialogForm
    {
        public string SelectedFont { get; private set; }
        public bool ShowHintExplanation { get; private set; }
        public bool AnimatedOn { get; private set; }
        public string UserSelectedTheme { get; private set; }

        private ComboBox themeCombo;
        private ComboBox fontCombo;
        private CheckBox hintToggle;
        private CheckBox animatedToggle;

        public SettingsForm(Size size, ColorTheme currentTheme, string userSelectedFont, bool showHintExplanation, bool animatedOn)
            : base(size, currentTheme, userSelectedFont)
        {
            this.SelectedFont = userSelectedFont;
            this.ShowHintExplanation = showHintExplanation;
            this.AnimatedOn = animatedOn;

            this.Height += 50;

            InitializeHeader("SETTINGS");
            CreateSettingsContent();
        }

        private void CreateSettingsContent()
        {
            TableLayoutPanel tlp = new TableLayoutPanel
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Padding = new Padding(LEFT_PADDING, TOP_PADDING + 30, RIGHT_PADDING, BOTTOM_PADDING),
                BackColor = Color.Transparent
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Create 8 even rows
            for (int i = 0; i < 8; i++)
            {
                tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100 / 8F));
            }

            // Setup Controls
            themeCombo = CreateThemedCombo(ThemeManager.Themes.Select(t => t.Name).ToArray(), base.Theme.Name);

            fontCombo = CreateThemedCombo(
                new[] { "Segoe UI Emoji", "Arial", "Calibri", "Consolas", "Noto Emoji" },
                SelectedFont);

            hintToggle = CreateToggle(ShowHintExplanation);
            animatedToggle = CreateToggle(AnimatedOn);

            AddSettingItem(tlp, "THEME", themeCombo);
            AddSettingItem(tlp, "FONT FAMILY", fontCombo);
            AddSettingItem(tlp, "HINT EXPLANATIONS", hintToggle);
            AddSettingItem(tlp, "SOLVING ANIMATION", animatedToggle);

            // Apply Button - customized version of the base button
            Button btnApply = CreateBaseStyledButton("APPLY CHANGES");
            btnApply.Dock = DockStyle.Top;
            btnApply.Height = 45;
            btnApply.Margin = new Padding(0, 10, 0, 5);
            btnApply.BackColor = base.Theme.HighlightColor; // Override color for emphasis
            btnApply.Click += OnApplyClick;

            tlp.Controls.Add(btnApply);
            this.Controls.Add(tlp);
        }

        private void OnApplyClick(object sender, EventArgs e)
        {
            SelectedFont = fontCombo.SelectedItem?.ToString() ?? "Segoe UI";
            ShowHintExplanation = hintToggle.Checked;
            AnimatedOn = animatedToggle.Checked;
            UserSelectedTheme = themeCombo.SelectedItem?.ToString() ?? base.Theme.Name;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void AddSettingItem(TableLayoutPanel panel, string title, Control input)
        {
            // Container for Label + Control
            Panel container = new Panel
            {
                Dock = DockStyle.Top,
                Height = input is CheckBox ? 60 : 70,
                BackColor = Color.Transparent,
            };

            // Label at the top
            Label lbl = new Label
            {
                Text = title,
                ForeColor = Color.FromArgb(180, base.Theme.TextColor), // Using inherited Theme
                Font = new Font(SelectedFont, 8f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };

            if (input is CheckBox)
            {
                input.Location = new Point(0, 22);
                input.Size = new Size(150, 30);
                input.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            }
            else
            {
                input.Location = new Point(0, 22);
                input.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                input.Width = panel.Width;
            }

            container.Controls.Add(lbl);
            container.Controls.Add(input);
            panel.Controls.Add(container);
        }

        private ComboBox CreateThemedCombo(string[] items, string selected)
        {
            ComboBox cb = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = base.Theme.PrimaryColor,
                ForeColor = base.Theme.TextColor,
                Font = new Font(SelectedFont, 11),
                Height = 30
            };
            cb.Items.AddRange(items);
            cb.SelectedIndex = Math.Max(0, Array.IndexOf(items, selected));
            return cb;
        }

        private CheckBox CreateToggle(bool initialState)
        {
            Color activeGreen = Color.FromArgb(25, 90, 25);
            Color inactiveGray = base.Theme.PrimaryColor;

            CheckBox chk = new CheckBox
            {
                Checked = initialState,
                Appearance = Appearance.Button,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = initialState ? "ENABLED" : "DISABLED",
                BackColor = initialState ? activeGreen : inactiveGray,
                ForeColor = base.Theme.TextColor,
                Font = new Font(SelectedFont, 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Size = new Size(100, 30)
            };

            chk.FlatAppearance.BorderSize = 0;

            chk.CheckedChanged += (s, e) =>
            {
                chk.Text = chk.Checked ? "ENABLED" : "DISABLED";
                chk.BackColor = chk.Checked ? activeGreen : inactiveGray;
            };

            chk.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = base.GetRoundedRegionPath(
                    new Rectangle(0, 0, chk.Width, chk.Height), 10))
                {
                    chk.Region = new Region(path);
                }
            };

            return chk;
        }
    }
}