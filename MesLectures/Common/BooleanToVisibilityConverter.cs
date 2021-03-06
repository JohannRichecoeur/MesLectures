﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MesLectures.Common
{
    /// <summary>
    /// Convertisseur de valeur qui convertit true en <see cref="Visibility.Visible"/> et false en
    /// <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
