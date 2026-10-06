namespace SatisMVVM.Commands;

/// <summary>
/// Provides data for the <see cref="AsyncRelayCommand.ExecutionFailed"/> event.
/// </summary>
public sealed class AsyncCommandExecutionFailedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AsyncCommandExecutionFailedEventArgs"/> class.
    /// </summary>
    /// <param name="exception">The exception thrown during command execution.</param>
    public AsyncCommandExecutionFailedEventArgs(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));

        Exception = exception;
    }

    /// <summary>
    /// Gets the exception thrown during command execution.
    /// </summary>
    public Exception Exception { get; }
}