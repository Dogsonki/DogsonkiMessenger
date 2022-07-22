using Client.Utility;

namespace Client.Pages.TemporaryPages;

public partial class ChangingAvatarPage : ContentPage
{
    public ChangingAvatarPage()
    {
        InitializeComponent();
    }

    public async Task Init()
    {
        byte[] ImageBuffer;
        var image = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
        {
            Title = "Pick new avatar"
        });

        Stream stream = await image.OpenReadAsync();
        ImageBuffer = Essential.StreamToBuffer(stream, stream.Length);

        ImageSource src = ImageSource.FromStream(() => new MemoryStream(ImageBuffer));

    }
}