using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CookBook
{
    /// <summary>
    /// Логика взаимодействия для FindWindow.xaml
    /// </summary>
    public partial class FindWindow : Window
    {
        public FindWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void comboFind_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show(comboFind.Text);
        }
    }
}
