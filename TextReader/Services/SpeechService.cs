using System.Speech.Synthesis;

public class SpeechService : IDisposable
{
    private readonly SpeechSynthesizer _synthesizer;

    public event Action<string>? WordSpoken;
    public SpeechService()
    {
        _synthesizer = new SpeechSynthesizer
        {
            Volume = 100,
            Rate = 0
        };
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
            _synthesizer.SpeakAsyncCancelAll();
            _synthesizer.Dispose();
        }
        catch { }
    }

}