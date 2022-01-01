using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.CodeDom.Compiler;
using System.Reflection;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace IL2Modder
{
    public partial class ScriptGenerator : Form
    {
        public Script script;
        public CockpitScript cscript;
        public bool cockpit = false;
        ICSharpCode.AvalonEdit.TextEditor editor;

        public ScriptGenerator()
        {
            InitializeComponent();

           editor = new ICSharpCode.AvalonEdit.TextEditor();
           TypeConverter typeConverter = new HighlightingDefinitionTypeConverter();
           editor.SyntaxHighlighting = (IHighlightingDefinition)typeConverter.ConvertFrom("C#");
           editor.FontSize = 14;
           elementHost1.Child = editor;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        public void SetText(String text)
        {
            editor.Text = text;
        }
        public void SetResults(CompilerResults cr)
        {
            if (cr.Errors.HasErrors)
            {
                for (int i = 0; i < cr.Output.Count; i++)
                    listBox1.Items.Add(cr.Output[i]);
                for (int i = 0; i < cr.Errors.Count; i++)
                    listBox1.Items.Add(i.ToString() + ": " + cr.Errors[i].ToString());

            }
            else
            {
                listBox1.Items.Add("Compiled successfully");
            }
            if (cr.Errors.HasWarnings)
            {
                for (int i = 0; i < cr.Output.Count; i++)
                    listBox1.Items.Add(cr.Output[i]);
                for (int i = 0; i < cr.Errors.Count; i++)
                    listBox1.Items.Add(i.ToString() + ": " + cr.Errors[i].ToString());
            }
        }
        public void Compile()
        {
            if (cockpit)
            {
                cscript = new CockpitScript();
                CompilerResults res = cscript.Compile(editor.Text);
                SetResults(res);
            }
            else
            {
                script = new Script();
                CompilerResults res = script.Compile(editor.Text);
                SetResults(res);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            if (cockpit)
            {
                cscript = new CockpitScript();
                CompilerResults res = cscript.Compile(editor.Text);
                SetResults(res);
            }
            else
            {
                script = new Script();
                CompilerResults res = script.Compile(editor.Text);
                SetResults(res);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".cs";
            sfd.Filter = "C# files (*.cs)|*.cs";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (TextWriter writer = File.CreateText(sfd.FileName))
                {
                    writer.Write(editor.Text);
                    writer.Close();
                }
            }
        }
    }
}
