﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Generator
{
    public class TextDetail
    {
        private readonly string _templateDir;
        private readonly CustomJPrototypeResolver _resolver;

        public string Content;
        public string Font;
        public int FontSize;
        public FontStyle FontStyle;
        public Color Color;
        public Color OutlineColor;
        public int OutlineThickness;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public VerticalAlignment VerticalTextAlignment;
        public HorizontalAlignment HorizontalTextAlignment;

        public TextDetail(string templateDir, JObject text, CustomJPrototypeResolver resolver)
        {
            _templateDir = templateDir;
            _resolver = resolver;
            Content = resolver.GetString(text, "Content");
            Font = resolver.GetString(text, "Font");
            FontSize = resolver.GetInt(text, "FontSize");
            FontStyle = resolver.GetFlagsEnumOrDefault(text, "FontStyle", FontStyle.Regular);
            Color = resolver.GetColorOrDefault(text, "Color", Color.Black);
            OutlineColor = resolver.GetColorOrDefault(text, "OutlineColor", Color.Transparent);
            OutlineThickness = resolver.GetIntOrDefault(text, "OutlineThickness", 0);
            X = resolver.GetInt(text, "X");
            Y = resolver.GetInt(text, "Y");
            Width = resolver.GetInt(text, "Width");
            Height = resolver.GetInt(text, "Height");
            VerticalTextAlignment = resolver.GetEnumOrDefault(text, "VerticalTextAlign", VerticalAlignment.Center);
            HorizontalTextAlignment = resolver.GetEnumOrDefault(text, "HorizontalTextAlign", HorizontalAlignment.Center);
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
                else if (x.StartsWith("[["))
                    textLines.Add(new Word(x.Substring(4, x.Length - 6), new Font(fontFamily, FontSize, (FontStyle)int.Parse(x.Substring(2, 2))), graphics, Color, spaceWidth, OutlineColor, OutlineThickness));
                else if (x.StartsWith("["))
                    textLines.Add(new Symbol(x, _templateDir, graphics, _resolver, font, fontFamily, FontStyle, Color));
                else 
                    textLines.Add(new Word(x, font, graphics, Color, spaceWidth, OutlineColor, OutlineThickness));
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
        private readonly Color _outlineColor;
        private readonly int _outlineThickness;
        public int Width { get; }

        public Word(string content, Font font, Graphics graphics, Color color, int spaceWidth, Color outlineColor, int outlineThickness)
        {
            _content = content;
            _font = font;
            _graphics = graphics;
            _color = color;
            _spaceWidth = spaceWidth;
            _outlineColor = outlineColor;
            _outlineThickness = outlineThickness;
            var measurement = graphics.MeasureString(_content, _font);
            Width = (int)measurement.Width - _spaceWidth;
        }

        public void Draw(int x, int y)
        {
            if (_outlineColor.A != Color.Transparent.A && _outlineThickness > 0)
            {
                _graphics.DrawString(_content,
                    _font,
                    new SolidBrush(_outlineColor),
                    new RectangleF(x - _spaceWidth / 2 - _outlineThickness, y, Width + 1 + _spaceWidth, _font.Height * 2),
                    new StringFormat
                    {
                        Trimming = StringTrimming.None,
                        LineAlignment = StringAlignment.Near
                    });
                
                _graphics.DrawString(_content,
                    _font,
                    new SolidBrush(_outlineColor),
                    new RectangleF(x - _spaceWidth / 2, y - _outlineThickness, Width + 1 + _spaceWidth, _font.Height * 2),
                    new StringFormat
                    {
                        Trimming = StringTrimming.None,
                        LineAlignment = StringAlignment.Near
                    });
                
                _graphics.DrawString(_content,
                    _font,
                    new SolidBrush(_outlineColor),
                    new RectangleF(x - _spaceWidth / 2 + _outlineThickness, y, Width + 1 + _spaceWidth, _font.Height * 2),
                    new StringFormat
                    {
                        Trimming = StringTrimming.None,
                        LineAlignment = StringAlignment.Near
                    });
                
                _graphics.DrawString(_content,
                    _font,
                    new SolidBrush(_outlineColor),
                    new RectangleF(x - _spaceWidth / 2, y + _outlineThickness, Width + 1 + _spaceWidth, _font.Height * 2),
                    new StringFormat
                    {
                        Trimming = StringTrimming.None,
                        LineAlignment = StringAlignment.Near
                    });
            }
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
        private readonly decimal _opacity;
        public int Width { get; }

        public Symbol(string symbol, string templateDir, Graphics graphics, CustomJPrototypeResolver resolver, Font font, FontFamily fontFamily, FontStyle fontStyle, Color opacity)
        {
            _image = Path.GetFullPath(Path.Combine(templateDir, resolver.GetRootValue(symbol.Substring(1, symbol.Length - 2))));
            _graphics = graphics;
            _opacity = (decimal)opacity.A / 255;
            var lineSpacing = font.Size * fontFamily.GetLineSpacing(fontStyle) / fontFamily.GetEmHeight(fontStyle);
            Width = (int)Math.Ceiling(lineSpacing);
        }

        public void Draw(int x, int y) => _graphics.DrawImage(Image.FromFile(_image).WithOpacity(_opacity), new Rectangle(x, y, Width, Width));
    }
}
