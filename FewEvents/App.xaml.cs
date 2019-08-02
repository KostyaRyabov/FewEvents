using System.Windows;
using System.Windows.Input;

namespace FewEvents
{
    public partial class App : Application
    {
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).ClearCB();
        }

        private void PART_EditableTextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).ShowCB();
        }
    }
}
