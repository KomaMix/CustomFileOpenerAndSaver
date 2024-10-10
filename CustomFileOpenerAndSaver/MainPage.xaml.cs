using System.Diagnostics;

namespace CustomFileOpenerAndSaver
{
    public partial class MainPage : ContentPage
    {
        private string fileExtension = ".tdbkp"; // Указываем нужное расширение для двоичных файлов

        public MainPage()
        {
            InitializeComponent();
        }

        // Обработка нажатия на кнопку "Добавить двоичный файл"
        private async void OnAddBinaryFileClicked(object sender, EventArgs e)
        {
            try
            {
                // Сохранение файла во внутреннюю память
                string appDirectory = FileSystem.Current.AppDataDirectory;
                string fileName = $"binaryfile{Guid.NewGuid()}{fileExtension}";
                string filePath = Path.Combine(appDirectory, fileName);

                // Генерация двоичных данных
                byte[] binaryData = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 };

                // Запись данных в файл
                await File.WriteAllBytesAsync(filePath, binaryData);

                await DisplayAlert("Файл сохранен", $"Файл {fileName} успешно сохранен.", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при сохранении двоичного файла: {ex.Message}");
            }
        }

        // Обработка нажатия на кнопку "Выбрать двоичный файл"
        private async void OnSelectBinaryFileClicked(object sender, EventArgs e)
        {
            try
            {
                string appDirectory = FileSystem.Current.AppDataDirectory;

                // Получаем список всех двоичных файлов с заданным расширением
                var files = Directory.GetFiles(appDirectory, $"*{fileExtension}");

                if (files.Length == 0)
                {
                    await DisplayAlert("Файлы не найдены", $"Файлы с расширением {fileExtension} не найдены.", "OK");
                    return;
                }

                // Обновляем ListView для отображения списка файлов
                fileListView.ItemsSource = files.Select(f => Path.GetFileName(f)).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при выборе двоичного файла: {ex.Message}");
            }
        }

        // Обработка выбора файла из списка
        private async void OnFileSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            string selectedFileName = e.SelectedItem.ToString();
            string appDirectory = FileSystem.Current.AppDataDirectory;
            string filePath = Path.Combine(appDirectory, selectedFileName);

            try
            {
                // Чтение содержимого двоичного файла
                byte[] fileContent = await File.ReadAllBytesAsync(filePath);

                // Преобразуем двоичные данные в строку для примера (например, в шестнадцатеричный вид)
                string contentPreview = BitConverter.ToString(fileContent);

                // Вывод всплывающего окна с названием файла и содержимым (в шестнадцатеричном виде)
                await DisplayAlert("Файл открыт", $"Вы открыли файл: {selectedFileName}\nСодержимое файла:\n{contentPreview}", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при открытии двоичного файла: {ex.Message}");
            }

            // Снимаем выделение с элемента списка
            ((ListView)sender).SelectedItem = null;
        }
    }

}
