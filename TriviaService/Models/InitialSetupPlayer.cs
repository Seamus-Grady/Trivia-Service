using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TriviaService.Models
{
    public class InitialSetupPlayer
    {
        public string userToken { get; set; }
        public string gameID { get; set; }
        public int currentPosition { get; set; }
        public int color { get; set; }
    }
}