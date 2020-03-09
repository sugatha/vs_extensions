namespace TallyUtil
{
    partial class NewProject
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
            this.label1 = new System.Windows.Forms.Label();
            this.Project_Name = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Project_Type = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.References = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Project Name";
            // 
            // Project_Name
            // 
            this.Project_Name.Location = new System.Drawing.Point(39, 48);
            this.Project_Name.Name = "Project_Name";
            this.Project_Name.Size = new System.Drawing.Size(255, 20);
            this.Project_Name.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(36, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Type";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // Project_Type
            // 
            this.Project_Type.FormattingEnabled = true;
            this.Project_Type.Items.AddRange(new object[] {
            "Shared Items",
            "Executable",
            "Static Library",
            "Shared Library"});
            this.Project_Type.Location = new System.Drawing.Point(39, 98);
            this.Project_Type.Name = "Project_Type";
            this.Project_Type.Size = new System.Drawing.Size(255, 21);
            this.Project_Type.TabIndex = 4;
            this.Project_Type.SelectedIndexChanged += new System.EventHandler(this.Project_Type_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(36, 138);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Reference";
            // 
            // References
            // 
            this.References.FormattingEnabled = true;
            this.References.Location = new System.Drawing.Point(39, 154);
            this.References.Name = "References";
            this.References.Size = new System.Drawing.Size(255, 21);
            this.References.TabIndex = 6;
            this.References.SelectedIndexChanged += new System.EventHandler(this.References_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(228, 209);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(153, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "Create Project";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // NewProject
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(441, 261);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.References);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Project_Type);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Project_Name);
            this.Controls.Add(this.label1);
            this.Name = "NewProject";
            this.Text = "NewProject";
            this.Load += new System.EventHandler(this.NewProject_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox Project_Name;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox Project_Type;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox References;
        private System.Windows.Forms.Button button1;
    }
}