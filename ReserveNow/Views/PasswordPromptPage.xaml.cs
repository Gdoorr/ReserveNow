namespace ReserveNow.Views;

public partial class PasswordPromptPage : ContentPage
{
    public string EnteredPassword { get; private set; }
    public PasswordPromptPage()
	{
		InitializeComponent();
	}
    private void OnCancelClicked(object sender, EventArgs e)
    {
        // Закрываем страницу без сохранения пароля
        EnteredPassword = null;
        Navigation.PopModalAsync();
    }

    private void OnConfirmClicked(object sender, EventArgs e)
    {
        // Сохраняем введенный пароль и закрываем страницу
        EnteredPassword = PasswordEntry.Text?.Trim();
        Navigation.PopModalAsync();
    }
}