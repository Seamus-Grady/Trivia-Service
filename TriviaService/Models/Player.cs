using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TriviaService.Models
{
    public class Player
    {
        public string Nickname { get; set; }

        public int Geography { get; set; }
        public int Entertainment { get; set; }

        public int History { get; set; }

        public int Art { get; set; }

        public int Science { get; set; }

        public int Sports { get; set; }
    }
}