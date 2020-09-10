﻿/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using UVtools.Core.Extensions;
using UVtools.GUI.Controls;

namespace UVtools.GUI.Forms
{
    public partial class FrmToolWindow : Form
    {
        #region Properties

        public CtrlToolWindowContent Content { get; set; }

        [Editor("System.ComponentModel.Design.MultilineStringEditor", typeof(UITypeEditor))]
        [SettingsBindable(true)]
        public string Description
        {
            get => lbDescription.Text;
            set => lbDescription.Text = value;
        }

        [Editor("System.ComponentModel.Design.MultilineStringEditor", typeof(UITypeEditor))]
        [SettingsBindable(true)]
        public bool ButtonResetDefaultsVisible
        {
            get => btnResetDefaults.Visible;
            set => btnResetDefaults.Visible = value;
        }

        [Editor("System.ComponentModel.Design.MultilineStringEditor", typeof(UITypeEditor))]
        [SettingsBindable(true)]
        public string ButtonOkText
        {
            get => btnOk.Text;
            set => btnOk.Text = value;
        }

        [Editor("System.ComponentModel.Design.MultilineStringEditor", typeof(UITypeEditor))]
        [SettingsBindable(true)]
        public string ButtonCancelText
        {
            get => btnOk.Text;
            set => btnOk.Text = value;
        }

        [SettingsBindable(true)]
        public bool LayerRangeVisible
        {
            get => pnLayerRange.Visible;
            set => pnLayerRange.Visible = value;
        }

        [SettingsBindable(true)]
        public bool LayerRangeEndVisible
        {
            get => nmLayerRangeEnd.Visible;
            set =>
                nmLayerRangeEnd.Visible = 
                lbLayerRangeTo.Visible = 
                lbLayerRangeToMM.Visible =
                btnLayerRangeSelect.Visible = value;
        }

        [ReadOnly(true)]
        [Browsable(false)]
        public uint LayerRangeStart
        {
            get => (uint)nmLayerRangeStart.Value;
            set => nmLayerRangeStart.Value = value;
        }

        [ReadOnly(true)]
        [Browsable(false)]
        public uint LayerRangeEnd
        {
            get => (uint)Math.Min(nmLayerRangeEnd.Value, Program.SlicerFile.LayerCount - 1);
            set => nmLayerRangeEnd.Value = value;
        }

        [ReadOnly(true)]
        [Browsable(false)]
        public virtual string ConfirmationText => $"{Text}?";


        #endregion

        #region Constructors

        public FrmToolWindow()
        {
            InitializeComponent();
        }

        public FrmToolWindow(string title, string description, string buttonOkText, bool layerRangeVisible = true, int layerIndex = -1, bool layerRangeEndVisible = true, bool hideContent = false) : this()
        {
            if (!ReferenceEquals(title, null)) Text = title;
            lbDescription.MaximumSize = new Size(Width - 10, 0);
            Description = description;
            ButtonOkText = buttonOkText;
            LayerRangeVisible = layerRangeVisible;
            LayerRangeEndVisible = layerRangeEndVisible;
            LayerRangeEnd = Program.SlicerFile.LayerCount - 1;
            if (layerIndex >= 0)
            {
                LayerRangeStart =
                LayerRangeEnd = (uint) layerIndex;
            }

            if (hideContent)
            {
                pnContent.Visible = false;
            }

            EventValueChanged(nmLayerRangeStart, EventArgs.Empty);
            EventValueChanged(nmLayerRangeEnd, EventArgs.Empty);
        }

        public FrmToolWindow(string description, string buttonOkText, bool layerRangeVisible = true, int layerIndex = -1, bool layerRangeEndVisible = true, bool hideContent = false) : this(null, description, buttonOkText, layerRangeVisible, layerIndex, layerRangeEndVisible, hideContent)
        { }


        public FrmToolWindow(string description, string buttonOkText, int layerIndex, bool hideContent = false) : this(null, description, buttonOkText, true, layerIndex, hideContent)
        { }

        public FrmToolWindow(CtrlToolWindowContent content, int layerIndex = -1) : this(content.Text, content.Description, content.ButtonOkText, content.LayerRangeVisible, layerIndex, content.LayerRangeEndVisible)
        {
            pnContent.Controls.Add(content);
            Width = Math.Max(MinimumSize.Width, content.Width);
            lbDescription.MaximumSize = new Size(Width - 10, 0);
            //content.Dock = DockStyle.Fill;
            btnResetDefaults.Visible = content.ButtonResetDefaultsVisible;
            btnOk.Enabled = content.ButtonOkEnabled;
            //content.AutoSize = true;
            Content = content;

            content.PropertyChanged += ContentOnPropertyChanged;
        }
        #endregion

        #region Overrides
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                // Need exclude controls that require ENTER
                if (!ReferenceEquals(ActiveControl, null))
                {
                    switch (ActiveControl)
                    {
                        case NumericUpDown _:
                        case TextBox textBox when textBox.Multiline:
                            return;
                    }
                }

                btnOk.PerformClick();
                return;
            }

