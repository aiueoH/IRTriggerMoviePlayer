namespace DirectionSampleTest
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox_input = new System.Windows.Forms.PictureBox();
            this.button_load = new System.Windows.Forms.Button();
            this.textBox_reuslt = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_input)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox_input
            // 
            this.pictureBox_input.Location = new System.Drawing.Point(13, 42);
            this.pictureBox_input.Name = "pictureBox_input";
            this.pictureBox_input.Size = new System.Drawing.Size(640, 480);
            this.pictureBox_input.TabIndex = 0;
            this.pictureBox_input.TabStop = false;
            // 
            // button_load
            // 
            this.button_load.Location = new System.Drawing.Point(13, 13);
            this.button_load.Name = "button_load";
            this.button_load.Size = new System.Drawing.Size(75, 23);
            this.button_load.TabIndex = 1;
            this.button_load.Text = "Load";
            this.button_load.UseVisualStyleBackColor = true;
            this.button_load.Click += new System.EventHandler(this.button_load_Click);
            // 
            // textBox_reuslt
            // 
            this.textBox_reuslt.Location = new System.Drawing.Point(95, 13);
            this.textBox_reuslt.Name = "textBox_reuslt";
            this.textBox_reuslt.ReadOnly = true;
            this.textBox_reuslt.Size = new System.Drawing.Size(100, 22);
            this.textBox_reuslt.TabIndex = 2;
            this.textBox_reuslt.Text = "先載入圖片";
            this.textBox_reuslt.WordWrap = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(670, 542);
            this.Controls.Add(this.textBox_reuslt);
            this.Controls.Add(this.button_load);
            this.Controls.Add(this.pictureBox_input);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_input)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox_input;
        private System.Windows.Forms.Button button_load;
        private System.Windows.Forms.TextBox textBox_reuslt;
    }
}

