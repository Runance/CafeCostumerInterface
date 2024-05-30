using System;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Drawing;
using Guna.UI2.WinForms;
using System.Diagnostics;
using System.IO;
using System.Linq;



namespace CafeCheckout2ndver
{

    public partial class Kiosk_ordering : Form
    {
        private string customerId;
        private int queuingNum;
        private string connectionString = @"Data Source=LAPTOP-R45B7D8N\SQLEXPRESS;Initial Catalog=Cafedatabase;Integrated Security=True;";
        private const string FONT_NAME = "Rockwell";
        private string receiptContent;

        private string productId;
        private int quantity;
        private string addOnsId;
        private int addOnsQuantity;
        private decimal totalPrice;
        private string customerStatus;

        private PrintDocument ReceiptPrintDocument;
        private OnScreenKeyboard onScreenKeyboard;

        public Kiosk_ordering(string customerId, int queuingNum)
        {
            InitializeComponent();
            this.customerId = customerId;
            this.queuingNum = queuingNum;

            ReceiptPrintDocument = new PrintDocument();
            ReceiptPrintDocument.PrintPage += new PrintPageEventHandler(ReceiptPrintDocument_PrintPage);
            ReceiptPrintDocument.EndPrint += new PrintEventHandler(ReceiptPrintDocument_EndPrint);

            LoadProducts();
            LoadAddons();
            
            // Initialize placeholder text and events for Guna2TextBox controls
            InitializePlaceholderText(ProductIdTextBox, "Enter Product ID");
            InitializePlaceholderText(QuantityTextBox, "Enter Quantity");
            // Initialize on-screen keyboard
            onScreenKeyboard = new OnScreenKeyboard();
            AttachKeyboardEvents(ProductIdTextBox);
            AttachKeyboardEvents(QuantityTextBox);
            AttachKeyboardEvents(AddOnsIdTextBox);
            AttachKeyboardEvents(AddOnsQuantityTextBox);
        }

        private void AttachKeyboardEvents(Guna2TextBox textBox)
        {
            textBox.Enter += (sender, e) =>
            {
                onScreenKeyboard.AttachTextBox(textBox);
                onScreenKeyboard.Show();
                onScreenKeyboard.BringToFront();
            };

            textBox.Leave += (sender, e) =>
            {
                onScreenKeyboard.Hide();
            };
        }

        private void InitializePlaceholderText(Guna2TextBox textBox, string placeholder)
        {
            textBox.Text = placeholder;
            textBox.ForeColor = Color.Gray;

            textBox.Enter += (sender, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.ForeColor = Color.Black;
                }
            };

