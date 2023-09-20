using UnityEngine;

namespace razz
{
    //Renders waves. All public methods or properties called by rotator events on inspector.
    [RequireComponent(typeof(LineRenderer), typeof(AudioSource))]
    public class WaveGraphLineRenderer : MonoBehaviour
    {
        private Vector3[] _points;
        private LineRenderer _lineRenderer;
        private AudioSource _audioSource;
        private int _index;

        [Tooltip("WaveType"), SerializeField]
        private WaveType currentWaveType = new WaveType();
        [Tooltip("Wave's Frequency"), Range(0.1f, 3.5f), SerializeField]
        private float frequency = 1f;
        [Tooltip("Wave's Amplitude"), Range(0.1f, 1f), SerializeField]
        private float amplitude = 1f;
        [Tooltip("Wave's Resolution"), Range(16, 1024), SerializeField]
        private int resolution = 1024;
        [Tooltip("Wave's Length"), Range(0.01f, 10.0f), SerializeField]
        private float length = 1;
        [Tooltip("Wave's Thickness"), Range(0.01f, 1f), SerializeField]
        private float thickness = 0.1f;
        [Tooltip("How fast wave moves"), Range(1, 5), SerializeField]
        private float periodicity = 1f;

        [Tooltip("Wave's Audio (WaveType ordered)"), SerializeField]
        public AudioClip[] currentWaveAudio;
        
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _lineRenderer = GetComponent<LineRenderer>();

            CreateWave();
            this.gameObject.SetActive(false);

            //On Unity 2021, lines face backwards. Also for some reason square wave type doesn't work correct.
#if UNITY_2021_1_OR_NEWER
            transform.Rotate(new Vector3(180f, 0, 0), Space.Self);
#endif
        }

        private void OnEnable()
        {
            currentWaveType = (WaveType)_index;
            _audioSource.clip = currentWaveAudio[_index];
            _audioSource.Play();
        }

        private void Update()
        {
            UpdateWave();
        }

        private void CreateWave()
        {
            _lineRenderer.positionCount = 0;
            float step = 2f / resolution * length;
            Vector3 position;
            position.y = transform.position.y;
            position.z = transform.position.z;
            _points = new Vector3[resolution];
            _lineRenderer.positionCount = resolution;
            _lineRenderer.startWidth = thickness;

            for (int i = 0; i < _points.Length; i++)
            {
                position.x = (((i + 0.5f) * step - 1f) - length + 1) + transform.position.x;
                _lineRenderer.SetPosition(i, position);
                _points[i] = _lineRenderer.GetPosition(i);
            }

            _audioSource.pitch = frequency;
        }

        private void UpdateWave()
        {
            if (_points.Length > 0)
            {
                for (int i = 0; i < _points.Length; i++)
                {
                    Vector3 point = _points[i];
                    Vector3 position = point;

                    switch (currentWaveType)
                    {
                        case WaveType.Sine:
                            position.y = WaveCalculation.CalculateWave(currentWaveType, position.x, amplitude, frequency, periodicity) / 10 + transform.position.y;
                            break;
                        case WaveType.Square:
                            position.y = WaveCalculation.CalculateWave(currentWaveType, position.x, amplitude, frequency, periodicity) / 10 + transform.position.y;
                            break;
                        case WaveType.Triangle:
                            position.y = WaveCalculation.CalculateWave(currentWaveType, position.x, amplitude, frequency, periodicity) / 10 + transform.position.y;
                            break;
                    }
                    _points[i] = position;
                    _lineRenderer.SetPosition(i, position);
                }
            }
        }

        #region Properties

        public float Frequency
        {
            get { return frequency; }
            set
            {
                if (frequency > 0f && frequency <= 10f)
                {
                    frequency = value;
                    CreateWave();
                }
            }
        }

        public float Amplitude
        {
            get { return amplitude; }
            set
            {
                if (amplitude > 0f && amplitude <= 1f)
                {
                    amplitude = value;
                    CreateWave();
                }
            }
        }

        public int Resolution
        {
            get { return resolution; }
            set
            {
                if (resolution > 0 && resolution <= 1024)
                {
                    resolution = value;
                    CreateWave();
                }
            }
        }

        public float Length
        {
            get { return length; }
            set
            {
                if (length > 0 && length <= 10)
                {
                    length = value;
                    CreateWave();
                }
            }
        }

        public float Thickness
        {
            get { return thickness; }
            set
            {
                if (thickness > 0f && thickness <= 1f)
                {
                    thickness = value;
                    CreateWave();
                }
            }
        }

        public float Periodicity
        {
            get { return periodicity; }
            set
            {
                if (periodicity > 0f && periodicity <= 10f)
                {
                    periodicity = value;
                    CreateWave();
                }
            }
        }

        public WaveType CurrentWaveType
        {
            get { return currentWaveType; }
            set
            {
                if (value != currentWaveType)
                {
                    currentWaveType = value;

                    _audioSource.clip = currentWaveAudio[(int)currentWaveType];
                    _audioSource.Play();
                }
            }
        }
        #endregion

        public void SetWaveType(int index)
        {
            _index = index;
            if (!this.isActiveAndEnabled) return;

            _audioSource.Stop();

            if (_index == 0)
            {
                currentWaveType = WaveType.Sine;

                _audioSource.clip = currentWaveAudio[(int)currentWaveType];
                _audioSource.Play();
            }
            if (_index == 1)
            {
                currentWaveType = WaveType.Square;

                _audioSource.clip = currentWaveAudio[(int)currentWaveType];
                _audioSource.Play();
            }
            if (_index == 2)
            {
                currentWaveType = WaveType.Triangle;

                _audioSource.clip = currentWaveAudio[(int)currentWaveType];
                _audioSource.Play();
            }
        }
    }
}