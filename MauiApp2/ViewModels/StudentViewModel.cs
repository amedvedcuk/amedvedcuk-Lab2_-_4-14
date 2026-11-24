using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiApp2.Models;

namespace MauiApp2.ViewModels;

public class StudentViewModel : INotifyPropertyChanged, IQueryAttributable
{
    private readonly Student _student = new();
    private readonly Command _addStudentCommand;

    public ObservableCollection<Student> Students { get; } = new();

    public StudentViewModel()
    {
        _addStudentCommand = new Command(AddStudent, CanAddStudent);
        OpenDetailsCommand = new Command<Student>(async (student) => await OpenDetailsAsync(student));
    }

    public string FullName
    {
        get => _student.FullName;
        set
        {
            if (_student.FullName != value)
            {
                _student.FullName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SummaryText));
                _addStudentCommand.ChangeCanExecute();
            }
        }
    }

    public string Group
    {
        get => _student.Group;
        set
        {
            if (_student.Group != value)
            {
                _student.Group = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SummaryText));
            }
        }
    }

    public double AverageScore
    {
        get => _student.AverageScore;
        set
        {
            if (Math.Abs(_student.AverageScore - value) > 0.0001)
            {
                _student.AverageScore = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ScoreIndicatorColor));
                OnPropertyChanged(nameof(SummaryText));
            }
        }
    }

    public string SummaryText =>
        $"Студент: {(string.IsNullOrWhiteSpace(FullName) ? "—" : FullName)}, " +
        $"група {(string.IsNullOrWhiteSpace(Group) ? "—" : Group)}";

    public Color ScoreIndicatorColor =>
        AverageScore >= 4.0 ? Colors.ForestGreen : Colors.IndianRed;

    public ICommand AddStudentCommand => _addStudentCommand;
    public ICommand OpenDetailsCommand { get; }

    private bool CanAddStudent() => !string.IsNullOrWhiteSpace(FullName);

    private void AddStudent()
    {
        Students.Add(new Student
        {
            FullName = FullName.Trim(),
            Group = Group.Trim(),
            AverageScore = Math.Round(AverageScore, 2)
        });

        FullName = string.Empty;
        Group = string.Empty;
        AverageScore = 0;
    }

    private async Task OpenDetailsAsync(Student? student)
    {
        if (student is null)
            return;

        var parameters = new Dictionary<string, object>
        {
            { "SelectedStudent", student }
        };

        await Shell.Current.GoToAsync("studentdetail", parameters);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("UpdatedStudent", out var updatedObj) && updatedObj is Student updated)
        {
            if (query.TryGetValue("OriginalStudent", out var origObj) && origObj is Student original)
            {
                var index = Students.IndexOf(original);
                if (index >= 0)
                {
                    Students[index] = updated;
                    return;
                }
            }

            var existing = Students.FirstOrDefault(s => s.FullName == updated.FullName);
            if (existing is not null)
            {
                var idx = Students.IndexOf(existing);
                Students[idx] = updated;
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}