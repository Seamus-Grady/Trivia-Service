using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace TriviaService.Models
{
        public class Card
        {
        [DataMember(EmitDefaultValue = false)]
        public string Geography { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Entertainment { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string History { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Art { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Science { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Sports { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string GeographyA { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string EntertainmentA { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string HistoryA { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string ArtA { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string ScienceA { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string SportsA { get; set; }
        }
}