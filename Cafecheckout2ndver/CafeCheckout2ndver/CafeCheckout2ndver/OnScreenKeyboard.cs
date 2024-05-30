using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CafeCheckout2ndver
{
    public partial class OnScreenKeyboard : Form
    {
        private Guna2TextBox attachedTextBox;
        private bool shiftEnabled = false;

        public OnScreenKeyboard()
        {
            InitializeComponent();
        }

        public void AttachTextBox(Guna2TextBox textBox)
        {
            attachedTextBox = textBox;
            this.BringToFront();
        }

        private void KeyButton_Click(object sender, EventArgs e)
        {
            Guna2Button button = sender as Guna2Button;
            if (attachedTextBox == null)
                return;

            if (button.Text == "Space")
                attachedTextBox.AppendText(" ");
            else if (button.Text == "Backspace" && attachedTextBox.Text.Length > 0)
                attachedTextBox.Text = attachedTextBox.Text.Substring(0, attachedTextBox.Text.Length - 1);
            else if (button.Text == "Enter")
                attachedTextBox.AppendText(Environment.NewLine);
            else if (button.Text == "Shift")
                ToggleShift();
            else
                attachedTextBox.AppendText(button.Text);
        }

        private void ToggleShift()
        {
            shiftEnabled = !shiftEnabled;
            foreach (Control control in this.Controls)
            {
                if (control is Guna2Button button && button.Text.Length == 1)
                {
                    button.Text = shiftEnabled ? button.Text.ToUpper() : button.Text.ToLower();
                }
            }
        }
    }
}