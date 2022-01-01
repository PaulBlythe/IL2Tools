namespace IL2Modder
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adjustMatrixToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dumpAsTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editMaterialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showHooksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addMshNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotate90ZToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractEulersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flipNormalsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mapToVariableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addAnimationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.focusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addCollisionMeshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.objectViewer1 = new IL2Modder.ObjectViewer();
            this.splitOffFacegroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyNodeToolStripMenuItem,
            this.pasteNodeToolStripMenuItem,
            this.deleteNodeToolStripMenuItem,
            this.adjustMatrixToolStripMenuItem,
            this.renameNodeToolStripMenuItem,
            this.dumpAsTextToolStripMenuItem,
            this.editMaterialToolStripMenuItem,
            this.showHooksToolStripMenuItem,
            this.replaceNodeToolStripMenuItem,
            this.addMshNodeToolStripMenuItem,
            this.rotate90ZToolStripMenuItem,
            this.extractEulersToolStripMenuItem,
            this.flipNormalsToolStripMenuItem,
            this.mapToVariableToolStripMenuItem,
            this.addAnimationToolStripMenuItem,
            this.focusToolStripMenuItem,
            this.addCollisionMeshToolStripMenuItem,
            this.splitOffFacegroupToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(187, 422);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // copyNodeToolStripMenuItem
            // 
            this.copyNodeToolStripMenuItem.Name = "copyNodeToolStripMenuItem";
            this.copyNodeToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.copyNodeToolStripMenuItem.Text = "Copy node";
            this.copyNodeToolStripMenuItem.Click += new System.EventHandler(this.copyNodeToolStripMenuItem_Click);
            // 
            // pasteNodeToolStripMenuItem
            // 
            this.pasteNodeToolStripMenuItem.Name = "pasteNodeToolStripMenuItem";
            this.pasteNodeToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.pasteNodeToolStripMenuItem.Text = "Paste node";
            this.pasteNodeToolStripMenuItem.Click += new System.EventHandler(this.pasteNodeToolStripMenuItem_Click);
            // 
            // deleteNodeToolStripMenuItem
            // 
            this.deleteNodeToolStripMenuItem.Name = "deleteNodeToolStripMenuItem";
            this.deleteNodeToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.deleteNodeToolStripMenuItem.Text = "Delete node";
            // 
            // adjustMatrixToolStripMenuItem
            // 
            this.adjustMatrixToolStripMenuItem.Name = "adjustMatrixToolStripMenuItem";
            this.adjustMatrixToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.adjustMatrixToolStripMenuItem.Text = "Adjust matrix";
            this.adjustMatrixToolStripMenuItem.Click += new System.EventHandler(this.adjustMatrixToolStripMenuItem_Click);
            // 
            // renameNodeToolStripMenuItem
            // 
            this.renameNodeToolStripMenuItem.Name = "renameNodeToolStripMenuItem";
            this.renameNodeToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.renameNodeToolStripMenuItem.Text = "Rename node";
            // 
            // dumpAsTextToolStripMenuItem
            // 
            this.dumpAsTextToolStripMenuItem.Name = "dumpAsTextToolStripMenuItem";
            this.dumpAsTextToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.dumpAsTextToolStripMenuItem.Text = "Dump as text";
            this.dumpAsTextToolStripMenuItem.Click += new System.EventHandler(this.dumpAsTextToolStripMenuItem_Click);
            // 
            // editMaterialToolStripMenuItem
            // 
            this.editMaterialToolStripMenuItem.Name = "editMaterialToolStripMenuItem";
            this.editMaterialToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.editMaterialToolStripMenuItem.Text = "Edit material";
            this.editMaterialToolStripMenuItem.Click += new System.EventHandler(this.editMaterialToolStripMenuItem_Click);
            // 
            // showHooksToolStripMenuItem
            // 
            this.showHooksToolStripMenuItem.Name = "showHooksToolStripMenuItem";
            this.showHooksToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.showHooksToolStripMenuItem.Text = "Show hooks";
            this.showHooksToolStripMenuItem.Click += new System.EventHandler(this.showHooksToolStripMenuItem_Click);
            // 
            // replaceNodeToolStripMenuItem
            // 
            this.replaceNodeToolStripMenuItem.Name = "replaceNodeToolStripMenuItem";
            this.replaceNodeToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.replaceNodeToolStripMenuItem.Text = "Replace node";
            this.replaceNodeToolStripMenuItem.Click += new System.EventHandler(this.replaceNodeToolStripMenuItem_Click);
            // 
            // addMshNodeToolStripMenuItem
            // 
            this.addMshNodeToolStripMenuItem.Name = "addMshNodeToolStripMenuItem";
            this.addMshNodeToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.addMshNodeToolStripMenuItem.Text = "Add msh node";
            this.addMshNodeToolStripMenuItem.Click += new System.EventHandler(this.addMshNodeToolStripMenuItem_Click);
            // 
            // rotate90ZToolStripMenuItem
            // 
            this.rotate90ZToolStripMenuItem.Name = "rotate90ZToolStripMenuItem";
            this.rotate90ZToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.rotate90ZToolStripMenuItem.Text = "Rotate 90 Z";
            this.rotate90ZToolStripMenuItem.Click += new System.EventHandler(this.rotate90ZToolStripMenuItem_Click);
            // 
            // extractEulersToolStripMenuItem
            // 
            this.extractEulersToolStripMenuItem.Name = "extractEulersToolStripMenuItem";
            this.extractEulersToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.extractEulersToolStripMenuItem.Text = "Extract eulers";
            this.extractEulersToolStripMenuItem.Click += new System.EventHandler(this.extractEulersToolStripMenuItem_Click);
            // 
            // flipNormalsToolStripMenuItem
            // 
            this.flipNormalsToolStripMenuItem.Name = "flipNormalsToolStripMenuItem";
            this.flipNormalsToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.flipNormalsToolStripMenuItem.Text = "Flip Normals";
            this.flipNormalsToolStripMenuItem.Click += new System.EventHandler(this.flipNormalsToolStripMenuItem_Click);
            // 
            // mapToVariableToolStripMenuItem
            // 
            this.mapToVariableToolStripMenuItem.Name = "mapToVariableToolStripMenuItem";
            this.mapToVariableToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.mapToVariableToolStripMenuItem.Text = "Map to variable";
            this.mapToVariableToolStripMenuItem.Click += new System.EventHandler(this.mapToVariableToolStripMenuItem_Click);
            // 
            // addAnimationToolStripMenuItem
            // 
            this.addAnimationToolStripMenuItem.Name = "addAnimationToolStripMenuItem";
            this.addAnimationToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.addAnimationToolStripMenuItem.Text = "Add animation";
            this.addAnimationToolStripMenuItem.Click += new System.EventHandler(this.addAnimationToolStripMenuItem_Click);
            // 
            // focusToolStripMenuItem
            // 
            this.focusToolStripMenuItem.Name = "focusToolStripMenuItem";
            this.focusToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.focusToolStripMenuItem.Text = "Focus";
            this.focusToolStripMenuItem.Click += new System.EventHandler(this.focusToolStripMenuItem_Click);
            // 
            // addCollisionMeshToolStripMenuItem
            // 
            this.addCollisionMeshToolStripMenuItem.Name = "addCollisionMeshToolStripMenuItem";
            this.addCollisionMeshToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.addCollisionMeshToolStripMenuItem.Text = "Add collision mesh";
            this.addCollisionMeshToolStripMenuItem.Click += new System.EventHandler(this.addCollisionMeshToolStripMenuItem_Click);
            // 
            // objectViewer1
            // 
            this.objectViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.objectViewer1.EyeX = 0F;
            this.objectViewer1.EyeY = 0F;
            this.objectViewer1.EyeZ = 0F;
            this.objectViewer1.Location = new System.Drawing.Point(0, 0);
            this.objectViewer1.Name = "objectViewer1";
            this.objectViewer1.Size = new System.Drawing.Size(1283, 690);
            this.objectViewer1.TabIndex = 21;
            this.objectViewer1.Text = "objectViewer1";
            this.objectViewer1.Click += new System.EventHandler(this.objectViewer1_Click);
            this.objectViewer1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.objectViewer1_MouseDoubleClick);
            this.objectViewer1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.objectViewer1_MouseDown_1);
            this.objectViewer1.MouseEnter += new System.EventHandler(this.objectViewer1_MouseEnter_1);
            this.objectViewer1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.objectViewer1_MouseMove_1);
            this.objectViewer1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.objectViewer1_MouseUp_1);
            // 
            // splitOffFacegroupToolStripMenuItem
            // 
            this.splitOffFacegroupToolStripMenuItem.Name = "splitOffFacegroupToolStripMenuItem";
            this.splitOffFacegroupToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.splitOffFacegroupToolStripMenuItem.Text = "Split off facegroup";
            this.splitOffFacegroupToolStripMenuItem.Click += new System.EventHandler(this.splitOffFacegroupToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1283, 690);
            this.Controls.Add(this.objectViewer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "IL2 Modder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem adjustMatrixToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dumpAsTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editMaterialToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showHooksToolStripMenuItem;
        private ObjectViewer objectViewer1;
        private System.Windows.Forms.ToolStripMenuItem replaceNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addMshNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rotate90ZToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractEulersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem flipNormalsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mapToVariableToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addAnimationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem focusToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addCollisionMeshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem splitOffFacegroupToolStripMenuItem;
    }
}

