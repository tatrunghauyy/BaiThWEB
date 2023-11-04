using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SachOnline.Models;
using PagedList;
using PagedList.Mvc;
using System.IO;

namespace SachOnline.Areas.Admin.Controllers
{
	public class ChuDeController : Controller
	{
		private dbSachOnlineDataContext db = new dbSachOnlineDataContext("Data Source=DESKTOP-33LF4EI\\SQLEXPRESS;Initial Catalog=SachOnline;Integrated Security=True");
		// GET: Admin/ChuDe
		public ActionResult ChuDe(int? page)
		{
			if (Session["Admin"] == null)
			{
				return RedirectToAction("Login", "Admin");
			}
			int iPageNum = (page ?? 1);
			int iPageSize = 7;
			return View(db.CHUDEs.ToList().OrderBy(n => n.MaCD).ToPagedList(iPageNum, iPageSize));
		}
		[HttpGet]
		public ActionResult Create()
		{
			if (Session["Admin"] == null)
			{
				return RedirectToAction("Login", "Admin");
			}
			ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe");
			return View();
		}
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Create(CHUDE chude, FormCollection f, HttpPostedFileBase fFileUpLoad)
		{
			ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe");

			if (ModelState.IsValid)
			{
				// Kiểm tra xem TenChuDe đã tồn tại trong cơ sở dữ liệu hay chưa
				var existingChuDe = db.CHUDEs.FirstOrDefault(c => c.TenChuDe == chude.TenChuDe);

				if (existingChuDe != null)
				{
					ModelState.AddModelError("TenChuDe", "Tên chủ đề đã tồn tại");
					return View(chude);
				}
				else
				{
					// Nếu TenChuDe chưa tồn tại, thêm mới
					db.CHUDEs.InsertOnSubmit(chude);
					db.SubmitChanges();
					return RedirectToAction("ChuDe");
				}
			}
			return View(chude);
		}
		[HttpGet]
		public ActionResult Delete(int id)
		{
			var chude = db.CHUDEs.SingleOrDefault(n => n.MaCD == id);
			if (chude == null)
			{
				Response.StatusCode = 404;
				return null;
			}
			return View(chude);
		}
		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteComfirm(int id, FormCollection f)
		{
			var chude = db.CHUDEs.SingleOrDefault(n => n.MaCD == id);

			if (chude == null)
			{
				Response.StatusCode = 404;
				return null;
			}
			var cdcs = db.SACHes.Where(s => s.MaCD == id);
			if (cdcs.Count() > 0)
			{
				ViewBag.ThongBao = "Đang có sách thuộc chủ đề này <br>" + " Nếu muốn xóa thì phải xóa hết mã chủ đề hoặc thay đổi mã chủ đề này trong bảng Sách";
				return View(cdcs);
			}
			db.CHUDEs.DeleteOnSubmit(chude);
			db.SubmitChanges();
			return RedirectToAction("ChuDe");
		}

	}
}