using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Choice
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}
