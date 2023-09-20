using UnityEngine;

namespace razz
{
	public struct Line
	{
		private const float verticalLineGradient = 1e5f;

		private float _gradient;
		private float _yIntercept;
		private Vector2 _pointOnLine1;
		private Vector2 _pointOnLine2;
		private float _gradientPerpendicular;
		private bool _approachSide;

		public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
		{
			float dx = pointOnLine.x - pointPerpendicularToLine.x;
			float dy = pointOnLine.y - pointPerpendicularToLine.y;

			if (dx == 0) _gradientPerpendicular = verticalLineGradient;
			else _gradientPerpendicular = dy / dx;

			if (_gradientPerpendicular == 0) _gradient = verticalLineGradient;
			else _gradient = -1 / _gradientPerpendicular;

			_yIntercept = pointOnLine.y - _gradient * pointOnLine.x;
			_pointOnLine1 = pointOnLine;
			_pointOnLine2 = pointOnLine + new Vector2(1, _gradient);

			_approachSide = false;
			_approachSide = GetSide(pointPerpendicularToLine);
		}

		bool GetSide(Vector2 p)
		{
			return (p.x - _pointOnLine1.x) * (_pointOnLine2.y - _pointOnLine1.y) > (p.y - _pointOnLine1.y) * (_pointOnLine2.x - _pointOnLine1.x);
		}

		public bool HasCrossedLine(Vector2 p)
		{
			return GetSide(p) != _approachSide;
		}

		public float DistanceFromPoint(Vector2 p)
		{
			float yInterceptPerpendicular = p.y - _gradientPerpendicular * p.x;
			float intersectX = (yInterceptPerpendicular - _yIntercept) / (_gradient - _gradientPerpendicular);
			float intersectY = _gradient * intersectX + _yIntercept;
			return Vector2.Distance(p, new Vector2(intersectX, intersectY));
		}

		public void DrawWithGizmos(Vector3 point, float length)
		{
			point.y = 0;
			Vector3 lineDir = new Vector3(1, 0, _gradient).normalized;
			Vector3 lineCentre = new Vector3(_pointOnLine1.x, 0, _pointOnLine1.y);
			Color startColor = Color.yellow;
			Color endColor = Color.green;
			float t = Mathf.InverseLerp(-length * 0.5f, length * 0.5f, _pointOnLine1.y);
			Color lineColor = Color.Lerp(startColor, endColor, t);

			Vector3 leftPoint = lineCentre - lineDir * length * 0.5f;
			Vector3 rightPoint = lineCentre + lineDir * length * 0.5f;

			lineColor.a = 0.4f;
			Gizmos.color = lineColor;
			Gizmos.DrawLine(leftPoint, rightPoint);
			lineColor.a = 0.08f;
			Gizmos.color = lineColor;
			Gizmos.DrawLine(rightPoint, point);
			Gizmos.DrawLine(leftPoint, point);
		}
	}
}
