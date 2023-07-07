using EcommerceProject2.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace EcommerceProject2.Controllers
{
    public class CartController : Controller
    {

        // GET: Cart
        public ActionResult GetOrderCartItems()
        {
            int customerId = GetCustomerId();
            if(customerId == 0)
            {
                ViewBag.TotalPrice = "There are no products in your cart.";
            }
            else if (customerId != 0)
            {
                CalculateTotalPrice(customerId);

                using (var entities = new EcommerceDBEntities())
                {
                    //This SQL query command combines the Order_Cart_Table and Products tables to retrieve the customer's order card items.
                    string query = @"SELECT o.* , p.* FROM Order_Cart_Table o JOIN Products p ON o.Product_id = p.Product_id WHERE o.Customer_id = @CustomerId";
                    var orderCartItems = entities.Database.SqlQuery<Order_Cart_Table>(query, new SqlParameter("@CustomerId", customerId)).ToList();

                    //With this loop, get the associated product using a separate SQL query for each order card item. With this command we can reach the Product name with "@item.Products.Product_name"
                    foreach (var CartItem in orderCartItems)
                    {
                        string productQuery = @"SELECT * FROM Products WHERE Product_id = @ProductId";
                        var product = entities.Database.SqlQuery<Products>(productQuery, new SqlParameter("@ProductId", CartItem.Product_id)).FirstOrDefault();
                        CartItem.Products = product;
                    }
                    /* The LINQ Command equivalent of SQL Query 
                        var orderCartItems = entities.Order_Cart_Table
            .Where(o => o.Customer_id == customerId)
            .Include(o => o.Products)
            .ToList();*/

                    return View(orderCartItems);
                }
            }
            return View();
        }

        // POST: Cart/AddToOrderCart
        [HttpPost]
        public ActionResult AddToOrderCart(int productId, int amount, int price)
        {
            int customerId = GetCustomerId();
            if (customerId != 0)
            {

                using (var entities = new EcommerceDBEntities())
                {
                    

                    var orderCart = new Order_Cart_Table
                    {
                        Customer_id = customerId,
                        Order_Total = 0
                    };
                    var parameters = new SqlParameter[]
                    {
                            new SqlParameter("@ProductId", productId),
                            new SqlParameter("@Quantity", amount),
                            new SqlParameter("@CustomerId", customerId),
                            new SqlParameter("@Total",price)
                    };

                    // Ürün var mı diye kontrol etmek için SQL sorgusu
                    string checkQuery = "SELECT COUNT(*) FROM Order_Cart_Table WHERE Product_id = @ProductId AND Customer_id = @CustomerId";

                    int itemCount = entities.Database.SqlQuery<int>(checkQuery, parameters).FirstOrDefault();


                    if (itemCount == 0)
                    {
                        // Ürün sepette yok, ekleme işlemi için SQL sorgusu
                        string insertQuery = "INSERT INTO Order_Cart_Table (Product_id, Amount, Customer_id,Order_Total) VALUES (@ProductId, @Quantity,@CustomerId, @Total)";
                        entities.Database.ExecuteSqlCommand(insertQuery, new SqlParameter("@ProductId", productId),
                        new SqlParameter("@Quantity", amount),
                        new SqlParameter("@CustomerId", customerId),
                        new SqlParameter("@Total", price)
                        );

                    }
                    else
                    {
                        // Ürün zaten sepette, güncelleme işlemi için SQL sorgusu
                        string updateQuery = "UPDATE Order_Cart_Table SET Amount = Amount + @Quantity, Order_Total = @Total WHERE Product_id = @ProductId AND Customer_id = @CustomerId";
                        entities.Database.ExecuteSqlCommand(updateQuery, new SqlParameter("@ProductId", productId),
                         new SqlParameter("@Quantity", amount),
                         new SqlParameter("@CustomerId", customerId),
                         new SqlParameter("@Total", price)
                         );
                    }

                    decimal? totalPrice = CalculateTotalPrice(customerId);
                    ViewBag.CustomerName = customerId;
                    ViewBag.TotalPrice = totalPrice;

                   

                    return RedirectToAction("Index","Home");
                   
                }
            }
                return RedirectToAction("Login","Customer");
         
        }

        // POST: Cart/RemoveFromOrderCart
        [HttpPost]
        public ActionResult RemoveFromOrderCart(int orderCartId)
        {
            
            using (var entities = new EcommerceDBEntities())
            {
                string query2 = "DELETE FROM Order_Table WHERE Order_Cart_id = @OrderCartID";
                entities.Database.ExecuteSqlCommand(query2, new SqlParameter("@OrderCartID", orderCartId));
            }

            using (var entities = new EcommerceDBEntities())
            {
                var orderCart = entities.Order_Cart_Table.Find(orderCartId);

                if (orderCart != null)
                {
                    if (orderCart.Amount > 1)
                    {
                        orderCart.Amount--;
                        entities.SaveChanges();
                    }
                    else
                    {
                        string query = "DELETE FROM Order_Cart_Table WHERE Order_Cart_id = @OrderCartId";
                        entities.Database.ExecuteSqlCommand(query, new SqlParameter("@OrderCartId", orderCartId));

                    }
                }
            }
            int customerId = GetCustomerId();
            
            return RedirectToAction("GetOrderCartItems");
        }

        public ActionResult TotalCount()
        {
            using (var entities = new EcommerceDBEntities())
            {
                if (User.Identity.IsAuthenticated)
                {
                    int customerId = GetCustomerId();
                    var queryy = "SELECT COUNT(Order_Cart_id) FROM Order_Cart_Table WHERE Customer_id =@customerId ";
                    int count = entities.Database.SqlQuery<int>(queryy, new SqlParameter("@customerId", customerId)).FirstOrDefault();

                    ViewBag.Count = count;

                    Debug.WriteLine($"CustomerId: {customerId}");
                    Debug.WriteLine($"Count: {count}");
                }
            }
            return PartialView();
        }

        private decimal? CalculateTotalPrice(int customerId)
        {

            using (var entities = new EcommerceDBEntities())
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@CustomerId", customerId)
                    };

                string totalPriceQuery = "SELECT CAST(SUM(p.price * s.Amount) AS DECIMAL(18, 2)) FROM Order_Cart_Table s INNER JOIN Products p ON s.Product_id = p.Product_id WHERE s.Customer_id = @CustomerId";

                decimal? totalPrice = entities.Database.SqlQuery<decimal?>(totalPriceQuery, parameters).FirstOrDefault();

                if (totalPrice.HasValue && totalPrice != 0)
                {
                    ViewBag.TotalPrice = totalPrice.Value;
                    return totalPrice.Value;

                }
                else
                {
                    ViewBag.TotalPrice ="There are no products in your cart.";
                }

                return 0;
            }


        }
        private int GetCustomerId()
        {
            int customerId = 0;
            if (Session["Customer_id"] != null && int.TryParse(Session["Customer_id"].ToString(), out customerId))
            {
                return customerId;
            }
            return 0;
        }

        public ActionResult OrderComplete()
        {
          
           return View();
        }

        }
    }

