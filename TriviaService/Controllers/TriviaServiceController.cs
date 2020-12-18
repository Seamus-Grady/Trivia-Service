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

                        return gameID;
                    }
                }
            }
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