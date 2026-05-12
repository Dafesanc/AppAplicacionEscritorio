using System.Windows.Controls;
using GestionComercial.Presentation.ViewModels;

namespace GestionComercial.Presentation.Views;

public partial class ChatBotView : UserControl
{
    public ChatBotView()
    {
        InitializeComponent();
    }

    // Cuando cambia la colección de mensajes, hacer scroll al último
    internal void BindViewModel(ChatBotViewModel vm)
    {
        vm.Messages.CollectionChanged += (_, _) =>
            Dispatcher.InvokeAsync(() => MessagesScroll.ScrollToBottom(),
                System.Windows.Threading.DispatcherPriority.Background);
    }
}
