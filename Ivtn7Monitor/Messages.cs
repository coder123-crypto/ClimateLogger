// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Windows;

namespace Ivtn7Monitor
{
    internal static class Messages
    {
        public static void ShowError(string message)
        {
            MessageBox.Show(message, nameof(Ivtn7Monitor), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}