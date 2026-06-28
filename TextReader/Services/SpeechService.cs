using System.Speech.Synthesis;

public class SpeechService : IDisposable
{
    private readonly SpeechSynthesizer _synthesizer;

    public SpeechService()
    {
        _synthesizer = new SpeechSynthesizer
        {
            Volume = 100,
            Rate = 0
        };
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