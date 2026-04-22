using System.Windows.Controls;
using GestionComercial.Presentation.ViewModels.Modules;

namespace GestionComercial.Presentation.Views.Modules;

public partial class ClientesView : UserControl
{
    public ClientesView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is ClientesViewModel vm)
                await vm.CargarAsync();
        };
    }
}
