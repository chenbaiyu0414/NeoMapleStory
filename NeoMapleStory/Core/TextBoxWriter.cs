using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace NeoMapleStory.Core
{
    class TextBoxWriter : TextWriter
    {
        readonly TextBox _mTextBox;

        public TextBoxWriter(TextBox textbox)
        {
            _mTextBox = textbox;
        }

        public override void Write(string value)
        {
            _mTextBox.BeginInvoke(new Action(() =>
            {
                _mTextBox.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] {value}");
            }));
        }

        public override void WriteLine(string value)
        {
            _mTextBox.BeginInvoke(new Action(() =>
            {
                _mTextBox.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] {value}{Environment.NewLine}");
            }));
        }

        public override Encoding Encoding => Encoding.Default;
    }
}
