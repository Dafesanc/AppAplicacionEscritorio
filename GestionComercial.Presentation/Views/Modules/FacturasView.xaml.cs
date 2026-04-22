using System.Windows.Controls;
using GestionComercial.Presentation.ViewModels.Modules;

namespace GestionComercial.Presentation.Views.Modules;

public partial class FacturasView : UserControl
{
    public FacturasView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is FacturasViewModel vm)
                await vm.CargarAsync();
        };
    }
}
