using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace TriviaService.Models
{
    public class GameState
    {
        [DataMember(EmitDefaultValue = false)]
        public Player Player1 { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Player Player2 { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Player Player3 { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Player Player4 { get; set; }

        [DataMember(EmitDefaultValue =false)]
        public int CurrentQuestionCategory { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string currentQuestion { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string currentQuestionAnswer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int QuestionAnswered { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Player currentPlayer { get; set; }
    }
}