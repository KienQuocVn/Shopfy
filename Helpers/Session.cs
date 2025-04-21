using Newtonsoft.Json;
using Shofy.Data;
using Shofy.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
    public static class WishlistHelper
    {
        // Lấy danh sách ProductID từ Wishlist
        public static List<int> GetWishlistProductIds(string wishlistJson)
        {
            if (string.IsNullOrEmpty(wishlistJson))
                return new List<int>();

            return JsonConvert.DeserializeObject<List<int>>(wishlistJson) ?? new List<int>();
        }

        // Cập nhật Wishlist vào chuỗi JSON
        public static string UpdateWishlistJson(List<int> productIds)
        {
            return JsonConvert.SerializeObject(productIds);
        }

        // Thêm sản phẩm vào Wishlist
        public static async Task<bool> AddToWishlistAsync(ShofyContext context, int userId, int productId)
        {
            var user = await context.User.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null)
                return false;

            var product = await context.Product.FirstOrDefaultAsync(p => p.ProductID == productId && p.Status == "Active");
            if (product == null)
                return false;

            var wishlist = GetWishlistProductIds(user.Wishlist);
            if (!wishlist.Contains(productId))
            {
                wishlist.Add(productId);
                user.Wishlist = UpdateWishlistJson(wishlist);
                await context.SaveChangesAsync();
                return true;
            }

            return false; // Sản phẩm đã có trong Wishlist
        }

        // Xóa sản phẩm khỏi Wishlist
        public static async Task<bool> RemoveFromWishlistAsync(ShofyContext context, int userId, int productId)
        {
            var user = await context.User.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null)
                return false;

            var wishlist = GetWishlistProductIds(user.Wishlist);
            if (wishlist.Remove(productId))
            {
                user.Wishlist = UpdateWishlistJson(wishlist);
                await context.SaveChangesAsync();
                return true;
            }

            return false; // Sản phẩm không có trong Wishlist
        }

        // Lấy danh sách sản phẩm trong Wishlist
        public static async Task<List<Product>> GetWishlistProductsAsync(ShofyContext context, int userId)
        {
            var user = await context.User.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null)
                return new List<Product>();

            var wishlist = GetWishlistProductIds(user.Wishlist);
            return await context.Product
                .Where(p => wishlist.Contains(p.ProductID) && p.Status == "Active")
                .ToListAsync();
        }
        
    }


}