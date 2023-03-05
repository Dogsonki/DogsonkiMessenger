using Client.Utility;

namespace Clinet.IO;

public class FileManager
{
    public static async Task<byte[]> FileFromSelectedFile()
    {
        var image = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
        {
            Title = "Pick image"
        });

        if (image is null)
        {
            return new byte[0];
        }

        Stream stream = await image.OpenReadAsync();

        byte[] data = stream.StreamToBuffer();

        stream.Close();

        return data;
    }
}