namespace Consoleify.GoogleTVNetworkCEC
{
    partial class SettingsForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtCommand = new TextBox();
            label1 = new Label();
            btnApply = new Button();
            btnCancel = new Button();
            label2 = new Label();
            txtIpAddress = new TextBox();
            SuspendLayout();
            // 
            // txtCommand
            // 
            txtCommand.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCommand.Location = new Point(157, 45);
            txtCommand.Name = "txtCommand";
            txtCommand.Size = new Size(427, 23);
            txtCommand.TabIndex = 0;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(12, 48);
            label1.Name = "label1";
            label1.Size = new Size(139, 15);
            label1.TabIndex = 1;
            label1.Text = "HDMI Switch Command:";
            // 
            // btnApply
            // 
            btnApply.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnApply.Location = new Point(509, 102);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(75, 23);
            btnApply.TabIndex = 2;
            btnApply.Text = "Apply";
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Click += btnApply_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Location = new Point(428, 102);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 15);
            label2.Name = "label2";
            label2.Size = new Size(82, 15);
            label2.TabIndex = 4;
            label2.Text = "TV IP Address:";
            // 
            // txtIpAddress
            // 
            txtIpAddress.Location = new Point(157, 12);
            txtIpAddress.Name = "txtIpAddress";
            txtIpAddress.Size = new Size(427, 23);
            txtIpAddress.TabIndex = 5;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(596, 137);
            Controls.Add(txtIpAddress);
            Controls.Add(label2);
            Controls.Add(btnCancel);
            Controls.Add(btnApply);
            Controls.Add(label1);
            Controls.Add(txtCommand);
            Name = "SettingsForm";
            Text = "Form";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtCommand;
        private Label label1;
        private Button btnApply;
        private Button btnCancel;
        private Label label2;
        private TextBox txtIpAddress;
    }
}
