using SatisMVVM.Core;

namespace SatisMVVM.Navigation;

/// <summary>
/// Defines a service for navigating between view models.
/// </summary>
/// <remarks>
/// Provides operations for navigating to a new view model, replacing the current
/// view model, and returning to the previous view model.
/// </remarks>
public interface INavigationService
{
    /// <summary>
    /// Gets a value indicating whether navigation to the previous view model is possible.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Navigates to a view model of the specified type and passes a parameter to it.
    /// </summary>
    /// <typeparam name="TViewModel">
    /// The type of the destination view model.
    /// </typeparam>
    /// <typeparam name="TParameter">
    /// The type of the parameter passed to the destination view model.
    /// </typeparam>
    /// <param name="parameter">
    /// The parameter passed to the destination view model.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous navigation operation.
    /// </returns>
    Task NavigateTo<TViewModel, TParameter>(TParameter parameter)
        where TViewModel : ViewModelBase;

    /// <summary>
    /// Navigates to a view model of the specified type.
    /// </summary>
    /// <typeparam name="TViewModel">
    /// The type of the destination view model.
    /// </typeparam>
    /// <returns>
    /// A task representing the asynchronous navigation operation.
    /// </returns>
    Task NavigateTo<TViewModel>()
        where TViewModel : ViewModelBase;

    /// <summary>
    /// Replaces the current view model with a new instance of the specified type.
    /// </summary>
    /// <typeparam name="TViewModel">
    /// The type of the replacement view model.
    /// </typeparam>
    /// <returns>
    /// A task representing the asynchronous replacement operation.
    /// </returns>
    Task ReplaceWith<TViewModel>()
        where TViewModel : ViewModelBase;

    /// <summary>
    /// Replaces the current view model with the specified instance.
    /// </summary>
    /// <param name="viewModel">
    /// The view model instance that replaces the current view model.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous replacement operation.
    /// </returns>
    Task ReplaceWith(ViewModelBase viewModel);

    /// <summary>
    /// Navigates back to the previous view model.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous navigation operation.
    /// </returns>
    Task GoBack();
}