using UnityEngine;
using TMPro;

public class MicrophonePitchDetection : MonoBehaviour
{
    public TextMeshProUGUI frequencyText;
    private AudioClip microphoneInput;
    private int sampleRate = 44100;
    private string micDevice;
    private const int sampleSize = 8192;
    private float[] audioData = new float[sampleSize];

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            for (int i = 0; i < Microphone.devices.Length; i++)
            {
                Debug.Log($"Microphone {i}: {Microphone.devices[i]}");
            }

            micDevice = Microphone.devices[0]; // �lk cihaz� se�iyoruz, de�i�tirebilirsiniz.
            microphoneInput = Microphone.Start(micDevice, true, 1, sampleRate);
            Debug.Log("Microphone started: " + micDevice);
        }
        else
        {
            Debug.LogError("No microphone detected!");
        }
    }

    void Update()
    {
        if (microphoneInput != null && Microphone.IsRecording(micDevice))
        {
            microphoneInput.GetData(audioData, 0);
            ApplyHannWindow(audioData); // G�r�lt� azaltma
            LowPassFilter(audioData, 1000f); // D���k ge�i� filtresi
            float frequency = DetectPitch(audioData);
            if (frequency > 0)
            {
                frequencyText.text = "Frequency: " + frequency.ToString("F2") + " Hz";
            }
            else
            {
                frequencyText.text = "Frequency: Not Detected";
            }
        }
    }

    void ApplyHannWindow(float[] data)
    {
        int N = data.Length;
        for (int i = 0; i < N; i++)
        {
            data[i] *= 0.5f * (1 - Mathf.Cos(2 * Mathf.PI * i / (N - 1)));
        }
    }

    void LowPassFilter(float[] data, float cutoffFrequency)
    {
        float RC = 1.0f / (cutoffFrequency * 2 * Mathf.PI);
        float dt = 1.0f / sampleRate;
        float alpha = dt / (RC + dt);
        for (int i = 1; i < data.Length; i++)
        {
            data[i] = data[i - 1] + alpha * (data[i] - data[i - 1]);
        }
    }

    float DetectPitch(float[] data)
    {
        int maxLag = data.Length / 2;
        float[] autocorrelation = new float[maxLag];

        // Autocorrelation hesaplama
        for (int lag = 0; lag < maxLag; lag++)
        {
            for (int i = 0; i < data.Length - lag; i++)
            {
                autocorrelation[lag] += data[i] * data[i + lag];
            }
        }

        // En b�y�k peak yerine ilk anlaml� peak'i bul
        int peakIndex = 0;
        float threshold = autocorrelation[0] * 0.5f; // �lk de�er �zerinden bir e�ik belirle
        for (int i = 1; i < maxLag - 1; i++)
        {
            if (autocorrelation[i] > threshold && autocorrelation[i] > autocorrelation[i - 1] && autocorrelation[i] > autocorrelation[i + 1])
            {
                peakIndex = i;
                break;
            }
        }

        // E�er peak bulunamazsa, hata durumunu y�net
        if (peakIndex == 0 || autocorrelation[peakIndex] <= 0)
        {
            Debug.LogWarning("No pitch detected. Autocorrelation failed.");
            return 0.0f;
        }

        // Periyot ve frekans hesaplama
        float period = (float)peakIndex;
        return sampleRate / period;
    }
}
