namespace SatisMVVM.Commands;

/// <summary>
/// Represents a command that delegates its execution logic to specified delegates.
/// </summary>
/// <remarks>
/// Supports commands with or without an execution parameter and optionally allows
/// specifying a predicate that determines whether the command can be executed.
/// </remarks>
public sealed class RelayCommand : CommandBase
{
    private readonly Action<object?> _execute;

    private readonly Predicate<object?>? _canExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand"/> class
    /// with parameterless execution and an optional execution condition.
    /// </summary>
    /// <param name="execute">
    /// The action to execute when the command is invoked.
    /// </param>
    /// <param name="canExecute">
    /// An optional function that determines whether the command can be executed.
    /// If <see langword="null"/>, the command can always be executed.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="execute"/> is <see langword="null"/>.
    /// </exception>
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute, nameof(execute));

        _execute = _ => execute();

        _canExecute = canExecute is null
            ? null
            : _ => canExecute.Invoke();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand"/> class
    /// with parameterized execution and an optional execution condition.
    /// </summary>
    /// <param name="execute">
    /// The action to execute with the command parameter.
    /// </param>
    /// <param name="canExecute">
    /// An optional predicate that determines whether the command can be executed
    /// with the specified parameter. If <see langword="null"/>, the command can
    /// always be executed.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="execute"/> is <see langword="null"/>.
    /// </exception>
    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute, nameof(execute));

        _execute = execute;
        _canExecute = canExecute;
    }

    /// <inheritdoc/>
    public override bool CanExecute(object? parameter)
        => _canExecute?.Invoke(parameter) ?? true;

    /// <inheritdoc/>
    public override void Execute(object? parameter)
        => _execute(parameter);
}