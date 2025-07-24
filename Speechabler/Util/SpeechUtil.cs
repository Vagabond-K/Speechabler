using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace Speechabler.Util
{
    [ServiceDescription(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    class SpeechUtil : NotifyPropertyChangeObject
    {
        public SpeechUtil()
        {
            if (!DesignerProperties.GetIsInDesignMode(new System.Windows.FrameworkElement()))
            {
                speechSynthesizer = new SpeechSynthesizer();
                speechSynthesizer.Volume = 100;
                speechSynthesizer.SetOutputToDefaultAudioDevice();
                speechSynthesizer.SpeakStarted += OnSpeakStarted;
                speechSynthesizer.SpeakCompleted += OnSpeakCompleted;
                visualSpeechSynthesizer = new SpeechSynthesizer();
            }
        }

        private void OnSpeakStarted(object sender, SpeakStartedEventArgs e)
        {
            _ = Task.Run(() =>
            {
                using (var audioStream = new MemoryStream())
                {
                    visualSpeechSynthesizer.SetOutputToAudioStream(audioStream, new SpeechAudioFormatInfo(100, AudioBitsPerSample.Sixteen, AudioChannel.Mono));
                    visualSpeechSynthesizer.Speak(speechText);
                    audioData = audioStream.ToArray();
                }


                if (audioData == null) return;

                var stopwatch = Stopwatch.StartNew();
                int lastIndex = 0;
                double lastValue = 1;
                while (IsSpeeching)
                {
                    int index = (int)stopwatch.ElapsedMilliseconds / 10;
                    if (audioData.Length <= index * 2 + 1)
                    {
                        break;
                    }

                    lastValue -= 0.015 * (index - lastIndex);

                    double value = (double)(audioData[index * 2] << 8 | audioData[index * 2 + 1]) / ushort.MaxValue;
                    if (lastValue < value)
                        lastValue = value;

                    lastIndex = index;

                    AudioVisual = lastValue;
                }
            });
        }

        private void OnSpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            IsSpeeching = false;
        }


        private readonly SpeechSynthesizer speechSynthesizer;
        private readonly SpeechSynthesizer visualSpeechSynthesizer;
        private byte[] audioData;
        private string speechText;

        public bool IsSpeeching { get => Get(false); private set => Set(value); }
        public double AudioVisual { get => Get(0d); private set => Set(value); }

        public IInstantCommand CancelCommand => GetCommand(Cancel);

        public void Cancel()
        {
            speechSynthesizer.SpeakAsyncCancelAll();
            IsSpeeching = false;
        }

        public void Speech(string message)
        {
            Cancel();

            if (!string.IsNullOrWhiteSpace(message))
            {
                try
                {
                    speechText = message.Replace("\n", " ");

                    AudioVisual = 0;
                    IsSpeeching = true;
                    speechSynthesizer.SpeakAsync(speechText);
                }
                catch { }
            }
        }
    }
}
