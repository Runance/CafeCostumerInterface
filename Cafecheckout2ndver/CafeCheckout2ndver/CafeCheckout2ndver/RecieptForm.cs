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
    public partial class RecieptForm : Form
    {
       
        public RecieptForm(string receiptContent)
        {
            InitializeComponent();
            receiptTextBox.Text = receiptContent;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
