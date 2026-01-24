namespace CS2Cheat.Utils;

public abstract class ThreadedServiceBase :
    IDisposable
{
    #region

    protected virtual string ThreadName => nameof(ThreadedServiceBase);

    protected virtual TimeSpan ThreadTimeout { get; set; } = new(0, 0, 0, 3);

    protected virtual TimeSpan ThreadFrameSleep { get; set; } = new(0, 0, 0, 0, 1);

    private Thread Thread { get; set; }

    #endregion

    #region

    protected ThreadedServiceBase()
    {
        Thread = new Thread(ThreadStart)
        {
            Name = ThreadName,
            IsBackground = true, // Garante que a thread feche se o programa principal fechar
            Priority = ThreadPriority.Highest // <--- Adicione isso
        };
    }

    public virtual void Dispose()
    {
        Thread.Interrupt();
        if (!Thread.Join(ThreadTimeout)) Thread.Abort();

        Thread = default;
    }

    #endregion

    #region

    public void Start()
    {
        Thread.Start();
    }

    private void ThreadStart()
    {
        while (true)
        {
            FrameAction();
            // Sleep(0) é melhor que Sleep(1) ou Yield para evitar lag em cheats
            Thread.Sleep(0);
        }
    }

    protected abstract void FrameAction();

    #endregion
}