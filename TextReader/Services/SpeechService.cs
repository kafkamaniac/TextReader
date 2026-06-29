using System.Speech.Synthesis;

public class SpeechService : IDisposable
{
    private readonly SpeechSynthesizer _synthesizer;

    public event Action<int> ProgressChanged;

    public event Action SpeakCompleted;
    public SpeechService()
    {
        _synthesizer = new SpeechSynthesizer
        {
            Volume = 100,
            Rate = 0
        };

        _synthesizer.SpeakCompleted += Synth_SpeakCompleted;
        _synthesizer.SpeakProgress += Synth_SpeakProgress;
    }

    public IEnumerable<InstalledVoice> GetVoices()
    {
        return _synthesizer.GetInstalledVoices();
    }

    public void Read(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        _synthesizer.SpeakAsyncCancelAll();

        _synthesizer.SpeakAsync(text);
    }

    public void Stop()
    {
        _synthesizer.SpeakAsyncCancelAll();
    }

    public void Pause()
    {
        if (_synthesizer.State == SynthesizerState.Speaking)
            _synthesizer.Pause();
    }

    public void Resume()
    {
        if (_synthesizer.State == SynthesizerState.Paused)
            _synthesizer.Resume();
    }

    public void SetVoice(string voiceName)
    {
        if (string.IsNullOrWhiteSpace(voiceName))
            return;

        _synthesizer.SelectVoice(voiceName);
    }

    public void SetRate(int rate)
    {
        _synthesizer.Rate = Math.Clamp(rate, -10, 10);
    }
    public void Dispose()
    {

        try
        {
            ProgressChanged = null;

            _synthesizer.SpeakCompleted -= Synth_SpeakCompleted;
            _synthesizer.SpeakProgress -= Synth_SpeakProgress;

            _synthesizer.SpeakAsyncCancelAll();
            _synthesizer.Dispose();
        }
        catch { }
    }

    private void Synth_SpeakProgress(object sender, SpeakProgressEventArgs e)
    {
        ProgressChanged?.Invoke(e.CharacterPosition);
    }

    private void Synth_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
    {
        SpeakCompleted?.Invoke();
    }
}