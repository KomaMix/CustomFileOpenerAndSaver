using CustomFileOpenerAndSaver.Models;
using CustomFileOpenerAndSaver.Services;
using System.Diagnostics;
using System.Text;

namespace CustomFileOpenerAndSaver
{
    public partial class MainPage : ContentPage
    {
        private readonly FileProcessingService _fileProcessingService = new FileProcessingService(); // Инициализируем сервис

        public MainPage()
        {
            InitializeComponent();
        }

        // Обработка нажатия на кнопку "Сохранить файл"
        private async void OnSaveFileClicked(object sender, EventArgs e)
        {
            // Получаем данные из полей ввода
            string fileName = fileNameEntry.Text;
            string fileContent = fileContentEditor.Text;

            if (string.IsNullOrWhiteSpace(fileContent))
            {
                await DisplayAlert("Ошибка", "Содержимое файла не может быть пустым", "OK");
                return;
            }

            try
            {
                // Преобразуем содержимое файла в Base64
                string base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent));

                // Создаем объект TransferFile
                TransferFile fileToSave = new TransferFile
                {
                    Name = fileName, // Если имя пустое, то будет сгенерировано в сервисе
                    Content = base64Content,
                    Extension = ".tdbkp" // Фиксированное расширение
                };

                // Сохраняем файл через сервис
                TransferFile savedFile = await _fileProcessingService.SaveFile(fileToSave);

                if (savedFile.Error == null)
                {
                    await DisplayAlert("Файл сохранен", $"Файл {savedFile.Name} успешно сохранен.", "OK");
                }
                else
                {
                    await DisplayAlert("Ошибка", $"Не удалось сохранить файл: {savedFile.Error.Message}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка при сохранении файла: {ex.Message}", "OK");
            }
        }

        // Обработка нажатия на кнопку "Выбрать двоичный файл"
        private async void OnSelectBinaryFileClicked(object sender, EventArgs e)
        {
            try
            {
                TransferFile searchCriteria = new TransferFile { Extension = ".tdbkp" };

                // Получаем список файлов с помощью сервиса
                string[] files = await _fileProcessingService.GetFiles(searchCriteria);

                if (files.Length == 0)
                {
                    await DisplayAlert("Файлы не найдены", $"Файлы с расширением {searchCriteria.Extension} не найдены.", "OK");
                    return;
                }

                // Обновляем ListView для отображения списка файлов
                fileListView.ItemsSource = files.Select(f => Path.GetFileName(f)).ToList();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка при выборе файлов: {ex.Message}", "OK");
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
                // Открываем файл через сервис
                TransferFile fileToOpen = new TransferFile { Path = filePath };
                TransferFile openedFile = await _fileProcessingService.OpenFile(fileToOpen);

                if (openedFile.Error == null)
                {
                    // Преобразуем содержимое обратно из Base64
                    string fileContent = Encoding.UTF8.GetString(Convert.FromBase64String(openedFile.Content));

                    // Вывод всплывающего окна с названием и содержимым файла
                    await DisplayAlert("Файл открыт", $"Вы открыли файл: {openedFile.Name}\nСодержимое файла:\n{fileContent}", "OK");
                }
                else
                {
                    await DisplayAlert("Ошибка", $"Не удалось открыть файл: {openedFile.Error.Message}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка при открытии файла: {ex.Message}", "OK");
            }

            // Снимаем выделение с элемента списка
            ((ListView)sender).SelectedItem = null;
        }
    }

}
