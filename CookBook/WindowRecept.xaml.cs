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
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Reflection;
using FilePath = System.IO.Path;

namespace CookBook
{
    /// <summary>
    /// Логика взаимодействия для WindowRecept.xaml
    /// </summary>
    public partial class WindowRecept : Window
    {
        public WindowRecept(string TextforIngredients, string TextforCoocking, string TextTitle, BitmapImage image)
        {
            InitializeComponent();
            ingredients.Text = TextforIngredients;
            recept.Text = TextforCoocking;
            img.Source = image;
            Title = TextTitle;
        }

        private void OKbtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        // кнопка "Сохранить" сохраняет сразу в двух форматах *.txt & *.pdf
        // подключена библиотека iTextSharp через NuGet для сохранения в pdf формате
        private void SAVEbtn_Click(object sender, RoutedEventArgs e)
        {
            //tabSelInd = tabControl1.SelectedIndex;
            SaveFileDialog save = new SaveFileDialog();
            save.DefaultExt = "txt";
            save.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(save.FileName);
                writer.WriteLine(Title);
                writer.WriteLine("------------------------");
                writer.WriteLine("Ингредиенты: ");
                writer.WriteLine(ingredients.Text);
                writer.WriteLine("Приготовление: ");
                writer.WriteLine(recept.Text);
                writer.Close();
                //--------------------------saving in pdf
                //Объект документа пдф
                iTextSharp.text.Document doc = new iTextSharp.text.Document();

                //Создаем объект записи пдф-документа в файл
                PdfWriter.GetInstance(doc, new FileStream(save.FileName + ".pdf", FileMode.Create));

                //Открываем документ
                doc.Open();

                //Определение шрифта необходимо для сохранения кириллического текста
                //Иначе мы не увидим кириллический текст
                //Если мы работаем только с англоязычными текстами, то шрифт можно не указывать
                BaseFont baseFont = BaseFont.CreateFont("..\\..\\arial.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                iTextSharp.text.Font font = new iTextSharp.text.Font(baseFont, iTextSharp.text.Font.DEFAULTSIZE, iTextSharp.text.Font.NORMAL);

                //Создаем объект таблицы и передаем в нее число столбцов таблицы из нашего датасета
                PdfPTable table = new PdfPTable(1);

                //Добавим в таблицу общий заголовок
                PdfPCell cell = new PdfPCell(new Phrase("Блюдо: " + Title, font));
                cell.HorizontalAlignment = 1;
                //Убираем границу первой ячейки, чтобы была как заголовок
                cell.Border = 0;
                table.AddCell(cell);
                //Добавляем все остальные ячейки
                table.AddCell(new Phrase("Ингредиенты: ", font));
                table.AddCell(new Phrase(ingredients.Text, font));
                table.AddCell(new Phrase("Приготовление: ", font));
                table.AddCell(new Phrase(recept.Text, font));
                //Добавляем таблицу в документ
                doc.Add(table);
                //}
                //Закрываем документ
                doc.Close();

                System.Windows.Forms.MessageBox.Show("Pdf-документ сохранен");
            }
        }
    }
}
