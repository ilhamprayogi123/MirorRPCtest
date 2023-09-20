using UnityEngine;

namespace razz
{
	public class Path
	{
		public readonly Vector3[] lookPoints;
		public readonly Line[] turnBoundaries;
		public readonly int finishLineIndex;
		public readonly int slowDownIndex;
		public readonly int startEarlyIndex;

		public Path(Vector3[] waypoints, Vector3 startPos, float turnDst, float stoppingDst, bool forwardStart, int forwardNodes, float startEarly)
		{
			lookPoints = waypoints;
			turnBoundaries = new Line[lookPoints.Length];
			finishLineIndex = Mathf.Max(0, turnBoundaries.Length - 1);
			startEarlyIndex = finishLineIndex;

			Vector2 previousPoint = V3ToV2(startPos);
			for (int i = 0; i < lookPoints.Length; i++)
			{
				Vector2 currentPoint = V3ToV2(lookPoints[i]);
				Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;

				Vector2 turnBoundaryPoint;
                if (turnBoundaries.Length <= 2 || i == turnBoundaries.Length - 2 || i == turnBoundaries.Length - 1)
					turnBoundaryPoint = currentPoint;
                else if (i < 2 && forwardStart)
					turnBoundaryPoint = currentPoint - dirToCurrentPoint * turnDst * forwardNodes * 0.05f;
                else turnBoundaryPoint = currentPoint - dirToCurrentPoint * turnDst;

				turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDst);
				previousPoint = turnBoundaryPoint;
			}

			float dstFromEndPoint = 0;
			bool slowdownDone = false;
			bool earlyDone = false;
			for (int i = lookPoints.Length - 1; i > 0; i--)
			{
				dstFromEndPoint += Vector2.Distance(V3ToV2(lookPoints[i]), V3ToV2(lookPoints[i - 1]));
				if (dstFromEndPoint > stoppingDst && !slowdownDone)
				{
					slowDownIndex = i;
					slowdownDone = true;
				}
                if (dstFromEndPoint > startEarly && !earlyDone)
                {
					startEarlyIndex = i;
					earlyDone = true;
				}
                if (slowdownDone && earlyDone) break;

				if (i == 1 && !slowdownDone) slowDownIndex = 0;
				if (i == 1 && !earlyDone) startEarlyIndex = 0;
			}
		}

		private Vector2 V3ToV2(Vector3 v3)
		{
			return new Vector2(v3.x, v3.z);
		}

		public void DrawWithGizmos()
		{
			Color pointColor = Color.green;
			Color lineColor = Color.yellow;

			for (int i = 0; i < lookPoints.Length; i++)
			{
				float t = i / (float)(lookPoints.Length - 1);
				Gizmos.color = Color.Lerp(Color.yellow, pointColor, t);
				Gizmos.DrawCube(lookPoints[i], Vector3.one * 0.1f);

				if (i == lookPoints.Length - 1) continue;

				Gizmos.color = Color.Lerp(Color.grey, lineColor, t);
				Vector3 startPos = lookPoints[i];
				Vector3 endPos = lookPoints[i + 1];
				Gizmos.DrawLine(startPos, endPos);
			}

			Gizmos.color = Color.white;
            for (int j = 1; j < turnBoundaries.Length; j++)
            {
				turnBoundaries[j].DrawWithGizmos(lookPoints[j], 0.5f);
			}
		}
	}
}
