/*
 Создать приложение «Книга кулинарных рецептов».
Основная задача проекта – создание и использования кулинарных рецептов.
Интерфейс приложения должен предоставлять такие возможности:
■■ Создавать кулинарный рецепт. Рецепт может содержать информацию о
компонентах рецепта, изображения, пошаговую инструкцию по созданию
блюда и т. д. При создании рецепта нужно сохранять: название рецепта,
тип рецепта (первые блюда, салаты и т. д.), название кухни, к которой от-
носится данный рецепт (французская, китайская и т. д.).
■■ Удаление/изменение рецепта.
■■ Каталог рецептов по категориям. Например, первые блюда, салаты и т. д.
■■ Экспорт конкретного рецепта в файл формата: .doc, .docx, .pdf.
■■ Поиск рецептов по набору параметров. Например, по названию, по ком-
понентам, по названию кухни и т. д.
 */
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Image = System.Drawing.Image;
using System.Runtime.Serialization.Formatters.Binary;

namespace CookBook
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string connectionString;
        SqlDataAdapter adapter;
        DataTable bCookTable;
        string fileName = "";
        SqlConnection connection = null;
        //переменные для заполнени формы ОТКРЫТЬ
        string ingredients = "";
        string cooking = "";
        string ID = "";
        string title = "";
        BitmapImage picture;
        public MainWindow()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }
        private void UpdateDB() //ф-я обновления
        {
            try
            {
                SqlCommandBuilder comandbuilder = new SqlCommandBuilder(adapter);
                adapter.Update(bCookTable);
                System.Windows.Forms.MessageBox.Show("Изменения сохранены!");
            }
            catch(Exception ex)
            {

            }
        }
        //ф-я удаления из базы
        private void Delete()
        {
            if (bCookGrid.SelectedItems != null)
            {
                for (int i = 0; i < bCookGrid.SelectedItems.Count; i++)
                {
                    DataRowView datarowView = bCookGrid.SelectedItems[i] as DataRowView;
                    if (datarowView != null)
                    {
                        DataRow dataRow = (DataRow)datarowView.Row;
                        dataRow.Delete();
                    }
                }
            }
            UpdateDB();
        }
        //удаление
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            Delete();
        }
        //кнопка обновить/сохранить
        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateDB();
        }
        //ф-я загрузки содержимого  из базы данных
        private void LOAD()
        {
            string sql = "SELECT * FROM BCook";
            bCookTable = new DataTable();

            try
            {
                connection = new SqlConnection(connectionString);
                SqlCommand command = new SqlCommand(sql, connection);
                adapter = new SqlDataAdapter(command);

                // установка команды на добавление для вызова хранимой процедуры
                adapter.InsertCommand = new SqlCommand("InsertRecept", connection);
                adapter.InsertCommand.CommandType = CommandType.StoredProcedure;
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@dishName", SqlDbType.NVarChar, 2147483647, "DishName"));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@ingredients", SqlDbType.NVarChar, 2147483647, "Ingredients"));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@coocking", SqlDbType.NVarChar, 2147483647, "Cooking"));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@typeDish", SqlDbType.NVarChar, 50, "TypeDish"));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@kitchen", SqlDbType.NVarChar, 50, "Kitchen"));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@portions", SqlDbType.Int, 0, "Portions"));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@timeForPrep", SqlDbType.Int, 0, "TimeForPrep"));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@notes", SqlDbType.NVarChar, 2147483647, "Notes"));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@picture", SqlDbType.NVarChar, 2147483647, "Picture"));
                SqlParameter parameter = adapter.InsertCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");

                parameter.Direction = ParameterDirection.Output;
                connection.Open();
                adapter.Fill(bCookTable);
                bCookGrid.ItemsSource = bCookTable.DefaultView;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }
        //загрузка главного окна
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            SplashScreen splash = new SplashScreen("SplashScreen1.png");
            splash.Show(false);
            splash.Close(TimeSpan.FromSeconds(5.0));
            LOAD();
        }
        //ф-я загрузки картинки
        private void LoadPicture()
        {
            try
            {
                byte[] imageData;

                // Create the byte array.
                var originalImage = Image.FromFile(fileName);
                using (var ms = new MemoryStream())
                {
                    originalImage.Save(ms, ImageFormat.Jpeg);
                    imageData = ms.ToArray();
                }
                connection.Open();
                SqlCommand comm = new SqlCommand(
                    "UPDATE BCook SET Picture = @Picture, Pic_path = @pic_path WHERE ID = @id;", connection
                    );
                if (ID == null || ID.Length == 0) return;
                int index = -1;
                int.TryParse(ID, out index);
                if (index == -1) return;
                comm.Parameters.Add("@id", SqlDbType.Int).Value = index;
                string picturePath = $"resources/{ID}.jpg";
                comm.Parameters.Add("@Picture", SqlDbType.NVarChar, picturePath.Length).Value = picturePath;
                comm.Parameters.Add("@pic_path", SqlDbType.NVarChar, picturePath.Length).Value = picturePath;
                comm.ExecuteNonQuery();
                connection.Close();
                // Convert back to image.
                using (var ms = new MemoryStream(imageData))
                {
                    Image image = Image.FromStream(ms);
                    image.Save($"..\\..\\resources\\{ID}.jpg");
                }

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {
                if (connection != null) connection.Close();
            }
        }
        //ф-я ОКТРЫТЬ
        private void OPEN()
        {
            if (bCookGrid.SelectedItems != null)
            {
                WindowRecept winRecept = new WindowRecept(ingredients, cooking, title, picture);
                winRecept.ShowDialog();
                if (winRecept.DialogResult == true)
                {
                    winRecept.Close();
                }
            }
        }

        //кнопка ОТКРЫТЬ
        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            OPEN();
        }
        //событие изменения выбора в списках рецептов
        private void bCookGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object item = bCookGrid.SelectedItem;
            try
            {
                ID = (bCookGrid.SelectedCells[0].Column.GetCellContent(item) as TextBlock).Text;
                ingredients = (bCookGrid.SelectedCells[2].Column.GetCellContent(item) as TextBlock).Text;
                cooking = (bCookGrid.SelectedCells[3].Column.GetCellContent(item) as TextBlock).Text;
                title = (bCookGrid.SelectedCells[1].Column.GetCellContent(item) as TextBlock).Text;
                if (String.IsNullOrEmpty((bCookGrid.SelectedCells[10].Column.GetCellContent(item) as TextBlock).Text))
                {
                    picture = new BitmapImage();
                    picture.BeginInit();
                    picture.UriSource = new Uri("..\\..\\resources\\logo.png", UriKind.Relative);
                    picture.EndInit();
                }
                else
                {
                    picture = new BitmapImage();
                    picture.BeginInit();
                    picture.UriSource = new Uri($"..\\..\\resources\\{ID}.jpg", UriKind.Relative);
                    picture.EndInit();
                }
            }
            catch(Exception ex)
            {

            }
        }
        //ф-я загрузки картинки
        private void LOAD_PICTURE()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Graphics File|*.bmp;*.gif;*.jpg; *.png";
            ofd.FileName = "";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileName = ofd.FileName;
                LoadPicture();
            }
            UpdateDB();
            LOAD();
        }
        //кнопка загрузки картики
        private void loadpicButton_Click(object sender, RoutedEventArgs e)
        {
            LOAD_PICTURE();
        }
        //ф-я поиска
        private void FIND()
        {
            bCookGrid.SelectedIndex = -1;
            FindWindow fw = new FindWindow();
            fw.ShowDialog();
            if (fw.DialogResult == false)
            {
                fw.Close();
            }
            if (fw.DialogResult == true)
            {
                string columName = "";
                switch (fw.comboFind.Text)
                {
                    case "ID":
                        columName = "ID";
                        break;
                    case "Название":
                        columName = "DishName";
                        break;
                    case "Состав":
                        columName = "Ingredients";
                        break;
                    case "Приготовление":
                        columName = "Cooking";
                        break;
                    case "Вид":
                        columName = "TypeDish";
                        break;
                    case "Кухня":
                        columName = "Kitchen";
                        break;
                    case "Порций":
                        columName = "Portions";
                        break;
                    case "Время":
                        columName = "TimeForPrep";
                        break;
                    case "Примечания":
                        columName = "Notes";
                        break;
                }

                string sql = "SELECT * FROM BCook WHERE " + columName + " LIKE '%" + fw.findText.Text + "%';";
                bCookTable = new DataTable();

                try
                {
                    connection = new SqlConnection(connectionString);
                    SqlCommand command = new SqlCommand(sql, connection);
                    adapter = new SqlDataAdapter(command);

                    // установка команды на добавление для вызова хранимой процедуры
                    adapter.InsertCommand = new SqlCommand("InsertRecept", connection);
                    adapter.InsertCommand.CommandType = CommandType.StoredProcedure;
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@dishName", SqlDbType.NVarChar, 2147483647, "DishName"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@ingredients", SqlDbType.NVarChar, 2147483647, "Ingredients"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@coocking", SqlDbType.NVarChar, 2147483647, "Cooking"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@typeDish", SqlDbType.NVarChar, 50, "TypeDish"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@kitchen", SqlDbType.NVarChar, 50, "Kitchen"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@portions", SqlDbType.Int, 0, "Portions"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@timeForPrep", SqlDbType.Int, 0, "TimeForPrep"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@notes", SqlDbType.NVarChar, 2147483647, "Notes"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@picture", SqlDbType.VarBinary, 2147483647, "Picture"));
                    SqlParameter parameter = adapter.InsertCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");

                    parameter.Direction = ParameterDirection.Output;
                    connection.Open();
                    adapter.Fill(bCookTable);
                    bCookGrid.ItemsSource = bCookTable.DefaultView;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (connection != null)
                        connection.Close();
                }
            }
        }
        //кнопка поиска
        private void findButton_Click(object sender, RoutedEventArgs e)
        {
            FIND();
        }
        //кнопка загрузить все
        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            LOAD();
        }
        //Спарвка, "О программе"
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show(@"Это книга кулинарных рецептов 'CookBook ver. 1'. 
Разработана в рамках учебной программы Компьютерной Академии ШАГ в ознакомительных целях с WPF. 
Использовался язык программирования С# и Microsoft Visual Studio Enterprise 2019.
Программист - Дьяконов Кирилл Владимирович, руководитель проекта - Горчинский Сергей Владимирович.
г. Чернигов март 2021 год. Эта программа для бесплатного ознакомительного распространения.
Для связи - alkiddkv@gmail.com
Использование:
--- Для программы необходимо развернуть базу данных BookCook(идет в комплекте);
--- Добавление нового рецепта производится путем записи в новых строках;
--- Остальное управление производится либо через меню либо кнопками управления;", "О программе");
        }
        //Выход из программы
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        //меню "Показать все"
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            LOAD();
        }
        // меню "ОБновить/Сохранить"
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            UpdateDB();
        }
        // меню "ОТКРЫТЬ"
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            OPEN();
        }
        // меню "Найти"
        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            FIND();
        }
        // меню "Добавить фото"
        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            LOAD_PICTURE();
        }
        // меню "Удалить"
        private void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {
            Delete();
        }
    }
}
