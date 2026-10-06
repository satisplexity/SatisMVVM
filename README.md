<p align="center">
  <img src="https://raw.githubusercontent.com/satisplexity/SatisMVVM/main/assets/SatisMVVM.png"
       width="128"
       alt="SatisMVVM logo">
</p>

<h1 align="center">SatisMVVM</h1>

# SatisMVVM

A lightweight MVVM library for .NET with observable objects, commands, and view-model navigation.

[NuGet](https://www.nuget.org/packages/SatisMVVM) · [GitHub](https://github.com/satisplexity/SatisMVVM) · [MIT License](https://github.com/satisplexity/SatisMVVM/blob/main/LICENSE)

SatisMVVM provides a small set of building blocks for separating application logic from the user interface. It does not depend on WPF-specific controls or services; the examples below show how to use it in a WPF application.

> SatisMVVM is in early development. The public API may change between releases.

## Requirements

- .NET 10 or a compatible later target framework.
- For the WPF examples: a WPF application targeting `net10.0-windows`.

The library currently targets `net10.0` and references `Microsoft.Extensions.DependencyInjection`. It does not target .NET Framework or .NET Standard.

## Installation

Run the following command in your application's project directory:

```shell
dotnet add package SatisMVVM
```

Alternatively, search for **SatisMVVM** in Visual Studio's NuGet Package Manager.

## What's included

| Namespace | Type | Purpose |
| --- | --- | --- |
| `SatisMVVM.Core` | `ObservableObject` | Property change notifications and `SetProperty`. |
| `SatisMVVM.Core` | `ViewModelBase` | Base class for view models; inherits from `ObservableObject`. |
| `SatisMVVM.Commands` | `CommandBase` | Base implementation of `ICommand` with explicit availability notifications. |
| `SatisMVVM.Commands` | `RelayCommand` | Synchronous commands with an optional execution condition. |
| `SatisMVVM.Commands` | `AsyncRelayCommand` | Asynchronous commands that prevent overlapping execution of the same instance. |
| `SatisMVVM.Commands` | `AsyncCommandExecutionFailedEventArgs` | Exception information for the `ExecutionFailed` event. |
| `SatisMVVM.Navigation` | `NavigationStore` | Stores the current view model. |
| `SatisMVVM.Navigation` | `INavigationService` | Contract for forward, replacement, and backward navigation. |
| `SatisMVVM.Navigation` | `NavigationService` | Abstract navigation implementation using dependency injection and a history stack. |

## Observable properties and commands

Inherit from `ViewModelBase` and use `SetProperty` in property setters. It updates the backing field and raises `PropertyChanged` only when the value changes.

```csharp
using SatisMVVM.Commands;
using SatisMVVM.Core;

public sealed class CounterViewModel : ViewModelBase
{
    private int _count;

    public int Count
    {
        get => _count;
        private set
        {
            if (SetProperty(ref _count, value))
                ResetCommand.RaiseCanExecuteChanged();
        }
    }

    public RelayCommand IncrementCommand { get; }
    public RelayCommand ResetCommand { get; }

    public CounterViewModel()
    {
        IncrementCommand = new RelayCommand(() => Count++);
        ResetCommand = new RelayCommand(() => Count = 0, () => Count > 0);
    }
}
```

With a `CounterViewModel` instance assigned to the view's `DataContext`, bind to its properties and commands:

```xml
<StackPanel>
    <TextBlock Text="{Binding Count}" />
    <Button Content="Increment" Command="{Binding IncrementCommand}" />
    <Button Content="Reset" Command="{Binding ResetCommand}" />
</StackPanel>
```

Call `RaiseCanExecuteChanged()` when a value used by a command's condition changes. SatisMVVM does not automatically connect commands to WPF's `CommandManager`.

For dependent properties, call `OnPropertyChanged(nameof(YourProperty))` after updating the underlying value.

### Command parameters

`RelayCommand` also accepts an `Action<object?>` and an optional `Predicate<object?>`:

```csharp
var selectCommand = new RelayCommand(
    parameter => System.Diagnostics.Debug.WriteLine($"Selected: {parameter}"),
    parameter => parameter is string);

if (selectCommand.CanExecute("Example"))
    selectCommand.Execute("Example");
```

WPF passes `CommandParameter` to these delegates. When invoking a `RelayCommand` directly, check `CanExecute` yourself: its `Execute` method does not enforce the condition.

## Asynchronous commands

Use `AsyncRelayCommand` for operations returning a `Task`:

```csharp
using System.Threading.Tasks;
using SatisMVVM.Commands;
using SatisMVVM.Core;

public sealed class LoadingViewModel : ViewModelBase
{
    private string _status = "Ready";

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public AsyncRelayCommand LoadCommand { get; }

    public LoadingViewModel()
    {
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        LoadCommand.ExecutionFailed += (_, args) =>
            Status = $"Loading failed: {args.Exception.Message}";
    }

    private async Task LoadAsync()
    {
        Status = "Loading...";
        await Task.Delay(500); // Replace with your asynchronous operation.
        Status = "Loaded";
    }
}
```

- While running, the command returns `false` from `CanExecute` and rejects overlapping executions of the same instance.
- It raises `CanExecuteChanged` at the beginning and end of execution.
- For calls from code, await `ExecuteAsync()`; exceptions propagate to the caller.
- For calls through `ICommand.Execute`, including WPF command bindings, exceptions are reported through `ExecutionFailed`. Subscribe to this event to handle them; without a subscriber, they are swallowed.
- `IsExecuting` is available for inspection, but currently does not raise `PropertyChanged`. It cannot by itself drive a live loading-indicator binding.

## Navigation

Navigation switches view models. Your UI remains responsible for displaying the corresponding views.

### Define the service and destinations

`NavigationService` is abstract. Create a concrete application service:

```csharp
using System;
using SatisMVVM.Core;
using SatisMVVM.Navigation;

public sealed class AppNavigationService : NavigationService
{
    public AppNavigationService(
        NavigationStore store,
        IServiceProvider serviceProvider)
        : base(store, serviceProvider)
    {
    }
}

public sealed class HomeViewModel : ViewModelBase { }

public sealed class SettingsViewModel : ViewModelBase { }

public sealed class DetailsViewModel : ViewModelBase
{
    public int ItemId { get; }

    public DetailsViewModel(int itemId)
    {
        ItemId = itemId;
    }
}
```

### Register and navigate

The following code illustrates registration and navigation inside an asynchronous method:

```csharp
using Microsoft.Extensions.DependencyInjection;
using SatisMVVM.Navigation;

var services = new ServiceCollection();

services.AddSingleton<NavigationStore>();
services.AddSingleton<AppNavigationService>();
services.AddSingleton<INavigationService>(provider =>
    provider.GetRequiredService<AppNavigationService>());

services.AddTransient<HomeViewModel>();
services.AddTransient<SettingsViewModel>();

using var provider = services.BuildServiceProvider();
var navigation = provider.GetRequiredService<INavigationService>();

await navigation.NavigateTo<HomeViewModel>();
await navigation.NavigateTo<SettingsViewModel>();

if (navigation.CanGoBack)
    await navigation.GoBack();

// Pass a non-null value to the destination constructor.
await navigation.NavigateTo<DetailsViewModel, int>(42);

// Replace the current destination without pushing it onto the history stack.
await navigation.ReplaceWith<HomeViewModel>();
```

In a desktop application, retain the service provider for the application lifetime and dispose it at shutdown. The `using` above is for a self-contained example.

Navigation without a parameter resolves destinations from the container. Use transient registrations when each visit should create a new instance. With a non-null parameter, navigation creates a destination using `ActivatorUtilities.CreateInstance`, supplying that parameter and resolving remaining dependencies from the container; the destination itself does not need registration.

### Display the current view model in WPF

Expose the store from your shell view model:

```csharp
using SatisMVVM.Core;
using SatisMVVM.Navigation;

public sealed class ShellViewModel : ViewModelBase
{
    public NavigationStore Navigation { get; }

    public ShellViewModel(NavigationStore navigation)
    {
        Navigation = navigation;
    }
}
```

Construct the shell with the same `NavigationStore` used by the navigation service and assign it to the shell view's `DataContext`. Then bind a content host:

```xml
<ContentControl Content="{Binding Navigation.CurrentViewModel}" />
```

Define WPF `DataTemplate` resources for your destination view-model types to map them to views. SatisMVVM does not create views or provide those templates.

### Navigation behavior

- The current view model can be `null` before the first navigation, even though the store's current property declaration is non-nullable.
- History stores view-model instances. `GoBack()` restores the previous instance and its state; an empty history is a no-op.
- `ReplaceWith` preserves existing history, but does not push the replaced view model onto it.
- Passing `null` to the parameterized overload takes the container-resolution path; it does not explicitly pass a null constructor argument.
- Navigation methods currently perform their work synchronously and return completed tasks.
- `Dispose()` on the navigation service unsubscribes from the store. It does not dispose view models or clear the history. Plan ownership and cleanup for view models that hold resources, especially those created with a parameter.

## UI thread usage

SatisMVVM does not dispatch notifications to a UI thread. Invoke navigation and command availability notifications on the appropriate UI thread. For WPF, start asynchronous commands on that thread and marshal background updates back to it when needed.

## License

SatisMVVM is distributed under the [MIT License](https://github.com/satisplexity/SatisMVVM/blob/main/LICENSE).
