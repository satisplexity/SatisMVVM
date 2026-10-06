namespace SatisMVVM.Core;

/// <summary>
/// Provides a base class for view models with property change notification support.
/// </summary>
/// <remarks>
/// Inherits from <see cref="ObservableObject"/> to provide property change
/// notification functionality to derived view models.
/// </remarks>
public abstract class ViewModelBase : ObservableObject { }