using System;
using System.Windows.Input;

namespace APMOk.Models;

/// <summary>
/// Simplistic delegate command
/// </summary>
internal class DelegateCommand : ICommand
{
    public Action? CommandAction { get; set; }
    public Func<bool>? CanExecuteFunc { get; set; }

    public void Execute(object? parameter)
    {
        CommandAction?.Invoke();
    }

    public bool CanExecute(object? parameter)
    {
        return CanExecuteFunc is null || CanExecuteFunc();
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value ?? throw new ArgumentNullException(nameof(value)); }
        remove { CommandManager.RequerySuggested -= value ?? throw new ArgumentNullException(nameof(value)); }
    }
}
