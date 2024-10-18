﻿using CommunityToolkit.Maui.Storage;
using CustomFileOpenerAndSaver.Interfaces;
using CustomFileOpenerAndSaver.Models;

#if ANDROID

using CustomFileOpenerAndSaver.Platforms.Android;

#endif

using CustomFileOpenerAndSaver.Services;

using System.Diagnostics;
using System.Text;

namespace CustomFileOpenerAndSaver
{
    public partial class MainPage : ContentPage
    {
        // Сервис для работы с внутренней памятью
        private IInternalFilesManager _fileManagerStorage;
        private IFileSaverService _fileSaverService;

        // Текущий выбранный файл в списке
        // Пока сделал так. Думаю, в будущем это можно изменить по Blazor
        private TransferFile _selectedFile;

        public MainPage()
        {
            InitializeComponent();
            _fileManagerStorage = new InternalFilesManager();
#if ANDROID
            _fileSaverService = new FileSaverService();
#endif
        }

        // Кнопка создания файла
        private async void OnCreateFileClicked(object sender, EventArgs e)
        {
            var fileName = FileNameEntry.Text;
            var fileExtension = FileExtensionEntry.Text;
            var fileContent = FileContentEditor.Text;

            var file = new TransferFile
            {
                Name = fileName,
                Extension = fileExtension,
                Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(fileContent))
            };

            var result = await _fileManagerStorage.CreateFileAsync(file);
            

            if (result.Error != null)
            {
                await DisplayAlert("Ошибка", result.Error.Message, "OK");
            }
            else
            {
                await DisplayAlert("Успех", "Файл успешно создан", "OK");

                OnGetAllFilesClicked(sender, e);
            }

            UnfocusTextWriter();
        }


        // Кнопка получения списка файлов
        private async void OnGetAllFilesClicked(object sender, EventArgs e)
        {
#if ANDROID
            CheckPermissionManager.CheckExternalStoragePermission();
#endif

            var files = _fileManagerStorage.GetAllFileNames();
            FilesListView.ItemsSource = files;

            UnfocusTextWriter();
        }


