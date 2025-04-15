using Microsoft.AspNetCore.Http;

namespace Shofy.Helpers
{
    public static class SessionExtensions
    {
        public static int? GetUserId(this ISession session)
        {
            return session.GetInt32("UserID");
        }

        public static void SetUserId(this ISession session, int userID)
        {
            session.SetInt32("UserID", userID);
        }

        public static string GetUsername(this ISession session)
        {
            return session.GetString("Username") ?? string.Empty;
        }

        public static void SetUsername(this ISession session, string username)
        {
            session.SetString("Username", username);
        }

        public static string GetUserRole(this ISession session)
        {
            return session.GetString("UserRole") ?? string.Empty;
        }

        public static void SetUserRole(this ISession session, string role)
        {
            session.SetString("UserRole", role);
        }

        public static void ClearUser(this ISession session)
        {
            session.Remove("UserId");
            session.Remove("UserRole");
            session.Remove("Username");
        }
    }
}