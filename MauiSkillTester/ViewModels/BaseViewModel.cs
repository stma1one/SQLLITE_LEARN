using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiSkillTester.ViewModels;

/// <summary>
/// מחלקת בסיס לכל ה-ViewModels המממשת את ממשק INotifyPropertyChanged
/// ומאפשרת עדכון דו-כיווני של המסך (Data Binding).
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    private bool _isLoading;
    private string _title = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// האם מתבצעת פעולת טעינה ברקע (עבור מחוון טעינה ActivityIndicator)
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotLoading));
            }
        }
    }

    public bool IsNotLoading => !IsLoading;

    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged();
            }
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
