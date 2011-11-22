﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SoundInTheory.DynamicImage.Util;

namespace SoundInTheory.DynamicImage
{
	public class TextLayer : Layer
	{
		#region Properties

		/// <summary>
		/// Width of the text layer; if omitted, size will be calculated automatically 
		/// and all text will be rendered on a single line.
		/// </summary>
		[Browsable(true), DefaultValue(null), Category("Layout"), Description("Width of the text layer; if omitted, size will be calculated automatically and all text will be rendered on a single line")]
		public int? Width
		{
			get
			{
				object value = this.PropertyStore["Width"];
				if (value != null)
					return (int?) value;
				return null;
			}
			set
			{
				this.PropertyStore["Width"] = value;
			}
		}

		/// <summary>
		/// Height of the text layer; if both Width and Height are omitted,
		/// size will be calculated automatically and all text will be rendered
		/// on a single line; if just Height is omitted then text will be 
		/// wrapped based on the Width.
		/// </summary>
		[Browsable(true), DefaultValue(null), Category("Layout"), Description("Height of the text layer; if both Width and Height are omitted, size will be calculated automatically and all text will be rendered on a single line; if just Height is omitted then text will be wrapped based on the Width.")]
		public int? Height
		{
			get
			{
				object value = this.PropertyStore["Height"];
				if (value != null)
					return (int?) value;
				return null;
			}
			set
			{
				this.PropertyStore["Height"] = value;
			}
		}

		[Browsable(true), DefaultValue(false), Category("Layout")]
		public bool Multiline
		{
			get
			{
				object value = this.PropertyStore["Multiline"];
				if (value != null)
					return (bool) value;
				return false;
			}
			set
			{
				this.PropertyStore["Multiline"] = value;
			}
		}

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public Font Font
		{
			get { return (Font)(PropertyStore["Font"] ?? (PropertyStore["Font"] = new Font())); }
			set { PropertyStore["Font"] = value; }
		}

		[Browsable(true), DefaultValue(typeof(Colors), "Black")]
		public Color ForeColour
		{
			get
			{
				object value = this.PropertyStore["ForeColour"];
				if (value != null)
					return (Color) value;
				return Colors.Black;
			}
			set
			{
				this.PropertyStore["ForeColour"] = value;
			}
		}

		[Browsable(true), DefaultValue(null)]
		public Color? ClearTypeBackColour
		{
			get
			{
				object value = this.PropertyStore["ClearTypeBackColour"];
				if (value != null)
					return (Color?) value;
				return null;
			}
			set
			{
				this.PropertyStore["ClearTypeBackColour"] = value;
			}
		}

		[Browsable(true), DefaultValue(null)]
		public Color? StrokeColour
		{
			get
			{
				object value = this.PropertyStore["StrokeColour"];
				if (value != null)
					return (Color) value;
				return null;
			}
			set
			{
				this.PropertyStore["StrokeColour"] = value;
			}
		}

		[Browsable(true), DefaultValue(0)]
		public double StrokeWidth
		{
			get
			{
				object value = this.PropertyStore["StrokeWidth"];
				if (value != null)
					return (double)value;
				return 0;
			}
			set
			{
				this.PropertyStore["StrokeWidth"] = value;
			}
		}

		[Browsable(true), DefaultValue("")]
		public string Text
		{
			get
			{
				object value = this.PropertyStore["Text"];
				if (value != null)
					return (string) value;
				return string.Empty;
			}
			set
			{
				this.PropertyStore["Text"] = value;
			}
		}

		[Browsable(true), DefaultValue(TextAlignment.Left)]
		public TextAlignment HorizontalTextAlignment
		{
			get { return (TextAlignment) (PropertyStore["HorizontalTextAlignment"] ?? TextAlignment.Left); }
			set { PropertyStore["HorizontalTextAlignment"] = value; }
		}

		[Browsable(true), DefaultValue(VerticalAlignment.Top)]
		public VerticalAlignment VerticalTextAlignment
		{
			get { return (VerticalAlignment) (PropertyStore["VerticalTextAlignment"] ?? VerticalAlignment.Top); }
			set { PropertyStore["VerticalTextAlignment"] = value; }
		}

