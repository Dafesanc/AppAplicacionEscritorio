using System.Windows.Controls;
using GestionComercial.Presentation.ViewModels.Modules;

namespace GestionComercial.Presentation.Views.Modules;

public partial class PesajesView : UserControl
{
    public PesajesView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is PesajesViewModel vm)
                await vm.CargarAsync();
        };
    }
}
