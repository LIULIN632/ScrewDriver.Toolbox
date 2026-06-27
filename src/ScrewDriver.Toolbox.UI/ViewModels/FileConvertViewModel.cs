using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class FileConvertViewModel : BaseViewModel
{
    public ObservableCollection<DroppedFile> FileList { get; set; } = new();
    public ICommand ConvertCommand { get; }

    public FileConvertViewModel()
    {
        ConvertCommand = new RelayCommand<string>(ConvertImages);
    }

    public void AddFiles(string[] files)
    {
        foreach (var file in files)
        {
            var ext = Path.GetExtension(file).ToLower();
            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp")
            {
                var info = new FileInfo(file);
                FileList.Add(new DroppedFile
                {
                    FilePath = file,
                    SizeKB = (info.Length / 1024).ToString() + " KB"
                });
            }
        }
    }

    private void ConvertImages(string? targetFormat)
    {
        if (string.IsNullOrEmpty(targetFormat)) return;
        foreach (var file in FileList)
        {
            string outputPath = Path.Combine(
                Path.GetDirectoryName(file.FilePath) ?? "",
                Path.GetFileNameWithoutExtension(file.FilePath) + "." + targetFormat.ToLower()
            );
            try
            {
                using var stream = new FileStream(file.FilePath, FileMode.Open);
                var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
                var frame = decoder.Frames[0];
                BitmapEncoder encoder = targetFormat switch
                {
                    "PNG" => new PngBitmapEncoder(),
                    "JPG" => new JpegBitmapEncoder(),
                    "BMP" => new BmpBitmapEncoder(),
                    _ => new PngBitmapEncoder()
                };
                encoder.Frames.Add(frame);
                using var outStream = new FileStream(outputPath, FileMode.Create);
                encoder.Save(outStream);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"转换 {file.FilePath} 失败: {ex.Message}");
            }
        }
        System.Windows.MessageBox.Show("批量转换完成！");
        FileList.Clear();
    }
}

public class DroppedFile
{
    public string FilePath { get; set; } = "";
    public string SizeKB { get; set; } = "";
}
