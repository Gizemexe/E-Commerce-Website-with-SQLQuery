using EcommerceProject2.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EcommerceProject2.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            var entities = new EcommerceDBEntities();

            var productList = entities.Products
                    .SqlQuery("Select * from Products")
                    .ToList<Products>();

            TempData["products"] = productList;
            return View();
        }

        public ActionResult Details(int id)
        {
            var entities = new EcommerceDBEntities();

            var query = "SELECT TOP 1 * FROM Products WHERE Product_id = @id";
            var parameters = new SqlParameter("@id", id);
            var result = entities.Database.SqlQuery<Products>(query, parameters).FirstOrDefault();

            return View(result);
        }

        public ActionResult Contact(int id)
        {
            
            var entities = new EcommerceDBEntities();

                string query = "SELECT * FROM Customer WHERE Customer_id = @CustomerId";
                var parameters = new SqlParameter("@CustomerId", id);
                var info = entities.Database.SqlQuery<Customer>(query, parameters).FirstOrDefault();

                return View(info);
        }

        public ActionResult AdminContact(int id)
        {
            
            var entities = new EcommerceDBEntities();

            string query = "SELECT * FROM Admin WHERE Admin_id = @adminID";
            var parameters = new SqlParameter("@adminID", id);
            var info = entities.Database.SqlQuery<Admin>(query, parameters).FirstOrDefault();

            return View(info);
        }
        public ActionResult Categories(int id)
        {
            var entities = new EcommerceDBEntities();
            List<Products> productsList;
            switch (id)
            {
                case 14:
                    productsList = entities.Products
                                        .SqlQuery("SELECT * FROM Products WHERE Category_id = @id", new SqlParameter("@id", id))
                                        .ToList<Products>();
                    TempData["product"] = productsList;
                    break;
                case 15:
                    productsList = entities.Products
                                       .SqlQuery("SELECT * FROM Products WHERE Category_id = @id", new SqlParameter("@id", id))
                                       .ToList<Products>();
                    TempData["product"] = productsList;
                    break;
                case 16:
                    productsList = entities.Products
                                        .SqlQuery("SELECT * FROM Products WHERE Category_id = @id", new SqlParameter("@id", id))
                                        .ToList<Products>();
                    TempData["product"] = productsList;
                    break;
                case 17:
                    productsList = entities.Products
                                        .SqlQuery("SELECT * FROM Products WHERE Category_id = @id", new SqlParameter("@id", id))
                                        .ToList<Products>();
                    TempData["product"] = productsList;
                    break;
                case 18:
                    productsList = entities.Products
                                        .SqlQuery("SELECT * FROM Products WHERE Category_id = @id", new SqlParameter("@id", id))
                                        .ToList<Products>();
                    TempData["product"] = productsList;
                    break;
                case 19:
                    productsList = entities.Products
                                        .SqlQuery("SELECT * FROM Products WHERE Category_id = @id", new SqlParameter("@id", id))
                                        .ToList<Products>();
                    TempData["product"] = productsList;
                    break;

            }


            return View();
        }
    }
}