            if ((ModifierKeys & Keys.Shift) == Keys.Shift && (ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (e.KeyCode == Keys.A)
                {
                    btnLayerRangeAllLayers.PerformClick();
                    e.Handled = true;
                    return;
                }

                if (e.KeyCode == Keys.C)
                {
                    btnLayerRangeCurrentLayer.PerformClick();
                    e.Handled = true;
                    return;
                }

                if (e.KeyCode == Keys.B)
                {
                    btnLayerRangeBottomLayers.PerformClick();
                    e.Handled = true;
                    return;
                }

                if (e.KeyCode == Keys.N)
                {
                    btnLayerRangeNormalLayers.PerformClick();
                    e.Handled = true;
                    return;
                }

                if (e.KeyCode == Keys.F)
                {
                    btnLayerRangeFirstLayer.PerformClick();
                    e.Handled = true;
                    return;
                }

                if (e.KeyCode == Keys.L)
                {
                    btnLayerRangeLastLayer.PerformClick();
                    e.Handled = true;
                    return;
                }
            }
        }

        #endregion

        #region Events
        private void ContentOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Content.ButtonOkEnabled))
            {
                btnOk.Enabled = Content.ButtonOkEnabled;
                return;
            }
        }

        private void EventClick(object sender, EventArgs e)
        {
            if (ReferenceEquals(sender, btnLayerRangeAllLayers))
            {
                nmLayerRangeStart.Value = 0;
                nmLayerRangeEnd.Value = Program.SlicerFile.LayerCount-1;
                return;
            }

            if (ReferenceEquals(sender, btnLayerRangeCurrentLayer))
            {
                nmLayerRangeStart.Value = Program.FrmMain.ActualLayer;
                nmLayerRangeEnd.Value = Program.FrmMain.ActualLayer;
                return;
            }

            if (ReferenceEquals(sender, btnLayerRangeBottomLayers))
            {
                nmLayerRangeStart.Value = 0;
                nmLayerRangeEnd.Value = Program.SlicerFile.InitialLayerCount-1;
                return;
            }

            if (ReferenceEquals(sender, btnLayerRangeNormalLayers))
            {
                nmLayerRangeStart.Value = Program.SlicerFile.InitialLayerCount - 1;
                nmLayerRangeEnd.Value = Program.SlicerFile.LayerCount - 1;
                return;
            }

            if (ReferenceEquals(sender, btnLayerRangeFirstLayer))
            {
                nmLayerRangeStart.Value = 
                nmLayerRangeEnd.Value = 0;
                return;
            }

            if (ReferenceEquals(sender, btnLayerRangeLastLayer))
            {
                nmLayerRangeStart.Value =
                    nmLayerRangeEnd.Value = Program.SlicerFile.LayerCount - 1;
                return;
            }

            if (ReferenceEquals(sender, btnResetDefaults))
            {
                ResetDefaults();
                return;
            }

            if (ReferenceEquals(sender, btnOk))
            {
                if (!btnOk.Enabled) return;

                if (LayerRangeStart > LayerRangeEnd)
                {
                    MessageErrorBox("Layer range start can't be higher than layer end.\nPlease fix and try again.");
                    nmLayerRangeStart.Select();
                    return;
                }

                if (!ValidateForm()) return;
                if (!ReferenceEquals(Content, null) && !Content.ValidateForm()) return;

                if (MessageQuestionBox($"Are you sure you want to {Content?.ConfirmationText ?? ConfirmationText}") == DialogResult.Yes)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }

                return;
            }

            if (ReferenceEquals(sender, btnCancel))
            {
                DialogResult = DialogResult.Cancel;
                return;
            }
        }

        private void EventValueChanged(object sender, EventArgs e)
        {
            if (ReferenceEquals(sender, nmLayerRangeStart) || ReferenceEquals(sender, nmLayerRangeEnd))
            {
                uint layerIndex = (uint) ((NumericUpDown) sender).Value;
                if (layerIndex >= Program.SlicerFile.LayerCount) return;
                var layer = Program.SlicerFile[layerIndex];
                var text = $"({layer.PositionZ}mm)";

                uint layerCount = LayerRangeEndVisible ? (uint) Math.Max(0, nmLayerRangeEnd.Value - nmLayerRangeStart.Value + 1) : 1;
                lbLayerRangeCount.Text = $"({layerCount} layers / {Program.SlicerFile.LayerHeight * layerCount}mm)";

                if (layerCount == 0)
                {
                    lbLayerRangeCount.ForeColor = Color.Red;
                    btnOk.Enabled = false;
                }
                else
                {
                    lbLayerRangeCount.ForeColor = Color.Black;
                    btnOk.Enabled = true;
                }
                

                if (ReferenceEquals(sender, nmLayerRangeStart))
                {
                    lbLayerRangeFromMM.Text = text;
                }
                else if (ReferenceEquals(sender, nmLayerRangeEnd))
                {
                    lbLayerRangeToMM.Text = text;
                }
                
                return;
            }
        }


        #endregion

        #region Methods

        public virtual bool ValidateForm() => true;

        /// <summary>
        /// Called when button reset to defaults is clicked
        /// </summary>
        public virtual void ResetDefaults()
        {
            Content?.ResetDefaults();
        }

        public DialogResult MessageErrorBox(string message) => GUIExtensions.MessageBoxError($"{Text} Error", message);
        public DialogResult MessageQuestionBox(string message, string title = null) => GUIExtensions.MessageBoxQuestion($"{title ?? Text}", message);

        public T GetContentCtrl<T>()
        {
            if (Content is T content)
            {
                return content;
            }
            try
            {
                return (T)Convert.ChangeType(Content, typeof(T));
            }
            catch (InvalidCastException)
            {
                return default(T);
            }
        }

        #endregion

    }
}
