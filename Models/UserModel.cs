using Kokoro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Models
{
    public class UserModel
    {
        public UserModel(ulong id, string username)
        {
            UserId = id;
            UserName = username;
        }

        public ulong UserId { get; set; }
        public string UserName { get; set; }
        public string Mention {
            get 
            {
                return $"<@{UserId}>";
            } 
        } 
    }
}

public static class KaraokeList
{
    private static List<UserModel> users = new List<UserModel>();

    public static List<UserModel> Users
    {
        get { return users; }
        set { users = value; }
    }

}
