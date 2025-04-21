using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shofy.Data;
using Shofy.Models;
using System.Collections.Generic;
using System.Linq;

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

        public static List<CartItem> GetCart(this ISession session, ShofyContext context)
        {
            var userId = session.GetUserId();
            if (!userId.HasValue)
            {
                return new List<CartItem>();
            }

            return context.Cart
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .Where(c => c.UserID == userId.Value)
                .SelectMany(c => c.CartItems)
                .ToList();
        }

        public static void SetCart(this ISession session, ShofyContext context, List<CartItem> cartItems)
        {
            var userId = session.GetUserId();
            if (!userId.HasValue)
            {
                return;
            }

            var cart = context.Cart
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserID == userId.Value);

            if (cart == null)
            {
                cart = new Cart { UserID = userId.Value, CreatedAt = DateTime.Now };
                context.Cart.Add(cart);
                context.SaveChanges();
            }

            var existingItems = context.CartItem.Where(ci => ci.CartID == cart.CartID).ToList();
            context.CartItem.RemoveRange(existingItems);

            foreach (var item in cartItems)
            {
                item.CartID = cart.CartID;
                context.CartItem.Add(item);
            }

            context.SaveChanges();
        }

        public static void ClearCart(this ISession session, ShofyContext context)
        {
            var userId = session.GetUserId();
            if (!userId.HasValue)
            {
                return;
            }

            var cart = context.Cart
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserID == userId.Value);

            if (cart != null)
            {
                context.CartItem.RemoveRange(cart.CartItems);
                context.SaveChanges();
            }
        }

        public static void ClearUser(this ISession session)
        {
            session.Remove("UserId");
            session.Remove("UserRole");
            session.Remove("Username");
        }
    }
}