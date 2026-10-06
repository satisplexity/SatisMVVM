using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using SatisMVVM.Core;

namespace SatisMVVM.Navigation;
/// <summary>
/// Provides a base implementation of the <see cref="INavigationService"/> interface
/// for navigating between view models.
/// </summary>
/// <remarks>
/// Maintains the current view model, manages navigation history, and resolves
/// destination view models through an <see cref="IServiceProvider"/>.
/// Implements <see cref="IDisposable"/> to release event subscriptions.
/// </remarks>
public abstract class NavigationService : ObservableObject, INavigationService, IDisposable
{
    private readonly NavigationStore _store;
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<ViewModelBase> _history = new();

    private bool _disposed;

    /// <summary>
    /// Gets the currently active view model.
    /// </summary>
    public ViewModelBase CurrentViewModel => _store.CurrentViewModel;

    /// <summary>
    /// Gets a value indicating whether navigation to a previous view model is possible.
    /// </summary>
    public bool CanGoBack => _history.Count > 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationService"/> class.
    /// </summary>
    /// <param name="store">
    /// The store responsible for holding the currently active view model.
    /// </param>
    /// <param name="serviceProvider">
    /// The service provider used to resolve destination view models and their dependencies.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="store"/> or
    /// <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    protected NavigationService(NavigationStore store, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _store = store;
        _serviceProvider = serviceProvider;

        _store.PropertyChanged += OnStorePropertyChanged;
    }

    /// <summary>
    /// Navigates to a new view model and adds the current view model
    /// to the navigation history.
    /// </summary>
    /// <typeparam name="TViewModel">
    /// The type of the destination view model.
    /// </typeparam>
    /// <returns>A task representing the navigation operation.</returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the navigation service has been disposed.
    /// </exception>
    public Task NavigateTo<TViewModel>()
        where TViewModel : ViewModelBase
        => NavigateTo(typeof(TViewModel), null, addToHistory: true);

    /// <summary>
    /// Navigates to a new view model, passing a parameter to its constructor,
    /// and adds the current view model to the navigation history.
    /// </summary>
    /// <typeparam name="TViewModel">
    /// The type of the destination view model.
    /// </typeparam>
    /// <typeparam name="TParameter">
    /// The type of the parameter passed to the destination view model.
    /// </typeparam>
    /// <param name="parameter">
    /// The parameter supplied when creating the destination view model.
    /// </param>
    /// <returns>A task representing the navigation operation.</returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the navigation service has been disposed.
    /// </exception>
    public Task NavigateTo<TViewModel, TParameter>(TParameter parameter)
        where TViewModel : ViewModelBase
        => NavigateTo(typeof(TViewModel), parameter, addToHistory: true);

    /// <summary>
    /// Replaces the current view model with a new instance of the specified type
    /// without adding the current view model to the navigation history.
    /// </summary>
    /// <typeparam name="TViewModel">
    /// The type of the replacement view model.
    /// </typeparam>
    /// <returns>A task representing the replacement operation.</returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the navigation service has been disposed.
    /// </exception>
    public Task ReplaceWith<TViewModel>()
        where TViewModel : ViewModelBase
        => NavigateTo(typeof(TViewModel), null, addToHistory: false);

    /// <summary>
    /// Replaces the current view model with the specified instance.
    /// </summary>
    /// <param name="viewModel">
    /// The view model instance that becomes active.
    /// </param>
    /// <returns>A task representing the replacement operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="viewModel"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the navigation service has been disposed.
    /// </exception>
    public Task ReplaceWith(ViewModelBase viewModel)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(viewModel);

        _store.CurrentViewModel = viewModel;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Navigates back to the most recently visited view model, if one exists.
    /// </summary>
    /// <returns>A task representing the navigation operation.</returns>
    /// <remarks>
    /// Does nothing when the navigation history is empty.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the navigation service has been disposed.
    /// </exception>
    public Task GoBack()
    {
        ThrowIfDisposed();

        bool canGoBackBefore = CanGoBack;

        if (!_history.TryPop(out var previousViewModel))
            return Task.CompletedTask;

        _store.CurrentViewModel = previousViewModel;

        if (canGoBackBefore != CanGoBack)
            OnPropertyChanged(nameof(CanGoBack));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates and activates a destination view model.
    /// </summary>
    /// <param name="viewModelType">
    /// The type of the destination view model.
    /// </param>
    /// <param name="parameter">
    /// An optional parameter supplied to the destination view model's constructor.
    /// </param>
    /// <param name="addToHistory">
    /// Determines whether the current view model is added to the navigation history.
    /// </param>
    /// <returns>A completed task representing the navigation operation.</returns>
    /// <remarks>
    /// Creates the destination before modifying the navigation state so that
    /// a failure during creation does not change the current view model or history.
    /// </remarks>
    private Task NavigateTo(Type viewModelType, object? parameter, bool addToHistory)
    {
        ThrowIfDisposed();

        var destination = parameter is null
            ? (ViewModelBase)_serviceProvider.GetRequiredService(viewModelType)
            : (ViewModelBase)ActivatorUtilities.CreateInstance(
                _serviceProvider,
                viewModelType,
                parameter);

        bool canGoBackBefore = CanGoBack;

        if (addToHistory && _store.CurrentViewModel is { } currentViewModel)
            _history.Push(currentViewModel);

        _store.CurrentViewModel = destination;

        if (canGoBackBefore != CanGoBack)
            OnPropertyChanged(nameof(CanGoBack));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles property change notifications from the navigation store.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="args">The event data containing the changed property name.</param>
    private void OnStorePropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(NavigationStore.CurrentViewModel) or null or "")
            OnPropertyChanged(nameof(CurrentViewModel));
    }

    /// <summary>
    /// Throws an exception if the navigation service has been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the navigation service has been disposed.
    /// </exception>
    private void ThrowIfDisposed()
        => ObjectDisposedException.ThrowIf(_disposed, this);

    /// <summary>
    /// Releases the event subscription to the navigation store.
    /// </summary>
    /// <remarks>
    /// Calling this method multiple times has no additional effect.
    /// </remarks>
    public virtual void Dispose()
    {
        if (_disposed)
            return;

        _store.PropertyChanged -= OnStorePropertyChanged;
        _disposed = true;
    }
}