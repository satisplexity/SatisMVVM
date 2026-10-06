using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace SatisMVVM.Core;

/// <summary>
/// Provides a base class for objects that notify clients when property values change.
/// </summary>
/// <remarks>
/// Implements <see cref="INotifyPropertyChanged"/> and provides helper methods 
/// for raising property change notifications and updating property values.
/// </remarks>
public abstract class ObservableObject : INotifyPropertyChanged
{
    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that changed. Automatically supplied by the compiler
    /// when omitted.
    /// </param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Updates a field if its value differs from the specified value
    /// and raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="field">A reference to the backing field.</param>
    /// <param name="value">The new property value.</param>
    /// <param name="propertyName">
    /// The name of the property associated with the backing field.
    /// Automatically supplied by the compiler when omitted.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the field was updated;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;

        OnPropertyChanged(propertyName);

        return true;
    }
}