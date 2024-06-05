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
using System.Collections.Generic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;



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
        private string customerStatus;
        private string entryType;

        private PrintDocument ReceiptPrintDocument;
        private OnScreenKeyboard onScreenKeyboard;

        public Kiosk_ordering(string customerId, int queuingNum, string entryType)
        {
            InitializeComponent();
            this.customerId = customerId;
            this.queuingNum = queuingNum;
            this.entryType = entryType; // Store entryType

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
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Center the form at the top of the screen
            var screenBounds = Screen.FromHandle(this.Handle).Bounds;
            int topCenterX = (screenBounds.Width - this.Width) / 2;
            this.Location = new Point(topCenterX, 0);
        }
        private void AttachKeyboardEvents(Guna2TextBox textBox)
        {
            textBox.Enter += (sender, e) =>
            {
                onScreenKeyboard.AttachTextBox(textBox);
                PositionOnScreenKeyboard();
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
        private void PositionOnScreenKeyboard()
        {
            // Position the keyboard just below the form
            int belowFormX = this.Location.X + (this.Width - onScreenKeyboard.Width) / 2;
            int belowFormY = this.Location.Y + this.Height;
            onScreenKeyboard.Location = new Point(belowFormX, belowFormY);
        }
        private void LoadProducts()
        {
            string connectionString = @"Data Source=LAPTOP-R45B7D8N\SQLEXPRESS;Initial Catalog=Cafedatabase;Integrated Security=True;";
            string query = "SELECT * FROM [dbo].[Products] WHERE Pro_Status <> 1"; // Retrieve products where Pro_Status is not 1

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int index = 1;

                        while (reader.Read())
                        {
                            string pictureBoxName = $"pictureBox{index}";
                            string flowLayoutPanelName = $"flowLayoutPanel{index}";

                            PictureBox pictureBox = this.Controls.Find(pictureBoxName, true).FirstOrDefault() as PictureBox;
                            FlowLayoutPanel flowLayoutPanel = this.Controls.Find(flowLayoutPanelName, true).FirstOrDefault() as FlowLayoutPanel;

                            if (pictureBox != null && flowLayoutPanel != null)
                            {
                                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

                                flowLayoutPanel.FlowDirection = FlowDirection.TopDown;
                                flowLayoutPanel.WrapContents = false;
                                flowLayoutPanel.AutoSize = false;
                                flowLayoutPanel.AutoScroll = true;
                                flowLayoutPanel.Controls.Clear();

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

                                AddLabelToPanel(flowLayoutPanel, "Product ID: " + reader["Product_Id"].ToString());
                                AddLabelToPanel(flowLayoutPanel, "Name: " + reader["Product_Name"].ToString());
                                AddLabelToPanel(flowLayoutPanel, "Description: " + reader["Description"].ToString());
                                AddLabelToPanel(flowLayoutPanel, "Price: " + reader["Price"].ToString());

                                flowLayoutPanel.Visible = flowLayoutPanel.Controls.Count > 0;
                            }

                            index++;
                        }

                        for (int i = index; i <= 20; i++)
                        {
                            var pictureBox = this.Controls.Find($"pictureBox{i}", true).FirstOrDefault() as PictureBox;
                            var flowLayoutPanel = this.Controls.Find($"flowLayoutPanel{i}", true).FirstOrDefault() as FlowLayoutPanel;
                            if (pictureBox != null) pictureBox.Visible = false;
                            if (flowLayoutPanel != null) flowLayoutPanel.Visible = false;
                        }

                        UpdateTabVisibility(MenuTab, "ProductTab1", flowLayoutPanel1.Visible || flowLayoutPanel2.Visible || flowLayoutPanel3.Visible || flowLayoutPanel4.Visible);
                        UpdateTabVisibility(MenuTab, "ProductTab2", flowLayoutPanel5.Visible || flowLayoutPanel6.Visible || flowLayoutPanel7.Visible || flowLayoutPanel8.Visible);
                        UpdateTabVisibility(MenuTab, "ProductTab3", flowLayoutPanel9.Visible || flowLayoutPanel10.Visible || flowLayoutPanel11.Visible || flowLayoutPanel12.Visible);
                        UpdateTabVisibility(MenuTab, "ProductTab4", flowLayoutPanel13.Visible || flowLayoutPanel14.Visible || flowLayoutPanel15.Visible || flowLayoutPanel16.Visible);
                        UpdateTabVisibility(MenuTab, "ProductTab5", flowLayoutPanel17.Visible || flowLayoutPanel18.Visible || flowLayoutPanel19.Visible || flowLayoutPanel20.Visible);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading products: {ex.Message}");
            }
        }
        private void LoadAddons()
        {
            string connectionString = @"Data Source=LAPTOP-R45B7D8N\SQLEXPRESS;Initial Catalog=Cafedatabase;Integrated Security=True;";
            string query = "SELECT * FROM [dbo].[Add_Ons] WHERE ADO_Status <> 1"; // Retrieve add-ons where ADO_Status is not 1

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int index = 1;

                        while (reader.Read())
                        {
                            string pictureBoxName = $"AdopictureBox{index}";
                            string flowLayoutPanelName = $"AdoflowLayoutPanel{index}";

                            PictureBox pictureBox = this.Controls.Find(pictureBoxName, true).FirstOrDefault() as PictureBox;
                            FlowLayoutPanel flowLayoutPanel = this.Controls.Find(flowLayoutPanelName, true).FirstOrDefault() as FlowLayoutPanel;

                            if (pictureBox != null && flowLayoutPanel != null)
                            {
                                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

                                flowLayoutPanel.FlowDirection = FlowDirection.TopDown;
                                flowLayoutPanel.WrapContents = false;
                                flowLayoutPanel.AutoSize = false;
                                flowLayoutPanel.AutoScroll = true;
                                flowLayoutPanel.Controls.Clear();

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

                                AddLabelToPanel(flowLayoutPanel, "Add-On ID: " + reader["Addon_Id"].ToString());
                                AddLabelToPanel(flowLayoutPanel, "Name: " + reader["Addon_Name"].ToString());
                                AddLabelToPanel(flowLayoutPanel, "Description: " + reader["Description"].ToString());
                                AddLabelToPanel(flowLayoutPanel, "Price: " + reader["Price"].ToString());

                                flowLayoutPanel.Visible = flowLayoutPanel.Controls.Count > 0;
                            }

                            index++;
                        }

                        for (int i = index; i <= 8; i++)
                        {
                            var pictureBox = this.Controls.Find($"AdopictureBox{i}", true).FirstOrDefault() as PictureBox;
                            var flowLayoutPanel = this.Controls.Find($"AdoflowLayoutPanel{i}", true).FirstOrDefault() as FlowLayoutPanel;
                            if (pictureBox != null) pictureBox.Visible = false;
                            if (flowLayoutPanel != null) flowLayoutPanel.Visible = false;
                        }

                        UpdateTabVisibility(MenuTab, "AddonTab1", AdoflowLayoutPanel1.Visible || AdoflowLayoutPanel2.Visible || AdoflowLayoutPanel3.Visible || AdoflowLayoutPanel4.Visible);
                        UpdateTabVisibility(MenuTab, "AddonTab2", AdoflowLayoutPanel5.Visible || AdoflowLayoutPanel6.Visible || AdoflowLayoutPanel7.Visible || AdoflowLayoutPanel8.Visible);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading add-ons: {ex.Message}");
            }
        }
        private void AddLabelToPanel(FlowLayoutPanel panel, string text)
        {
            Label label = new Label
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10) // Adds some margin between labels
            };
            panel.Controls.Add(label);
        }

        private void UpdateTabVisibility(Guna2TabControl tabControl, string tabPageName, bool isVisible)
        {
            TabPage tabPage = tabControl.TabPages.Cast<TabPage>().FirstOrDefault(tp => tp.Name == tabPageName);

            if (tabPage == null)
            {
                tabPage = new TabPage { Name = tabPageName };
                tabControl.TabPages.Add(tabPage);
            }

            tabPage.Visible = isVisible;
        }

        private bool ValidateTransactionInputs(out string productId, out int quantity, out string addOnsId, out int addOnsQuantity)
        {
            productId = ProductIdTextBox.Text.Trim();
            quantity = int.TryParse(QuantityTextBox.Text.Trim(), out int q) ? q : 0;
            addOnsId = AddOnsIdTextBox.Text.Trim();
            addOnsQuantity = int.TryParse(AddOnsQuantityTextBox.Text.Trim(), out int aq) ? aq : 0;

            if (string.IsNullOrEmpty(productId) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid product ID and quantity.");
                return false;
            }

            if (!ProductExists(productId))
            {
                MessageBox.Show("The entered product ID does not exist.");
                return false;
            }

            if (!IsProductAvailable(productId))
            {
                MessageBox.Show("The entered product is not available. Please enter again.");
                return false;
            }

            if (GetProductStock(productId) < quantity)
            {
                MessageBox.Show("Not enough product stock.");
                return false;
            }

            if (!string.IsNullOrEmpty(addOnsId))
            {
                if (!AddOnExists(addOnsId))
                {
                    MessageBox.Show("The entered add-on ID does not exist.");
                    return false;
                }

                if (!IsAddOnAvailable(addOnsId))
                {
                    MessageBox.Show("The entered add-on is not available. Please enter again.");
                    return false;
                }

                if (GetAddOnsStock(addOnsId) < addOnsQuantity)
                {
                    MessageBox.Show("Not enough add-ons stock.");
                    return false;
                }
            }

            return true;
        }
        private bool IsProductAvailable(string productId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT Pro_Status FROM Products WHERE Product_Id = @ProductId", connection);
                command.Parameters.AddWithValue("@ProductId", productId);
                int status = (int)command.ExecuteScalar();
                return status != 1;
            }
        }

        private bool IsAddOnAvailable(string addOnsId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT ADO_Status FROM Add_Ons WHERE Addon_ID = @AddOnsId", connection);
                command.Parameters.AddWithValue("@AddOnsId", addOnsId);
                int status = (int)command.ExecuteScalar();
                return status != 1;
            }
        }

        private bool AddOnExists(string addOnsId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Add_Ons WHERE Addon_ID = @AddOnsId", connection);
                command.Parameters.AddWithValue("@AddOnsId", addOnsId);
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        private bool ProductExists(string productId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Products WHERE Product_Id = @ProductId", connection);
                command.Parameters.AddWithValue("@ProductId", productId);
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
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

                Debug.WriteLine("Validated Inputs");

                decimal productPrice = GetProductPrice(productId);
                int productStock = GetProductStock(productId);

                Debug.WriteLine($"Product Price: {productPrice}, Stock: {productStock}");

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

                    Debug.WriteLine($"AddOns Price: {addOnsPrice}, Stock: {addOnsStock}");

                    if (addOnsStock < addOnsQuantity)
                    {
                        MessageBox.Show("Not enough add-ons stock.");
                        return;
                    }
                }
                decimal currentTotalPrice = (productPrice * quantity) + (addOnsPrice * addOnsQuantity);
                customerStatus = "Queued";

                Debug.WriteLine($"Total Price: {currentTotalPrice}");

                InsertTransaction(customerId, productId, quantity, addOnsId, addOnsQuantity, currentTotalPrice, customerStatus);
                UpdateStocks(productId, quantity, addOnsId, addOnsQuantity);

                Debug.WriteLine("Transaction Inserted and Stocks Updated");

                var result = MessageBox.Show("Do you want to enter more products?", "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Refresh form for new input
                    ProductIdTextBox.Text = "";
                    QuantityTextBox.Text = "";
                    AddOnsIdTextBox.Text = "";
                    AddOnsQuantityTextBox.Text = "";
                    LoadProducts();
                    LoadAddons();
                }
                else
                {
                    // Notify the user that their order will be processed
                    MessageBox.Show("Your order has been processed. The receipt will now be printed.", "Order Processed", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Generate and print the receipt for all products entered by the customer
                    PrintReceipt();

                    if (MessageBox.Show("Please Wait and Watch your Queuing Number", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        // Update customer status to 'ForHere' or 'ToGo' instead of 'Queued'
                        UpdateCustomerStatusToCompleted(customerId, entryType); // Use entryType

                        // Close the current form
                        this.Close();

                        // Open the Kiosk_landing form
                        Kiosk_interface kioskLanding = new Kiosk_interface();
                        kioskLanding.Refresh();
                        kioskLanding.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing the transaction: " + ex.Message);
                Debug.WriteLine("Exception: " + ex.ToString());
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
        private void Cancel_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Update the status of all transactions for the customer to "Cancelled"
                    string cancelTransactionsQuery = "UPDATE [dbo].[Transactions] SET Status = @Status WHERE Customer_Id = @CustomerId";
                    SqlCommand cancelCommand = new SqlCommand(cancelTransactionsQuery, connection);
                    cancelCommand.Parameters.AddWithValue("@Status", "Cancelled");
                    cancelCommand.Parameters.AddWithValue("@CustomerId", customerId);
                    cancelCommand.ExecuteNonQuery();

                    // Update the customer status to "Cancelled"
                    string cancelCustomerQuery = "UPDATE [dbo].[Customer] SET Entry_type = @EntryType WHERE Customer_Id = @CustomerId";
                    SqlCommand customerCommand = new SqlCommand(cancelCustomerQuery, connection);
                    customerCommand.Parameters.AddWithValue("@EntryType", "Cancelled");
                    customerCommand.Parameters.AddWithValue("@CustomerId", customerId);
                    customerCommand.ExecuteNonQuery();

                    MessageBox.Show("Transaction has been cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.Close();
                Kiosk_interface kioskInterface = new Kiosk_interface();
                kioskInterface.Refresh();
                kioskInterface.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cancelling the transaction: " + ex.Message);
                Debug.WriteLine("Exception: " + ex.ToString());
            }
        }
        private void UpdateCustomerStatusToCompleted(string customerId, string entryType)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE [dbo].[Customer] SET Entry_type = @EntryType WHERE Customer_Id = @CustomerId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EntryType", entryType); // Set to "ForHere" or "ToGo"
                command.Parameters.AddWithValue("@CustomerId", customerId);
                int rowsAffected = command.ExecuteNonQuery();

                Debug.WriteLine($"Updated customer {customerId} status to {entryType}. Rows affected: {rowsAffected}");
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
        //Method to insert or save the whole transaction
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
        //Method to get the Products name

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
        //Method to get the Addons on database

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
        //Method for updating stocks based on customer inputs

        private string GenerateReceiptContent()
        {
            StringBuilder receiptBuilder = new StringBuilder();
            receiptBuilder.AppendLine("CafeCheckOut Receipt");
            receiptBuilder.AppendLine("---------------------------------------");
            receiptBuilder.AppendLine($"Customer ID: {customerId}");
            receiptBuilder.AppendLine($"Queuing Number: {queuingNum}");
            receiptBuilder.AppendLine("---------------------------------------");

            decimal totalCustomerPrice = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Transactions WHERE Customer_Id = @CustomerId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CustomerId", customerId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        decimal productPrice = GetProductPrice(reader["Product_Id"].ToString());
                        int productQuantity = Convert.ToInt32(reader["Quantity"]);
                        decimal currentTotalPrice = productPrice * productQuantity;

                        receiptBuilder.AppendLine($"ProductID: {reader["Product_Id"]}");
                        receiptBuilder.AppendLine($"Product: {GetProductName(reader["Product_Id"].ToString())}");
                        receiptBuilder.AppendLine($"Price: {productPrice}");
                        receiptBuilder.AppendLine($"Quantity: {productQuantity}");

                        if (reader["Addon_Id"] != DBNull.Value)
                        {
                            decimal addOnsPrice = GetAddOnsPrice(reader["Addon_Id"].ToString());
                            int addOnsQuantity = Convert.ToInt32(reader["Addon_Quantity"]);
                            currentTotalPrice += addOnsPrice * addOnsQuantity;

                            receiptBuilder.AppendLine($"Add-Ons ID: {reader["Addon_Id"]}");
                            receiptBuilder.AppendLine($"Add-On: {GetAddOnsName(reader["Addon_Id"].ToString())}");
                            receiptBuilder.AppendLine($"Price: {addOnsPrice}");
                            receiptBuilder.AppendLine($"Quantity: {addOnsQuantity}");
                        }

                        receiptBuilder.AppendLine("---------------------------------------");
                        totalCustomerPrice += currentTotalPrice;
                    }
                }
            }

            receiptBuilder.AppendLine($"Total Price: ₱{totalCustomerPrice}");
            receiptBuilder.AppendLine($"Status: {customerStatus}");
            receiptBuilder.AppendLine("Your order will be processed");
            receiptBuilder.AppendLine("Thank you!");

            return receiptBuilder.ToString();
        }
        //Method to set the whole layout of reciept

        private void ReceiptPrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font font = new Font(FONT_NAME, 15, FontStyle.Italic);
            e.Graphics.DrawString(receiptContent, font, Brushes.Black, new PointF());
        }
        //Method to set the font style or size of reciept

        private void ReceiptPrintDocument_EndPrint(object sender, PrintEventArgs e)
        {
            MessageBox.Show("Receipt printed successfully!");
        }
        //Method to print reciecpt
    }
}
