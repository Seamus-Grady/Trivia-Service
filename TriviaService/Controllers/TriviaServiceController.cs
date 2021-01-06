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
        public string PostCreateGame([FromBody] JoinGamePlayerHost player)
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


                    }
                    using (SqlCommand command = new SqlCommand("insert into PlayerUser(GameID, PlayerID, Geography, Entertainment, History, Art, Science, Sports) values (@GameID, @PlayerID, 0, 0, 0, 0, 0, 0)", conn, trans))
                    {
                        command.Parameters.AddWithValue("@GameID", gameID);
                        command.Parameters.AddWithValue("@PlayerID", player.UserToken);
                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Query failed unexpectedly");
                        }
                        trans.Commit();
                        return gameID.ToString();
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
            StringBuilder strCommand = new StringBuilder();
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
                    if(!shuffleDeck.Equals(""))
                    {
                        strCommand.Append("update Games set CurrentQuestionCat = @CurrentQuestionCat, CurrentQuestion = @CurrentQuestion, CurrentAnswer = @CurrentAnswer, Deck = @Deck where GameID = @GameID");
                    }
                    else
                    {
                        strCommand.Append("update Games set CurrentQuestionCat = @CurrentQuestionCat, CurrentQuestion = @CurrentQuestion, CurrentAnswer = @CurrentAnswer where GameID = @GameID");
                    }
                    using (SqlCommand command = new SqlCommand(strCommand.ToString(), conn, trans))
                    {
                        command.Parameters.AddWithValue("@CurrentQuestionCat", ci.category);
                        command.Parameters.AddWithValue("@CurrentQuestion", currentQuestion);
                        command.Parameters.AddWithValue("@CurrentAnswer", currentAnswer);
                        if (!shuffleDeck.Equals(""))
                        {
                            command.Parameters.AddWithValue("@Deck", shuffleDeck);
                        }
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
        [Route("TriviaService/game-setup")]
        public void PutInitalSetup([FromBody] InitialSetupPlayer isp)
        {
            if (isp == null)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            using (SqlConnection conn = new SqlConnection(TriviaServiceDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("update PlayerUser set CurrentPosition = @CurrentPosition where PlayerID = @PlayerID and GameID = @GameID", conn, trans))
                    {
                        command.Parameters.AddWithValue("@CurrentPosition", isp.currentPosition);
                        command.Parameters.AddWithValue("@PlayerID", isp.userToken);
                        command.Parameters.AddWithValue("@GameID", isp.gameID);
                        if(command.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Query failed unexpectedly");
                        }
                        trans.Commit();
                    }
                }
            }
        }
        [Route("TriviaService/end-of-turn")]
        public void PutEndOfPlayerTurn([FromBody] PlayerTurn pt)
        {
            int playerChoice;
            if (pt == null)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            string nextPlayerTurn = "";
            StringBuilder str = new StringBuilder();
            StringBuilder str2 = new StringBuilder();
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
                                command.Parameters.AddWithValue("@GameID", pt.gameID);
                                command.Parameters.AddWithValue("@PlayerID", pt.userToken);
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
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (SqlCommand command = new SqlCommand("select * from Games where GameID = @GameID", conn, trans))
                            {
                                command.Parameters.AddWithValue("@GameID", pt.gameID);
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
                                        int.TryParse(pt.playerTurn, out playerChoice);
                                        playerChoice++;
                                        while(true)
                                        {
                                            if(playerChoice > 4)
                                            {
                                                playerChoice = 1;
                                            }
                                            if(reader["Player" + playerChoice] != DBNull.Value)
                                            {
                                                nextPlayerTurn = reader["Player" + playerChoice].ToString();
                                                break;
                                            }
                                            playerChoice++;
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
                                command2.Parameters.AddWithValue("@GameID", pt.gameID);
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
        }
        [Route("TriviaService/join-game")]
        public string PostJoinGame([FromBody] JoinGamePlayerFriend player)
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
                                        reader.Close();
                                        if (command2.ExecuteNonQuery() != 1)
                                        {
                                            throw new Exception("Query failed unexpectedly");
                                        }
                                        
                                    }
                                    using (SqlCommand command2 = new SqlCommand("insert into PlayerUser(GameID, PlayerID, Geography, Entertainment, History, Art, Science, Sports) values (@GameID, @PlayerID, 0, 0, 0, 0, 0, 0)", conn, trans))
                                    {
                                        command2.Parameters.AddWithValue("@GameID",player.gameID);
                                        command2.Parameters.AddWithValue("@PlayerID", player.UserToken);
                                        if (command2.ExecuteNonQuery() != 1)
                                        {
                                            throw new Exception("Query failed unexpectedly");
                                        }
                                        trans.Commit();
                                        return "1";
                                    }
                                }
                                else if (reader["Player3"] == DBNull.Value)
                                {
                                    using (SqlCommand command2 = new SqlCommand("update Games set Player3 = @Player3 where GameID = @GameID", conn, trans))
                                    {
                                        command2.Parameters.AddWithValue("@Player3", player.UserToken);
                                        command2.Parameters.AddWithValue("@GameID", player.gameID);
                                        reader.Close();
                                        if (command2.ExecuteNonQuery() != 1)
                                        {
                                            throw new Exception("Query failed unexpectedly");
                                        }
                                    }
                                    using (SqlCommand command2 = new SqlCommand("insert into PlayerUser(GameID, PlayerID, Geography, Entertainment, History, Art, Science, Sports) values (@GameID, @PlayerID, 0, 0, 0, 0, 0, 0)", conn, trans))
                                    {
                                        command2.Parameters.AddWithValue("@GameID", player.gameID);
                                        command2.Parameters.AddWithValue("@PlayerID", player.UserToken);
                                        if (command2.ExecuteNonQuery() != 1)
                                        {
                                            throw new Exception("Query failed unexpectedly");
                                        }
                                        trans.Commit();
                                        return "2";
                                    }
                                }
                                else if (reader["Player4"] == DBNull.Value)
                                {
                                    using (SqlCommand command2 = new SqlCommand("update Games set Player4 = @Player4 where GameID = @GameID", conn, trans))
                                    {
                                        command2.Parameters.AddWithValue("@Player4", player.UserToken);
                                        command2.Parameters.AddWithValue("@GameID", player.gameID);
                                        reader.Close();
                                        if (command2.ExecuteNonQuery() != 1)
                                        {
                                            throw new Exception("Query failed unexpectedly");
                                        }
                                    }
                                    using (SqlCommand command2 = new SqlCommand("insert into PlayerUser(GameID, PlayerID, Geography, Entertainment, History, Art, Science, Sports) values (@GameID, @PlayerID, 0, 0, 0, 0, 0, 0)", conn, trans))
                                    {
                                        command2.Parameters.AddWithValue("@GameID", player.gameID);
                                        command2.Parameters.AddWithValue("@PlayerID", player.UserToken);
                                        if (command2.ExecuteNonQuery() != 1)
                                        {
                                            throw new Exception("Query failed unexpectedly");
                                        }
                                        trans.Commit();
                                        return "3";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return "";
        }
        [Route("TriviaService/games/{gameID}/{brief}")]
        public GameState GetGameState([FromUri] string gameID, [FromUri] bool brief)
        {
            GameState gameState;
            if(gameID == null)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            lock (sync)
            {
                using (SqlConnection conn = new SqlConnection(TriviaServiceDB))
                {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        gameState = new GameState();
                        using (SqlCommand command = new SqlCommand("select * from PlayerUser join Users on PlayerID = UserID join Games on Games.GameID = PlayerUser.GameID where Games.GameID = @GameID", conn, trans))
                        {
                            command.Parameters.AddWithValue("@GameID", gameID);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {

                                while (reader.Read() != false)
                                {
                                    if (reader["Player1"] != DBNull.Value && reader["PlayerID"].ToString().Equals(reader["Player1"].ToString()))
                                    {
                                        gameState.Player1 = new Player()
                                        {
                                            Nickname = reader["NickName"].ToString(),
                                            Geography = (int)reader["Geography"],
                                            Entertainment = (int)reader["Entertainment"],
                                            History = (int)reader["History"],
                                            Art = (int)reader["Art"],
                                            Science = (int)reader["Science"],
                                            Sports = (int)reader["Sports"]
                                        };
                                        if (reader["CurrentPosition"] != DBNull.Value)
                                        {
                                            gameState.Player1.currentPosition = (int)reader["CurrentPosition"];
                                        }
                                        if (reader["CurrentPositionMovement"] != DBNull.Value)
                                        {
                                            gameState.Player1.currentPositionMovement = reader["CurrentPositionMovement"].ToString();
                                        }
                                        if (reader["CurrentPlayer"] != DBNull.Value && reader["PlayerID"].ToString().Equals(reader["CurrentPlayer"].ToString()))
                                        {
                                            gameState.currentPlayer = gameState.Player1;
                                        }
                                    }
                                    if (reader["Player2"] != DBNull.Value && reader["PlayerID"].ToString().Equals(reader["Player2"].ToString()))
                                    {
                                        gameState.Player2 = new Player()
                                        {
                                            Nickname = reader["NickName"].ToString(),
                                            Geography = (int)reader["Geography"],
                                            Entertainment = (int)reader["Entertainment"],
                                            History = (int)reader["History"],
                                            Art = (int)reader["Art"],
                                            Science = (int)reader["Science"],
                                            Sports = (int)reader["Sports"]
                                        };
                                        if (reader["CurrentPosition"] != DBNull.Value)
                                        {
                                            gameState.Player2.currentPosition = (int)reader["CurrentPosition"];
                                        }
                                        if (reader["CurrentPositionMovement"] != DBNull.Value)
                                        {
                                            gameState.Player2.currentPositionMovement = reader["CurrentPositionMovement"].ToString();
                                        }
                                        if (reader["CurrentPlayer"] != DBNull.Value && reader["PlayerID"].ToString().Equals(reader["CurrentPlayer"].ToString()))
                                        {
                                            gameState.currentPlayer = gameState.Player2;
                                        }
                                    }
                                    if (reader["Player3"] != DBNull.Value && reader["PlayerID"].ToString().Equals(reader["Player3"].ToString()))
                                    {
                                        gameState.Player3 = new Player()
                                        {
                                            Nickname = reader["NickName"].ToString(),
                                            Geography = (int)reader["Geography"],
                                            Entertainment = (int)reader["Entertainment"],
                                            History = (int)reader["History"],
                                            Art = (int)reader["Art"],
                                            Science = (int)reader["Science"],
                                            Sports = (int)reader["Sports"]
                                        };
                                        if (reader["CurrentPosition"] != DBNull.Value)
                                        {
                                            gameState.Player3.currentPosition = (int)reader["CurrentPosition"];
                                        }
                                        if (reader["CurrentPositionMovement"] != DBNull.Value)
                                        {
                                            gameState.Player3.currentPositionMovement = reader["CurrentPositionMovement"].ToString();
                                        }
                                        if (reader["CurrentPlayer"] != DBNull.Value && reader["PlayerID"].ToString().Equals(reader["CurrentPlayer"].ToString()))
                                        {
                                            gameState.currentPlayer = gameState.Player3;
                                        }
                                    }
                                    if (reader["Player4"] != DBNull.Value && reader["PlayerID"].ToString().Equals(reader["Player4"].ToString()))
                                    {
                                        gameState.Player4 = new Player()
                                        {
                                            Nickname = reader["NickName"].ToString(),
                                            Geography = (int)reader["Geography"],
                                            Entertainment = (int)reader["Entertainment"],
                                            History = (int)reader["History"],
                                            Art = (int)reader["Art"],
                                            Science = (int)reader["Science"],
                                            Sports = (int)reader["Sports"]
                                        };
                                        if (reader["CurrentPosition"] != DBNull.Value)
                                        {
                                            gameState.Player4.currentPosition = (int)reader["CurrentPosition"];
                                        }
                                        if (reader["CurrentPositionMovement"] != DBNull.Value)
                                        {
                                            gameState.Player4.currentPositionMovement = reader["CurrentPositionMovement"].ToString();
                                        }
                                        if (reader["CurrentPlayer"] != DBNull.Value && reader["PlayerID"].ToString().Equals(reader["CurrentPlayer"].ToString()))
                                        {
                                            gameState.currentPlayer = gameState.Player4;
                                        }
                                    }
                                    if (!brief)
                                    {
                                        if (reader["CurrentQuestion"] != DBNull.Value)
                                        {
                                            gameState.currentQuestion = reader["CurrentQuestion"].ToString();
                                        }
                                        if (reader["CurrentAnswer"] != DBNull.Value)
                                        {
                                            gameState.currentQuestionAnswer = reader["CurrentAnswer"].ToString();
                                        }
                                        if (reader["CurrentQuestionCat"] != DBNull.Value)
                                        {
                                            gameState.CurrentQuestionCategory = (int)reader["CurrentQuestionCat"];
                                        }
                                    }
                                }
                            }
                        }
                        trans.Commit();
                        return gameState;
                    }
                }
            }
        }
        [Route("TriviaService/games")]
        public void PutCancelJoinRequest([FromBody] CancelJoinPlayer cjp)
        {
            int choice;
            if (cjp == null || cjp.UserToken == null || !int.TryParse(cjp.playerTurn, out choice) || choice < 0 || choice > 3)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            lock (sync)
            {
                using (SqlConnection conn = new SqlConnection(TriviaServiceDB))
                {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        using (SqlCommand command = new SqlCommand("select UserID from Users where UserID = @UserID", conn, trans))
                        {
                            command.Parameters.AddWithValue("@UserID", cjp.UserToken);
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
                        using (SqlCommand command = new SqlCommand("select Player1, Player2, Player3, Player4 from Games where GameID = @GameID", conn, trans))
                        {
                            command.Parameters.AddWithValue("@GameID", cjp.gameID);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    reader.Read();
                                    if (choice == 0)
                                    {
                                        string player1 = "";
                                        string player2 = "";
                                        string player3 = "";
                                        string player4 = "";
                                        int count = 0;
                                        if (reader["Player1"] != DBNull.Value)
                                        {
                                            player1 = reader["Player1"].ToString();
                                            count++;
                                        }
                                        if (reader["Player2"] != DBNull.Value)
                                        {
                                            player2 = reader["Player2"].ToString();
                                            count++;
                                        }
                                        if (reader["Player3"] != DBNull.Value)
                                        {
                                            player3 = reader["Player3"].ToString();
                                            count++;
                                        }
                                        if (reader["Player4"] != DBNull.Value)
                                        {
                                            player4 = reader["Player4"].ToString();
                                            count++;
                                        }
                                        reader.Close();
                                        for(int i = 1; i <= count; i++)
                                        {
                                            using (SqlCommand command2 = new SqlCommand("delete from PlayerUser where GameID = @GameID and PlayerID = @PlayerID", conn, trans))
                                            {
                                                command2.Parameters.AddWithValue("@GameID", cjp.gameID);
                                                switch(i)
                                                {
                                                    case 1:
                                                        command2.Parameters.AddWithValue("@PlayerID", player1);
                                                        break;
                                                    case 2:
                                                        command2.Parameters.AddWithValue("@PlayerID", player2);
                                                        break;
                                                    case 3:
                                                        command2.Parameters.AddWithValue("@PlayerID", player3);
                                                        break;
                                                    case 4:
                                                        command2.Parameters.AddWithValue("@PlayerID", player4);
                                                        break;
                                                }
                                                
                                                if (command2.ExecuteNonQuery() != 1)
                                                {
                                                    reader.Close();
                                                    trans.Commit();
                                                    throw new Exception("Query failed unexpectedly");
                                                }

                                            }
                                        }
                                        


                                        using (SqlCommand command2 = new SqlCommand("delete from Games where GameID = @GameID", conn, trans))
                                        {
                                            command2.Parameters.AddWithValue("@GameID", cjp.gameID);
                                            if (command2.ExecuteNonQuery() != 1)
                                            {
                                                reader.Close();
                                                trans.Commit();
                                                throw new Exception("Query failed unexpectedly");
                                            }
                                            trans.Commit();
                                        }
                                    }
                                    else
                                    {
                                        using (SqlCommand command2 = new SqlCommand("delete from PlayerUser where GameID = @GameID and PlayerID = @PlayerID", conn, trans))
                                        {
                                            command2.Parameters.AddWithValue("@GameID", cjp.gameID);
                                            command2.Parameters.AddWithValue("@PlayerID", reader["Player" + (choice + 1)].ToString());
                                            reader.Close();
                                            if (command2.ExecuteNonQuery() != 1)
                                            {
                                                reader.Close();
                                                trans.Commit();
                                                throw new Exception("Query failed unexpectedly");
                                            }
                                        }
                                        string player = "";
                                        switch (choice)
                                        {
                                            case 1:
                                                player = "Player2";
                                                break;
                                            case 2:
                                                player = "Player3";
                                                break;
                                            case 3:
                                                player = "Player4";
                                                break;
                                        }
                                        using (SqlCommand command2 = new SqlCommand("update Games set " + player + " = null where GameID = @GameID", conn, trans))
                                        {
                                            command2.Parameters.AddWithValue("@GameID", cjp.gameID);
                                            if (command2.ExecuteNonQuery() != 1)
                                            {
                                                reader.Close();
                                                trans.Commit();
                                                throw new Exception("Query failed unexpectedly");
                                            }
                                            trans.Commit();
                                        }
                                    }
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
                }
            }
        }
        [Route("TriviaService/start-game")]
        public void PutStartGame([FromBody] JoinGamePlayerFriend player)
        {
            if(player == null || player.UserToken == null)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            using(SqlConnection conn = new SqlConnection(TriviaServiceDB))
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
                    using (SqlCommand command = new SqlCommand("update Games set CurrentPlayer = @CurrentPlayer, Deck = @Deck where GameID = @GameID and Player1 = @Player1", conn, trans))
                    {
                        command.Parameters.AddWithValue("@CurrentPlayer", player.UserToken);
                        command.Parameters.AddWithValue("@Deck", GenerateDeck());
                        command.Parameters.AddWithValue("@GameID", player.gameID);
                        command.Parameters.AddWithValue("@Player1", player.UserToken);
                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Query failed unexpectedly");
                        }
                        trans.Commit();
                    }
                }
            }
        }
        private static readonly object sync = new object();
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