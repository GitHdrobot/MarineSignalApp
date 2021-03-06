﻿namespace MarineSignalApp
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.logContainer = new DevExpress.XtraEditors.ListBoxControl();
            this.groupControl2 = new DevExpress.XtraEditors.GroupControl();
            this.checkEditError = new DevExpress.XtraEditors.CheckEdit();
            this.checkEditWarn = new DevExpress.XtraEditors.CheckEdit();
            this.checkEditDemod = new DevExpress.XtraEditors.CheckEdit();
            this.checkEditDebug = new DevExpress.XtraEditors.CheckEdit();
            this.ribbonControl1 = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.skinRibbonGalleryBarItem1 = new DevExpress.XtraBars.SkinRibbonGalleryBarItem();
            this.skinRibbonGalleryBarItem2 = new DevExpress.XtraBars.SkinRibbonGalleryBarItem();
            this.barButtonItem2 = new DevExpress.XtraBars.BarButtonItem();
            this.barStaticItem1 = new DevExpress.XtraBars.BarStaticItem();
            this.ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroup1 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonPage2 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroup2 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.groupControl3 = new DevExpress.XtraEditors.GroupControl();
            this.groupControl4 = new DevExpress.XtraEditors.GroupControl();
            this.groupControl5 = new DevExpress.XtraEditors.GroupControl();
            this.simpleButton3 = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton2 = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.textEdit3 = new DevExpress.XtraEditors.TextEdit();
            this.textEdit2 = new DevExpress.XtraEditors.TextEdit();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.textEdit1 = new DevExpress.XtraEditors.TextEdit();
            this.fileDlg = new System.Windows.Forms.OpenFileDialog();
            this.BgFetchDataWorker = new System.ComponentModel.BackgroundWorker();
            this.bgSyncHFrameWorker = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logContainer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
            this.groupControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditError.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditWarn.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditDemod.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditDebug.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl5)).BeginInit();
            this.groupControl5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit3.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // groupControl1
            // 
            this.groupControl1.Controls.Add(this.logContainer);
            this.groupControl1.Location = new System.Drawing.Point(2, 346);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(980, 149);
            this.groupControl1.TabIndex = 0;
            this.groupControl1.Text = "控制台";
            this.groupControl1.DoubleClick += new System.EventHandler(this.groupControl1_DoubleClick);
            // 
            // logContainer
            // 
            this.logContainer.Location = new System.Drawing.Point(5, 24);
            this.logContainer.Name = "logContainer";
            this.logContainer.Size = new System.Drawing.Size(970, 120);
            this.logContainer.TabIndex = 0;
            // 
            // groupControl2
            // 
            this.groupControl2.Controls.Add(this.checkEditError);
            this.groupControl2.Controls.Add(this.checkEditWarn);
            this.groupControl2.Controls.Add(this.checkEditDemod);
            this.groupControl2.Controls.Add(this.checkEditDebug);
            this.groupControl2.Location = new System.Drawing.Point(988, 346);
            this.groupControl2.Name = "groupControl2";
            this.groupControl2.Size = new System.Drawing.Size(171, 149);
            this.groupControl2.TabIndex = 1;
            this.groupControl2.Text = "输出控制";
            // 
            // checkEditError
            // 
            this.checkEditError.EditValue = true;
            this.checkEditError.Location = new System.Drawing.Point(25, 110);
            this.checkEditError.Name = "checkEditError";
            this.checkEditError.Properties.Caption = "错误信息";
            this.checkEditError.Size = new System.Drawing.Size(75, 19);
            this.checkEditError.TabIndex = 3;
            // 
            // checkEditWarn
            // 
            this.checkEditWarn.EditValue = true;
            this.checkEditWarn.Location = new System.Drawing.Point(25, 84);
            this.checkEditWarn.Name = "checkEditWarn";
            this.checkEditWarn.Properties.Caption = "警告信息";
            this.checkEditWarn.Size = new System.Drawing.Size(75, 19);
            this.checkEditWarn.TabIndex = 2;
            // 
            // checkEditDemod
            // 
            this.checkEditDemod.EditValue = true;
            this.checkEditDemod.Location = new System.Drawing.Point(25, 34);
            this.checkEditDemod.Name = "checkEditDemod";
            this.checkEditDemod.Properties.Caption = "解调信息";
            this.checkEditDemod.Size = new System.Drawing.Size(75, 19);
            this.checkEditDemod.TabIndex = 1;
            // 
            // checkEditDebug
            // 
            this.checkEditDebug.EditValue = true;
            this.checkEditDebug.Location = new System.Drawing.Point(25, 59);
            this.checkEditDebug.Name = "checkEditDebug";
            this.checkEditDebug.Properties.Caption = "调试信息";
            this.checkEditDebug.Size = new System.Drawing.Size(75, 19);
            this.checkEditDebug.TabIndex = 0;
            // 
            // ribbonControl1
            // 
            this.ribbonControl1.Dock = System.Windows.Forms.DockStyle.None;
            this.ribbonControl1.ExpandCollapseItem.Id = 0;
            this.ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbonControl1.ExpandCollapseItem,
            this.skinRibbonGalleryBarItem1,
            this.skinRibbonGalleryBarItem2,
            this.barButtonItem2,
            this.barStaticItem1});
            this.ribbonControl1.Location = new System.Drawing.Point(0, 0);
            this.ribbonControl1.MaxItemId = 12;
            this.ribbonControl1.Minimized = true;
            this.ribbonControl1.Name = "ribbonControl1";
            this.ribbonControl1.PageHeaderItemLinks.Add(this.barStaticItem1);
            this.ribbonControl1.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            this.ribbonPage1,
            this.ribbonPage2});
            this.ribbonControl1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ribbonControl1.Size = new System.Drawing.Size(1161, 50);
            this.ribbonControl1.ToolbarLocation = DevExpress.XtraBars.Ribbon.RibbonQuickAccessToolbarLocation.Hidden;
            // 
            // skinRibbonGalleryBarItem1
            // 
            this.skinRibbonGalleryBarItem1.Caption = "skinRibbonGalleryBarItem1";
            this.skinRibbonGalleryBarItem1.Id = 1;
            this.skinRibbonGalleryBarItem1.Name = "skinRibbonGalleryBarItem1";
            // 
            // skinRibbonGalleryBarItem2
            // 
            this.skinRibbonGalleryBarItem2.Caption = "skinRibbonGalleryBarItem2";
            this.skinRibbonGalleryBarItem2.Id = 2;
            this.skinRibbonGalleryBarItem2.Name = "skinRibbonGalleryBarItem2";
            // 
            // barButtonItem2
            // 
            this.barButtonItem2.Caption = "barButtonItem2";
            this.barButtonItem2.Id = 9;
            this.barButtonItem2.Name = "barButtonItem2";
            // 
            // barStaticItem1
            // 
            this.barStaticItem1.AllowGlyphSkinning = DevExpress.Utils.DefaultBoolean.False;
            this.barStaticItem1.Caption = "同步状态";
            this.barStaticItem1.Glyph = ((System.Drawing.Image)(resources.GetObject("barStaticItem1.Glyph")));
            this.barStaticItem1.Id = 11;
            this.barStaticItem1.LargeGlyph = ((System.Drawing.Image)(resources.GetObject("barStaticItem1.LargeGlyph")));
            this.barStaticItem1.Name = "barStaticItem1";
            this.barStaticItem1.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // ribbonPage1
            // 
            this.ribbonPage1.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.ribbonPageGroup1});
            this.ribbonPage1.Name = "ribbonPage1";
            this.ribbonPage1.Text = "皮肤";
            // 
            // ribbonPageGroup1
            // 
            this.ribbonPageGroup1.ItemLinks.Add(this.skinRibbonGalleryBarItem2);
            this.ribbonPageGroup1.Name = "ribbonPageGroup1";
            this.ribbonPageGroup1.Text = "皮肤";
            // 
            // ribbonPage2
            // 
            this.ribbonPage2.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.ribbonPageGroup2});
            this.ribbonPage2.Name = "ribbonPage2";
            this.ribbonPage2.Text = "帮助";
            // 
            // ribbonPageGroup2
            // 
            this.ribbonPageGroup2.Name = "ribbonPageGroup2";
            // 
            // groupControl3
            // 
            this.groupControl3.Location = new System.Drawing.Point(875, 56);
            this.groupControl3.Name = "groupControl3";
            this.groupControl3.Size = new System.Drawing.Size(284, 284);
            this.groupControl3.TabIndex = 3;
            this.groupControl3.Text = "星座图";
            // 
            // groupControl4
            // 
            this.groupControl4.Location = new System.Drawing.Point(339, 56);
            this.groupControl4.Name = "groupControl4";
            this.groupControl4.Size = new System.Drawing.Size(530, 284);
            this.groupControl4.TabIndex = 4;
            this.groupControl4.Text = "数据采集";
            // 
            // groupControl5
            // 
            this.groupControl5.Controls.Add(this.simpleButton3);
            this.groupControl5.Controls.Add(this.simpleButton2);
            this.groupControl5.Controls.Add(this.simpleButton1);
            this.groupControl5.Controls.Add(this.textEdit3);
            this.groupControl5.Controls.Add(this.textEdit2);
            this.groupControl5.Controls.Add(this.labelControl2);
            this.groupControl5.Controls.Add(this.labelControl1);
            this.groupControl5.Controls.Add(this.textEdit1);
            this.groupControl5.Location = new System.Drawing.Point(2, 56);
            this.groupControl5.Name = "groupControl5";
            this.groupControl5.Size = new System.Drawing.Size(331, 284);
            this.groupControl5.TabIndex = 6;
            this.groupControl5.Text = "读取文件";
            // 
            // simpleButton3
            // 
            this.simpleButton3.Location = new System.Drawing.Point(185, 175);
            this.simpleButton3.Name = "simpleButton3";
            this.simpleButton3.Size = new System.Drawing.Size(75, 23);
            this.simpleButton3.TabIndex = 7;
            this.simpleButton3.Text = "停止读文件";
            this.simpleButton3.Click += new System.EventHandler(this.simpleButton3_Click);
            // 
            // simpleButton2
            // 
            this.simpleButton2.Location = new System.Drawing.Point(54, 175);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(72, 23);
            this.simpleButton2.TabIndex = 6;
            this.simpleButton2.Text = "开始读文件";
            this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(236, 119);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(48, 23);
            this.simpleButton1.TabIndex = 5;
            this.simpleButton1.Text = "...";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // textEdit3
            // 
            this.textEdit3.Location = new System.Drawing.Point(39, 120);
            this.textEdit3.MenuManager = this.ribbonControl1;
            this.textEdit3.Name = "textEdit3";
            this.textEdit3.Size = new System.Drawing.Size(158, 20);
            this.textEdit3.TabIndex = 4;
            // 
            // textEdit2
            // 
            this.textEdit2.Location = new System.Drawing.Point(161, 76);
            this.textEdit2.MenuManager = this.ribbonControl1;
            this.textEdit2.Name = "textEdit2";
            this.textEdit2.Size = new System.Drawing.Size(123, 20);
            this.textEdit2.TabIndex = 3;
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(39, 79);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(72, 14);
            this.labelControl2.TabIndex = 2;
            this.labelControl2.Text = "读取文件次数";
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(39, 35);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(72, 14);
            this.labelControl1.TabIndex = 1;
            this.labelControl1.Text = "读取文件长度";
            // 
            // textEdit1
            // 
            this.textEdit1.Location = new System.Drawing.Point(161, 32);
            this.textEdit1.MenuManager = this.ribbonControl1;
            this.textEdit1.Name = "textEdit1";
            this.textEdit1.Size = new System.Drawing.Size(123, 20);
            this.textEdit1.TabIndex = 0;
            // 
            // fileDlg
            // 
            this.fileDlg.FileName = "openFileDialog1";
            // 
            // BgFetchDataWorker
            // 
            this.BgFetchDataWorker.WorkerReportsProgress = true;
            this.BgFetchDataWorker.WorkerSupportsCancellation = true;
            this.BgFetchDataWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BgFetchDataWorker_DoWork);
            this.BgFetchDataWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BgFetchDataWorker_ProgressChanged);
            this.BgFetchDataWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BgFetchDataWorker_RunWorkerCompleted);
            // 
            // bgSyncHFrameWorker
            // 
            this.bgSyncHFrameWorker.WorkerReportsProgress = true;
            this.bgSyncHFrameWorker.WorkerSupportsCancellation = true;
            this.bgSyncHFrameWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgSyncHFrameWorker_DoWork);
            this.bgSyncHFrameWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgSyncHFrameWorker_ProgressChanged);
            this.bgSyncHFrameWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgSyncHFrameWorker_RunWorkerCompleted);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1161, 499);
            this.Controls.Add(this.groupControl3);
            this.Controls.Add(this.groupControl5);
            this.Controls.Add(this.groupControl4);
            this.Controls.Add(this.groupControl2);
            this.Controls.Add(this.groupControl1);
            this.Controls.Add(this.ribbonControl1);
            this.Name = "Form1";
            this.Ribbon = this.ribbonControl1;
            this.Text = "海事卫星信号分析";
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.logContainer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
            this.groupControl2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.checkEditError.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditWarn.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditDemod.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditDebug.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl5)).EndInit();
            this.groupControl5.ResumeLayout(false);
            this.groupControl5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit3.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.GroupControl groupControl1;
        private DevExpress.XtraEditors.ListBoxControl logContainer;
        private DevExpress.XtraEditors.GroupControl groupControl2;
        private DevExpress.XtraEditors.CheckEdit checkEditDebug;
        private DevExpress.XtraEditors.CheckEdit checkEditDemod;
        private DevExpress.XtraEditors.CheckEdit checkEditWarn;
        private DevExpress.XtraEditors.CheckEdit checkEditError;
        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage1;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup1;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage2;
        private DevExpress.XtraBars.SkinRibbonGalleryBarItem skinRibbonGalleryBarItem1;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup2;
        private DevExpress.XtraBars.SkinRibbonGalleryBarItem skinRibbonGalleryBarItem2;
        private DevExpress.XtraBars.BarButtonItem barButtonItem2;
        private DevExpress.XtraEditors.GroupControl groupControl3;
        private DevExpress.XtraEditors.GroupControl groupControl4;
        private DevExpress.XtraEditors.GroupControl groupControl5;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.TextEdit textEdit1;
        private DevExpress.XtraEditors.TextEdit textEdit2;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.TextEdit textEdit3;
        private DevExpress.XtraEditors.SimpleButton simpleButton2;
        private DevExpress.XtraEditors.SimpleButton simpleButton3;
        private System.Windows.Forms.OpenFileDialog fileDlg;
        private System.ComponentModel.BackgroundWorker BgFetchDataWorker;
        private System.ComponentModel.BackgroundWorker bgSyncHFrameWorker;
        private DevExpress.XtraBars.BarStaticItem barStaticItem1;
    }
}

