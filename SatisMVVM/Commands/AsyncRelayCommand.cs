namespace SatisMVVM.Commands;

/// <summary>
/// Represents a command that executes an asynchronous operation.
/// </summary>
/// <remarks>
/// Supports parameterized and parameterless asynchronous operations.
/// Prevents concurrent execution of the same command instance.
/// Exceptions can be observed through <see cref="ExecuteAsync"/>
/// or reported through <see cref="ExecutionFailed"/> when invoked
/// through <see cref="System.Windows.Input.ICommand.Execute(object?)"/>.
/// </remarks>
public sealed class AsyncRelayCommand : CommandBase
{
    private readonly Func<object?, Task> _execute;
    private readonly Predicate<object?>? _canExecute;

    private int _isExecuting;

    /// <summary>
    /// Occurs when an exception escapes command execution initiated
    /// through <see cref="System.Windows.Input.ICommand.Execute(object?)"/>.
    /// </summary>
    public event EventHandler<AsyncCommandExecutionFailedEventArgs>? ExecutionFailed;

    /// <summary>
    /// Gets a value indicating whether the command is currently executing.
    /// </summary>
    public bool IsExecuting => Volatile.Read(ref _isExecuting) != 0;

    /// <summary>
    /// Initializes a command with parameterless asynchronous execution.
    /// </summary>
    /// <param name="execute">The asynchronous operation to execute.</param>
    /// <param name="canExecute">
    /// An optional function that determines whether the command can execute.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="execute"/> is null.
    /// </exception>
    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute, nameof(execute));

        _execute = _ => execute();
        _canExecute = canExecute is null
            ? null
            : _ => canExecute();
    }

    /// <summary>
    /// Initializes a command with parameterized asynchronous execution.
    /// </summary>
    /// <param name="execute">The asynchronous operation to execute.</param>
    /// <param name="canExecute">
    /// An optional predicate that determines whether the command can execute.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="execute"/> is null.
    /// </exception>
    public AsyncRelayCommand(Func<object?, Task> execute, Predicate<object?>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute, nameof(execute));

        _execute = execute;
        _canExecute = canExecute;
    }

    /// <inheritdoc/>
    public override bool CanExecute(object? parameter)
        => !IsExecuting && (_canExecute?.Invoke(parameter) ?? true);

    /// <summary>
    /// Executes the command asynchronously.
    /// </summary>
    /// <param name="parameter">The parameter passed to the command.</param>
    /// <returns>A task representing the asynchronous execution.</returns>
    /// <remarks>
    /// Returns a completed task if the command cannot execute or another
    /// execution is already in progress. Exceptions thrown by the operation
    /// propagate to the returned task.
    /// </remarks>
    public async Task ExecuteAsync(object? parameter = null)
    {
        if (!CanExecute(parameter))
            return;

        if (Interlocked.CompareExchange(ref _isExecuting, 1, 0) != 0)
            return;

        try
        {
            RaiseCanExecuteChanged();

            await _execute(parameter);
        }
        finally
        {
            Volatile.Write(ref _isExecuting, 0);

            RaiseCanExecuteChanged();
        }
    }

    /// <inheritdoc/>
    public override async void Execute(object? parameter)
    {
        try
        {
            await ExecuteAsync(parameter);
        }
        catch (Exception exception)
        {
            ExecutionFailed?.Invoke(this, new AsyncCommandExecutionFailedEventArgs(exception));
        }
    }
}