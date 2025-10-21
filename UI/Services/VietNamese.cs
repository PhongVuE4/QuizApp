using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UI.Services
{
    public class VietNamese
    {
        public string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            text = text.Normalize(NormalizationForm.FormD);
            var regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string normalized = regex.Replace(text, string.Empty)
                .Replace('đ', 'd').Replace('Đ', 'D');
            return normalized;
        }
    }
}
