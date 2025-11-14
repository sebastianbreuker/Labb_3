using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labb_3.Models
{
    public enum Difficulty { Easy, Medium, Hard }
    public class QuestionPack
    {
        public string Name { get; set; } = "New Pack";

        public Difficulty Difficulty { get; set; } = Difficulty.Medium;

        public int TimeLimitInSeconds { get; set; } = 20;

        public List<Question> Questions { get; set; } = new();
    }
        
            
        
         
    
}
