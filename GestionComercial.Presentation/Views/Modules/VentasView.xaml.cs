using System.Windows.Controls;
using GestionComercial.Presentation.ViewModels.Modules;

namespace GestionComercial.Presentation.Views.Modules;

public partial class VentasView : UserControl
{
    public VentasView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is VentasViewModel vm)
                await vm.CargarAsync();
        };
    }
}
