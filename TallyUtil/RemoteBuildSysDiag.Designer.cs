namespace TallyUtil
{
    partial class RemoteBuildSysDiag
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
            this.Hosts = new System.Windows.Forms.ComboBox();
            this.Confirm = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Hosts
            // 
            this.Hosts.FormattingEnabled = true;
            this.Hosts.Location = new System.Drawing.Point(25, 46);
            this.Hosts.Name = "Hosts";
            this.Hosts.Size = new System.Drawing.Size(257, 21);
            this.Hosts.TabIndex = 0;
            this.Hosts.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // Confirm
            // 
            this.Confirm.Location = new System.Drawing.Point(164, 91);
            this.Confirm.Name = "Confirm";
            this.Confirm.Size = new System.Drawing.Size(118, 28);
            this.Confirm.TabIndex = 1;
            this.Confirm.Text = "Generate Profile";
            this.Confirm.UseVisualStyleBackColor = true;
            this.Confirm.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select the Remote System";
            // 
            // RemoteBuildSysDiag
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(302, 139);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Confirm);
            this.Controls.Add(this.Hosts);
            this.Name = "RemoteBuildSysDiag";
            this.Text = "Remote Build System Selection";
            this.Load += new System.EventHandler(this.RemoteBuildSysDiag_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox Hosts;
        private System.Windows.Forms.Button Confirm;
        private System.Windows.Forms.Label label1;
    }
}