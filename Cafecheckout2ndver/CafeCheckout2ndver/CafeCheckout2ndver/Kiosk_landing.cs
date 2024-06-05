using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using Guna.UI2.Designer;
using System.Diagnostics;

namespace CafeCheckout2ndver
{
    public partial class Kiosk_interface : Form
    {
        private SqlConnection connection;
        private int customerCounter = 1; // Counter to keep track of the customer number

        public Kiosk_interface()
        {
            InitializeComponent();
            string connectionString = "Data Source=LAPTOP-R45B7D8N\\SQLEXPRESS;Initial Catalog=Cafedatabase;Integrated Security=True;";
            connection = new SqlConnection(connectionString);
            this.FormClosing += Kiosk_interface_FormClosing; // Add event handler for form closing
        }

        // Event handler for form closing to confirm user intent
        private void Kiosk_interface_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult result = MessageBox.Show("Are you sure you want to close the application?", "Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    this.Close();
                }
            }
        }

        // Event handler for "For Here" button click
        private void For_Here_Butt_Click(object sender, EventArgs e)
        {
            SaveCustomer("Here");
        }

        // Event handler for "To Go" button click
        private void To_Go_Butt_Click(object sender, EventArgs e)
        {
            SaveCustomer("To Go");
        }

        // Method to save customer data to the database
        private void SaveCustomer(string entryType)
        {
            try
            {
                connection.Open();
                CheckAndResetCounter();

                // Generate a unique customer ID
                string customerId = GenerateUniqueCustomerId(connection);

                string query = "INSERT INTO [dbo].[Customer] (Customer_Id, Entry_type, Entry_Time) VALUES (@CustomerId, @EntryType, @EntryTime); SELECT SCOPE_IDENTITY()";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CustomerId", customerId);
                command.Parameters.AddWithValue("@EntryType", entryType);
                command.Parameters.AddWithValue("@EntryTime", DateTime.Now);

                // Get the queuing number of the inserted customer
                int queuingNum = Convert.ToInt32(command.ExecuteScalar());

                MessageBox.Show($"Customer saved with Entry Type: {entryType} and Queuing Number: {queuingNum}");

                customerCounter++; // Increment counter after successful insertion

                // Open the ordering form and hide the current form
                Kiosk_ordering form2 = new Kiosk_ordering(customerId, queuingNum, entryType);
                form2.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        // Method to generate a unique customer ID
        private string GenerateUniqueCustomerId(SqlConnection connection)
        {
            string newCustomerId;
            int attempt = 0;

            do
            {
                newCustomerId = $"COS#{customerCounter + attempt}";
                attempt++;
            }
            while (CustomerIdExists(connection, newCustomerId));

            customerCounter += attempt - 1; // Update customerCounter to reflect the new unique ID
            return newCustomerId;
        }

        // Method to check if a customer ID already exists in the database
        private bool CustomerIdExists(SqlConnection connection, string customerId)
        {
            string query = "SELECT COUNT(*) FROM [dbo].[Customer] WHERE Customer_Id = @CustomerId";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", customerId);
            int count = (int)command.ExecuteScalar();

            return count > 0;
        }

        // Method to check and reset the customer counter if a new day has started
        private void CheckAndResetCounter()
        {
            string query = "SELECT MAX(Entry_Time) FROM [dbo].[Customer]";
            SqlCommand command = new SqlCommand(query, connection);
            object result = command.ExecuteScalar();

            if (result != null && result != DBNull.Value)
            {
                DateTime lastEntryTime = Convert.ToDateTime(result);
                if (lastEntryTime.Date != DateTime.Today)
                {
                    // Reset counter for a new day
                    customerCounter = 1;
                }
                else
                {
                    query = "SELECT MAX(Queuing_num) FROM [dbo].[Customer] WHERE CAST(Entry_Time AS DATE) = CAST(GETDATE() AS DATE)";
                    command = new SqlCommand(query, connection);
                    result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        // Set counter to the next queuing number
                        customerCounter = Convert.ToInt32(result) + 1;
                    }
                    else
                    {
                        // If there are no entries today, start from 1
                        customerCounter = 1;
                    }
                }
            }
            else
            {
                // If there are no entries in the table, start from 1
                customerCounter = 1;
            }
        }

        // Event handler for "About" button click
        private void About_Butt_Click(object sender, EventArgs e)
        {
            About_Us About = new About_Us();
            About.Refresh();
            About.Show();
            this.Hide();
        }
    }
}