using EcommerceProject2.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.SqlClient;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebGrease.ImageAssemble;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace EcommerceProject2.Controllers
{
    public class OrdersController : Controller
    {
        // GET: Orders
        public ActionResult Index()
        {
            var entities = new EcommerceDBEntities();
            
            var orders = entities.Database.SqlQuery<Order_Table>("SELECT o.* , c.* , p.* FROM Order_Table o JOIN Customer c ON o.Customer_id = c.Customer_id JOIN Products p ON o.Product_id = p.Product_id ").ToList();
            foreach (var order in orders)
            {
                string customerQuery = @"SELECT * FROM Customer WHERE Customer_id = @CustomerID";
                var customer = entities.Database.SqlQuery<Customer>(customerQuery, new SqlParameter("@CustomerID", order.Customer_id)).FirstOrDefault();
                order.Customer = customer;
            }

            foreach (var orderr in orders)
            {
                string ProductQuery = @"SELECT * FROM Products WHERE Product_id = @productID";
                var product = entities.Database.SqlQuery<Products>(ProductQuery, new SqlParameter("@productID", orderr.Product_id)).FirstOrDefault();
                orderr.Products = product;
            }
            TempData["orders"] = orders;
            return View();
        }

        [HttpPost]
        public ActionResult BuyNow()
        {
            int customerID = GetCustomerId();

            using (var entities = new EcommerceDBEntities())
            {
                
                var model = entities.Database.SqlQuery<Order_Cart_Table>("SELECT * FROM Order_Cart_Table WHERE Customer_id = @customerID", 
                                                                                       new SqlParameter("@customerID", customerID)).ToList();
                int row = 0;
                foreach (var item in model)
                {
                    var order = new Order_Table
                    {
                        Customer_id = model[row].Customer_id,
                        Product_id = model[row].Product_id,
                        Order_Cart_id = model[row].Order_Cart_id,
                        amount = model[row].Amount,
                        Total = model[row].Order_Total
                    };




                    string orderdetails = "INSERT INTO Order_Table (Order_Cart_id,Product_id,Customer_id,Total,amount) VALUES (@orderCartId,@productID,@CustomerID,@Total,@amount)";
                    entities.Database.ExecuteSqlCommand(orderdetails,
                       new SqlParameter("@orderCartId", order.Order_Cart_id),
                        new SqlParameter("@productID", order.Product_id),
                         new SqlParameter("@CustomerID", order.Customer_id),
                          new SqlParameter("@Total", order.Total),
                           new SqlParameter("@amount", order.amount));

                    // Update the inventory quantity 
                   var updateQuantityQuery = "UPDATE Inventory_Table SET quantity = quantity - @amount WHERE Inventory_id = (SELECT Inventory_id FROM Products WHERE Product_id = @productID)";
                   entities.Database.ExecuteSqlCommand(updateQuantityQuery,
                        new SqlParameter("@amount", order.amount),
                        new SqlParameter("@productID", order.Product_id));
                    
                    row++;

                }

                

               var query = "DELETE FROM Order_Cart_Table WHERE Customer_id = @customerID";
               entities.Database.ExecuteSqlCommand(query, new SqlParameter("@customerID", customerID));

               entities.SaveChanges();
            
            }

            return RedirectToAction("OrderComplete", "Cart");

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

    }
}