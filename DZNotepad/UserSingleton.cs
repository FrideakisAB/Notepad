using System.IO;

namespace DZNotepad
{
    public class UserSingleton
    {
        private static UserSingleton singleton;

        public User LoginUser { get; set; }
        public static string RootPath { get => Path.Combine(Directory.GetCurrentDirectory(), "Users\\russian"); }

        public static UserSingleton Get()
        {
            if (singleton == null)
                singleton = new UserSingleton();

            return singleton;
        }
    }
}
