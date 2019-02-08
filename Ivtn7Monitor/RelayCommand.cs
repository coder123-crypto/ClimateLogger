// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Windows.Input;

namespace Ivtn7Monitor
{
    internal sealed class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public string Text { get; private set; }

        public RelayCommand(Action action, Func<bool> canExecute = null) : this(action, string.Empty, canExecute)
        {
        }

        public RelayCommand(Action action, string text,  Func<bool> canExecute = null)
        {
            Text = text;
            _action = action;
            _canExecute = canExecute;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Text))
            {
                return Text;
            }

            return base.ToString();
        }

        bool ICommand.CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        void ICommand.Execute(object parameter)
        {
            _action();
        }

        private readonly Action _action;
        private readonly Func<bool> _canExecute;
    }
}