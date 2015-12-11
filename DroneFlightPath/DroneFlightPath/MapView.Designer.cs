﻿namespace DroneFlightPath {
  partial class MapView {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      this.button_Run = new System.Windows.Forms.Button();
      this.button_Step = new System.Windows.Forms.Button();
      this.button_Pause = new System.Windows.Forms.Button();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.pictureBox = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // button_Run
      // 
      this.button_Run.Location = new System.Drawing.Point(3, 2);
      this.button_Run.Name = "button_Run";
      this.button_Run.Size = new System.Drawing.Size(75, 23);
      this.button_Run.TabIndex = 0;
      this.button_Run.Text = "Run";
      this.button_Run.UseVisualStyleBackColor = true;
      // 
      // button_Step
      // 
      this.button_Step.Location = new System.Drawing.Point(84, 2);
      this.button_Step.Name = "button_Step";
      this.button_Step.Size = new System.Drawing.Size(75, 23);
      this.button_Step.TabIndex = 0;
      this.button_Step.Text = "Step";
      this.button_Step.UseVisualStyleBackColor = true;
      // 
      // button_Pause
      // 
      this.button_Pause.Location = new System.Drawing.Point(165, 2);
      this.button_Pause.Name = "button_Pause";
      this.button_Pause.Size = new System.Drawing.Size(75, 23);
      this.button_Pause.TabIndex = 0;
      this.button_Pause.Text = "Pause";
      this.button_Pause.UseVisualStyleBackColor = true;
      // 
      // imageList1
      // 
      this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
      this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      // 
      // pictureBox
      // 
      this.pictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBox.Location = new System.Drawing.Point(3, 31);
      this.pictureBox.Name = "pictureBox";
      this.pictureBox.Size = new System.Drawing.Size(812, 662);
      this.pictureBox.TabIndex = 1;
      this.pictureBox.TabStop = false;
      // 
      // Map
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(952, 705);
      this.Controls.Add(this.pictureBox);
      this.Controls.Add(this.button_Pause);
      this.Controls.Add(this.button_Step);
      this.Controls.Add(this.button_Run);
      this.Name = "Map";
      this.Text = "Map";
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button button_Run;
    private System.Windows.Forms.Button button_Step;
    private System.Windows.Forms.Button button_Pause;
    private System.Windows.Forms.ImageList imageList1;
    private System.Windows.Forms.PictureBox pictureBox;
  }
}