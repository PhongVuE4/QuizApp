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
using UI.Services;

namespace UI.Views.Pages
{
    /// <summary>
    /// Interaction logic for QuestionListPage.xaml
    /// </summary>
    public partial class QuestionListPage : Page
    {
        private readonly Frame _frame;
        private readonly VietNamese _vietnamese = new VietNamese();
        private readonly Dictionary<string, (string Class, string Subject, List<Question> Questions)> _questionSets;
        private List<Question> _selectedSet;

        private string _selectedClass = "";
        private string _selectedSubject = "";
        public QuestionListPage(Frame frame, List<Question> questions)
        {
            InitializeComponent();
            _frame = frame;
            _vietnamese = new VietNamese();

            _questionSets = questions
                .GroupBy(q => $"{q.Class}-{q.Subject}")
                .ToDictionary(
                    g => g.Key,
                    g => (Class: g.First().Class, Subject: g.First().Subject, Questions: g.ToList())
                );
            LoadFilters();

            // Thêm event search gợi ý
            ClassComboBox.IsEditable = true;
            SubjectComboBox.IsEditable = true;
            ClassComboBox.PreviewTextInput += ComboBox_PreviewTextInput;
            SubjectComboBox.PreviewTextInput += ComboBox_PreviewTextInput;
            ClassComboBox.KeyUp += ComboBox_KeyDown;
            SubjectComboBox.KeyUp += ComboBox_KeyDown;

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
            QuestionSetPanel.Children.Clear();
            var filteredSets = _questionSets.Values.AsEnumerable();

            if (!string.IsNullOrEmpty(_selectedClass))
                filteredSets = filteredSets.Where(q => q.Class == _selectedClass);

            if (!string.IsNullOrEmpty(_selectedSubject))
                filteredSets = filteredSets.Where(q => q.Subject == _selectedSubject);

            foreach (var set in filteredSets)
            {
                var btn = new Button
                {
                    Content = $"{set.Subject}\n {set.Class}\n🧮 {set.Questions.Count} câu hỏi",
                    Tag = $"{set.Class}-{set.Subject}",
                    Width = 200,
                    Height = 100,
                    Margin = new Thickness(10),
                    Background = Brushes.White,
                    BorderBrush = Brushes.LightGray,
                    Cursor = Cursors.Hand
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
        //private void LoadQuestions()
        //{
        //    if (_selectedSet == null) return;

        //    QuestionList.Visibility = Visibility.Visible;
        //    QuestionList.ItemsSource = _selectedSet
        //        .Select((q, i) => new { QuestionText = $"{i + 1}. {q.QuestionText}" })
        //        .ToList();
        //}

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
        private void ComboBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null || _questionSets == null) return;

            string input = combo.Text + e.Text;
            string normalized = _vietnamese.RemoveDiacritics(input.Trim().ToLower());

            var allItems = (combo == ClassComboBox)
                ? _questionSets.Values.Select(q => q.Class).Distinct().OrderBy(s => s).ToList()
                : _questionSets.Values.Select(q => q.Subject).Distinct().OrderBy(s => s).ToList();

            var filtered = allItems
                .Where(i => _vietnamese.RemoveDiacritics(i.ToLower()).Contains(normalized))
                .ToList();

            combo.ItemsSource = filtered;
            combo.IsDropDownOpen = true;

            // Giữ caret ở cuối
            if (combo.Template.FindName("PART_EditableTextBox", combo) is TextBox tb)
            {
                tb.SelectionStart = combo.Text.Length;
                tb.SelectionLength = 0;
            }
        }

        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null) return;

            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                var list = combo.ItemsSource?.Cast<string>().ToList();
                if (list != null && list.Any())
                {
                    // Chọn item đầu tiên nếu có gợi ý
                    var first = list.FirstOrDefault();
                    if (first != null)
                    {
                        combo.SelectedItem = first;
                        combo.Text = first;
                    }
                }

                combo.IsDropDownOpen = false;
                e.Handled = true;

                // Cho phép Tab chuyển focus tự nhiên
                TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
                (combo as UIElement)?.MoveFocus(request);
            }
        }
    }
}
