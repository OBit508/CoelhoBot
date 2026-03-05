using System;
using System.Collections.Generic;
using System.Text;

namespace CoelhoBot.Modules
{
    public class ConsoleWriter : TextWriter
    {
        private readonly Action<string> _writeAction;
        public ConsoleWriter(Action<string> writeAction)
        {
            _writeAction = writeAction;
        }
        public override Encoding Encoding => Encoding.UTF8;
        public override void Write(char value)
        {
            _writeAction(value.ToString());
        }
        public override void Write(string? value)
        {
            if (value != null)
            {
                _writeAction(value);
            }
        }
        public override void WriteLine(string? value)
        {
            if (value != null)
            {
                _writeAction(value + NewLine);
            }
            else
            {
                _writeAction(NewLine);
            }
        }
    }
}
