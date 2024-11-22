namespace LimpiarCsv
{
    partial class Form1
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
            btnProcessCSV = new Button();
            label1 = new Label();
            cmbCsvType = new ComboBox();
            SuspendLayout();
            // 
            // btnProcessCSV
            // 
            btnProcessCSV.Location = new Point(79, 101);
            btnProcessCSV.Name = "btnProcessCSV";
            btnProcessCSV.Size = new Size(123, 23);
            btnProcessCSV.TabIndex = 0;
            btnProcessCSV.Text = "Procesar";
            btnProcessCSV.UseVisualStyleBackColor = true;
            btnProcessCSV.Click += button1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(79, 127);
            label1.Name = "label1";
            label1.Size = new Size(38, 15);
            label1.TabIndex = 1;
            label1.Text = "label1";
            // 
            // cmbCsvType
            // 
            cmbCsvType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCsvType.FormattingEnabled = true;
            cmbCsvType.Location = new Point(81, 72);
            cmbCsvType.Name = "cmbCsvType";
            cmbCsvType.Size = new Size(121, 23);
            cmbCsvType.TabIndex = 2;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(cmbCsvType);
            Controls.Add(label1);
            Controls.Add(btnProcessCSV);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnProcessCSV;
        private Label label1;
        private ComboBox cmbCsvType;
    }
}