            textBox.Leave += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = Color.Gray;
                }
            };
        }
        private void LoadProducts()
        {
            string connectionString = @"Data Source=LAPTOP-R45B7D8N\SQLEXPRESS;Initial Catalog=Cafedatabase;Integrated Security=True;";
            string query = "SELECT * FROM [dbo].[Products]"; // Retrieve all products

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    int index = 1;

                    while (reader.Read())
                    {
                        // Construct control names dynamically
                        string pictureBoxName = $"pictureBox{index}";
                        string flowLayoutPanelName = $"flowLayoutPanel{index}";

                        // Find controls by name
                        PictureBox pictureBox = this.Controls.Find(pictureBoxName, true).FirstOrDefault() as PictureBox;
                        FlowLayoutPanel flowLayoutPanel = this.Controls.Find(flowLayoutPanelName, true).FirstOrDefault() as FlowLayoutPanel;

                        if (pictureBox != null && flowLayoutPanel != null)
                        {
                            // Update PictureBox properties
                            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // Ensures the image is not stretched

                            // Update FlowLayoutPanel properties
                            flowLayoutPanel.FlowDirection = FlowDirection.TopDown; // Ensures the labels are displayed line by line
                            flowLayoutPanel.WrapContents = false; // Prevents automatic wrapping of labels
                            flowLayoutPanel.AutoSize = false; // Prevents autosizing
                            flowLayoutPanel.AutoScroll = true; // Enables scrolling if the content exceeds the panel's size

                            // Clear existing controls
                            flowLayoutPanel.Controls.Clear();

                            // Retrieve and display the image
                            if (!reader.IsDBNull(reader.GetOrdinal("IMAGE")))
                            {
                                byte[] imageBytes = (byte[])reader["IMAGE"];
                                using (MemoryStream ms = new MemoryStream(imageBytes))
                                {
                                    pictureBox.Image = Image.FromStream(ms);
                                }
                                pictureBox.Visible = true;
                            }
                            else
                            {
                                pictureBox.Visible = false;
                            }

                            // Add other product details
                            AddLabelToPanel(flowLayoutPanel, "Product ID: " + reader["Product_Id"].ToString());
                            AddLabelToPanel(flowLayoutPanel, "Name: " + reader["Product_Name"].ToString());
                            AddLabelToPanel(flowLayoutPanel, "Description: " + reader["Description"].ToString());
                            AddLabelToPanel(flowLayoutPanel, "Price: " + reader["Price"].ToString());
                            AddLabelToPanel(flowLayoutPanel, "Stocks: " + reader["Stock"].ToString());

                            // Ensure FlowLayoutPanel visibility
                            flowLayoutPanel.Visible = flowLayoutPanel.Controls.Count > 0;
                        }
                        else
                        {
                            // Log the missing controls for debugging
                            Debug.WriteLine($"Missing controls for index {index}: PictureBox = {pictureBox != null}, FlowLayoutPanel = {flowLayoutPanel != null}");
                        }

                        index++;
                    }

                    // Hide remaining picture boxes and panels if any
                    for (int i = index; i <= 16; i++)
                    {
                        var pictureBox = this.Controls.Find($"pictureBox{i}", true).FirstOrDefault() as PictureBox;
                        var flowLayoutPanel = this.Controls.Find($"flowLayoutPanel{i}", true).FirstOrDefault() as FlowLayoutPanel;
                        if (pictureBox != null) pictureBox.Visible = false;
                        if (flowLayoutPanel != null) flowLayoutPanel.Visible = false;
                    }
                }
            }
        }

        private void LoadAddons()
        {
            string connectionString = @"Data Source=LAPTOP-R45B7D8N\SQLEXPRESS;Initial Catalog=Cafedatabase;Integrated Security=True;";
            string query = "SELECT * FROM [dbo].[Add_Ons]"; // Retrieve all add-ons

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    int index = 1;

                    while (reader.Read())
                    {
                        // Construct control names dynamically
                        string pictureBoxName = $"AdopictureBox{index}";
                        string flowLayoutPanelName = $"AdoflowLayoutPanel{index}";

                        // Find controls by name
                        PictureBox pictureBox = this.Controls.Find(pictureBoxName, true).FirstOrDefault() as PictureBox;
                        FlowLayoutPanel flowLayoutPanel = this.Controls.Find(flowLayoutPanelName, true).FirstOrDefault() as FlowLayoutPanel;

                        // Log control retrieval for debugging
                        Debug.WriteLine($"PictureBox {pictureBoxName} found: {pictureBox != null}");
                        Debug.WriteLine($"FlowLayoutPanel {flowLayoutPanelName} found: {flowLayoutPanel != null}");

                        if (pictureBox != null && flowLayoutPanel != null)
                        {
                            // Update PictureBox properties
                            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

                            // Update FlowLayoutPanel properties
                            flowLayoutPanel.FlowDirection = FlowDirection.TopDown;
                            flowLayoutPanel.WrapContents = false;
                            flowLayoutPanel.AutoScroll = true;
                            
                            // Clear existing controls
                            flowLayoutPanel.Controls.Clear();

                            // Retrieve and display the image
                            if (!reader.IsDBNull(reader.GetOrdinal("ADO_IMAGE")))
                            {
                                byte[] imageBytes = (byte[])reader["ADO_IMAGE"];
                                using (MemoryStream ms = new MemoryStream(imageBytes))
                                {
                                    pictureBox.Image = Image.FromStream(ms);
                                }
                                pictureBox.Visible = true;
                            }
                            else
                            {
                                pictureBox.Visible = false;
                            }

                            // Add other add-on details
                            AddLabelToPanel(flowLayoutPanel, "Addons ID: " + reader["Addon_Id"].ToString());
                            AddLabelToPanel(flowLayoutPanel, "Name: " + reader["Addon_Name"].ToString());
                            AddLabelToPanel(flowLayoutPanel, "Description: " + reader["Description"].ToString());
                            AddLabelToPanel(flowLayoutPanel, "Price: " + reader["Price"].ToString());
                            AddLabelToPanel(flowLayoutPanel, "Stocks: " + reader["Stock"].ToString());

                            flowLayoutPanel.Visible = flowLayoutPanel.Controls.Count > 0;
                        }

                        index++;
                    }

                    // Hide remaining picture boxes and panels if any
                    for (int i = index; i <= 8; i++)
                    {
                        var pictureBox = this.Controls.Find($"AdopictureBox{i}", true).FirstOrDefault() as PictureBox;
                        var flowLayoutPanel = this.Controls.Find($"AdoflowLayoutPanel{i}", true).FirstOrDefault() as FlowLayoutPanel;
                        if (pictureBox != null) pictureBox.Visible = false;
                        if (flowLayoutPanel != null) flowLayoutPanel.Visible = false;
                    }
                }
            }
        }

        private void AddLabelToPanel(FlowLayoutPanel panel, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Label label = new Label
                {
                    Text = text,
                    AutoSize = true,
                    Padding = new Padding(3)
                };
                panel.Controls.Add(label);
                Debug.WriteLine($"Label added to {panel.Name}: {text}");
            }
            else
            {
                Debug.WriteLine($"Empty or null text, no label added to {panel.Name}");
            }
        }

        private bool ValidateTransactionInputs(out string productId, out int quantity, out string addOnsId, out int addOnsQuantity)
        {
            productId = ProductIdTextBox.Text.Trim();
            quantity = int.TryParse(QuantityTextBox.Text.Trim(), out int q) ? q : 0;
            addOnsId = AddOnsIdTextBox.Text.Trim();
            addOnsQuantity = int.TryParse(AddOnsQuantityTextBox.Text.Trim(), out int aq) ? aq : 0;

            if (string.IsNullOrEmpty(productId) || quantity <= 0)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(addOnsId) && addOnsQuantity <= 0)
            {
                return false;
            }

            return true;
        }

        private void Proceed_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateTransactionInputs(out productId, out quantity, out addOnsId, out addOnsQuantity))
                {
                    MessageBox.Show("Invalid inputs. Please check your product ID, Quantity, and Add-Ons (if provided).");
                    return;
                }

                decimal productPrice = GetProductPrice(productId);
                int productStock = GetProductStock(productId);

                if (productStock < quantity)
                {
                    MessageBox.Show("Not enough product stock.");
                    return;
                }

                decimal addOnsPrice = 0;
                int addOnsStock = 0;

                if (!string.IsNullOrEmpty(addOnsId))
                {
                    addOnsPrice = GetAddOnsPrice(addOnsId);
                    addOnsStock = GetAddOnsStock(addOnsId);

                    if (addOnsStock < addOnsQuantity)
                    {
                        MessageBox.Show("Not enough add-ons stock.");
                        return;
                    }
                }

                totalPrice = (productPrice * quantity) + (addOnsPrice * addOnsQuantity);
                customerStatus = "Queued";

                InsertTransaction(customerId, productId, quantity, addOnsId, addOnsQuantity, totalPrice, customerStatus);
                UpdateStocks(productId, quantity, addOnsId, addOnsQuantity);

                // Show the receipt preview
                PrintReceipt();

                if (MessageBox.Show("Transaction successful. Please Wait Watch your Queuing Number", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                {
                    // Close the current form
                    this.Close();

                    // Open the Kiosk_landing form
                    Kiosk_interface kioskLanding = new Kiosk_interface();
                    kioskLanding.Show();
                }
            }
            catch 
            {
                MessageBox.Show("Error processing the transaction: Please Correct The Inputs");
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            UpdateCustomerStatusToCancelled(customerId);
            MessageBox.Show("Transaction has been cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.Close();
            Kiosk_interface kioskInterface = new Kiosk_interface();
            kioskInterface.Show();
        }
        private void UpdateCustomerStatusToCancelled(string customerId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Customer SET Entry_type = @CustomerStatus WHERE Customer_Id = @CustomerId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CustomerStatus", "Cancelled");
                command.Parameters.AddWithValue("@CustomerId", customerId);
                command.ExecuteNonQuery();
            }
        }
        private decimal GetProductPrice(string productId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT Price FROM Products WHERE Product_Id = @ProductId", connection);
                command.Parameters.AddWithValue("@ProductId", productId);
                return (int)command.ExecuteScalar();
            }
        }

        private int GetProductStock(string productId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT Stock FROM Products WHERE Product_Id = @ProductId", connection);
                command.Parameters.AddWithValue("@ProductId", productId);
                return (int)command.ExecuteScalar();
            }
        }

        private decimal GetAddOnsPrice(string addOnsId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT Price FROM Add_Ons WHERE Addon_ID = @AddOnsId", connection);
                command.Parameters.AddWithValue("@AddOnsId", addOnsId);
                return (int)command.ExecuteScalar();
            }
        }

        private int GetAddOnsStock(string addOnsId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT Stock FROM Add_Ons WHERE Addon_ID = @AddOnsId", connection);
                command.Parameters.AddWithValue("@AddOnsId", addOnsId);
                return (int)command.ExecuteScalar();
            }
        }

        private void InsertTransaction(string customerId, string productId, int quantity, string addOnsId, int addOnsQuantity, decimal totalPrice, string status)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(
                    "INSERT INTO Transactions (Customer_Id, Product_Id, Quantity, Addon_Id, Addon_Quantity, Total_Price, Status) " +
                    "VALUES (@CustomerId, @ProductId, @Quantity, @AddOnsId, @AddOnsQuantity, @TotalPrice, @Status)", connection);
                command.Parameters.AddWithValue("@CustomerId", customerId);
                command.Parameters.AddWithValue("@ProductId", productId);
                command.Parameters.AddWithValue("@Quantity", quantity);
                command.Parameters.AddWithValue("@AddOnsId", string.IsNullOrEmpty(addOnsId) ? (object)DBNull.Value : addOnsId);
                command.Parameters.AddWithValue("@AddOnsQuantity", addOnsQuantity);
                command.Parameters.AddWithValue("@TotalPrice", totalPrice);
                command.Parameters.AddWithValue("@Status", status);
                command.ExecuteNonQuery();
            }
        }

        private void UpdateStocks(string productId, int quantity, string addOnsId, int addOnsQuantity)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand updateProductStockCommand = new SqlCommand(
                    "UPDATE Products SET Stock = Stock - @Quantity WHERE Product_Id = @ProductId", connection);
                updateProductStockCommand.Parameters.AddWithValue("@Quantity", quantity);
                updateProductStockCommand.Parameters.AddWithValue("@ProductId", productId);
                updateProductStockCommand.ExecuteNonQuery();

                if (!string.IsNullOrEmpty(addOnsId))
                {
                    SqlCommand updateAddOnsStockCommand = new SqlCommand(
                        "UPDATE Add_Ons SET Stock = Stock - @AddOnsQuantity WHERE Addon_ID = @AddOnsId", connection);
                    updateAddOnsStockCommand.Parameters.AddWithValue("@AddOnsQuantity", addOnsQuantity);
                    updateAddOnsStockCommand.Parameters.AddWithValue("@AddOnsId", addOnsId);
                    updateAddOnsStockCommand.ExecuteNonQuery();
                }
            }
        }

        private void PrintReceipt()
        {
            receiptContent = GenerateReceiptContent();

            // Show the receipt preview dialog
            PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();
            printPreviewDialog.Document = ReceiptPrintDocument;

            if (printPreviewDialog.ShowDialog() == DialogResult.OK)
            {
                ReceiptPrintDocument.Print();
            }
        }


        private string GenerateReceiptContent()
        {
            StringBuilder receiptBuilder = new StringBuilder();
            receiptBuilder.AppendLine("CafeCheckOut Receipt");
            receiptBuilder.AppendLine("---------------------------------------");
            receiptBuilder.AppendLine($"Customer ID: {customerId}");
            receiptBuilder.AppendLine($"Queuing Number: {queuingNum}");
            receiptBuilder.AppendLine("---------------------------------------");
            receiptBuilder.AppendLine($"ProductID: {productId}");
            receiptBuilder.AppendLine($"Product: {GetProductName(productId)}");
            receiptBuilder.AppendLine($"Price: {GetProductPrice(productId)}");
            receiptBuilder.AppendLine($"Quantity: {quantity}");
            if (!string.IsNullOrEmpty(addOnsId))
            {
            receiptBuilder.AppendLine($"Add-Ons ID: {addOnsId}");
            receiptBuilder.AppendLine($"Add-On: {GetAddOnsName(addOnsId)}");
            receiptBuilder.AppendLine($"Price: {GetAddOnsPrice(addOnsId)}");
            receiptBuilder.AppendLine($"Quantity: {addOnsQuantity}");
            }
            receiptBuilder.AppendLine("---------------------------------------");
            receiptBuilder.AppendLine($"Total Price: ₱{totalPrice}");
            receiptBuilder.AppendLine($"Status: {customerStatus}");
            receiptBuilder.AppendLine("Please Wait For Your Order");
            receiptBuilder.AppendLine("Thank you!");


            return receiptBuilder.ToString();
        }

        private string GetProductName(string productId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT Product_Name FROM Products WHERE Product_Id = @ProductId", connection);
                command.Parameters.AddWithValue("@ProductId", productId);
                return (string)command.ExecuteScalar();
            }
        }

        private string GetAddOnsName(string addOnsId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT Addon_Name FROM Add_Ons WHERE Addon_ID = @AddOnsId", connection);
                command.Parameters.AddWithValue("@AddOnsId", addOnsId);
                return (string)command.ExecuteScalar();
            }
        }

        private void ReceiptPrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font font = new Font(FONT_NAME, 30 ,FontStyle.Italic);
            e.Graphics.DrawString(receiptContent, font, Brushes.Black, new PointF(100, 100));
        }

        private void ReceiptPrintDocument_EndPrint(object sender, PrintEventArgs e)
        {
            MessageBox.Show("Receipt printed successfully!");
        }

    }
}