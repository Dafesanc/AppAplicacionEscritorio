using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GestionComercial.Presentation.Converters;

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
            return string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible;
        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : (object)false;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : (object)false;
}

public class BoolToActiveTagConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? "Active" : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// ConverterParameter: "TextoCargando|TextoNormal"
/// </summary>
public class BoolToLoadingTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isLoading && parameter is string param)
        {
            var parts = param.Split('|');
            if (parts.Length == 2)
                return isLoading ? parts[0] : parts[1];
        }
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class EstadoColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "ACTIVO" or "COMPLETADA" or "PAGADA" => "#10B981",
            "INACTIVO" or "ANULADA"               => "#6B7280",
            "BLOQUEADO" or "VENCIDA"              => "#EF4444",
            "BORRADOR" or "EMITIDA"               => "#F59E0B",
            "CRÉDITO"                              => "#3B82F6",
            _ => "#6B7280"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
