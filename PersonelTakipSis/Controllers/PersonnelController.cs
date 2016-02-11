using PersonelTakipSis.Models;
using PTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PersonelTakipSis.Controllers
{
    public class PersonnelController : Controller
    {
        PetaPoco.Database db = PtsDb.Instance;

        //
        // GET: /Personnel/

        public ActionResult Index()
        {
            User user = Session["User"] as User;
            ViewBag.Username = user.Username;

            //Personel List
            var results = db.Fetch<Personnel>(PetaPoco.Sql.Builder
                                              .Append("Select p.*, d.DepName as DepartmentName")
                                              .Append("From Personnel p ")
                                              .Append("Join Department d On p.DepartmentID = d.ID")
                                              .Append("Where p.isActive = @0", 1));

            return View(results);
        }

        public ActionResult FormerPersonnels()
        {
            User user = Session["User"] as User;
            ViewBag.Username = user.Username;

            //Former Personel List
            var results = db.Fetch<Personnel>(PetaPoco.Sql.Builder
                                              .Append("Select p.*, d.DepName as DepartmentName")
                                              .Append("From Personnel p ")
                                              .Append("Join Department d On p.DepartmentID = d.ID")
                                              .Append("Where p.isActive = @0", 0));
            return View(results);
        }



        // Create new personnel
        [HttpGet]
        public ActionResult Create()
        {
            return View(new Personnel());
        }

        [HttpPost]
        public ActionResult Create(Personnel personnel, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //image upload
                    if (file == null || !isImageValid(file))
                    {
                        ModelState.AddModelError(string.Empty, string.Empty);
                    }
                    else
                    {
                        isActivePersonel(personnel);
                        db.Insert(personnel);

                        //Save Image File To Local
                        string pic = "Image_" + personnel.ID + ".png";
                        var image = System.Drawing.Image.FromStream(file.InputStream);
                        string path = System.IO.Path.Combine(HttpContext.Server.MapPath("~/Uploads/Images"), pic);
                        file.SaveAs(path);

                        personnel.PhotoPath = "~/Uploads/Images/" + pic;
                        db.Update(personnel);

                        return RedirectToAction("Index");
                    }

                }
                catch (Exception)
                {
                    throw;
                }
            }
            return View(personnel);

        }

        private void isActivePersonel(Personnel personnel)
        {
            if (personnel.ExitDate.HasValue)
            {
                personnel.isActive = false;
            }
            else
            {
                personnel.isActive = true;
            }
        }


        //Edit Personnel
        [HttpGet]
        public ActionResult Edit(int ID)
        {
            var personnel = getPersonnel(ID);
            return View(personnel);
        }

        [HttpPost]
        public ActionResult Edit(Personnel personnel, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                if ((file != null && isImageValid(file)) || (file == null))
                {
                    personnel.PhotoPath = getPersonnel(personnel.ID).PhotoPath;
                    try
                    {
                        isActivePersonel(personnel);
                        db.Update(personnel);
                        return RedirectToAction("Index");
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else if (file != null && !isImageValid(file))
                {
                    ModelState.AddModelError(String.Empty, String.Empty);
                }

            }
            else
            {
                personnel.PhotoPath = getPersonnel(personnel.ID).PhotoPath;
            }
            return View(personnel);

        }

        // Delete Personnel
        public ActionResult Delete(int ID)
        {
            try
            {
                var personnel = getPersonnel(ID);
                deletePersonnelFiles(personnel);
                db.Delete(personnel);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Delete Former Personnel
        public ActionResult DeleteFormer(int ID)
        {
            try
            {
                var personnel = getPersonnel(ID);
                deletePersonnelFiles(personnel);
                db.Delete(personnel);
                return RedirectToAction("FormerPersonnels");
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Delete Personnel Files
        private void deletePersonnelFiles(Personnel personnel)
        {
            string imagePath = System.IO.Path.Combine(HttpContext.Server.MapPath(personnel.PhotoPath));
            string adliSicilKaydiPath = System.IO.Path.Combine(HttpContext.Server.MapPath(personnel.AdliSicilKaydi));
            string saglikRaporuPath = System.IO.Path.Combine(HttpContext.Server.MapPath(personnel.SaglikRaporu));
            string diplomaPath = System.IO.Path.Combine(HttpContext.Server.MapPath(personnel.Diploma));

            if (System.IO.File.Exists(imagePath)) { System.IO.File.Delete(imagePath); }
            if (System.IO.File.Exists(adliSicilKaydiPath)) { System.IO.File.Delete(adliSicilKaydiPath); }
            if (System.IO.File.Exists(saglikRaporuPath)) { System.IO.File.Delete(saglikRaporuPath); }
            if (System.IO.File.Exists(diplomaPath)) { System.IO.File.Delete(diplomaPath); }

        }

        //Upload
        [HttpGet]
        public ActionResult Uploads(int ID)
        {
            var personnel = getPersonnel(ID);
            return View(personnel);
        }

        [HttpPost]
        public ActionResult Uploads(Personnel personnel, List<HttpPostedFileBase> files)
        {
            try
            {
                Personnel getPerson = getPersonnel(personnel.ID);
                for (int i = 0; i < files.Count(); i++)
                {
                    if (files[i] != null)
                    {
                        SaveUploads(getPerson, files[i], i);
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {

                throw;
            }
        }

        public FileResult Downloads(string filePath)
        {
            //string fullPath = System.IO.Path.Combine(HttpContext.Server.MapPath(filePath));
            return File(filePath, "application/force-download", System.IO.Path.GetFileName(filePath));
        }

        //QuitJob
        public ActionResult QuitJob(int ID)
        {
            try
            {
                var personnel = getPersonnel(ID);
                personnel.ExitDate = DateTime.Now;
                personnel.isActive = false;
                db.Update(personnel);
                return RedirectToAction("Index", "Personnel");
            }
            catch (Exception)
            {

                throw;
            }
        }


        private Personnel getPersonnel(int ID)
        {
            var personnel = db.SingleOrDefault<Personnel>("Select p.*, d.DepName DepartmentName From Personnel p Join Department d On p.DepartmentID = d.ID Where p.ID = @0", ID);
            return personnel;
        }

        // Is it Image file or not
        private bool isImageValid(HttpPostedFileBase file)
        {
            var supportedTypes = new[] { "jpg", "jpeg", "png" };
            var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);

            if (supportedTypes.Contains(fileExt))
            {
                return true;
            }
            return false;

        }

        // Save Personnel's Files
        private void SaveUploads(Personnel personnel, HttpPostedFileBase file, int index)
        {
            string uploadFile;
            switch (index)
            {
                case 0:
                    uploadFile = "AdliSicilKaydi";
                    break;
                case 1:
                    uploadFile = "SaglikRaporu";
                    break;
                case 2:
                    uploadFile = "Diploma";
                    break;
                default:
                    uploadFile = "";
                    break;
            }

            string extension = System.IO.Path.GetExtension(file.FileName);
            string mapPath = "~/Uploads/" + uploadFile;
            string filePathForDb = uploadFile + "_" + personnel.ID + extension;

            //var fileName = System.IO.Path.GetFileName(file.FileName);
            var path = System.IO.Path.Combine(Server.MapPath(mapPath), filePathForDb);
            file.SaveAs(path);


            switch (index)
            {
                case 0:
                    personnel.AdliSicilKaydi = mapPath + "/" + filePathForDb;
                    break;
                case 1:
                    personnel.SaglikRaporu = mapPath + "/" + filePathForDb;
                    break;
                case 2:
                    personnel.Diploma = mapPath + "/" + filePathForDb;
                    break;
            }

            db.Update(personnel);


        }
    }
}
