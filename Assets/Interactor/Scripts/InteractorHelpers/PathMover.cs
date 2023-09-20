using UnityEngine;
using System.Collections;

namespace razz
{
    //Moves object on designeted points with Coroutines
    //TODO There are lots of improvements to do here.
    [HelpURL("https://negengames.com/interactor/components.html#pathmovercs")]
    [DisallowMultipleComponent]
    public class PathMover : MonoBehaviour
    {
        public EaseType moveEase;
        [Tooltip("Probability in every seconds, but beware: it can happen twice or more in a row even if you gave a long time. Its random.")]
        public int odd = 1000;
        [Tooltip("Time interval for each points")]
        public float moveDuration;
        [Tooltip("Points for target to move in order")]
        public Vector3[] points;
        [Tooltip("Starting points index")]
        public int startIndex;

        public bool playing = false;

        public void StartMove(float ikDuration)
        {
            if (points.Length == 0 || playing) return;

            playing = true;
            StartCoroutine(MoveOnPath(ikDuration));
            StartCoroutine(Timer(ikDuration));
        }

        IEnumerator Timer(float ikDuration)
        {
            yield return new WaitForSeconds((Mathf.Max(1 ,(points.Length - 1)) * moveDuration) + (ikDuration * 0.5f));
            playing = false;
        }

        IEnumerator MoveOnPath(float ikDuration)
        {
            yield return new WaitForSeconds(ikDuration * 0.5f);
            int index = startIndex;
            transform.localPosition = points[index];
            while (playing)
            {
                index = (index + 1) % points.Length;
                yield return StartCoroutine(transform.MoveTo(points[index], moveDuration, moveEase));
            }
        }
    }
}
