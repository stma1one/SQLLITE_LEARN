using MauiSkillTester.Services;
using MauiSkillTester.ViewModels;

namespace MauiSkillTester
{
	public partial class MainPage : ContentPage
	{
		/// <summary>
		/// בנאי ברירת מחדל המאפשר ל-MAUI DataTemplate ב-AppShell ליצור את הדף ללא קריסה
		/// </summary>
		public MainPage() : this(new MainViewModel(new SQLiteDatabaseService()))
		{
		}

		/// <summary>
		/// בנאי ראשי המקבל את ה-MainViewModel דרך Dependency Injection
		/// </summary>
		public MainPage(MainViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			if (BindingContext is MainViewModel viewModel)
			{
				await viewModel.LoadDataAsync();
			}
		}
	}
}
