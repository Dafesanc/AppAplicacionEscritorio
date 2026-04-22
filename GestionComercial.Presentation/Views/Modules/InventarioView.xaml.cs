using System.Windows.Controls;
using GestionComercial.Presentation.ViewModels.Modules;

namespace GestionComercial.Presentation.Views.Modules;

public partial class InventarioView : UserControl
{
    public InventarioView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is InventarioViewModel vm)
                await vm.CargarAsync();
        };
    }
}
