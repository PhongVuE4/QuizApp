using Core.Models;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
using System.Windows.Threading;

namespace UI.Views.Pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private readonly Frame _frame;
        private readonly VietNamese _vietNamese = new VietNamese();
        private List<Question> _allQuestions = new();
        private readonly string _dataDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        private readonly string _dataPath;

        public HomePage(Frame frame)
        {
            InitializeComponent();
            _frame = frame;
            _dataPath = System.IO.Path.Combine(_dataDir, "questions.json");
            Loaded += async (s, e) => await InitializeModeAsync();
        }

        private async Task InitializeModeAsync()
        {
            // Disable các nút trong khi đang kiểm tra
            OnlineModeRadio.IsEnabled = false;
            OfflineModeRadio.IsEnabled = false;
            RefreshButton.IsEnabled = false;

            LogTextBlock.Text = "⏳ Đang kiểm tra kết nối máy chủ...";
            await Dispatcher.Yield(DispatcherPriority.Background);  // ⚡ ép UI update ngay
                                                                    // hoặc: await Task.Delay(50);
            bool apiAvailable = false;
            try
            {
                apiAvailable = await CheckApiAvailableAsync();
            }
            catch (Exception ex)
            {
                LogTextBlock.Text = $"❌ Lỗi khi kiểm tra API: {ex.Message}";
            }

            // Bật lại lựa chọn sau khi kiểm tra
            OnlineModeRadio.IsEnabled = true;
            OfflineModeRadio.IsEnabled = true;

            if (apiAvailable)
            {
                OnlineModeRadio.IsChecked = true;
                RefreshButton.IsEnabled = true;
                LogTextBlock.Text = "✅ Kết nối API thành công. Có thể chọn Online.";
            }
            else
            {
                OfflineModeRadio.IsChecked = true;
                RefreshButton.IsEnabled = false;
                LogTextBlock.Text = "⚠️ Không thể kết nối máy chủ. Chỉ sử dụng Offline.";
            }


            await LoadOfflineDataAsync();
            UpdateDropdowns();


            ClassComboBox.PreviewTextInput += ComboBox_PreviewTextInput;
            ClassComboBox.KeyDown += ComboBox_KeyDown;

            SubjectComboBox.PreviewTextInput += ComboBox_PreviewTextInput;
            SubjectComboBox.KeyDown += ComboBox_KeyDown;


        }

        private async Task<bool> CheckApiAvailableAsync()
        {
            var api = new ApiService();

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                var task = api.CheckHealthAsync(); // giả sử hàm này không có token
                var completed = await Task.WhenAny(task, Task.Delay(3000, cts.Token));

                if (completed == task)
                    return await task; // trả về kết quả thật
                else
                    return false; // timeout
            }
            catch
            {
                return false;
            }
        }

        private async Task LoadOfflineDataAsync()
        {
            if (File.Exists(_dataPath))
            {
                string json = await File.ReadAllTextAsync(_dataPath);
                _allQuestions = JsonSerializer.Deserialize<List<Question>>(json) ?? new();
            }
        }

        private void UpdateDropdowns()
        {
            if (_allQuestions == null || _allQuestions.Count == 0) return;

            ClassComboBox.ItemsSource = _allQuestions.Select(q => q.Class).Distinct().ToList();
            SubjectComboBox.ItemsSource = _allQuestions.Select(q => q.Subject).Distinct().ToList();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedClass = ClassComboBox.SelectedItem?.ToString();
            string selectedSubject = SubjectComboBox.SelectedItem?.ToString();

            var api = new ApiService();
            List<Question> questions = new();

            try
            {
                if (OnlineModeRadio.IsChecked == true)
                {
                    questions = await api.GetQuestionByClassAndSubject(selectedClass, selectedSubject);
                }
                else
                {
                    questions = _allQuestions
                        .Where(q =>
                            (string.IsNullOrEmpty(selectedClass) || q.Class == selectedClass) &&
                            (string.IsNullOrEmpty(selectedSubject) || q.Subject == selectedSubject))
                        .ToList();
                }

                if (questions.Count == 0)
                {
                    MessageBox.Show("Không có câu hỏi nào!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                _frame.Navigate(new QuestionListPage(_frame, questions));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải câu hỏi:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ModeChanged(object sender, RoutedEventArgs e)
        {
            if (OnlineModeRadio.IsChecked == true)
            {
                LogTextBlock.Text = "🌐 Đang kiểm tra kết nối máy chủ...";

                var api = new ApiService();
                bool online = await api.CheckHealthAsync();

                if (online)
                {
                    LogTextBlock.Text = "✅ Chế độ Online: dữ liệu được lấy trực tiếp từ server.";
                    RefreshButton.IsEnabled = true;
                }
                else
                {
                    LogTextBlock.Text = "❌ Không thể kết nối server. Tự động chuyển về chế độ Offline.";
                    OnlineModeRadio.IsChecked = false;
                    OfflineModeRadio.IsChecked = true;
                    RefreshButton.IsEnabled = false;
                }
            }
            else if (OfflineModeRadio.IsChecked == true)
            {
                LogTextBlock.Text = "💾 Chế độ Offline: sử dụng dữ liệu đã tải về.";
                RefreshButton.IsEnabled = false;
            }
        }


        // 🔹 Khi gõ chữ → tự mở dropdown + lọc
        private void ComboBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null || _allQuestions == null) return;

            string text = combo.Text + e.Text;
            string normalizedInput = _vietNamese.RemoveDiacritics(text.Trim().ToLower());

            var allItems = _allQuestions
                .Select(q => combo == ClassComboBox ? q.Class : q.Subject)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            // lọc tiếng Việt không dấu
            var filtered = allItems
                .Where(i => _vietNamese.RemoveDiacritics(i.ToLower()).Contains(normalizedInput))
                .ToList();

            combo.ItemsSource = filtered;
            combo.IsDropDownOpen = true;

            // Giữ caret ở cuối, không bị bôi đen
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            if (tb != null)
            {
                tb.SelectionStart = tb.Text.Length;
                tb.SelectionLength = 0;
            }
        }

        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null) return;

            // Khi nhấn Tab hoặc Enter thì auto chọn gợi ý đầu tiên
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                var list = combo.ItemsSource?.Cast<string>().ToList();
                if (list != null && list.Any())
                {
                    // Nếu text khớp 1 phần → chọn item gần nhất (đầu tiên)
                    var first = list.FirstOrDefault();
                    if(first != null){
                        combo.SelectedItem = first;
                        combo.Text = first;
                    }
                }
                combo.IsDropDownOpen = false;
                // Di chuyển focus sang control kế tiếp (để Tab hoạt động tự nhiên)
                e.Handled = true;
                TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
                (combo as UIElement)?.MoveFocus(request);
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LogTextBlock.Text = "🔄 Đang tải dữ liệu mới từ server...";
                var api = new ApiService();

                // ✅ Gọi API lấy toàn bộ câu hỏi
                var allQuestions = await api.GetQuestionsAsync();
                if (allQuestions == null || allQuestions.Count == 0)
                {
                    LogTextBlock.Text = "⚠️ Không có dữ liệu mới từ server.";
                    return;
                }

                // ✅ Đảm bảo thư mục tồn tại
                if (!Directory.Exists(_dataDir))
                    Directory.CreateDirectory(_dataDir);

                // ✅ Ghi đè file JSON cũ
                string json = JsonSerializer.Serialize(allQuestions, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_dataPath, json, Encoding.UTF8);

                // ✅ Load lại vào app
                _allQuestions = allQuestions;
                UpdateDropdowns();

                LogTextBlock.Text = $"✅ Cập nhật thành công {allQuestions.Count} câu hỏi từ server!";
            }
            catch (Exception ex)
            {
                LogTextBlock.Text = $"❌ Lỗi khi cập nhật dữ liệu: {ex.Message}";
            }
        }

    }
}
