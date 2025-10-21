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

namespace UI.Views
{
    /// <summary>
    /// Interaction logic for QuizPage.xaml
    /// </summary>
    public partial class QuizPage : Page
    {
        private readonly List<Question> _questions;
        private int _currentIndex = 0;
        private readonly Dictionary<int, string> _userAnswers = new();

        public QuizPage(Frame frame, List<Question> questions)
        {
            InitializeComponent();
            _questions = questions;
            LoadQuestion();
        }
        private void LoadQuestion()
        {
            if(_questions == null || !_questions.Any())
                return;

            var q = _questions[_currentIndex];
            TitleText.Text = $"Câu {_currentIndex + 1}/{_questions.Count}";
            QuestionText.Text = q.QuestionText;

            OptionsPanel.ItemsSource = q.Choices;

            // ✅ Khôi phục đáp án đã chọn (nếu có)
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var item in OptionsPanel.Items)
                {
                    var container = OptionsPanel.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                    if (container == null) continue;

                    var rb = FindVisualChild<RadioButton>(container);
                    if (rb != null && _userAnswers.TryGetValue(_currentIndex, out var saved))
                    {
                        rb.IsChecked = (rb.Tag?.ToString() == saved);
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
        private void Option_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                _userAnswers[_currentIndex] = rb.Tag?.ToString() ?? "";
                NextButton.IsEnabled = true;
            }
        }
        private void SaveAnswer()
        {
            for (int i = 0; i < OptionsPanel.Items.Count; i++)
            {
                var container = OptionsPanel.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                if (container == null) continue;

                var radioButton = FindVisualChild<RadioButton>(container);
                if (radioButton != null && radioButton.IsChecked == true)
                {
                    _userAnswers[_currentIndex] = radioButton.Content.ToString();
                    return;
                }
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            SaveAnswer();
            if (!_userAnswers.ContainsKey(_currentIndex))
            {
                MessageBox.Show("Vui lòng chọn đáp án trước khi tiếp tục.", "Cảnh báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentIndex < _questions.Count - 1)
            {
                _currentIndex++;
                LoadQuestion();
                NextButton.IsEnabled = _userAnswers.ContainsKey(_currentIndex);
            }
        }
        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            SaveAnswer();
            if (_currentIndex > 0)
            {
                _currentIndex--;
                LoadQuestion();
            }
        }
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (_userAnswers.Count < _questions.Count)
            {
                MessageBox.Show("Bạn chưa trả lời hết các câu hỏi!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int correctCount = 0;
            for (int i = 0; i < _questions.Count; i++)
            {
                var correct = _questions[i].Choices.FirstOrDefault(c => c.IsCorrect)?.Text;
                if (_userAnswers.ContainsKey(i) && _userAnswers[i] == correct)
                    correctCount++;
            }

            MessageBox.Show(
                $"Bạn trả lời đúng {correctCount}/{_questions.Count} câu!",
                "Kết quả",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            NavigationService?.GoBack();
        }

        // Helper: tìm control con trong Visual Tree
        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild)
                    return tChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
