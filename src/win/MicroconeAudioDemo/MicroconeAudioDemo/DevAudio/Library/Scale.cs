namespace DevAudio.Library
{
	public static class Scale
	{
		public static float SliderToGain(double slider)
		{
			// the slider control range is 0 - 10, on the mac it is 0.0 - 2.0
			return (float)LogToLinear(slider * .2);
		}
		public static float GainToSlider(float gain)
		{
			// the slider control range is 0 - 10, on the mac it is 0.0 - 2.0
			return (float)(LinearToLog(System.Math.Max(gain, .5)) / .2);
		}
		public static double LinearToLog(double value)
		{
			return (System.Math.Log10(value) / .3) + 1;
		}
		public static double LogToLinear(double value)
		{
			return System.Math.Pow(10, (value - 1) * .3);
		}
	}
}
