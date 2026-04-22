using System.Windows.Controls;
using GestionComercial.Presentation.ViewModels.Modules;

namespace GestionComercial.Presentation.Views.Modules;

public partial class VehiculosView : UserControl
{
    public VehiculosView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is VehiculosViewModel vm)
                await vm.CargarDatosAsync();
        };
    }
}
