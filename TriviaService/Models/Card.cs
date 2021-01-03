using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace TriviaService.Models
{
    [DataContract]
        public class Card
        {
            public string Geography { get; set; }
            public string Entertainment { get; set; }
            public string History { get; set; }
            public string Art { get; set; }
            public string Science { get; set; }
            public string Sports { get; set; }
            public string GeographyA { get; set; }
            public string EntertainmentA { get; set; }
            public string HistoryA { get; set; }
            public string ArtA { get; set; }
            public string ScienceA { get; set; }
            public string SportsA { get; set; }

        }
}