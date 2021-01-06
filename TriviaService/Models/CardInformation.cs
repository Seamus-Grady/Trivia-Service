using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TriviaService.Models
{
    public class CardInformation
    {
        public string gameID { get; set; }
        public int? position { get; set; }
        public int category { get; set; }
        public string playerMovement { get; set; }
    }
}