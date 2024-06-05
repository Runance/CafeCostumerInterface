using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CafeCheckout2ndver
{
    partial class OnScreenKeyboard
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
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OnScreenKeyboard));
            this.SuspendLayout();
            // 
            // OnScreenKeyboard
            // 
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1027, 300);
            this.DoubleBuffered = true;
            this.Name = "OnScreenKeyboard";
            this.Text = "On-Screen Keyboard";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnScreenKeyboard_FormClosing);
            this.Load += new System.EventHandler(this.OnScreenKeyboard_Load);
            this.ResumeLayout(false);

        }

        private void OnScreenKeyboard_Load(object sender, EventArgs e)
        {
            AddKeyboardButtons();
        }

        private void OnScreenKeyboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cancel the form closing to hide it instead of disposing
            e.Cancel = true;
            this.Hide();
        }

        private void AddKeyboardButtons()
        {
            string[][] keys = new string[][]
            {
                new string[] {"1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "Backspace"},
                new string[] {"Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P"},
                new string[] {"A", "S", "D", "F", "G", "H", "J", "K", "L"},
                new string[] {"Shift", "Z", "X", "C", "V", "B", "N", "M", "Enter"},
                new string[] {"Space"}
            };

            int x = 10, y = 10;
            int buttonWidth = 80, buttonHeight = 50, margin = 5; // Increased button size

            for (int row = 0; row < keys.Length; row++)
            {
                x = 10; // Reset x position for each row
                for (int col = 0; col < keys[row].Length; col++)
                {
                    Guna2Button keyButton = new Guna2Button();
                    keyButton.Text = keys[row][col];
                    keyButton.Width = buttonWidth;
                    keyButton.Height = buttonHeight;
                    keyButton.Left = x;
                    keyButton.Top = y;
                    keyButton.Font = new Font("Rockwell", 12F, FontStyle.Italic); // Set font to Rockwell Italic

                    // Set button colors
                    keyButton.FillColor = Color.FromArgb(48, 48, 48); // Fill color
                    keyButton.ForeColor = Color.White; // Font color

                    // Adjust widths for special keys
                    if (keyButton.Text == "Backspace")
                        keyButton.Width = buttonWidth * 2;
                    if (keyButton.Text == "Shift")
                        keyButton.Width = (int)(buttonWidth * 1.5);
                    if (keyButton.Text == "Enter")
                        keyButton.Width = (int)(buttonWidth * 1.5);
                    if (keyButton.Text == "Space")
                    {
                        keyButton.Width = buttonWidth * 5;
                        keyButton.Left = (this.ClientSize.Width - keyButton.Width) / 2; // Center the space bar
                    }

                    keyButton.Click += new EventHandler(KeyButton_Click);
                    this.Controls.Add(keyButton);

                    x += keyButton.Width + margin;
                }
                y += buttonHeight + margin;
            }
        }
    }
}