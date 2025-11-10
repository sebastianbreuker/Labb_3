using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labb_3.Models
{
    class Question
    {
        public Question(string query, string correctanswer, string incorrectanswer, string incorrectanswer2, string incorrectanswer3)
        {
            Query = query;
            CorrectAnswer = correctanswer;
            IncorrectAnswers = [incorrectanswer, incorrectanswer2, incorrectanswer3];
        }
        public string Query { get; set; }

        public string CorrectAnswer { get; set; }

        public string[] IncorrectAnswers { get; set; }
    }
}
