using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Http;
using TriviaService.Models;

namespace TriviaService.Controllers
{
    public class TriviaServiceController : ApiController
    {
        private static string TriviaServiceDB;

        /// <summary>
        /// Creates the connection to the database.
        /// </summary>
        static TriviaServiceController()
        {
            TriviaServiceDB = ConfigurationManager.ConnectionStrings["TriviaServiceDB"].ConnectionString;
        }

        [Route("TriviaService/users")]
        public string PostMakeUser([FromBody] string Nickname)
        {
            //Responds with Forbidden status code if the nickname is NOT legal
            if (Nickname == null || Nickname.Trim().Length == 0 || Nickname.Trim().Length > 50)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            using (SqlConnection conn = new SqlConnection(TriviaServiceDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    //inserts the new user into the registered "Users" table
                    using (SqlCommand command = new SqlCommand("insert into Users (UserID, Nickname) values(@UserID, @Nickname)", conn, trans))
                    {
                        //creates the unique UserToken for the given player
                        string userToken = Guid.NewGuid().ToString();

                        command.Parameters.AddWithValue("@UserID", userToken);
                        command.Parameters.AddWithValue("@Nickname", Nickname.Trim());

                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Query failed unexpectedly");
                        }
                        trans.Commit();
                        return userToken;
                    }
                }
            }
        }
        [Route("TriviaService/create-game")]
        public int PostCreateGame([FromBody] JoinGamePlayerHost player)
        {
            int gameID;

            if(player == null || player.UserToken == null)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            using (SqlConnection conn = new SqlConnection(TriviaServiceDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("select UserID from Users where UserID = @UserID", conn, trans))
                    {
                        command.Parameters.AddWithValue("@UserID", player.UserToken);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if(!reader.HasRows)
                            {
                                reader.Close();
                                trans.Commit();
                                throw new HttpResponseException(HttpStatusCode.Forbidden);
                            }
                        }
                    }
                    using (SqlCommand command = new SqlCommand("insert into Games(Player1) output inserted.GameID values (@Player1)", conn, trans))
                    {
                        command.Parameters.AddWithValue("@Player1", player.UserToken);

                        gameID = (int)command.ExecuteScalar();

                        trans.Commit();
                    }
                    using (SqlCommand command = new SqlCommand("insert into PlayerUser(GameID, PlayerID) values (@GameID, @PlayerID)", conn, trans))
                    {
                        command.Parameters.AddWithValue("@GameID", gameID);
                        command.Parameters.AddWithValue("@PlayerID", player.UserToken);
                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Query failed unexpectedly");
                        }
                        trans.Commit();
                        return gameID;
                    }
                }
            }
        }
        [Route ("TriviaService/cards-game")]
        public Card GetCard([FromBody] CardInformation ci)
        {
            string shuffleDeck = "";
            int cardNum;
            string currentQuestion = "";
            string currentAnswer = "";
            Card cardToReturn = new Card();
            if (ci == null)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            using (SqlConnection conn = new SqlConnection(TriviaServiceDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    if (ci.category != 6)
                    {
                        using (SqlCommand command = new SqlCommand("select Deck from Games where gameID = @gameID", conn, trans))
                        {
                            command.Parameters.AddWithValue("@gameID", ci.gameID);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    reader.Read();
                                    shuffleDeck = reader["Deck"].ToString();
                                    if (shuffleDeck.Length == 0)
                                    {
                                        shuffleDeck = GenerateDeck();
                                    }
                                    int.TryParse(shuffleDeck.Substring(shuffleDeck.LastIndexOf(',')), out cardNum);
                                }
                                else
                                {
                                    reader.Close();
                                    trans.Commit();
                                    throw new HttpResponseException(HttpStatusCode.Forbidden);
                                }
                            }
                        }
                        using (SqlCommand command = new SqlCommand("select * from Deck where CardID = @CardID", conn, trans))
                        {
                            command.Parameters.AddWithValue("@CardID", cardNum);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    reader.Read();
                                    cardToReturn.Geography = reader["Geography"].ToString();
                                    cardToReturn.Entertainment = reader["Entertainment"].ToString();
                                    cardToReturn.History = reader["History"].ToString();
                                    cardToReturn.Art = reader["Art"].ToString();
                                    cardToReturn.Science = reader["Science"].ToString();
                                    cardToReturn.Sports = reader["Sports"].ToString();
                                    cardToReturn.GeographyA = reader["GeographyA"].ToString();
                                    cardToReturn.EntertainmentA = reader["EntertainmentA"].ToString();
                                    cardToReturn.HistoryA = reader["HistoryA"].ToString();
                                    cardToReturn.ArtA = reader["ArtA"].ToString();
                                    cardToReturn.ScienceA = reader["ScienceA"].ToString();
                                    cardToReturn.SportsA = reader["SportsA"].ToString();
                                }
                                else
                                {
                                    reader.Close();
                                    trans.Commit();
                                    throw new HttpResponseException(HttpStatusCode.Forbidden);
                                }
                            }
                        }
                    }
                    switch (ci.category)
                    {
                        case 0:
                            currentQuestion = cardToReturn.Geography;
                            currentAnswer = cardToReturn.GeographyA;
                            break;
                        case 1:
                            currentQuestion = cardToReturn.Entertainment;
                            currentAnswer = cardToReturn.EntertainmentA;
                            break;
                        case 2:
                            currentQuestion = cardToReturn.History;
                            currentAnswer = cardToReturn.HistoryA;
                            break;
                        case 3:
                            currentQuestion = cardToReturn.Art;
                            currentAnswer = cardToReturn.ArtA;
                            break;
                        case 4:
                            currentQuestion = cardToReturn.Science;
                            currentAnswer = cardToReturn.ScienceA;
                            break;
                        case 5:
                            currentQuestion = cardToReturn.Sports;
                            currentAnswer = cardToReturn.SportsA;
                            break;
                        case 6:
                            currentQuestion = "Roll Again";
                            currentAnswer = "Roll Again";
                            break;
                    }
                    using (SqlCommand command = new SqlCommand("update Games set CurrentQuestionCat = @CurrentQuestionCat, CurrentQuestion = @CurrentQuestion, CurrentAnswer = @CurrentAnswer, Deck = @Deck where GameID = @GameID", conn, trans))
                    {
                        command.Parameters.AddWithValue("@CurrentQuestionCat", ci.category);
                        command.Parameters.AddWithValue("@CurrentQuestion", currentQuestion);
                        command.Parameters.AddWithValue("@CurrentAnswer", currentAnswer);
                        command.Parameters.AddWithValue("@Deck", shuffleDeck);
                        command.Parameters.AddWithValue("@GameID", ci.gameID);
                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Query failed unexpectedly");
                        }
                        trans.Commit();
                    }
                    using (SqlCommand command = new SqlCommand("update PlayerUser set CurrentPosition = @CurrentPosition, CurrentPositionMovement = @CurrentPositionMovement where GameID = @GameID", conn, trans))
                    {
                        command.Parameters.AddWithValue("@CurrentPosition", ci.position);
                        command.Parameters.AddWithValue("@CurrentPositionMovement", ci.playerMovement);
                        command.Parameters.AddWithValue("@GameID", ci.gameID);
                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Query failed unexpectedly");
                        }
                        trans.Commit();
                    }
                }
            }
            return cardToReturn;
        }
        [Route("TriviaService/end-of-turn")]
        public bool PostEndOfPlayerTurn([FromBody] PlayerTurn pt)
        {
            if (pt == null)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            string nextPlayerTurn = "";
            StringBuilder str = new StringBuilder();
            StringBuilder str2 = new StringBuilder();
            bool playerWon = false;
            using (SqlConnection conn = new SqlConnection(TriviaServiceDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    if (pt.answeredQuestion == 1 && !pt.isAPiece)
                    {
                        using (SqlCommand command = new SqlCommand("update Games set CurrentQuestionCat = null, CurrentQuestion = null, CurrentAnswer = null where GameID = @GameID", conn, trans))
                        {
                            if (command.ExecuteNonQuery() != 1)
                            {
                                throw new Exception("Query failed unexpectedly");
                            }
                            trans.Commit();
                        }
                        return false;
                    
                    }
                    else
                    {
                        str2.Append("update Games set ");
                        if (pt.isAPiece)
                        {
                            str.Append("update PlayerUser set ");
                            switch (pt.category)
                            {
                                case 0:
                                    str.Append("Geography = 1");
                                    break;
                                case 1:
                                    str.Append("Entertainment = 1");
                                    break;
                                case 2:
                                    str.Append("History = 1");
                                    break;
                                case 3:
                                    str.Append("Art = 1");
                                    break;
                                case 4:
                                    str.Append("Science = 1");
                                    break;
                                case 5:
                                    str.Append("Sports = 1");
                                    break;
                            }
                            str.Append(" where PlayerID = @PlayerID and GameID = @GameID");
                            using (SqlCommand command = new SqlCommand(str.ToString(), conn, trans))
                            {
                                command.Parameters.AddWithValue("@PlayerID", pt.userToken);
                                command.Parameters.AddWithValue("@GameID", pt.gameID);
                                if (command.ExecuteNonQuery() != 1)
                                {
                                    throw new Exception("Query failed unexpectedly");
                                }
                                trans.Commit();
                            }
                            using (SqlCommand command = new SqlCommand("select * from PlayerUser where GameID = @GameID and PlayerID = @PlayerID", conn, trans))
                            {
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    if (!reader.HasRows)
                                    {
                                        reader.Close();
                                        trans.Commit();
                                        throw new HttpResponseException(HttpStatusCode.Forbidden);
                                    }
                                    else
                                    {
                                        reader.Read();
                                        if ((int)reader["Geography"] == 1 && (int)reader["Entertainment"] == 1 && (int)reader["History"] == 1 && (int)reader["Art"] == 1
                                            && (int)reader["Science"] == 1 && (int)reader["Sport"] == 1)
                                        {
                                            str2.Append(" CurrentPlayer = null,");
                                            playerWon = true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (SqlCommand command = new SqlCommand("select * from Games where GameID = @GameID", conn, trans))
                            {
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    if (!reader.HasRows)
                                    {
                                        reader.Close();
                                        trans.Commit();
                                        throw new HttpResponseException(HttpStatusCode.Forbidden);
                                    }
                                    else
                                    {
                                        reader.Read();
                                        switch (pt.playerTurn)
                                        {
                                            case 0:
                                                nextPlayerTurn = reader["Player1"].ToString();
                                                break;
                                            case 1:
                                                nextPlayerTurn = reader["Player2"].ToString();
                                                break;
                                            case 2:
                                                nextPlayerTurn = reader["Player3"].ToString();
                                                break;
                                            case 3:
                                                nextPlayerTurn = reader["Player4"].ToString();
                                                break;
                                        }
                                    }
                                }
                                str2.Append("CurrentPlayer = ");
                                str2.Append(nextPlayerTurn);
                                str2.Append(", ");
                            }
                            str2.Append("CurrentQuestionCat = null, CurrentQuestion = null, CurrentAnswer = null where GameID = @GameID");
                            using (SqlCommand command2 = new SqlCommand(str2.ToString(), conn, trans))
                            {
                                if (command2.ExecuteNonQuery() != 1)
                                {
                                    throw new Exception("Query failed unexpectedly");
                                }
                                trans.Commit();
                            }
                            
                        }
                    }
                }
            }
            return playerWon;
        }
        [Route("TriviaService/join-game")]
        public void PostJoinGame([FromBody] JoinGamePlayerFriend player)
        {

            if (player == null || player.UserToken == null)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            using (SqlConnection conn = new SqlConnection(TriviaServiceDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("select UserID from Users where UserID = @UserID", conn, trans))
                    {
                        command.Parameters.AddWithValue("@UserID", player.UserToken);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                reader.Close();
                                trans.Commit();
                                throw new HttpResponseException(HttpStatusCode.Forbidden);
                            }
                        }
                    }
                    using (SqlCommand command = new SqlCommand("select * from Games where GameID = @GameID", conn, trans))
                    {
                        command.Parameters.AddWithValue("@GameID", player.gameID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                if (reader["Player1"].ToString().Equals(player.UserToken))
                                {
                                    reader.Close();
                                    trans.Commit();
                                    throw new HttpResponseException(HttpStatusCode.Conflict);
                                }
                                if (reader["Player2"] != DBNull.Value)
                                {
                                    if (reader["Player2"].ToString().Equals(player.UserToken))
                                    {
                                        reader.Close();
                                        trans.Commit();
                                        throw new HttpResponseException(HttpStatusCode.Conflict);
                                    }
                                }
                                if (reader["Player3"] != DBNull.Value)
                                {
                                    if (reader["Player3"].ToString().Equals(player.UserToken))
                                    {
                                        reader.Close();
                                        trans.Commit();
                                        throw new HttpResponseException(HttpStatusCode.Conflict);
                                    }
                                }
                                if (reader["Player4"] != DBNull.Value)
                                {
                                    if (reader["Player4"].ToString().Equals(player.UserToken))
                                    {
                                        reader.Close();
                                        trans.Commit();
                                        throw new HttpResponseException(HttpStatusCode.Conflict);
                                    }
                                }
                                if (reader["Player2"] == DBNull.Value)
                                {
                                    using (SqlCommand command2 = new SqlCommand("update Games set Player2 = @Player2 where GameID = @GameID", conn, trans))
                                    {
                                        command2.Parameters.AddWithValue("@Player2", player.UserToken);
                                        command2.Parameters.AddWithValue("@GameID", player.gameID);

                                        if (command.ExecuteNonQuery() != 1)
                                        {
                                            throw new Exception("Query failed unexpectedly");
                                        }
                                        trans.Commit();
                                    }
                                    using (SqlCommand command2 = new SqlCommand("insert into PlayerUser(GameID, PlayerID) values (@GameID, @PlayerID)",conn, trans))
                                    {
                                        command.Parameters.AddWithValue("@GameID",player.gameID);
                                        command.Parameters.AddWithValue("@PlayerID", player.UserToken);
                                        if (command.ExecuteNonQuery() != 1)
                                        {
                                            throw new Exception("Query failed unexpectedly");
                                        }
                                        trans.Commit();
                                        reader.Close();
                                    }
                                }
                                else if (reader["Player3"] == DBNull.Value)
                                {
                                    using (SqlCommand command2 = new SqlCommand("update Games set Player3 = @Player3 where GameID = @GameID", conn, trans))
                                    {
                                        command2.Parameters.AddWithValue("@Player3", player.UserToken);
                                        command2.Parameters.AddWithValue("@GameID", player.gameID);

                                        if (command.ExecuteNonQuery() != 1)
                                        {
                                            throw new Exception("Query failed unexpectedly");
                                        }
                                        trans.Commit();
                                        reader.Close();
                                    }
                                    using (SqlCommand command2 = new SqlCommand("insert into PlayerUser(GameID, PlayerID) values (@GameID, @PlayerID)", conn, trans))
                                    {
                                        command.Parameters.AddWithValue("@GameID", player.gameID);
                                        command.Parameters.AddWithValue("@PlayerID", player.UserToken);
                                        if (command.ExecuteNonQuery() != 1)
                                        {
                                            throw new Exception("Query failed unexpectedly");
                                        }
                                        trans.Commit();
                                        reader.Close();
                                    }
                                }
                                else if (reader["Player4"] == DBNull.Value)
                                {
                                    using (SqlCommand command2 = new SqlCommand("update Games set Player4 = @Player4 where GameID = @GameID", conn, trans))
                                    {
                                        command2.Parameters.AddWithValue("@Player4", player.UserToken);
                                        command2.Parameters.AddWithValue("@GameID", player.gameID);

                                        if (command.ExecuteNonQuery() != 1)
                                        {
                                            throw new Exception("Query failed unexpectedly");
                                        }
                                        trans.Commit();
                                        reader.Close();
                                    }
                                    using (SqlCommand command2 = new SqlCommand("insert into PlayerUser(GameID, PlayerID) values (@GameID, @PlayerID)", conn, trans))
                                    {
                                        command.Parameters.AddWithValue("@GameID", player.gameID);
                                        command.Parameters.AddWithValue("@PlayerID", player.UserToken);
                                        if (command.ExecuteNonQuery() != 1)
                                        {
                                            throw new Exception("Query failed unexpectedly");
                                        }
                                        trans.Commit();
                                        reader.Close();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        [Route("TriviaService/start-game")]
        public GameState startGame([FromBody] JoinGamePlayerFriend player)
        {
            if(player.UserToken == null)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            return null;
        }

        public string GenerateDeck()
        {
            StringBuilder str = new StringBuilder();
            Random ran = new Random();
            var randomNumbers = Enumerable.Range(1, 94).OrderBy(x => ran.Next()).Take(94).ToList();
            foreach(var x in randomNumbers)
            {
                str.Append(x + ",");
            }
            str.Remove(str.Length - 2, 1);
            return str.ToString();
        }
    }
}