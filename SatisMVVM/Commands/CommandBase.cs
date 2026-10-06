using System.Windows.Input;

namespace SatisMVVM.Commands;

/// <summary>
/// Provides a base implementation of the <see cref="ICommand"/> interface.
/// </summary>
/// <remarks>
/// Defines the contract for commands that can determine whether they can be executed
/// and perform an action when executed.
/// </remarks>
public abstract class CommandBase : ICommand
{
    /// <summary>
    /// Occurs when changes affecting the ability to execute the command take place.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Determines whether the command can be executed with the specified parameter.
    /// </summary>
    /// <param name="parameter">
    /// The parameter passed to the command.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the command can be executed; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public abstract bool CanExecute(object? parameter);

    /// <summary>
    /// Executes the command using the specified parameter.
    /// </summary>
    /// <param name="parameter">
    /// The parameter passed to the command.
    /// </param>
    public abstract void Execute(object? parameter);

    /// <summary>
    /// Raises the <see cref="CanExecuteChanged"/> event to notify subscribers
    /// that the command's execution availability may have changed.
    /// </summary>
    public void RaiseCanExecuteChanged() 
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}