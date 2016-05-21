using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace NeoMapleStory.Core
{
    internal class TextBoxWriter : TextWriter
    {
        private readonly TextBox m_mTextBox;

        public TextBoxWriter(TextBox textbox)
        {
            m_mTextBox = textbox;
        }

        public override Encoding Encoding => Encoding.Default;

        public override void Write(string value)
        {
            m_mTextBox.BeginInvoke(
                new Action(() => { m_mTextBox.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] {value}"); }));
        }

        public override void WriteLine(string value)
        {
            m_mTextBox.BeginInvoke(
                new Action(
                    () =>
                    {
                        m_mTextBox.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] {value}{Environment.NewLine}");
                    }));
        }
    }
}