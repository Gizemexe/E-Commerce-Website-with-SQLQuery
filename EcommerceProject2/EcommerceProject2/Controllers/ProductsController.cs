using EcommerceProject2.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Xml.XPath;
using static System.Net.WebRequestMethods;

namespace EcommerceProject2.Controllers
{
    
    public class ProductsController : Controller
    {
       
        // GET: Products
        public ActionResult Index()
        {
            var entities = new EcommerceDBEntities();

            var productList = entities.Products
                    .SqlQuery("Select * from Products")
                    .ToList<Products>();

            TempData["products"] = productList;

            return View();
        }
        [HttpGet]
        public ActionResult Products()
        {
            EcommerceDBEntities entities = new EcommerceDBEntities();

            List<SelectListItem> CategoryList = (from i in entities.Category.ToList()
                                                 select new SelectListItem
                                                 {
                                                     Text = i.Category_name,
                                                     Value = i.Category_id.ToString(),
                                                 }).ToList();

            List<SelectListItem> stock = (from m in entities.Inventory_Table.ToList()
                                          select new SelectListItem
                                          {
                                              Text = m.quantity.ToString(),
                                              Value = m.Inventory_id.ToString(),
                                          }).ToList();

            ViewBag.CategoryList = CategoryList;
            ViewBag.Stock = stock;
            return View();
        }

        [HttpPost]
        public ActionResult Products(Products product, HttpPostedFileBase img)
        {


            using (var entities = new EcommerceDBEntities())
            {

                string path = uploadimage(img);
               

                var categories = entities.Category.Where(i => i.Category_id == product.Category.Category_id).FirstOrDefault();
                product.Category = categories;

                var stock = entities.Inventory_Table.Where(i => i.Inventory_id == product.Inventory_Table.Inventory_id).FirstOrDefault();
                product.Inventory_Table = stock;

              


                var name = new SqlParameter("@proname", product.Product_name);
                var image = new SqlParameter("@path", product.image = path);
                var desc = new SqlParameter("@prodesc", product.Product_Desc);
                var price = new SqlParameter("@price", product.price);
                var catID = new SqlParameter("@cat_id", product.Category.Category_id);
                var InvID = new SqlParameter("@Invent_id", product.Inventory_Table.Inventory_id);
                var ID = new SqlParameter("@id", product.Admin_id = Convert.ToInt32(Session["Admin_id"].ToString()));

                entities.Database.ExecuteSqlCommand("INSERT INTO Products(Product_name,Product_Desc,price,image,Category_id,Inventory_id,Admin_id) VALUES(@proname,@prodesc,@price,@path,@cat_id,@Invent_id,@id)", name, desc, price, image, catID, InvID, ID);


            

                    return RedirectToAction("Index");

                
            }
            
        }
        [HttpGet]
        public ActionResult DeleteProducts() {

            return View();
        }
        [HttpPost]
        public ActionResult DeleteProducts(int productID)
        {
            using (EcommerceDBEntities entities = new EcommerceDBEntities())
            {
               //var product = entities.Products.Find(productID); 

                string sqlQuery = "DELETE FROM Products WHERE Product_id = @productId";
                SqlParameter productIdParam = new SqlParameter("@productId", productID);

                
                entities.Database.ExecuteSqlCommand(sqlQuery, productIdParam);


                return RedirectToAction("Index");
            }
            
        }
        [HttpGet]
        public ActionResult UpdateProducts(int productID)
        {
            
            EcommerceDBEntities entities = new EcommerceDBEntities();

            List<SelectListItem> CategoryList = (from i in entities.Category.ToList()
                                                 select new SelectListItem
                                                 {
                                                     Text = i.Category_name,
                                                     Value = i.Category_id.ToString(),
                                                 }).ToList();

            List<SelectListItem> stock = (from m in entities.Inventory_Table.ToList()
                                          select new SelectListItem
                                          {
                                              Text = m.quantity.ToString(),
                                              Value = m.Inventory_id.ToString(),
                                          }).ToList();

            ViewBag.CategoryList = CategoryList;
            ViewBag.Stock = stock;

            string sqlCommand = "SELECT * FROM Products WHERE Product_id = @productID";
            var p = entities.Database.SqlQuery<Products>(sqlCommand, new SqlParameter("@productID", productID)).SingleOrDefault();


            return View(p);
        }
        [HttpPost]
        public ActionResult UpdateProducts(Products product, HttpPostedFileBase img)
        {
            using (EcommerceDBEntities entities = new EcommerceDBEntities())
            {
                string path = uploadimage(img);

                var existingProduct = entities.Database.SqlQuery<Products>("SELECT * FROM Products WHERE Product_id = @productId", new SqlParameter("@productId", product.Product_id)).FirstOrDefault();

                if (existingProduct != null)
                {
                    existingProduct.Product_name = product.Product_name;
                    existingProduct.Product_Desc = product.Product_Desc;
                    existingProduct.price = product.price;
                    existingProduct.image = path;
                    existingProduct.Category_id = product.Category.Category_id;
                    existingProduct.Inventory_id = product.Inventory_Table.Inventory_id;
                    object adminIdObject = Session["Admin_id"];
                    int adminId = 0;

                    if (adminIdObject != null && int.TryParse(adminIdObject.ToString(), out adminId))
                    {
                        existingProduct.Admin_id = adminId;
                    }

                    string updateQuery = "UPDATE Products SET Product_name = @productName, Product_Desc = @productDesc, price = @productPrice, image = @imagePath, Category_id = @categoryId, Inventory_id = @inventoryId, Admin_id = @AdminID WHERE Product_id = @productId";

                    SqlParameter[] parameters =
                    {
                new SqlParameter("@productName", existingProduct.Product_name),
                new SqlParameter("@productDesc", existingProduct.Product_Desc),
                new SqlParameter("@productPrice", existingProduct.price),
                new SqlParameter("@categoryId", existingProduct.Category_id),
                new SqlParameter("@inventoryId", existingProduct.Inventory_id),
                new SqlParameter("@imagePath", existingProduct.image),
                new SqlParameter("@AdminID", existingProduct.Admin_id),
                new SqlParameter("@productId", existingProduct.Product_id)
            };

                    int rowsAffected = entities.Database.ExecuteSqlCommand(updateQuery, parameters);

                    entities.SaveChanges();

                    if (rowsAffected > 0)
                    {
                        // Güncelleme başarılı
                        ViewBag.Message = "Product updated successfully.";
                    }
                    else
                    {
                        // Güncelleme başarısız
                        ViewBag.Message = "Failed to update product.";
                    }
                    return RedirectToAction("Index");
                }

                return View();
            }
        }





        //UPLOAD IMAGE CODE//
        public string uploadimage(HttpPostedFileBase file)
        {
            Random r = new Random();

            string path = "-1";

            int random = r.Next();

            if (file != null && file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName);
                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                {
                    try
                    {
                        string filename = random + Path.GetFileName(file.FileName);
                        string serverPath = Server.MapPath("~/Content/Images/");
                        path = Path.Combine(serverPath, filename);
                        file.SaveAs(path);
                        path = "Content/Images/" + filename;
                    }
                    catch (Exception ex)
                    {
                        path = "-1";

                        throw;
                    }
                }

                else
                {
                    Response.Write("<script>alert('Only .JPG , .JPEG , .PNG Formats Are Allowed')</script>");
                }

            }

            else
            {
                Response.Write("<script>alert('Please Select a File')</script>");

                path = "-1";
            }

            return path;
        }
    }
}