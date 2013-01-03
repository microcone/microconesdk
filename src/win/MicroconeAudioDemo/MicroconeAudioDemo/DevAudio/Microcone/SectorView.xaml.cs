using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DevAudio.Microcone
{
	public partial class SectorView : UserControl
	{
		public SectorView()
		{
			InitializeComponent();
			for (int i = 0; i < sectorCount; i++)
			{
				_sectorEnabled[i] = 1;
				_sectorLocation[i] = .5f;
				_sectorActive[i] = 0;
			}
			_typeFace = new Typeface(FontFamily, FontStyle, FontWeights.Bold, FontStretch);
		}
		Typeface _typeFace;
		const int sectorCount = 6;

		int[] _sectorEnabled = new int[sectorCount];
		float[] _sectorLocation = new float[sectorCount];
		int[] _sectorActive = new int[sectorCount];
		List<Ellipse> _balls = new List<Ellipse>(sectorCount);
		List<Path> _sectors = new List<Path>(sectorCount);

		public void SetEnabled(int[] sectorEnabled)
		{
			for (int i = 0; i < sectorCount; i++)
			{
				_sectorEnabled[i] = sectorEnabled[i];
				if (_sectors.Count > i)
					_sectors[i].Fill = (_sectorEnabled[i] == 1) ? _enabledSector : _disabledSector;
			}
		}
		public void SetLocation(int[] sectorActive, float[] sectorLocation)
		{
#if DrawConversationMarkers
			if (_segments != null) 
                return;
#endif
            for (int i = 0; i < sectorCount; i++)
			{
				_sectorActive[i] = sectorActive[i];
			}

            var centre = new Point { X = ActualWidth / 2, Y = ActualHeight / 2 };
            var theWidth = (float)(ActualHeight < ActualWidth ? ActualHeight : ActualWidth);
			for (int i = 0; i < sectorCount; i++)
			{
				_sectorLocation[i] = sectorLocation[i];
				if (_balls.Count > i)
				{
					var ball = _balls[i];
					ball.Visibility = ((_sectorEnabled[i] == 1) && (_sectorActive[i] == 1)) ?
							Visibility.Visible : Visibility.Hidden;
					if (ball.Visibility == Visibility.Visible)
					{
						var rotationAngle = (i * -60f) + 180f + 30f;
						var centreLocation = ComputeCartesianCoordinate(rotationAngle - (_sectorLocation[i] * 60f), 1.05 * (theWidth / 2.5));
						centreLocation.Offset(centre.X - ball.Width / 2, centre.Y - ball.Height / 2); // RD: Fixed offset bug here (was hardwired to 15)
						Canvas.SetLeft(ball, centreLocation.X);
						Canvas.SetTop(ball, centreLocation.Y);
					}
				}
			}
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			_balls.Clear();
			_sectors.Clear();
			Canvas.Children.Clear();

            var centre = new Point { X = ActualWidth / 2, Y = ActualHeight / 2 };
            var theWidth = (float)(ActualHeight < ActualWidth ? ActualHeight : ActualWidth);
			var arcRadius = theWidth / 2;

			// sectors
			for (int i = 0; i < sectorCount; i++)
			{
				var rotationAngle = (i * -60f) + 90f;
				_sectors.Add(DrawWedge(Canvas, centre, arcRadius, i, rotationAngle + 60f, 59f, (_sectorEnabled[i] == 1) ? _enabledSector : _disabledSector));
				DrawText(Canvas, centre, arcRadius, i, rotationAngle);
			}
			// Black wedges & blue wedge
			for (int i = 0; i < sectorCount; i++)
			{
				var rotationAngle = (i * -60f) + 90f;
				var centreLocation = ComputeCartesianCoordinate(rotationAngle - 90f, theWidth / 14.5);
				centreLocation.Offset(centre.X, centre.Y);
				DrawWedge(Canvas, centreLocation, theWidth / 4f, i, rotationAngle + 68f, 44f, (i == 0) ? _blueColour : _blackColour);
				if (i == 0)
					DrawWedge(Canvas, centreLocation, theWidth / 4.5f, i, rotationAngle + 68f, 44f, _blackColour);
			}
			// Gray wedges
			for (int i = 0; i < sectorCount; i++)
			{
				var rotationAngle = (i * -60f) + 90f;
				var centreLocation = ComputeCartesianCoordinate(rotationAngle - 90f, theWidth / 14.5);
				centreLocation.Offset(centre.X, centre.Y);
				DrawWedge(Canvas, centreLocation, theWidth / 6f, i, rotationAngle + 72f, 36f, _grayColour);
			}
			// inner wedges
			for (int i = 0; i < sectorCount; i++)
			{
				var rotationAngle = (i * -60f) + 90f;
				var centreLocation = ComputeCartesianCoordinate(rotationAngle - 90f, theWidth / 14.5);
				centreLocation.Offset(centre.X, centre.Y);
				DrawWedge(Canvas, centreLocation, theWidth / 7f, i, rotationAngle + 74f, 32f, _blackColour);
			}
			DrawCord(Canvas, centre, theWidth);
			// balls
			for (int i = 0; i < sectorCount; i++)
			{
				_balls.Add(DrawBall(Canvas, centre, i, theWidth));
			}
#if DrawConversationMarkers
            if (_segments != null)
				UpdateBalls();
#endif

			base.OnRenderSizeChanged(sizeInfo);
		}

		private Ellipse DrawBall(Canvas canvas, Point centre, int i, float theWidth)
		{
			var ballRadius = theWidth / (6 * 2.5);
			var rotationAngle = (i * -60f) + 180f + 30f;
			var centreLocation = ComputeCartesianCoordinate(rotationAngle - (_sectorLocation[i] * 60f), 1.05 * (theWidth / 2.5));
			centreLocation.Offset(centre.X - ballRadius, centre.Y - ballRadius);
			var circle = new Ellipse
			{
				Fill = TrackColours[i],
				Height = ballRadius * 2,
				Width = ballRadius * 2,
				Visibility = Visibility.Hidden,
			};
			Canvas.SetLeft(circle, centreLocation.X);
			Canvas.SetTop(circle, centreLocation.Y);
			canvas.Children.Add(circle);
			return circle;
		}
		Brush _enabledSector = new SolidColorBrush(Color.FromScRgb(.8f, .64f, .82f, .92f));
		Brush _disabledSector = new SolidColorBrush(Color.FromScRgb(.35f, .25f, .25f, .25f));
		Brush _blueColour = new SolidColorBrush(Color.FromScRgb(1f, 0f, .51f, .78f));
		Brush _blackColour = new SolidColorBrush(Color.FromScRgb(1f, 0f, 0f, 0f));
		Brush _grayColour = new SolidColorBrush(Color.FromScRgb(1f, .51f, .57f, .64f));
		Brush _whiteColour = new SolidColorBrush(Color.FromScRgb(1f, 1f, 1f, 1f));

		private void DrawCord(Canvas canvas, Point centre, float theWidth)
		{
			var geometry = new StreamGeometry() { FillRule = FillRule.EvenOdd };
			using (var context = geometry.Open())
			{
				var arcRadius = theWidth / 3f;
				var curveLength = arcRadius / 5f;
				var curveWidth = arcRadius / 10f;
				var curveStartPoint = new Point(centre.X, centre.Y - arcRadius / 1.5f);
				var curveExtent = new Point(-curveWidth, curveLength);
				curveExtent.Offset(curveStartPoint.X, curveStartPoint.Y);
				var leftCtrlPoint = new Point(-1f * curveWidth, curveLength / 3f);
				leftCtrlPoint.Offset(curveStartPoint.X, curveStartPoint.Y);
				var rightCtrlPoint = new Point(2f * curveWidth, 2f * curveLength / 3f);
				rightCtrlPoint.Offset(curveStartPoint.X, curveStartPoint.Y);
				context.BeginFigure(curveStartPoint, false, false);
				context.BezierTo(leftCtrlPoint, rightCtrlPoint, curveExtent, true, true);
			}
			geometry.Freeze();
			var path = new System.Windows.Shapes.Path() { Data = geometry, Stroke = _blackColour, StrokeThickness = 4f };
			canvas.Children.Add(path);
		}
		private Path DrawWedge(Canvas canvas, Point centre, float arcRadius, int i, float rotationAngle, float wedgeAngle, Brush sectorBrush)
		{
			var geometry = new StreamGeometry() { FillRule = FillRule.EvenOdd };
			using (var context = geometry.Open())
			{
				DrawGeometry(context, centre, rotationAngle, wedgeAngle, arcRadius, 0);
			}
			geometry.Freeze();
			var path = new System.Windows.Shapes.Path() { Data = geometry, Fill = sectorBrush };
			canvas.Children.Add(path);
			return path;
		}

		private void DrawGeometry(StreamGeometryContext context, Point centre, float RotationAngle, float WedgeAngle, float Radius, float InnerRadius)
		{
			var innerArcStartPoint = ComputeCartesianCoordinate(RotationAngle, InnerRadius);
			innerArcStartPoint.Offset(centre.X, centre.Y);

			var innerArcEndPoint = ComputeCartesianCoordinate(RotationAngle + WedgeAngle, InnerRadius);
			innerArcEndPoint.Offset(centre.X, centre.Y);

			var outerArcStartPoint = ComputeCartesianCoordinate(RotationAngle, Radius);
			outerArcStartPoint.Offset(centre.X, centre.Y);

			var outerArcEndPoint = ComputeCartesianCoordinate(RotationAngle + WedgeAngle, Radius);
			outerArcEndPoint.Offset(centre.X, centre.Y);

			var largeArc = WedgeAngle > 180.0;

			var outerArcSize = new Size(Radius, Radius);
			var innerArcSize = new Size(InnerRadius, InnerRadius);

			context.BeginFigure(innerArcStartPoint, true, true);
			context.LineTo(outerArcStartPoint, true, true);
			context.ArcTo(outerArcEndPoint, outerArcSize, 0, largeArc, SweepDirection.Clockwise, true, true);
			context.LineTo(innerArcEndPoint, true, true);
			context.ArcTo(innerArcStartPoint, innerArcSize, 0, largeArc, SweepDirection.Counterclockwise, true, true);
		}
		private void DrawText(Canvas canvas, Point centre, float arcRadius, int i, float rotationAngle)
		{
			var text = new FormattedText((1 + i).ToString(), CultureInfo.CurrentUICulture, FlowDirection, _typeFace, FontSize, _whiteColour);
			var textLocation = new Point
			{
				X = centre.X + ((arcRadius / 1.5) * (float)Math.Cos(rotationAngle * Math.PI / 180f)) - (text.Width / 2),
				Y = centre.Y + ((arcRadius / 1.5) * (float)Math.Sin(rotationAngle * Math.PI / 180f)) - (text.Height / 2),
			};
			var geometry = text.BuildGeometry(textLocation);
			var path = new System.Windows.Shapes.Path() { Data = geometry, Fill = _whiteColour };
			canvas.Children.Add(path);
		}

		// Converts a coordinate from the polar coordinate system to the cartesian coordinate system.
		static Point ComputeCartesianCoordinate(double angle, double radius)
		{
			// convert to radians
			var angleRad = (Math.PI / 180.0) * (angle - 90);

			var x = radius * Math.Cos(angleRad);
			var y = radius * Math.Sin(angleRad);

			return new Point(x, y);
		}
		Brush[] TrackColours
		{
			get
			{
				if (_trackColours == null)
				{
					_trackColours = new[] { "#0652A3", "#0981FF", "#127100", "#1FC752", "#642887", "#A36AB9", "#887766", "#BBAABB", "#BB3344", "#88DDDD" }
						.Select(hexColor => new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor))).ToArray();
				}
				return _trackColours;
			}
		}
		Brush[] _trackColours;
	}
}
