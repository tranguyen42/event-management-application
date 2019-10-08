namespace VM
{
    partial class VMachine
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VMachine));
            this.btnScanRFID = new System.Windows.Forms.Button();
            this.panel15 = new System.Windows.Forms.Panel();
            this.btnPay = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tbBalance = new System.Windows.Forms.TextBox();
            this.tbCustomerName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lbVendingMachine = new System.Windows.Forms.ListBox();
            this.cbSelectItemVM = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.tbcVM = new System.Windows.Forms.TabControl();
            this.tabVM = new System.Windows.Forms.TabPage();
            this.panel15.SuspendLayout();
            this.tbcVM.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnScanRFID
            // 
            this.btnScanRFID.BackColor = System.Drawing.Color.SkyBlue;
            this.btnScanRFID.ForeColor = System.Drawing.Color.MidnightBlue;
            this.btnScanRFID.Location = new System.Drawing.Point(67, 280);
            this.btnScanRFID.Margin = new System.Windows.Forms.Padding(2);
            this.btnScanRFID.Name = "btnScanRFID";
            this.btnScanRFID.Size = new System.Drawing.Size(188, 46);
            this.btnScanRFID.TabIndex = 19;
            this.btnScanRFID.Text = "Scan RFID";
            this.btnScanRFID.UseVisualStyleBackColor = false;
            this.btnScanRFID.Click += new System.EventHandler(this.btnScanRFID_Click);
            // 
            // panel15
            // 
            this.panel15.BackColor = System.Drawing.Color.White;
            this.panel15.Controls.Add(this.btnPay);
            this.panel15.Controls.Add(this.btnCancel);
            this.panel15.Controls.Add(this.tbBalance);
            this.panel15.Controls.Add(this.tbCustomerName);
            this.panel15.Controls.Add(this.label2);
            this.panel15.Controls.Add(this.label1);
            this.panel15.Controls.Add(this.lbVendingMachine);
            this.panel15.Controls.Add(this.cbSelectItemVM);
            this.panel15.Controls.Add(this.btnScanRFID);
            this.panel15.Controls.Add(this.label13);
            this.panel15.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel15.Location = new System.Drawing.Point(407, 13);
            this.panel15.Margin = new System.Windows.Forms.Padding(2);
            this.panel15.Name = "panel15";
            this.panel15.Size = new System.Drawing.Size(318, 477);
            this.panel15.TabIndex = 15;
            // 
            // btnPay
            // 
            this.btnPay.BackColor = System.Drawing.Color.SkyBlue;
            this.btnPay.Enabled = false;
            this.btnPay.ForeColor = System.Drawing.Color.MidnightBlue;
            this.btnPay.Location = new System.Drawing.Point(166, 408);
            this.btnPay.Margin = new System.Windows.Forms.Padding(2);
            this.btnPay.Name = "btnPay";
            this.btnPay.Size = new System.Drawing.Size(126, 46);
            this.btnPay.TabIndex = 27;
            this.btnPay.Text = "Pay";
            this.btnPay.UseVisualStyleBackColor = false;
            this.btnPay.Click += new System.EventHandler(this.btnPay_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.SkyBlue;
            this.btnCancel.Enabled = false;
            this.btnCancel.ForeColor = System.Drawing.Color.MidnightBlue;
            this.btnCancel.Location = new System.Drawing.Point(23, 408);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(126, 46);
            this.btnCancel.TabIndex = 26;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tbBalance
            // 
            this.tbBalance.Enabled = false;
            this.tbBalance.Location = new System.Drawing.Point(111, 232);
            this.tbBalance.Name = "tbBalance";
            this.tbBalance.Size = new System.Drawing.Size(177, 26);
            this.tbBalance.TabIndex = 25;
            // 
            // tbCustomerName
            // 
            this.tbCustomerName.Enabled = false;
            this.tbCustomerName.Location = new System.Drawing.Point(111, 201);
            this.tbCustomerName.Name = "tbCustomerName";
            this.tbCustomerName.Size = new System.Drawing.Size(177, 26);
            this.tbCustomerName.TabIndex = 24;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 234);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 20);
            this.label2.TabIndex = 23;
            this.label2.Text = "Balance:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 204);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 20);
            this.label1.TabIndex = 22;
            this.label1.Text = "Customer: ";
            // 
            // lbVendingMachine
            // 
            this.lbVendingMachine.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbVendingMachine.FormattingEnabled = true;
            this.lbVendingMachine.HorizontalScrollbar = true;
            this.lbVendingMachine.ItemHeight = 16;
            this.lbVendingMachine.Items.AddRange(new object[] {
            "Welcome to Latitude!",
            "",
            "Please follow steps below to buy snack/drink ",
            "at our vending machine:",
            "1. Scan your RFID",
            "2. Select item nr. in the selection box",
            "3. Click pay & enjoy your selected snack/ drink",
            "or cancel if you change your mind",
            "",
            "We hope you have a great time at Latitude!"});
            this.lbVendingMachine.Location = new System.Drawing.Point(14, 13);
            this.lbVendingMachine.Name = "lbVendingMachine";
            this.lbVendingMachine.Size = new System.Drawing.Size(287, 164);
            this.lbVendingMachine.TabIndex = 21;
            // 
            // cbSelectItemVM
            // 
            this.cbSelectItemVM.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSelectItemVM.Enabled = false;
            this.cbSelectItemVM.FormattingEnabled = true;
            this.cbSelectItemVM.Location = new System.Drawing.Point(195, 353);
            this.cbSelectItemVM.Name = "cbSelectItemVM";
            this.cbSelectItemVM.Size = new System.Drawing.Size(44, 28);
            this.cbSelectItemVM.TabIndex = 20;
            this.cbSelectItemVM.SelectedIndexChanged += new System.EventHandler(this.cbSelectItemVM_SelectedIndexChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(79, 357);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(110, 20);
            this.label13.TabIndex = 1;
            this.label13.Text = "Select item nr.";
            // 
            // tbcVM
            // 
            this.tbcVM.Controls.Add(this.tabVM);
            this.tbcVM.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbcVM.ItemSize = new System.Drawing.Size(397, 30);
            this.tbcVM.Location = new System.Drawing.Point(13, 13);
            this.tbcVM.Name = "tbcVM";
            this.tbcVM.SelectedIndex = 0;
            this.tbcVM.Size = new System.Drawing.Size(382, 477);
            this.tbcVM.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tbcVM.TabIndex = 16;
            // 
            // tabVM
            // 
            this.tabVM.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabVM.Location = new System.Drawing.Point(4, 34);
            this.tabVM.Name = "tabVM";
            this.tabVM.Padding = new System.Windows.Forms.Padding(3);
            this.tabVM.Size = new System.Drawing.Size(374, 439);
            this.tabVM.TabIndex = 0;
            this.tabVM.Text = "Latitude\'s Vending Machine";
            this.tabVM.UseVisualStyleBackColor = true;
            // 
            // VMachine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(734, 502);
            this.Controls.Add(this.tbcVM);
            this.Controls.Add(this.panel15);
            this.Name = "VMachine";
            this.Text = "Latitude Vending Machine";
            this.panel15.ResumeLayout(false);
            this.panel15.PerformLayout();
            this.tbcVM.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnScanRFID;
        private System.Windows.Forms.Panel panel15;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox cbSelectItemVM;
        private System.Windows.Forms.TabControl tbcVM;
        private System.Windows.Forms.TabPage tabVM;
        private System.Windows.Forms.Button btnPay;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox tbBalance;
        private System.Windows.Forms.TextBox tbCustomerName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox lbVendingMachine;
    }
}

