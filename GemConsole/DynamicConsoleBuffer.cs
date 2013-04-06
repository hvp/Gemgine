﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Console
{
    public class DynamicConsoleBuffer
    {
        private String output;

        public class Input
        {
            public String input { get; set; }
            public int cursor { get; set; }
            public int scroll { get; set; }

            public Input()
            {
                input = "";
                cursor = 0;
                scroll = 0;
            }
        }

        public Input activeInput { get; set; }
        public Input[] inputs;
        public int outputScrollPoint { get; set; } //Expressed in lines-from-end
        private int scrollbackSize;
        public int maxInputLines { get; set; }
        TextDisplay display;
        
        public DynamicConsoleBuffer(int scrollbackSize, TextDisplay display)
        {
            this.scrollbackSize = scrollbackSize;
            Reset(display);
        }

        public void Reset(TextDisplay display)
        {
            output = "";
            outputScrollPoint = 0;
            this.display = display;
            inputs = new Input[2] { new Input(), new Input() };
            activeInput = inputs[0];
            maxInputLines = 8;
        }

        public void Clear()
        {
            output = "";
            outputScrollPoint = 0;
        }

        public void rawWrite(int spot, String str, Color fg, Color bg)
        {
            for (int i = 0; i < str.Length; ++i)
                display.SetChar(spot + i, str[i], fg, bg);
        }

        public void PopulateDisplay()
        {
            var totalInputLines = (int)Math.Ceiling((float)(activeInput.input.Length + 1) / (float)display.width);
            var inputLines = Math.Min(maxInputLines, totalInputLines);
            var visibleOutputLines = display.height - inputLines - 1;
            if (inputLines < totalInputLines)
            {
                //Some input is hidden. Center the scroll over it.
                var cursorLine = (activeInput.cursor + 1) / display.width;
                while (cursorLine < activeInput.scroll) --activeInput.scroll;
                while (cursorLine >= (activeInput.scroll + maxInputLines)) ++activeInput.scroll;
            }

            var totalOutputLines = (int)Math.Ceiling((float)output.Length / (float)display.width);
            var outputStart = totalOutputLines - visibleOutputLines - outputScrollPoint;
            if (totalOutputLines <= visibleOutputLines)
            {
                outputScrollPoint = outputStart = 0;
            }
            else if (outputStart < 0)
            {
                outputScrollPoint += outputStart;
                if (outputScrollPoint < 0) outputScrollPoint = 0;
                outputStart = 0;                
            }

            outputStart *= display.width;

            var i = 0;
            for (; i < display.width * visibleOutputLines  && (i + outputStart) < output.Length; ++i)
                display.SetChar(i, output[i + outputStart], Color.White, Color.Black);
            for (; i < display.width * visibleOutputLines; ++i)
                display.SetChar(i, ' ', Color.White, Color.Black);

            var infoLine = i;
            for (; i < display.width * (visibleOutputLines + 1); ++i)
                display.SetChar(i, ' ', Color.Orange, Color.Black);
            rawWrite(infoLine, String.Format("OH:{0,2}", outputScrollPoint), Color.Orange, Color.Black);
            var iLine = String.Format("^I:{0,2}, VI:{1,2}", activeInput.scroll, totalInputLines - maxInputLines - activeInput.scroll);
            rawWrite(infoLine + display.width - iLine.Length, iLine, Color.Orange, Color.Black);

            var inputStart = i;
            var inputc = activeInput.scroll * display.width;
            for (; inputc < activeInput.input.Length && i < display.height * display.width; ++i, ++inputc)
                display.SetChar(i, activeInput.input[inputc], Color.Green, Color.DarkGray);
            for (; i < display.height * display.width; ++i)
                display.SetChar(i, ' ', Color.Green, Color.DarkGray);

            if (activeInput.cursor < activeInput.input.Length && activeInput.cursor >= activeInput.scroll * display.width)
                display.SetChar(activeInput.cursor - (activeInput.scroll * display.width) + inputStart, activeInput.input[activeInput.cursor],
                    Color.Red, Color.Black);
            else display.SetChar(activeInput.cursor - (activeInput.scroll * display.width) + inputStart, ' ', Color.Red, Color.Black);
        }

        public void Write(String str)
        {
            foreach (var c in str)
            {
                if (c == '\n') output += new String(' ', display.width - (output.Length % display.width));
                else output += c;
            }

            if (output.Length > scrollbackSize * display.width)
                output = output.Substring(output.Length - (scrollbackSize * display.width));
        }
    }
}

