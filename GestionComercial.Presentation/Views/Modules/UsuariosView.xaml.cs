using System.Windows.Controls;
using GestionComercial.Presentation.ViewModels.Modules;

namespace GestionComercial.Presentation.Views.Modules;

public partial class UsuariosView : UserControl
{
    private UsuariosViewModel? _vm;

    public UsuariosView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => _vm = DataContext as UsuariosViewModel;
        Loaded += async (_, _) =>
        {
            if (DataContext is UsuariosViewModel vm)
            {
                _vm = vm;
                await vm.CargarAsync();
            }
        };
    }

    private void PbContrasena_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_vm != null && sender is PasswordBox pb)
            _vm.FormContrasena = pb.Password;
    }
}
