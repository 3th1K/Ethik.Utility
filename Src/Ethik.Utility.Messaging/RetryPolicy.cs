namespace Ethik.Utility.Messaging;

public class RetryPolicy
{
    public int MaxRetryAttempts { get; set; } = 5;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);
    public double BackoffExponent { get; set; } = 2;

    public TimeSpan GetRetryDelay(int attempt)
    {
        return TimeSpan.FromSeconds(
            InitialDelay.TotalSeconds * Math.Pow(BackoffExponent, attempt - 1));
    }
}
