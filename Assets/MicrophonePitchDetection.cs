using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MicrophonePitchDetection : MonoBehaviour
{
    public TextMeshProUGUI frequencyText;
    private AudioClip microphoneInput;
    private int sampleRate = 44100;
    private string micDevice;
    private const int sampleSize = 1024;
    private float[] audioData = new float[sampleSize];

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0];
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
        if (microphoneInput != null)
        {
            microphoneInput.GetData(audioData, 0);
            float frequency = DetectPitch(audioData);
            frequencyText.text = "Frequency: " + frequency.ToString("F2") + " Hz";
        }
    }

    float DetectPitch(float[] data)
    {
        int zeroCrossings = 0;
        for (int i = 1; i < data.Length; i++)
        {
            if ((data[i - 1] > 0 && data[i] <= 0) || (data[i - 1] <= 0 && data[i] > 0))
                zeroCrossings++;
        }
        float period = (float)data.Length / (float)zeroCrossings;
        return sampleRate / period;
    }
}
