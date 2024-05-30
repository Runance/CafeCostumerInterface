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
        private int customerCounter = 1;
       
        public Kiosk_interface()
        {
            InitializeComponent();
            string connectionString = "Data Source=LAPTOP-R45B7D8N\\SQLEXPRESS;Initial Catalog=Cafedatabase;Integrated Security=True;";
            connection = new SqlConnection(connectionString);
        }

        private void For_Here_Butt_Click(object sender, EventArgs e)
        {
            SaveCustomer("Here");
        }

        private void To_Go_Butt_Click(object sender, EventArgs e)
        {
            SaveCustomer("To Go");
        }
        
        private void SaveCustomer(string entryType)
        {
            
            try
            {
                connection.Open();

                string customerId = GenerateUniqueCustomerId(); // Generate unique Customer_Id

                // Insert customer data into the database
                string query = "INSERT INTO [dbo].[Customer] (Customer_Id, Entry_type, Entry_Time) VALUES (@CustomerId, @EntryType, @EntryTime); SELECT SCOPE_IDENTITY()";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CustomerId", customerId);
                command.Parameters.AddWithValue("@EntryType", entryType);
                command.Parameters.AddWithValue("@EntryTime", DateTime.Now);

                // Execute the insert query and get the identity value (Queuing_num)
                int queuingNum = Convert.ToInt32(command.ExecuteScalar());

                MessageBox.Show($"Customer saved with Entry Type: {entryType} and Queuing Number: {queuingNum}");

                // After saving the customer, navigate to the next form (Kiosk_ordering)
                Kiosk_ordering form2 = new Kiosk_ordering(customerId, queuingNum);
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

        private string GenerateUniqueCustomerId()
        {
            string customerId;
            bool isUnique = false;

            do
            {
                customerId = "COS#" + customerCounter++;
                isUnique = IsCustomerIdUnique(customerId);
            }
            while (!isUnique);

            return customerId;
        }

        private bool IsCustomerIdUnique(string customerId)
        {
            string query = "SELECT COUNT(*) FROM [dbo].[Customer] WHERE Customer_Id = @CustomerId";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", customerId);

            int count = Convert.ToInt32(command.ExecuteScalar());
            return count == 0;
        }

        private void About_Butt_Click(object sender, EventArgs e)
        {
            About_Us About= new About_Us();
            About.Refresh();
            About.Show();
            this.Hide();
        }
    }
}
