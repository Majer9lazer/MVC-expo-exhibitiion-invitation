using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using iTextSharp.text.pdf;
using LiteDB;
using MVC_Exhibition.Models;
using OfficeOpenXml;

namespace MVC_Exhibition.Controllers
{
    public class UserController : Controller
    {
        private static readonly DirectoryInfo Info = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

        private readonly LiteDatabase _db = new LiteDatabase(Info.FullName + "Expo.db");
        // GET: User
        public ActionResult Register(int statusCode = -1, string message = "")
        {

            if (!_db.GetCollection<Country>("Country").FindAll().Any())
            {
                _db.GetCollection<Country>("Country").Insert(1, new Country() { CountryName = "Казахстан", Cities = null });
            }
            if (!_db.GetCollection<City>("City").FindAll().Any())
            {
                LiteCollection<City> cityCol = _db.GetCollection<City>("City");
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
            ViewBag.UserCountryId = new SelectList(_db.GetCollection<Country>("Country").FindAll(), "CountryId", "CountryName");
            ViewBag.UserCityId = new SelectList(_db.GetCollection<City>("City").FindAll(), "CityId", "CityName");
            ViewBag.statusCode = statusCode;
            ViewBag.message = message;
            return View();
        }

        public async Task<ActionResult> RegisterForm(ExpoUser expoUser)
        {
            int statusCode;
            string message;
            if (ModelState.IsValid)
            {
                using (_db)
                {
                    try
                    {
                        string path;
                        LiteCollection<ExpoUser> liteCollection = _db.GetCollection<ExpoUser>("ExpoUser");
                        ExpoUser findedExpoUser = liteCollection.FindOne(f => f.UserName == expoUser.UserName && f.UserEmail == expoUser.UserEmail && f.UserPhoneNumber == expoUser.UserPhoneNumber);
                        if (findedExpoUser != null)
                        {
                            path = Server.MapPath("/Template/barcode-" + findedExpoUser.Id + ".gif");
                            if (!System.IO.File.Exists(path))
                            {
                                liteCollection.Delete(findedExpoUser.Id);
                            }
                            else
                            {
                                await findedExpoUser.SendMessageAsync(path);
                                return RedirectToAction("UserRegistrationInfo", "User", new { expoUserId = findedExpoUser.Id });
                            }
                        }
                        Guid userIdGuid = Guid.NewGuid();
                        expoUser.Id = userIdGuid;
                        expoUser.DateOfRegistration = DateTime.Now;
                        expoUser.DateOfVisiting = null;
                        Barcode128 code2 = new Barcode128
                        {
                            CodeType = Barcode.CODE128_UCC,
                            ChecksumText = true,
                            GenerateChecksum = true,
                            StartStopText = true,
                            Code = userIdGuid.ToString()
                        };

                        System.Drawing.Bitmap bm2 = new System.Drawing.Bitmap(code2.CreateDrawingImage(System.Drawing.Color.Black, System.Drawing.Color.White));
                        path = Server.MapPath("/Template/barcode-" + expoUser.Id + ".gif");
                        bm2.Save(path, System.Drawing.Imaging.ImageFormat.Gif);

                        liteCollection.Insert(expoUser);
                        liteCollection.EnsureIndex(e => e.Id);
                        await expoUser.SendMessageAsync(path);
                        return RedirectToAction("UserRegistrationInfo", "User", new { expoUserId = expoUser.Id });
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

        public ActionResult Login()
        {

            LiteCollection<Admin> liteCollection = _db.GetCollection<Admin>("Admin");
            List<Admin> admins = liteCollection.FindAll().ToList();
            if (!admins.Any())
            {
                try
                {
                    liteCollection.Insert(new Admin() { Login = "admin", Password = 123.ToString() });
                    liteCollection.EnsureIndex(e => e.Id);
                    admins = liteCollection.FindAll().ToList();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }

            return View(admins.FirstOrDefault());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Admin admin)
        {
            if (ModelState.IsValid)
            {
                LiteCollection<Admin> liteCollection = _db.GetCollection<Admin>("Admin");
                Admin findedAdmin = liteCollection.FindOne(f => f.Login == admin.Login && f.Password == admin.Password);
                if (findedAdmin == null)
                    return HttpNotFound();
                return View("UserInfo", _db.GetCollection<ExpoUser>("ExpoUser").FindAll());

            }

            return View(admin);
        }

        public ActionResult Delete(Guid? userId)
        {
            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int statusCode;
            string message;
            ExpoUser findedExpoUser = _db.GetCollection<ExpoUser>("ExpoUser").FindById(userId);
            if (findedExpoUser == null)
            {
                return HttpNotFound();
            }

            try
            {
                _db.GetCollection<ExpoUser>("ExpoUser").Delete(userId);
                statusCode = 0;
                message = "Удаление прошло успешно";
            }
            catch (Exception e)
            {
                statusCode = 2;
                message = e.ToString();
            }
            return RedirectToAction("UserInfo", "User", new { statusCode, message });
        }

        public ActionResult Details(Guid? userId)
        {
            ExpoUser findedExpoUser = _db.GetCollection<ExpoUser>("ExpoUser").FindById(userId);
            if (findedExpoUser == null)
            {
                return HttpNotFound();
            }
            return View(findedExpoUser);
        }

        public ActionResult UserInfo(int statusCode = -1, string message = "")
        {
            ViewBag.statusCode = statusCode;
            ViewBag.message = message;
            return View(_db.GetCollection<ExpoUser>("ExpoUser").FindAll());
        }

        public async Task<ActionResult> SetActive(Guid? userId)
        {
            int statusCode;
            string message;
            ExpoUser findedExpoUser = _db.GetCollection<ExpoUser>("ExpoUser").FindById(userId);
            if (findedExpoUser == null)
            {
                return HttpNotFound();
            }

            if (findedExpoUser.DateOfVisiting != null)
            {
                statusCode = 1;
                message = "Вы уже прибыли на выставку";
            }
            else
            {
                string subject = "Спасибо за посещение";
                string msg = $"Уважаемый(ая){findedExpoUser.UserName}\n Спасибо за то , что посетили нашу выставку";
                string fromName = "Поддержка по отправке сообщение";

                findedExpoUser.DateOfVisiting = DateTime.Now;

                string mailMessage = await findedExpoUser.SendMessageAsync(subject, msg, fromName);
                _db.GetCollection<ExpoUser>("ExpoUser").Update(userId, findedExpoUser);
                statusCode = 0;
                message = "Вы были активированы. " + mailMessage;
            }

            return RedirectToAction("UserInfo", "User", new { statusCode, message });
        }
        public ActionResult UploadData()
        {
            List<ExpoUser> expoUsers = _db.GetCollection<ExpoUser>("ExpoUser").FindAll().ToList();
            List<Country> countries = _db.GetCollection<Country>("Country").FindAll().ToList();
            List<City> cities = _db.GetCollection<City>("City").FindAll().ToList();
            int statusCode;
            string message;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "Exhibition2018.xlsx")))
            {
                if (!excelPackage.Workbook.Worksheets.Any())
                {
                    statusCode = 1;
                    message = "В файле excel шаблона под выгрузку данных нет! ";
                }
                else
                {
                    ExcelWorksheet exhibitionExcelWorksheet = excelPackage.Workbook.Worksheets.First(a => a.Name.Contains("Exhibition"));
                    int row = 8;
                    int col = 2;
                    exhibitionExcelWorksheet.Cells[3, 3].Value = expoUsers.Count;
                    exhibitionExcelWorksheet.Cells[4, 3].Value = expoUsers.Count(w => w.DateOfVisiting != null);
                    foreach (ExpoUser expoUser in expoUsers)
                    {
                        exhibitionExcelWorksheet.Cells[row, col].Value = expoUser.UserName;
                        exhibitionExcelWorksheet.Cells[row, ++col].Value = expoUser.UserCompanyName;
                        exhibitionExcelWorksheet.Cells[row, ++col].Value = expoUser.UserPhoneNumber;
                        exhibitionExcelWorksheet.Cells[row, ++col].Value = expoUser.UserEmail;
                        exhibitionExcelWorksheet.Cells[row, ++col].Value =
                             countries.Find(f => f.CountryId == expoUser.UserCountryId).CountryName;
                        exhibitionExcelWorksheet.Cells[row, ++col].Value =
                             cities.Find(f => f.CityId == expoUser.UserCityId).CityName;
                        exhibitionExcelWorksheet.Cells[row, ++col].Value = expoUser.DateOfVisiting?.ToShortDateString();

                        col = 2;
                        row++;
                    }
                    try
                    {
                        excelPackage.Save();
                        statusCode = 0;
                        message = "Данные были выгружены успешно.";
                    }
                    catch (Exception e)
                    {

                        statusCode = 2;
                        message = e.ToString();
                    }
                }
            }
            return RedirectToAction("UserInfo", "User", new { statusCode, message });
        }
        public ActionResult UserRegistrationInfo(Guid? expoUserId)
        {
            if (expoUserId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ExpoUser expoUser = _db.GetCollection<ExpoUser>("ExpoUser").FindById(expoUserId);
            return View(expoUser);
        }
        public FileResult GetFile(Guid? userGuid)
        {
            if (userGuid == null)
            {
                return null;
            }
            string filePath = Server.MapPath("~/Template/barcode-"+userGuid+".gif");
            string fileType = "application/gif";
            string fileName = userGuid+".gif";
            return File(filePath, fileType, fileName);
        }

        public ActionResult UserSeeBarCodeSignIn()
        {
            return View();
        }
    }
}