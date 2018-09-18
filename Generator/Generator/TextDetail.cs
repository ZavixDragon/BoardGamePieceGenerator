using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Generator
{
    public class TextDetail
    {
        private readonly CustomJInterpreter _interpreter;

        public string Content;
        public string Font;
        public int FontSize;
        public FontStyle FontStyle;
        public Color Color;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public VerticalAlignment VerticalTextAlignment;
        public HorizontalAlignment HorizontalTextAlignment;

        public TextDetail(JObject text, CustomJInterpreter interpreter)
        {
            _interpreter = interpreter;
            Content = interpreter.GetString(text, "Content");
            Font = interpreter.GetString(text, "Font");
            FontSize = interpreter.GetInt(text, "FontSize");
            FontStyle = interpreter.GetFlagsEnumOrDefault(text, "FontStyle", FontStyle.Regular);
            Color = interpreter.GetColorOrDefault(text, "Color", Color.Black);
            X = interpreter.GetInt(text, "X");
            Y = interpreter.GetInt(text, "Y");
            Width = interpreter.GetInt(text, "Width");
            Height = interpreter.GetInt(text, "Height");
            VerticalTextAlignment = interpreter.GetEnumOrDefault(text, "VerticalTextAlign", VerticalAlignment.Center);
            HorizontalTextAlignment = interpreter.GetEnumOrDefault(text, "HorizontalTextAlign", HorizontalAlignment.Center);
        }

        public void Apply(Graphics graphics)
        {
            var fontFamily = new FontFamily(Font); 
            var font = new Font(fontFamily, FontSize, FontStyle);
            var spaceWidth = (int)Math.Floor(graphics.MeasureString(" ", font).Width);
            var textLines = new TextLines(Width, font, spaceWidth, HorizontalTextAlignment);
            Content.Split(' ').ToList().ForEach(x =>
            {
                if (x == "\n")
                    textLines.NewLine();
                else if (x.StartsWith("{"))
                    textLines.Add(new Symbol(x, graphics, _interpreter, font, fontFamily, FontStyle));
                else 
                    textLines.Add(new Word(x, font, graphics, Color, spaceWidth));
            });
            if (VerticalTextAlignment == VerticalAlignment.Top)
                textLines.Draw(X, Y);
            else if (VerticalTextAlignment == VerticalAlignment.Center)
                textLines.Draw(X, Y + (Height - textLines.Height) / 2);
            else if (VerticalTextAlignment == VerticalAlignment.Bottom)
                textLines.Draw(X, Y + Height - textLines.Height);
        }
    }

    public class TextLines
    {
        private readonly List<TextLine> _lines = new List<TextLine>();
        private readonly int _maxWidth;
        private readonly int _lineHeight;
        private readonly int _spaceWidth;
        private readonly HorizontalAlignment _alignment;
        public int Height => _lines.Count * _lineHeight;

        public TextLines(int maxWidth, Font font, int spaceWidth, HorizontalAlignment alignment)
        {
            _maxWidth = maxWidth;
            _lineHeight = font.Height;
            _spaceWidth = spaceWidth;
            _alignment = alignment;
            NewLine();
        }

        public void Add(ITextSegment segment)
        {
            if (_lines.Last().Width + _spaceWidth + segment.Width > _maxWidth)
                NewLine();
            _lines.Last().Add(segment);
        }

        public void NewLine()
        {
            _lines.Add(new TextLine(_spaceWidth));
        }

        public void Draw(int x, int y)
        {
            for (var i = 0; i < _lines.Count; i++)
            {
                if (_alignment == HorizontalAlignment.Left)
                    _lines[i].Draw(x, y + _lineHeight * i);
                else if (_alignment == HorizontalAlignment.Center)
                    _lines[i].Draw(x + (_maxWidth - _lines[i].Width) / 2, y + _lineHeight * i);
                else if (_alignment == HorizontalAlignment.Right)
                    _lines[i].Draw(x + _maxWidth - _lines[i].Width, y + _lineHeight * i);
            }
        }
    }

    public class TextLine
    {
        private readonly List<ITextSegment> _segments = new List<ITextSegment>();
        private readonly int _spaceWidth;
        public int Width => _segments.Sum(x => x.Width) + ((_segments.Count - 1) * _spaceWidth);

        public TextLine(int spaceWidth)
        {
            _spaceWidth = spaceWidth;
        }

        public void Add(ITextSegment segment) => _segments.Add(segment);

        public void Draw(int x, int y)
        {
            var offset = 0;
            foreach (var segment in _segments)
            {         
                segment.Draw(x + offset, y);
                offset += segment.Width + _spaceWidth;
            }
        }
    }

    public interface ITextSegment
    {
        int Width { get; }
        void Draw(int x, int y);
    }

    public class Word : ITextSegment
    {
        private readonly string _content;
        private readonly Font _font;
        private readonly Graphics _graphics;
        private readonly Color _color;
        private readonly int _spaceWidth;
        public int Width { get; }

        public Word(string content, Font font, Graphics graphics, Color color, int spaceWidth)
        {
            _content = content;
            _font = font;
            _graphics = graphics;
            _color = color;
            _spaceWidth = spaceWidth;
            var measurement = graphics.MeasureString(_content, _font);
            Width = (int)measurement.Width - _spaceWidth;
        }

        public void Draw(int x, int y)
        {
            _graphics.DrawString(_content,
                _font,
                new SolidBrush(_color),
                new RectangleF(x - _spaceWidth / 2, y, Width + 1 + _spaceWidth, _font.Height * 2),
                new StringFormat
                {
                    Trimming = StringTrimming.None,
                    LineAlignment = StringAlignment.Near
                });
        }
    }

    public class Symbol : ITextSegment
    {
        private readonly string _image;
        private readonly Graphics _graphics;
        public int Width { get; }

        public Symbol(string symbol, Graphics graphics, CustomJInterpreter interpreter, Font font, FontFamily fontFamily, FontStyle fontStyle)
        {
            _image = interpreter.GetRootValue(symbol.Substring(1, symbol.Length - 2));
            _graphics = graphics;
            var lineSpacing = font.Size * fontFamily.GetLineSpacing(fontStyle) / fontFamily.GetEmHeight(fontStyle);
            Width = (int)Math.Ceiling(lineSpacing);
        }

        public void Draw(int x, int y) => _graphics.DrawImage(Image.FromFile(_image), new Rectangle(x, y, Width, Width));
    }
}
