using UnityEngine;

namespace KS.UnityDarken
{
	public static class Utils
	{
		public static Color InvertColor( Color color )
		//tried also hsv inversion but this looks better
		{
			return new Color( 1 - color.r, 1 - color.g, 1 - color.b, color.a );
		}

		public static Color InvertColorWithAddition( Color color, ColorWithRange[] colorsPalette )
		//tried also hsv inversion but this looks better
		{
			var r = color.r;
			var g = color.g;
			var b = color.b;
			var brightness = 0.2126f*r + 0.7152f*g + 0.0722f*b;

			Color additive = Color.black;

			foreach ( var col in colorsPalette )
			{
				if ( brightness > col.BrightnessRangeStart
					&& brightness < col.BrightnessRangeEnd )
				{
					var normalized = (brightness - col.BrightnessRangeStart)
						/ (col.BrightnessRangeEnd - col.BrightnessRangeStart);

					additive += col.ColorToAdd.Evaluate( normalized ) * col.Multiplier;
				}
			}

			return new Color(
				Mathf.Clamp01( 1 - r + additive.r ),
				Mathf.Clamp01( 1 - g + additive.g ),
				Mathf.Clamp01( 1 - b + additive.b ), color.a );
		}

		//kinda impossible to reverse it, have to store additive color somewhere
		public static Color ReverseInvertColorWithAddition( Color color, ColorWithRange[] colorsPalette )
		//tried also hsv inversion but this looks better
		{
			//var color = new Color( 1 - invertedColor.r, 1 - invertedColor.g, 1 - invertedColor.b, invertedColor.a );
			var r = color.r;
			var g = color.g;
			var b = color.b;
			//var brightness = 1 - (0.2126f*r + 0.7152f*g + 0.0722f*b);

			//Color additive = Color.black;

			//foreach ( var col in colorsPalette )
			//{
			//	if ( brightness > col.BrightnessRangeStart
			//		&& brightness < col.BrightnessRangeEnd )
			//	{
			//		additive += col.ColorToAdd.Evaluate( 0.5f );
			//	}
			//}
			//additive.a = 0;
			//color -= additive;
			return new Color( 1 - r, 1 - g, 1 - b, color.a );
		}
	}
}