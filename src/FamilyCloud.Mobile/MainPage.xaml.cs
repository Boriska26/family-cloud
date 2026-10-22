namespace FamilyCloud.Mobile;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object? sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Нажал {count} time";
		else
			CounterBtn.Text = $"Нажал {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
}