		public override bool HasFixedSize
		{
			get { return true; }
		}

		#endregion

		protected override void CreateImage()
		{
			// If width and height are not set, we need to measure the string.
			int calculatedWidth, calculatedHeight;
			Size measuredSize = MeasureString();
			if (this.Width == null || this.Height == null)
			{
				double width = this.Width ?? measuredSize.Width;
				double height = this.Height ?? measuredSize.Height;
				calculatedWidth = (int) width;
				calculatedHeight = (int)height;
			}
			else // otherwise just create the image at the desired size
			{
				calculatedWidth = Width.Value;
				calculatedHeight = Height.Value;
			}

			#region Draw text

			DrawingVisual dv = new DrawingVisual();
			DrawingContext dc = dv.RenderOpen();

			//RenderOptions.SetClearTypeHint(dv, ClearTypeHint.Auto);
			TextOptions.SetTextRenderingMode(dv, TextRenderingMode.Auto);
			//TextOptions.SetTextFormattingMode(dv, TextFormattingMode.Ideal)

			UseFormattedText(ft =>
			{
				Pen pen = null;
				if (StrokeWidth > 0 && StrokeColour != null)
					pen = new Pen(new SolidColorBrush(StrokeColour.Value), StrokeWidth);

				// Calculate position to draw text at, based on vertical text alignment.
				int x = CalculateHorizontalPosition((int) measuredSize.Width);
				int y = CalculateVerticalPosition((int) measuredSize.Height);

				dc.DrawGeometry(new SolidColorBrush(ForeColour), pen,
					ft.BuildGeometry(new Point(x, y)));
			});

			dc.Close();

			RenderTargetBitmap rtb = RenderTargetBitmapUtility.CreateRenderTargetBitmap(calculatedWidth, calculatedHeight);
			rtb.Render(dv);

			#endregion

			Bitmap = new FastBitmap(rtb);
		}

		private int CalculateHorizontalPosition(int measuredWidth)
		{
			switch (HorizontalTextAlignment)
			{
				case TextAlignment.Left:
				case TextAlignment.Justify :
					return 0;
				case TextAlignment.Right:
					if (Width != null)
						return Width.Value - measuredWidth;
					return 0;
				case TextAlignment.Center :
					if (Width != null)
						return (Width.Value - measuredWidth) / 2;
					return 0;
				default:
					throw new NotSupportedException();
			}
		}

		private int CalculateVerticalPosition(int measuredHeight)
		{
			switch (VerticalTextAlignment)
			{
				case VerticalAlignment.Top :
					return 0;
				case VerticalAlignment.Bottom :
					if (Height != null)
						return Height.Value - measuredHeight;
					return 0;
				case VerticalAlignment.Center :
					if (Height != null)
						return (Height.Value - measuredHeight) / 2;
					return 0;
				default :
					throw new NotSupportedException();
			}
		}

		private Size MeasureString()
		{
			Size size = System.Windows.Size.Empty;
			UseFormattedText(ft =>
			{
				size = new Size(ft.WidthIncludingTrailingWhitespace, ft.Height);
			});
			return size;
		}

		private void UseFormattedText(RenderCallback renderCallback)
		{
			Brush textBrush = new SolidColorBrush(ForeColour);
			FontDescription fontDescription = Font.GetFontDescription();
			FormattedText formattedText = new FormattedText(Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
			                                                fontDescription.Typeface, fontDescription.Size, textBrush);
			formattedText.SetTextDecorations(fontDescription.TextDecorations);
			if (Width != null)
				formattedText.MaxTextWidth = Width.Value;
			if (Height != null)
				formattedText.MaxTextHeight = Height.Value;
			if (!Multiline)
				formattedText.MaxLineCount = 1;
			formattedText.Trimming = TextTrimming.None;
			//formattedText.TextAlignment = HorizontalTextAlignment;

			renderCallback(formattedText);
		}

		public override string ToString()
		{
			return string.Format("Text Layer: {0}", this.Text);
		}

		private delegate void RenderCallback(FormattedText formattedText);
	}
}
