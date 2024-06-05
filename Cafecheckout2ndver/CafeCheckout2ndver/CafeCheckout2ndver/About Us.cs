using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CafeCheckout2ndver
{
    public partial class About_Us : Form
    {
        public About_Us()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // URL to open
            string url = "https://www.facebook.com/profile.php?id=100094209611125";

            try
            {
                // Open the URL in the default web browser
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // Handle any exceptions here
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // URL to open
            string url = "https://www.facebook.com/markrainiers.aguilar/";

            try
            {
                // Open the URL in the default web browser
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // Handle any exceptions here
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // URL to open
            string url = "https://www.facebook.com/kurtpapruz06";

            try
            {
                // Open the URL in the default web browser
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // Handle any exceptions here
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // URL to open
            string url = "https://www.facebook.com/rcb1216";

            try
            {
                // Open the URL in the default web browser
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // Handle any exceptions here
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // URL to open
            string url = "https://www.facebook.com/dngrnz";

            try
            {
                // Open the URL in the default web browser
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // Handle any exceptions here
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void About_Butt_Click(object sender, EventArgs e)
        {
            Kiosk_interface kiosk = new Kiosk_interface();
            kiosk.Refresh();
            kiosk.Show();
            this.Close();
        }
    }
}
