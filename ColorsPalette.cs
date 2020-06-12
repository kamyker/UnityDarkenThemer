using UnityEngine;

namespace KS.UnityDarken
{
	[System.Serializable]
	public class ColorWithRange
	{
		[Header("From 0 to 1, 0 = black, 0.5 = gray, 1 = white etc")]
		public float BrightnessRangeStart;
		public float BrightnessRangeEnd;

		public Gradient ColorToAdd;

		[Header("Can be negative to subtract color")]
		public float Multiplier = 1;
	}
}