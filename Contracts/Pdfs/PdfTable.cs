using System.Collections.Generic;
using System.Linq;
using BidiReshapeSharp;
using BidiReshapeSharp.Reshaper;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Dorbit.Framework.Contracts.Pdfs;

public class PdfTable
{
    public bool Rtl { get; set; }
    public bool ShowHeader { get; set; } = true;
    public int HeaderHeight { get; set; } = 50;
    public List<Column> Columns { get; set; } = [];
    public List<Row> Rows { get; set; } = [];

    public class Column(string header, int width)
    {
        public int Width { get; set; } = width;
        public string Header { get; set; } = header;
    }

    public class Row
    {
        public int Height { get; set; } = 30;
        public List<Cell> Cells { get; set; } = [];
    }

    public abstract class Cell
    {
        public XPen BorderColor { get; set; } = XPens.Black;
        public XBrush BackgroundColor { get; set; } = XBrushes.Transparent;
        public XBrush ForegroundColor { get; set; } = XBrushes.Black;

        public abstract void Draw(XGraphics gfx, XRect rect);
    }

    public class TextCell(object value) : Cell
    {
        public string Value { get; set; } = value.ToString();
        public XFont Font { get; set; } = new("Vazir", 12);
        public XStringAlignment HorizontalAlignment { get; set; } = XStringAlignment.Center;
        public XLineAlignment VerticalAlignment { get; set; } = XLineAlignment.Center;

        public override void Draw(XGraphics gfx, XRect rect)
        {
            var fixedText = BidiReshape.ProcessString(Value, new ReshaperConfig()
            {
                DeleteHarakat = true,
                DeleteTatweel = true,
                ShiftHarakatPosition = true,
                SupportLigatures = true,
                SupportZwj = true,
                UseUnshapedInsteadOfIsolated = true,
            });
            gfx.DrawString(fixedText, Font, ForegroundColor, rect, new XStringFormat()
            {
                Alignment = HorizontalAlignment,
                LineAlignment = VerticalAlignment
            });
        }
    }

    public class ImageCell(string filename, int width, int height) : Cell
    {
        public XStringAlignment HorizontalAlignment { get; set; } = XStringAlignment.Center;
        public XLineAlignment VerticalAlignment { get; set; } = XLineAlignment.Center;

        public override void Draw(XGraphics gfx, XRect rect)
        {
            var image = XImage.FromFile(filename);
            if (HorizontalAlignment == XStringAlignment.Near)
            {
                if (VerticalAlignment == XLineAlignment.Near)
                {
                    gfx.DrawImage(image, rect.X, rect.Y, width, height);
                }
                else if (VerticalAlignment == XLineAlignment.Center)
                {
                    gfx.DrawImage(image, rect.X, rect.Y + rect.Height - (height / 2.0), width, height);
                }
                else if (VerticalAlignment == XLineAlignment.Far)
                {
                    gfx.DrawImage(image, rect.X, rect.Y + rect.Height - height, width, height);
                }
            }
            else if (HorizontalAlignment == XStringAlignment.Center)
            {
                if (VerticalAlignment == XLineAlignment.Near)
                {
                    gfx.DrawImage(image, rect.X + (rect.Width / 2.0) - (width / 2.0), rect.Y, width, height);
                }
                else if (VerticalAlignment == XLineAlignment.Center)
                {
                    gfx.DrawImage(image, rect.X + (rect.Width / 2.0) - (width / 2.0), rect.Y + (rect.Height / 2.0) - (height / 2.0), width, height);
                }
                else if (VerticalAlignment == XLineAlignment.Far)
                {
                    gfx.DrawImage(image, rect.X + (rect.Width / 2.0) - (width / 2.0), rect.Y + rect.Height - height, width, height);
                }
            }
            else if (HorizontalAlignment == XStringAlignment.Far)
            {
                if (VerticalAlignment == XLineAlignment.Near)
                {
                    gfx.DrawImage(image, rect.X + rect.Width - width, rect.Y, width, height);
                }
                else if (VerticalAlignment == XLineAlignment.Center)
                {
                    gfx.DrawImage(image, rect.X + rect.Width - width, rect.Y + rect.Height - (height / 2.0), width, height);
                }
                else if (VerticalAlignment == XLineAlignment.Far)
                {
                    gfx.DrawImage(image, rect.X + rect.Width - width, rect.Y + rect.Height - height, width, height);
                }
            }
        }
    }

    public int Draw(PdfPage page, XGraphics gfx, int startX, int startY)
    {
        var widthSum = Columns.Sum(x => x.Width);
        var autoWidthCount = Columns.Count(x => x.Width == 0);
        if (autoWidthCount > 0)
        {
            var autoWidth = (int)(page.Width.Value - (startX * 2) - widthSum) / autoWidthCount;
            foreach (var column in Columns.Where(x => x.Width == 0))
            {
                column.Width = autoWidth;
            }
        }

        var rows = Rows.ToList();
        if (ShowHeader)
        {
            var header = new Row();
            foreach (var column in Columns)
            {
                var cell = new TextCell(column.Header);
                header.Cells.Add(cell);
            }

            rows.Insert(0, header);
        }

        var posY = startY;
        foreach (var row in rows)
        {
            var posX = (int)(Rtl ? page.Width.Value - startX : startX);
            for (var colIndex = 0; colIndex < Columns.Count; colIndex++)
            {
                var column = Columns[colIndex];
                var cell = row.Cells[colIndex];
                gfx.DrawRectangle(cell.BorderColor, cell.BackgroundColor, posX - (Rtl ? column.Width : 0), posY, column.Width, row.Height);
                var rect = new XRect(posX - (Rtl ? column.Width : 0) + 5, posY, column.Width - 10, row.Height);
                cell.Draw(gfx, rect);
                posX += column.Width * (Rtl ? -1 : 1);
            }

            posY += row.Height;
        }

        return posY;
    }
}