using UnityEngine;

namespace razz
{
    public enum WaveType { Sine, Square, Triangle }

    //Switches between wave functions depending on wave type
    public static class WaveCalculation
    {
        public static float CalculateWave(WaveType waveType, float x, float amplitude, float frequency, float periodicity)
        {
            switch (waveType)
            {
                case WaveType.Sine:
                    float sin = Mathf.Sin(frequency * Mathf.PI * (x + Time.time * periodicity / 3)) * amplitude;
                    return sin;
                case WaveType.Square:
                    float square = Mathf.Sign(Mathf.Sin(frequency * Mathf.PI * (x + Time.time * periodicity / 3))) * amplitude;
                    return square;
                case WaveType.Triangle:
                    float triangle = (Mathf.Abs(((frequency * Mathf.PI * (x + Time.time * periodicity / 3)) % 4) - 2) - 1) * amplitude;
                    return triangle;
            }
            return 0;
        }
    }
}
