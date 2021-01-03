using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TriviaService.Models
{
    public class PlayerTurn
    {
        public string userToken { get; set; }
        public int gameID { get; set; }
        public int category { get; set; }
        public bool isAPiece { get; set; }
        public int answeredQuestion { get; set;}
        public int playerTurn { get; set; }
    }
}