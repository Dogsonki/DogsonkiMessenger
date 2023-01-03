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

    public static string ToJSImageSource(byte[] source)
    {
        if(source is null)
        {
            throw new ArgumentNullException("source");
        }

        string base64 = Convert.ToBase64String(source);
        return string.Format("data:image/gif;base64,{0}", base64);
    }
}