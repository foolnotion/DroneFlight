﻿namespace DroneFlightPathUI {
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
      this.button_LoadMap = new System.Windows.Forms.Button();
      this.button_LoadSolution = new System.Windows.Forms.Button();
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
      this.button_Step.Click += new System.EventHandler(this.button_Step_Click);
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
      this.pictureBox.Size = new System.Drawing.Size(937, 662);
      this.pictureBox.TabIndex = 1;
      this.pictureBox.TabStop = false;
      // 
      // button_LoadMap
      // 
      this.button_LoadMap.Location = new System.Drawing.Point(865, 2);
      this.button_LoadMap.Name = "button_LoadMap";
      this.button_LoadMap.Size = new System.Drawing.Size(75, 23);
      this.button_LoadMap.TabIndex = 0;
      this.button_LoadMap.Text = "Load Map";
      this.button_LoadMap.UseVisualStyleBackColor = true;
      this.button_LoadMap.Click += new System.EventHandler(this.button_LoadMap_Click);
      // 
      // button_LoadSolution
      // 
      this.button_LoadSolution.Location = new System.Drawing.Point(776, 2);
      this.button_LoadSolution.Name = "button_LoadSolution";
      this.button_LoadSolution.Size = new System.Drawing.Size(83, 23);
      this.button_LoadSolution.TabIndex = 0;
      this.button_LoadSolution.Text = "Load Solution";
      this.button_LoadSolution.UseVisualStyleBackColor = true;
      this.button_LoadSolution.Click += new System.EventHandler(this.button_LoadSolution_Click);
      // 
      // MapView
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(952, 705);
      this.Controls.Add(this.pictureBox);
      this.Controls.Add(this.button_LoadSolution);
      this.Controls.Add(this.button_LoadMap);
      this.Controls.Add(this.button_Pause);
      this.Controls.Add(this.button_Step);
      this.Controls.Add(this.button_Run);
      this.Name = "MapView";
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
    private System.Windows.Forms.Button button_LoadMap;
    private System.Windows.Forms.Button button_LoadSolution;
  }
}