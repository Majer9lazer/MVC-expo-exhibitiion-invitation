using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text.pdf;
using LiteDB;
using MVC_Exhibition.Models;

namespace MVC_Exhibition.Controllers
{
    public class UserController : Controller
    {
        private static readonly DirectoryInfo Info = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

        private readonly LiteDatabase _db = new LiteDatabase(Info.FullName + "Expo.db");
        // GET: User
        public ActionResult Register(int statusCode = -1, string message = "")
        {
            ViewBag.UserCountryId = new SelectList(_db.GetCollection<Country>("Country").FindAll(), "CountryId", "CountryName");
            if (_db.GetCollection<City>("City").FindAll().Any())
            {

            }
            else
            {
                var cityCol = _db.GetCollection<City>("City");
                City almatyCity = new City()
                {
                    CitiesCountry = new Country()
                    {
                        CountryName = "Казахстан",
                        CountryId = 1,
                        Cities = null
                    },
                    CityId = 1,
                    CityName = "Алматы"
                };
                cityCol.EnsureIndex(f => f.CityId);
                cityCol.Insert(almatyCity);
            }
            ViewBag.UserCityId = new SelectList(_db.GetCollection<City>("City").FindAll(), "CityId", "CityName");
            ViewBag.statusCode = statusCode;
            ViewBag.message = message;
            return View();
        }

        public ActionResult RegisterForm(ExpoUser expoUser)
        {
            int statusCode;
            string message;
            if (ModelState.IsValid)
            {
                using (_db)
                {
                    try
                    {


                        Barcode128 code2 = new Barcode128();
                        code2.CodeType = Barcode.CODE128_UCC;
                        code2.ChecksumText = true;
                        code2.GenerateChecksum = true;
                        code2.StartStopText = true;
                        code2.Code = Guid.NewGuid().ToString();

                        System.Drawing.Bitmap bm2 = new System.Drawing.Bitmap(code2.CreateDrawingImage(System.Drawing.Color.Black, System.Drawing.Color.White));
                        bm2.Save(Server.MapPath("/Template/barcode" + expoUser.Id + ".gif"), System.Drawing.Imaging.ImageFormat.Gif);

                        var liteCollection = _db.GetCollection<ExpoUser>("ExpoUser");
                        int maxid = 1;
                        if (liteCollection.FindAll().Any())
                        {
                            maxid = liteCollection.FindAll().Max(f => f.Id) + 1;
                        }
                        expoUser.Id = maxid;
                        liteCollection.Insert(expoUser);
                        liteCollection.EnsureIndex(e => e.Id);
                        statusCode = 0;
                        message = "Пользователь был зарегестрирован успешно.";
                    }
                    catch (Exception e)
                    {
                        statusCode = 2;
                        message = e.ToString();
                    }
                }
            }
            else
            {
                statusCode = 1;
                message = "Данные пришли пустыми";
            }
            return RedirectToAction("Register", "User", new { statusCode, message });
        }
    }
}