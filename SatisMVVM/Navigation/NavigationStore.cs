using SatisMVVM.Core;

namespace SatisMVVM.Navigation;

/// <summary>
/// Stores the currently active view model and notifies subscribers when it changes.
/// </summary>
/// <remarks>
/// Inherits from <see cref="ObservableObject"/> to provide property change
/// notifications for the <see cref="CurrentViewModel"/> property.
/// </remarks>
public class NavigationStore : ObservableObject
{
    private ViewModelBase _currentViewModel = null!;

    /// <summary>
    /// Gets or sets the currently active view model.
    /// </summary>
    /// <remarks>
    /// Raises the <see cref="ObservableObject.PropertyChanged"/> event
    /// when the assigned view model differs from the current instance.
    /// </remarks>
    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }
}