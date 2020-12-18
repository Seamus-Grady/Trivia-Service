using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TriviaService.Models
{
    public class JoinGamePlayerFriend
    {
        public string UserToken { get; set; }

        public int gameID { get; set; }
    }
}