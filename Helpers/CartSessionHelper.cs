using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Shofy.Models;
using System.Collections.Generic;

namespace Shofy.Helpers
{
    public static class CartSessionHelper
    {
        private const string CartKey = "Cart";

        public static void SetCart(this ISession session, List<CartItem> cartItems)
        {
            session.SetString(CartKey, JsonConvert.SerializeObject(cartItems));
        }

        public static List<CartItem> GetCart(this ISession session)
        {
            var cartJson = session.GetString(CartKey);
            return string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
        }

        public static void ClearCart(this ISession session)
        {
            session.Remove(CartKey);
        }
    }
}
