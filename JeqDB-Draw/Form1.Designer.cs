namespace JeqDB_Draw
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.MapImg = new System.Windows.Forms.PictureBox();
            this.RC = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.RC_Draw = new System.Windows.Forms.ToolStripMenuItem();
            this.RC_output = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.MapImg)).BeginInit();
            this.RC.SuspendLayout();
            this.SuspendLayout();
            // 
            // MapImg
            // 
            this.MapImg.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.MapImg.Location = new System.Drawing.Point(0, 0);
            this.MapImg.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MapImg.Name = "MapImg";
            this.MapImg.Size = new System.Drawing.Size(667, 625);
            this.MapImg.TabIndex = 0;
            this.MapImg.TabStop = false;
            this.MapImg.Click += new System.EventHandler(this.MapImg_Click);
            // 
            // RC
            // 
            this.RC.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.RC.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RC_Draw,
            this.RC_output});
            this.RC.Name = "ContextMenuStrip";
            this.RC.Size = new System.Drawing.Size(211, 80);
            // 
            // RC_Draw
            // 
            this.RC_Draw.Name = "RC_Draw";
            this.RC_Draw.Size = new System.Drawing.Size(210, 24);
            this.RC_Draw.Text = "描画";
            this.RC_Draw.Click += new System.EventHandler(this.RC_Draw_Click);
            // 
            // RC_output
            // 
            this.RC_output.Name = "RC_output";
            this.RC_output.Size = new System.Drawing.Size(210, 24);
            this.RC_output.Text = "動画出力";
            this.RC_output.Click += new System.EventHandler(this.RC_output_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 625);
            this.ContextMenuStrip = this.RC;
            this.Controls.Add(this.MapImg);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Form1";
            this.Text = "JeqDB-Draw";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.MapImg)).EndInit();
            this.RC.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox MapImg;
        private System.Windows.Forms.ContextMenuStrip RC;
        private System.Windows.Forms.ToolStripMenuItem RC_Draw;
        private System.Windows.Forms.ToolStripMenuItem RC_output;
    }
}

