using System;
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
        public HorizontalAlignment HorizontalAlignment;
        public HorizontalAlignment LineAlignment;
        public VerticalAlignment VerticalAlignment;
        public VerticalAlignment WordAlignment;
        //It's flawed beacued it rotates it by this many degrees and then uses the x and y transform
        public int FlawedRotation;
        //It's flawed beacue it doesn't work with any special markdown or glyphs at all except newlines
        public bool FlawedDynamicFontSizeEnabled;

        public TextDetail(string templateDir, JObject text, CustomJPrototypeResolver resolver)
        {
            _templateDir = templateDir;
            _resolver = resolver;
            Content = resolver.GetString(text, "Content");
            Font = resolver.GetString(text, "Font");
            FontSize = resolver.GetIntOrDefault(text, "FontSize", 0);
            FontStyle = resolver.GetFlagsEnumOrDefault(text, "FontStyle", FontStyle.Regular);
            Color = resolver.GetColorOrDefault(text, "Color", Color.Black);
            OutlineColor = resolver.GetColorOrDefault(text, "OutlineColor", Color.Transparent);
            OutlineThickness = resolver.GetIntOrDefault(text, "OutlineThickness", 0);
            X = resolver.GetInt(text, "X");
            Y = resolver.GetInt(text, "Y");
            Width = resolver.GetInt(text, "Width");
            Height = resolver.GetInt(text, "Height");
            HorizontalAlignment = resolver.GetEnumOrDefault(text, "HorizontalAlignment", HorizontalAlignment.Center);
            LineAlignment = resolver.GetEnumOrDefault(text, "LineAlignment", HorizontalAlignment.Center);
            VerticalAlignment = resolver.GetEnumOrDefault(text, "VerticalAlignment", VerticalAlignment.Center);
            WordAlignment = resolver.GetEnumOrDefault(text, "WordAlignment", VerticalAlignment.Bottom);
            FlawedRotation = resolver.GetIntOrDefault(text, "FlawedRotation", 0);
            FlawedDynamicFontSizeEnabled = resolver.GetBoolOrDefault(text, "FlawedDynamicSizeEnabled", false);
        }

        public void Apply(Graphics graphics)
        {
            if (FlawedDynamicFontSizeEnabled)
            {
                DynamicFontSizeApply(graphics);
                return;
            }

            var fontFamily = new FontFamily(Font);
            var font = new Font(fontFamily, FontSize, FontStyle);
            var stylingStack = new List<TextStyling>
            {
                new TextStyling(new FontTool(graphics, font, FontStyle, fontFamily), Color, OutlineColor, OutlineThickness)
            };

            var currentStyle = stylingStack.Last();
            var textBlock = new TextBlock(Width, LineAlignment, WordAlignment, currentStyle.FontTool);
            var sequence = new TextSequence(WordAlignment);
            var str = "";
            var hasSequenceElement = false;

            for (var i = 0; i < Content.Length; i++)
            {
                if (Content[i] == '\n')
                {
                    if (str != "")
                    {
                        sequence.Add(currentStyle.CreateInStyle(str));
                        str = "";
                    }
                    if (hasSequenceElement)
                    {
                        textBlock.Add(sequence, currentStyle.FontTool);
                        sequence = new TextSequence(WordAlignment);
                        hasSequenceElement = false;
                    }
                    textBlock.NewLine(currentStyle.FontTool);
                }
                else if (Content[i] == '[' && Content[i + 1] == '[')
                {
                    if (str != "")
                    {
                        sequence.Add(currentStyle.CreateInStyle(str));
                        str = "";
                    }
                    var styleName = "";
                    i += 2;
                    for (var _ = 0; Content[i] != ' '; i++)
                        styleName += Content[i];

                    var prototype = (JObject) _resolver.Prototypes[styleName];
                    var newFontName = _resolver.GetStringOrDefault(prototype, "Font", Font);
                    var newFontSize = _resolver.GetIntOrDefault(prototype, "FontSize", FontSize);
                    var newFontStyle = _resolver.GetFlagsEnumOrDefault(prototype, "FontStyle", FontStyle);
                    var newColor = _resolver.GetColorOrDefault(prototype, "Color", Color);
                    var newOutlineColor = _resolver.GetColorOrDefault(prototype, "OutlineColor", OutlineColor);
                    var newOutlineThickness = _resolver.GetIntOrDefault(prototype, "OutlineThickness", OutlineThickness);

                    var newFamily = new FontFamily(newFontName);
                    var newFont = new Font(newFamily, newFontSize, newFontStyle);
                    stylingStack.Add(new TextStyling(new FontTool(graphics, newFont, newFontStyle, newFamily), newColor, newOutlineColor, newOutlineThickness));
                    currentStyle = stylingStack.Last();
                }
                else if (Content[i] == ']' && Content[i + 1] == ']')
                {
                    if (str != "")
                    {
                        sequence.Add(currentStyle.CreateInStyle(str));
                        str = "";
                    }
                    i++;
                    stylingStack.RemoveAt(stylingStack.Count - 1);
                    currentStyle = stylingStack.Last();
                }
                else if (Content[i] == '[')
                {
                    if (str != "")
                    {
                        sequence.Add(currentStyle.CreateInStyle(str));
                        str = "";
                    }
                    var glyph = "";
                    i++;
                    for (var _ = 0; Content[i] != ']'; i++)
                        glyph += Content[i];
                    sequence.Add(currentStyle.CreateInStyle(glyph, _templateDir, graphics, _resolver));
                    hasSequenceElement = true;
                }
                else if (Content[i] == ' ')
                {
                    if (str != "")
                    {
                        sequence.Add(currentStyle.CreateInStyle(str));
                        str = "";
                    }
                    if (hasSequenceElement)
                    {
                        textBlock.Add(sequence, currentStyle.FontTool);
                        sequence = new TextSequence(WordAlignment);
                        hasSequenceElement = false;
                    }
                    textBlock.AddSpace(currentStyle.CreateInStyle(" "));
                }
                else
                {
                    str += Content[i];
                    hasSequenceElement = true;
                }
            }

            if (str != "")
                sequence.Add(currentStyle.CreateInStyle(str));
            if (hasSequenceElement)
                textBlock.Add(sequence, currentStyle.FontTool);

            Finalize(graphics, textBlock);   
        }

        private void DynamicFontSizeApply(Graphics graphics)
        {
            var minFontSize = 0;
            var maxFontSize = Height;
            int fontSize = Height / 2;
            while (minFontSize != maxFontSize)
            {
                var fontFamilyAttempt = new FontFamily(Font);
                var fontAttempt = new Font(fontFamilyAttempt, fontSize, FontStyle);
                var fontToolAttempt = new FontTool(graphics, fontAttempt, FontStyle, fontFamilyAttempt);
                var measurement = fontToolAttempt.Measure(Content);
                if (measurement.Width > Width || measurement.Height > Height)
                    maxFontSize = fontSize - 1;
                else
                    minFontSize = fontSize;
                fontSize = (int) (Math.Ceiling((decimal)(maxFontSize - minFontSize) / 2) + minFontSize);
            }
            var fontFamily = new FontFamily(Font);
            var font = new Font(fontFamily, fontSize, FontStyle);
            var fontTool = new FontTool(graphics, font, FontStyle, fontFamily);
            var textBlock = new TextBlock(Width, LineAlignment, WordAlignment, fontTool);
            textBlock.Add(new CharacterSequence(Content, Color, OutlineColor, OutlineThickness, fontTool), fontTool);
            Finalize(graphics, textBlock);
        }

        private void Finalize(Graphics graphics, TextBlock textBlock)
        {
            var x = 0;
            if (HorizontalAlignment == HorizontalAlignment.Left)
                x = X;
            if (HorizontalAlignment == HorizontalAlignment.Center)
                x = X + (Width - textBlock.Width) / 2;
            if (HorizontalAlignment == HorizontalAlignment.Right)
                x = X + Width - textBlock.Width;
            var y = 0;
            if (VerticalAlignment == VerticalAlignment.Top)
                y = Y;
            if (VerticalAlignment == VerticalAlignment.Center)
                y = Y + (Height - textBlock.Height) / 2;
            if (VerticalAlignment == VerticalAlignment.Bottom)
                y = Y + Height - textBlock.Height;
            if (FlawedRotation > 0)
                graphics.RotateTransform(FlawedRotation);
            textBlock.Draw(x, y);
            if (FlawedRotation > 0)
                graphics.RotateTransform(-FlawedRotation);
        }
    }

    public class TextBlock
    {
        private readonly List<TextSequence> _textLines = new List<TextSequence>();
        private readonly List<IDrawableSegment> _spaces = new List<IDrawableSegment>();
        private readonly int _maxWidth;
        private readonly HorizontalAlignment _lineAlignment;
        private readonly VerticalAlignment _wordAlignment;

        public int Width => _textLines.Count == 0 ? 0 : _textLines.Max(x => x.Width);
        public int Height => _textLines.Sum(x => x.Height);

        public TextBlock(int maxWidth, HorizontalAlignment lineAlignment, VerticalAlignment wordAlignment, FontTool tool)
        {
            _maxWidth = maxWidth;
            _lineAlignment = lineAlignment;
            _wordAlignment = wordAlignment;
            NewLine(tool);
        }

        public void Add(IDrawableSegment segment, FontTool tool)
        {
            if (_textLines.Last().Width + segment.Width > _maxWidth)
            {
                _spaces.Clear();
                NewLine(tool);
                _textLines.Last().Add(segment);
            }
            else
            {
                _textLines.Last().Add(_spaces);
                _spaces.Clear();
                _textLines.Last().Add(segment);
            }
        }

        public void AddSpace(IDrawableSegment space) => _spaces.Add(space);

        public void NewLine(FontTool tool) => _textLines.Add(new TextSequence(_wordAlignment, tool.LineHeight));

        public void Draw(int x, int y)
        {
            var yOffset = 0;
            _textLines.ForEach(line =>
            {
                if (_lineAlignment == HorizontalAlignment.Left)
                    line.Draw(x, y + yOffset);
                if (_lineAlignment == HorizontalAlignment.Center)
                    line.Draw(x + (Width - line.Width) / 2, y + yOffset);
                if (_lineAlignment == HorizontalAlignment.Right)
                    line.Draw(x + Width - line.Width, y + yOffset);
                yOffset += line.Height;
            });
        }
    }

    public interface IDrawableSegment
    {
        int Width { get; }
        int Height { get; }
        void Draw(int x, int y);
    }

    public class TextSequence : IDrawableSegment
    {
        private readonly List<IDrawableSegment> _segments = new List<IDrawableSegment>();
        private readonly VerticalAlignment _wordAlignment;
        private readonly int _minHeight;
        public int Width => _segments.Sum(x => x.Width);
        public int Height => _segments.Select(x => x.Height).Concat(new List<int> { _minHeight }).Max();

        public TextSequence(VerticalAlignment wordAlignment, int minHeight = 0)
        {
            _wordAlignment = wordAlignment;
            _minHeight = minHeight;
        }

        public void Add(IDrawableSegment segment) => _segments.Add(segment);

        public void Add(IEnumerable<IDrawableSegment> segments) => _segments.AddRange(segments);

        public void Draw(int x, int y)
        {
            var xOffset = 0;
            _segments.ForEach(seg =>
            {
                if (_wordAlignment == VerticalAlignment.Top)
                    seg.Draw(x + xOffset, y);
                if (_wordAlignment == VerticalAlignment.Center)
                    seg.Draw(x + xOffset, y + (Height - seg.Height) / 2);
                if (_wordAlignment == VerticalAlignment.Bottom)
                    seg.Draw(x + xOffset, y + Height - seg.Height);
                xOffset += seg.Width;
            });
        }
    }

    public class CharacterSequence : IDrawableSegment
    {
        private readonly string _content;
        private readonly FontTool _fontTool;
        private readonly SolidBrush _primaryBrush;
        private readonly bool _hasOutline;
        private readonly SolidBrush _outlineBrush;
        private readonly int _outlineThickness;
        public int Width { get; }
        public int Height { get; }

        public CharacterSequence(string content, Color color, Color outlineColor, int outlineThickness, FontTool fontTool)
        {
            _content = content;
            _fontTool = fontTool;
            _primaryBrush = new SolidBrush(color);
            _hasOutline = outlineColor.A != Color.Transparent.A && _outlineThickness > 0;
            _outlineBrush = new SolidBrush(outlineColor);
            _outlineThickness = outlineThickness;
            var size = _fontTool.Measure(_content);
            Width = size.Width;
            Height = _fontTool.LineHeight;
        }

        public void Draw(int x, int y)
        {
            if (_hasOutline)
                DrawOutline(x, y);
            _fontTool.Draw(_content, _primaryBrush, x, y);
        }

        private void DrawOutline(int x, int y)
        {
            _fontTool.Draw(_content, _outlineBrush, x - _outlineThickness, y);
            _fontTool.Draw(_content, _outlineBrush, x + _outlineThickness, y);
            _fontTool.Draw(_content, _outlineBrush, x, y - _outlineThickness);
            _fontTool.Draw(_content, _outlineBrush, x, y + _outlineThickness);
        }
    }

    public class Glyph : IDrawableSegment
    {
        private readonly string _image;
        private readonly Graphics _graphics;
        private readonly decimal _opacity;
        public int Width { get; }
        public int Height { get; }

        public Glyph(string symbol, string templateDir, Graphics graphics, CustomJPrototypeResolver resolver, Color opacity, FontTool font)
        {
            _image = Path.GetFullPath(Path.Combine(templateDir, resolver.GetRootValue(symbol)));
            _graphics = graphics;
            _opacity = (decimal)opacity.A / 255;
            Width = font.GlyphHeight;
            Height = font.GlyphHeight;
        }

        public void Draw(int x, int y) => _graphics.DrawImage(Image.FromFile(_image).WithOpacity(_opacity), new Rectangle(x, y, Width, Height));
    }

    public class TextStyling
    {
        private readonly Color _color;
        private readonly Color _outlineColor;
        private readonly int _outlineThickness;
        public FontTool FontTool { get; }

        public TextStyling(FontTool fontTool, Color color, Color outlineColor, int outlineThickness)
        {
            _color = color;
            _outlineColor = outlineColor;
            _outlineThickness = outlineThickness;
            FontTool = fontTool;
        }

        public CharacterSequence CreateInStyle(string content) =>
            new CharacterSequence(content, _color, _outlineColor, _outlineThickness, FontTool);

        public Glyph CreateInStyle(string symbol, string templateDir, Graphics graphics, CustomJPrototypeResolver resolver) => 
            new Glyph(symbol, templateDir, graphics, resolver, _color, FontTool);
    }

    public class FontTool
    {
        private readonly StringFormat _format = new StringFormat { Trimming = StringTrimming.None, LineAlignment = StringAlignment.Near };
        private readonly Graphics _graphics;
        private readonly Font _font;
        private readonly float _spaceWidth;
        private readonly float _letterAndSpaceWidth;
        public int LineHeight { get; }
        public int GlyphHeight { get; }

        //MeasureString always gives you the full line height as the height
        public FontTool(Graphics graphics, Font font, FontStyle style, FontFamily family)
        {
            _graphics = graphics;
            _font = font;
            var spaceSize = graphics.MeasureString(" ", font);
            _spaceWidth = spaceSize.Width;
            _letterAndSpaceWidth = graphics.MeasureString("Z ", font).Width;
            LineHeight = (int)Math.Ceiling(spaceSize.Height);
            GlyphHeight = LineHeight;
        }

        //MeasureString ensures there is exactly one trailing space
        public Size Measure(string text)
        {
            var measurement = _graphics.MeasureString(text + "Z ", _font);
            return new Size((int) Math.Ceiling(measurement.Width - _letterAndSpaceWidth), (int) Math.Ceiling(measurement.Height)); 
        }

        //DrawString also draws with a trailing space
        public void Draw(string text, SolidBrush brush, int x, int y) => 
            _graphics.DrawString(text, _font, brush, new RectangleF(x - _spaceWidth / 2, y, Measure(text).Width + _spaceWidth, _font.Height * 2f), _format);
    }
}