        // Обработка выбора файла из списка
        private void OnFileSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is TransferFile selectedFile)
            {
                _selectedFile = selectedFile;
                OverwriteFileButton.IsEnabled = true;
                DeleteFileButton.IsEnabled = true;
                GetFileContentButton.IsEnabled = true;
                ShareFileButton.IsEnabled = true;
                FileSaveButton.IsEnabled = true;
                ChangeNameButton.IsEnabled = true;
            }

            UnfocusTextWriter();
        }

        // Перезапись файла из внутренней памяти
        private async void OnOverwriteFileClicked(object sender, EventArgs e)
        {
            if (_selectedFile != null)
            {
                var fileContent = FileContentEditor.Text;

                var updatedFile = new TransferFile
                {
                    Name = _selectedFile.Name,
                    Extension = _selectedFile.Extension,
                    Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(fileContent))
                };

                var result = await _fileManagerStorage.OverwriteFileAsync(updatedFile);

                if (result.Error != null)
                {
                    await DisplayAlert("Ошибка", result.Error.Message, "OK");
                }
                else
                {
                    await DisplayAlert("Успех", "Файл успешно обновлен", "OK");
                }
            }

            UnfocusTextWriter();
        }

        private async void OnChangeNameFileClicked(object sender, EventArgs e)
        {
            if (_selectedFile != null)
            {
                var fileName = FileNameEntry.Text;
                var fileExtension = FileExtensionEntry.Text;

                if (fileName == null || fileExtension == null)
                {
                    await DisplayAlert("Ошибка", "Введите название и расширение файла", "OK");
                    return;
                }

                // Проверяем, доступно ли сохранение по этому названию и расширению
                if (!_fileManagerStorage.FileExists(fileName, fileExtension))
                {

                    // Достаем файл из внутреннего хранилища
                    var internalFileWithContent = await _fileManagerStorage.GetFileContentAsync(_selectedFile);


                    // Создаем новый файл
                    var resultCreate = await _fileManagerStorage.CreateFileAsync(new TransferFile
                    {
                        Name = fileName,
                        Extension = fileExtension,
                        Content = internalFileWithContent.Content
                    });


                    // Удаляем старый файла, если смогли создать новый
                    TransferFile resultDelete = new TransferFile();
                    if (resultCreate.Error == null)
                    {
                        // Удаляем старый файл
                        resultDelete = _fileManagerStorage.DeleteFile(internalFileWithContent);
                    }
                    
                    // Вывод успешности операции
                    if (resultCreate.Error == null &&
                        resultDelete != null && resultDelete.Error == null)
                    {
                        await DisplayAlert("Успех", "Переименование прошло хорошо", "OK");
                    } else
                    {
                        await DisplayAlert("Ошибка", "Не удалось переименовать файл", "OK");
                    }

                } else
                {
                    await DisplayAlert("Ошибка", "Файл с таким названием уже есть в дата", "OK");
                }
            }

            OnGetAllFilesClicked(sender, e);
        }

        // Удаление файла из внутренней памяти
        private async void OnDeleteFileClicked(object sender, EventArgs e)
        {
            if (_selectedFile != null)
            {
                var result = _fileManagerStorage.DeleteFile(_selectedFile);

                if (result.Error != null)
                {
                    await DisplayAlert("Ошибка", result.Error.Message, "OK");
                }
                else
                {
                    await DisplayAlert("Успех", "Файл успешно удален", "OK");

                    OnGetAllFilesClicked(sender, e);
                }
            }

            UnfocusTextWriter();
        }

        // Получение содержимого файла
        private async void OnGetFileContentClicked(object sender, EventArgs e)
        {
            if (_selectedFile != null)
            {
                var result = await _fileManagerStorage.GetFileContentAsync(_selectedFile);

                if (result.Error != null)
                {
                    await DisplayAlert("Ошибка", result.Error.Message, "OK");
                }
                else
                {
                    var content = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(result.Content));
                    FileContentEditor.Text = content;
                    await DisplayAlert("Содержимое файла", content, "OK");
                }
            }

            UnfocusTextWriter();
        }

        // Отправка файла другому приложению
        private async void OnShareFileClicked(object sender, EventArgs e)
        {
            // Логика для отправки файла через Share API
            if (_selectedFile != null)
            {
                try
                {
                    if (_fileManagerStorage.FileExists(_selectedFile.Name, _selectedFile.Extension)) {
                        var fullPath = Path.Combine(FileSystem.AppDataDirectory, _selectedFile.Name + _selectedFile.Extension);

                        await Share.RequestAsync(new ShareFileRequest
                        {
                            Title = "Отправить файл",
                            File = new ShareFile(fullPath)
                        });
                    } else
                    {
                        await DisplayAlert("Ошибка", "Файл не найден", "OK");
                    }
                } catch (Exception ex)
                {
                    throw;
                }
               
            }
        }


        // Сохранения файла из внутренней памяти во внешнюю память
        private async void OnSaveFileClicked(object sender, EventArgs e)
        {
            try
            {
                if (_selectedFile != null)
                {
                    var resultFile = await _fileManagerStorage.GetFileContentAsync(_selectedFile);

                    var bytes = Convert.FromBase64String(resultFile.Content);

                    // Вызов платформенного сервиса для сохранения файла
                    await _fileSaverService.SaveFileAsync($"{_selectedFile.Name}{_selectedFile.Extension}", bytes);

                    await DisplayAlert("", "Файл сохранен", "OK");

                }
            } catch
            {
                throw;
            }
        }





        // Открытие файла из внешнего хранилища
        private async void OnFileOpenButtonExternalStorage(object sender, EventArgs e)
        {
            try
            {
                var filePickResult = await FilePicker.PickAsync();

                if (filePickResult != null)
                {

                    await DisplayAlert("", "Файл успешно открыт", "OK");

                } else
                {
                    await DisplayAlert("", "Не удалось открыть файл", "OK");
                }

                //string fileContent = await OpenFileFromMediaStore("Hg.tdbkp", "Ridan");

            } catch
            {
                throw;
            }
        }



        // После ввода текста и выполнения действия сделал грубое скрытие клавиатуры
        private void UnfocusTextWriter()
        {
            FileNameEntry.Unfocus();
            FileExtensionEntry.Unfocus();
            FileContentEditor.Unfocus();
        }
    }

}
