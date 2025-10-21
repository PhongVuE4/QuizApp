using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UI.Views.Pages
{
    /// <summary>
    /// Interaction logic for QuestionListPage.xaml
    /// </summary>
    public partial class QuestionListPage : Page
    {
        private readonly Frame _frame;
        private readonly Dictionary<string, (string Class, string Subject, List<Question> Questions)> _questionSets;
        private List<Question> _selectedSet;

        private string _selectedClass = "";
        private string _selectedSubject = "";
        public QuestionListPage(Frame frame, List<Question> questions)
        {
            InitializeComponent();
            _frame = frame;

            _questionSets = questions
                .GroupBy(q => $"{q.Class}-{q.Subject}")
                .ToDictionary(
                    g => g.Key,
                    g => (Class: g.First().Class, Subject: g.First().Subject, Questions: g.ToList())
                );
            LoadFilters();
            RenderCards();
        }
        private void LoadFilters()
        {
            // Tự động sinh danh sách từ dữ liệu thật
            var classes = _questionSets.Values.Select(q => q.Class).Distinct().OrderBy(c => c).ToList();
            var subjects = _questionSets.Values.Select(q => q.Subject).Distinct().OrderBy(s => s).ToList();

            ClassComboBox.ItemsSource = classes;
            SubjectComboBox.ItemsSource = subjects;

            if (!string.IsNullOrEmpty(_selectedClass))
                ClassComboBox.Text = _selectedClass;
            if (!string.IsNullOrEmpty(_selectedSubject))
                SubjectComboBox.Text = _selectedSubject;
        }
        private void RenderCards()
        {
            foreach (var set in _questionSets)
            {
                var btn = new Button
                {
                    Content = $"{set.Value.Subject}\nLớp {set.Value.Class}\n🧮 {set.Value.Questions.Count} câu hỏi",
                    Tag = set.Key,
                    Width = 200,
                    Height = 100,
                    Margin = new Thickness(10),
                    Background = Brushes.White,
                    BorderBrush = Brushes.LightGray,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                btn.Click += Card_Click;
                QuestionSetPanel.Children.Add(btn);
            }
        }

        private void Card_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var key = button.Tag.ToString();

            _selectedSet = _questionSets[key].Questions;
            foreach (Button b in QuestionSetPanel.Children)
                b.Background = Brushes.White;

            button.Background = new SolidColorBrush(Color.FromRgb(220, 248, 220));
            StartButton.IsEnabled = true;
        }
        private void LoadQuestions()
        {
            if (_selectedSet == null) return;

            QuestionList.Visibility = Visibility.Visible;
            QuestionList.ItemsSource = _selectedSet
                .Select((q, i) => new { QuestionText = $"{i + 1}. {q.QuestionText}" })
                .ToList();
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedClass = "";
            _selectedSubject = "";
            ClassComboBox.SelectedIndex = -1;
            SubjectComboBox.SelectedIndex = -1;
            RenderCards();
            QuestionList.Visibility = Visibility.Collapsed;
        }

        private void ClassComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedClass = ClassComboBox.SelectedItem?.ToString() ?? "";
            RenderCards();
        }

        private void SubjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedSubject = SubjectComboBox.SelectedItem?.ToString() ?? "";
            RenderCards();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSet == null) return;
            _frame.Navigate(new QuizPage(_frame, _selectedSet));
        }

    }
}
