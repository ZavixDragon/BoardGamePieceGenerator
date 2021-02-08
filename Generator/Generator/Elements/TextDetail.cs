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
        public Color BackgroundColor;
        public Color BackgroundBorderColor;
        public int BackgroundBorderThickness;
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
            BackgroundColor = resolver.GetColorOrDefault(text, "BackgroundColor", Color.Transparent);
            BackgroundBorderColor = resolver.GetColorOrDefault(text, "BackgroundBorderColor", Color.Transparent);
            BackgroundBorderThickness = resolver.GetIntOrDefault(text, "BackgroundBorderThickness", 0);
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

            
            var stylingStack = new List<TextStyling>
            {
                new TextStyling(graphics, Font, FontSize, FontStyle, Color, BackgroundColor, BackgroundBorderColor, BackgroundBorderThickness, OutlineColor, OutlineThickness)
            };

            var currentStyle = stylingStack.Last();
            var textBlock = new TextBlock(Width, LineAlignment, WordAlignment, currentStyle.FontTool, currentStyle.BackgroundTool);
            var sequence = currentStyle.CreateInStyle(WordAlignment);
            var str = "";
            var str2 = "";
            var hasSequenceElement = false;

            for (var i = 0; i < Content.Length; i++)
            {
                if (Content[i] == '\\')
                {
                    i++;
                    str += Content[i];
                    hasSequenceElement = true;
                }
                else if (Content[i] == '\n')
                {
                    if (str != "")
                    {
                        sequence.Add(currentStyle.CreateInStyle(str));
                        str = "";
                    }
                    if (hasSequenceElement)
                    {
                        textBlock.Add(sequence, currentStyle.FontTool, currentStyle.BackgroundTool);
                        sequence = currentStyle.CreateInStyle(WordAlignment);
                        hasSequenceElement = false;
                    }
                    textBlock.NewLine(currentStyle.FontTool, currentStyle.BackgroundTool);
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

                    stylingStack.Add(new TextStyling(graphics, currentStyle, _resolver, (JObject)_resolver.Prototypes[styleName]));
                    currentStyle = stylingStack.Last();
                    if (!hasSequenceElement)
                        sequence = currentStyle.CreateInStyle(WordAlignment);
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
                    if (str2 != "")
                    {
                        sequence.Add(currentStyle.CreateInStyle(str2, str, _templateDir, graphics, _resolver, WordAlignment));
                        str2 = "";
                        str = "";
                    }
                    else if (str != "")
                    {
                        sequence.Add(currentStyle.CreateInStyle(str));
                        str = "";
                    }
                    if (hasSequenceElement)
                    {
                        textBlock.Add(sequence, currentStyle.FontTool, currentStyle.BackgroundTool);
                        sequence = currentStyle.CreateInStyle(WordAlignment);
                        hasSequenceElement = false;
                    }
                    textBlock.AddSpace(currentStyle.CreateInStyle(" "));
                }
                else if (Content[i] == '_')
                {
                    str += " ";
                    hasSequenceElement = true;
                }
                else if (Content[i] == '@' && str != "" && Content.Length > i + 1 && Content[i + 1] != ' ')
                {
                    str2 = str;
                    str = "";
                }
                else
                {
                    str += Content[i];
                    hasSequenceElement = true;
                }
            }

            if (str2 != "")
                sequence.Add(currentStyle.CreateInStyle(str2, str, _templateDir, graphics, _resolver, WordAlignment));
            else if (str != "")
                sequence.Add(currentStyle.CreateInStyle(str));
            if (hasSequenceElement)
                textBlock.Add(sequence, currentStyle.FontTool, currentStyle.BackgroundTool);

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
            var backgroundTool = new BackgroundTool(graphics, BackgroundColor, BackgroundBorderColor, BackgroundBorderThickness);
            var textBlock = new TextBlock(Width, LineAlignment, WordAlignment, fontTool, backgroundTool);
            textBlock.Add(new CharacterSequence(Content, Color, OutlineColor, OutlineThickness, fontTool), fontTool, backgroundTool);
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

        public TextBlock(int maxWidth, HorizontalAlignment lineAlignment, VerticalAlignment wordAlignment, FontTool fontTool, BackgroundTool backgroundTool)
        {
            _maxWidth = maxWidth;
            _lineAlignment = lineAlignment;
            _wordAlignment = wordAlignment;
            NewLine(fontTool, backgroundTool);
        }

        public void Add(IDrawableSegment segment, FontTool fontTool, BackgroundTool backgroundTool)
        {
            if (_textLines.Last().Width + segment.Width > _maxWidth)
            {
                _spaces.Clear();
                NewLine(fontTool, backgroundTool);
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

        public void NewLine(FontTool fontTool, BackgroundTool backgroundTool) => _textLines.Add(new TextSequence(_wordAlignment, backgroundTool, fontTool.LineHeight));

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
        private readonly BackgroundTool _backgroundTool;
        private readonly int _minHeight;
        public int Width => _segments.Sum(x => x.Width);
        public int Height => _segments.Select(x => x.Height).Concat(new List<int> { _minHeight }).Max();

        public TextSequence(VerticalAlignment wordAlignment, BackgroundTool backgroundTool, int minHeight = 0)
        {
            _wordAlignment = wordAlignment;
            _backgroundTool = backgroundTool;
            _minHeight = minHeight;
        }

        public void Add(IDrawableSegment segment) => _segments.Add(segment);

        public void Add(IEnumerable<IDrawableSegment> segments) => _segments.AddRange(segments);

        public void Draw(int x, int y)
        {
            var xOffset = 0;
            _backgroundTool.Draw(x, y, Width, Height);
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

        public void Draw(int x, int y)
        {
            _graphics.DrawImage(Image.FromFile(_image).WithOpacity(_opacity), new Rectangle(x + 4, y + 4, Width - 8, Height - 8));
        }
    }

    public class GlyphedCharacterSequence : IDrawableSegment
    {
        private readonly CharacterSequence _characterSequence;
        private readonly Glyph _glyph;
        private readonly VerticalAlignment _wordAlignment;
        public int Width => Math.Max(_characterSequence.Width, _glyph.Width);
        public int Height => Math.Max(_characterSequence.Height, _glyph.Height);

        public GlyphedCharacterSequence(CharacterSequence characterSequence, Glyph glyph, VerticalAlignment wordAlignment)
        {
            _characterSequence = characterSequence;
            _glyph = glyph;
            _wordAlignment = wordAlignment;
        }
        
        public void Draw(int x, int y)
        {
            if (_characterSequence.Width > _glyph.Width)
            {
                _glyph.Draw(x + (_characterSequence.Width - _glyph.Width) / 2, y);
                if (_wordAlignment == VerticalAlignment.Center)
                    y += + (Height - _characterSequence.Height) / 2;
                if (_wordAlignment == VerticalAlignment.Bottom)
                    y += Height - _characterSequence.Height;
                _characterSequence.Draw(x, y);
            }
            else
            {
                _glyph.Draw(x, y);
                if (_wordAlignment == VerticalAlignment.Center)
                    y += + (Height - _characterSequence.Height) / 2;
                if (_wordAlignment == VerticalAlignment.Bottom)
                    y += Height - _characterSequence.Height;
                _characterSequence.Draw(x + (_glyph.Width - _characterSequence.Width) / 2, y);
            }
        }
    }

    public class TextStyling
    {
        public string FontName { get; }
        public int FontSize { get; }
        public FontStyle FontStyle { get; }
        public Color Color { get; }
        public Color BackgroundColor { get; }
        public Color BackgroundBorderColor { get; }
        public int BackgroundBorderThickness { get; }
        public Color OutlineColor { get; }
        public int OutlineThickness { get; }
        public FontTool FontTool { get; }
        public BackgroundTool BackgroundTool { get; }

        public TextStyling(Graphics graphics, TextStyling previousStyle, CustomJPrototypeResolver resolver, JObject prototype) 
            : this(graphics, 
                resolver.GetStringOrDefault(prototype, "Font", previousStyle.FontName),
                resolver.GetIntOrDefault(prototype, "FontSize", previousStyle.FontSize),
                resolver.GetFlagsEnumOrDefault(prototype, "FontStyle", previousStyle.FontStyle),
                resolver.GetColorOrDefault(prototype, "Color", previousStyle.Color),
                resolver.GetColorOrDefault(prototype, "BackgroundColor", previousStyle.BackgroundColor),
                resolver.GetColorOrDefault(prototype, "BackgroundBorderColor", previousStyle.BackgroundBorderColor),
                resolver.GetIntOrDefault(prototype, "BackgroundBorderThickness", previousStyle.BackgroundBorderThickness),
                resolver.GetColorOrDefault(prototype, "OutlineColor", previousStyle.OutlineColor),
                resolver.GetIntOrDefault(prototype, "OutlineThickness", previousStyle.OutlineThickness)) {}

        public TextStyling(Graphics graphics, string fontName, int fontSize, FontStyle fontStyle, Color color, Color backgroundColor, Color backgroundBorderColor, int backgroundBorderThickness, Color outlineColor, int outlineThickness)
        {
            FontName = fontName;
            FontSize = fontSize;
            FontStyle = fontStyle;
            Color = color;
            BackgroundColor = backgroundColor;
            BackgroundBorderColor = backgroundBorderColor;
            BackgroundBorderThickness = backgroundBorderThickness;
            OutlineColor = outlineColor;
            OutlineThickness = outlineThickness;
            var fontFamily = new FontFamily(fontName);
            var font = new Font(fontFamily, FontSize, FontStyle);
            FontTool = new FontTool(graphics, font, fontStyle, fontFamily);
            BackgroundTool = new BackgroundTool(graphics, backgroundColor, backgroundBorderColor, backgroundBorderThickness);
        }

        public TextSequence CreateInStyle(VerticalAlignment wordAlignment) =>
            new TextSequence(wordAlignment, BackgroundTool, FontTool.LineHeight);

        public CharacterSequence CreateInStyle(string content) =>
            new CharacterSequence(content, Color, OutlineColor, OutlineThickness, FontTool);

        public Glyph CreateInStyle(string symbol, string templateDir, Graphics graphics, CustomJPrototypeResolver resolver) => 
            new Glyph(symbol, templateDir, graphics, resolver, Color, FontTool);
        
        public GlyphedCharacterSequence CreateInStyle(string content, string symbol, string templateDir, Graphics graphics, CustomJPrototypeResolver resolver, VerticalAlignment wordAlignment) =>
            new GlyphedCharacterSequence(CreateInStyle(content), CreateInStyle(symbol, templateDir, graphics, resolver), wordAlignment);
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

    public class BackgroundTool
    {
        private readonly Graphics _graphics;
        private readonly bool _hasBackground;
        private readonly SolidBrush _backgroundBrush;
        private readonly bool _hasBackgroundBorder;
        private readonly Pen _backgroundBorderPen;

        public BackgroundTool(Graphics graphics, Color backgroundColor, Color backgroundBorderColor, int backgroundBorderThickness)
        {
            _graphics = graphics;
            _hasBackground = backgroundColor.A != Color.Transparent.A;
            _backgroundBrush = new SolidBrush(backgroundColor);
            _hasBackgroundBorder = backgroundBorderColor.A != Color.Transparent.A && backgroundBorderThickness != 0;
            _backgroundBorderPen = new Pen(backgroundBorderColor, backgroundBorderThickness);
        }

        public void Draw(int x, int y, int width, int height)
        {
            if (_hasBackground)
                _graphics.FillRectangle(_backgroundBrush, x, y, width, height);
            if (_hasBackgroundBorder)
                _graphics.DrawRectangle(_backgroundBorderPen, x, y, width, height);
        }
    }
}
