using System.Windows.Controls;
using GestionComercial.Presentation.ViewModels.Modules;

namespace GestionComercial.Presentation.Views.Modules;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is DashboardViewModel vm)
                await vm.CargarDatosAsync();
        };
    }
}
