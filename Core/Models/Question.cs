using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Question
    {
        public string QuestionId { get; set; }
        public string Subject { get; set; }
        public string Class { get; set; }
        public string Difficulty { get; set; }
        public string QuestionText { get; set; }
        public List<Choice> Choices { get; set; }
        public string Explanation { get; set; }
        public List<string> Tags { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
