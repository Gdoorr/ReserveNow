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
        // ��������� �������� ��� ���������� ������
        EnteredPassword = null;
        Navigation.PopModalAsync();
    }

    private void OnConfirmClicked(object sender, EventArgs e)
    {
        // ��������� ��������� ������ � ��������� ��������
        EnteredPassword = PasswordEntry.Text?.Trim();
        Navigation.PopModalAsync();
    }
}