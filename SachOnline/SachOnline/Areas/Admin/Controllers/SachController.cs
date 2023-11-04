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
	public class SachController : Controller
	{
		private dbSachOnlineDataContext db = new dbSachOnlineDataContext("Data Source=DESKTOP-33LF4EI\\SQLEXPRESS;Initial Catalog=SachOnline;Integrated Security=True");
		// GET: Admin/Sach
		public ActionResult Index(int ? page)
		{
			if (Session["Admin"] == null)
			{
				return RedirectToAction("Login", "Admin");
			}
			int iPageNum = (page ?? 1);
			int iPageSize = 7;
			return View(db.SACHes.ToList().OrderBy(n => n.MaSach).ToPagedList(iPageNum,iPageSize));
		}
		[HttpGet]
		public ActionResult Create()
		{
			ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe");
			ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");
			return View();
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Create(SACH sach, FormCollection f, HttpPostedFileBase fFileUpLoad)
		{
			//Đưa dữ liệu vào DropDown
			ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe");
			ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");
			if (fFileUpLoad == null)
			{
				//Nội dung thông báo yêu cầu chọn ảnh bìa
				ViewBag.ThongBao = "Hãy chọn ảnh bìa.";
				//Lưu thông tin để khi load lại trang do yêu cầu chọn ảnh bìa sẽ hiển thị các thông tin này lên trang
				ViewBag.TenSach = f["sTenSach"];
				ViewBag.MoTa = f["sMoTa"];
				ViewBag.SoLuong = int.Parse(f["iSoLuong"]);
				ViewBag.GiaBan = decimal.Parse(f["mGiaBan"]);
				ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe", int.Parse(f["MaCD"]));
				ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB", int.Parse(f["MaNXB"]));
				return View();
			}
			else
			{
				if (ModelState.IsValid)
				{
					//Lấy tên file (Khai báo thư viện: System.IO)
					var sFileName = Path.GetFileName(fFileUpLoad.FileName);
					//Lấy đường dẫn lưu file
					var path = Path.Combine(Server.MapPath("~/Images"), sFileName);
					//Kiểm tra ảnh bìa đã tồn tại chưa để lưu lên thư mục
					if (!System.IO.File.Exists(path))
					{
						fFileUpLoad.SaveAs(path);
					}
					//Lưu Sach vào CSDL
					sach.TenSach = f["sTenSach"];
					sach.MoTa = f["sMota"];
					sach.AnhBia = sFileName;
					sach.NgayCapNhat = Convert.ToDateTime(f["dNgayCapNhat"]);
					sach.SoLuongBan = int.Parse(f["iSoLuong"]);
					sach.GiaBan = decimal.Parse(f["mGiaBan"]);
					sach.MaCD = int.Parse(f["MaCD"]);
					sach.MaNXB = int.Parse(f["MaNXB"]);
					db.SACHes.InsertOnSubmit(sach);
					db.SubmitChanges();
					return RedirectToAction("Index");
				}
				return View();
			}	
		}

		public ActionResult Details(int id)
		{
			var sach = db.SACHes.SingleOrDefault(n => n.MaSach == id);
			if (sach == null)
			{
				Response.StatusCode = 404;
				return null;
			}
			return View(sach);
		}

		[HttpGet]
		public ActionResult Delete(int id)
		{
			var sach = db.SACHes.SingleOrDefault(n => n.MaSach == id);
			if (sach == null)
			{
				Response.StatusCode = 404;
				return null;
			}
			return View(sach);
		}
		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteComfirm(int id, FormCollection f)
		{
			var sach = db.SACHes.SingleOrDefault(n => n.MaSach == id);

			if (sach == null)
			{
				Response.StatusCode = 404;
				return null;
			}
			var ctdh = db.CHITIETDATHANGs.Where(ct => ct.MaSach == id);
			if (ctdh.Count() > 0)
			{
				//Nội dung sẽ hiển thị khi sách cần xóa đã có trong table ChiTietDonHang
				ViewBag.ThongBao = "Sách này đang có trong bảng Chi tiết đặt hàng <br>" + " Nếu muốn xóa thì phải xóa hết mã sách này trong bảng Chi tiết đặt hàng";
				return View(sach);
			}
			//Xóa hết thông tin của cuốn sách trong table VietSach trước khi xóa sách này
			var vietsach = db.VIETSACHes.Where(vs => vs.MaSach == id).ToList();
			if (vietsach != null)
			{
				db.VIETSACHes.DeleteAllOnSubmit(vietsach);
				db.SubmitChanges();
			}
			//Xóa sách
			db.SACHes.DeleteOnSubmit(sach);
			db.SubmitChanges();
			return RedirectToAction("Index");
		}
		[HttpGet]
		public ActionResult Edit(int id)
		{
			var sach = db.SACHes.SingleOrDefault(n => n.MaSach == id);
			if (sach == null)
			{
				Response.StatusCode = 404;
				return null;
			}
			//Hiển thị danh sách chủ đề và nhà xuất bản đồng thời chọn chủ đề và nhà xuất bản của cuốn hiện tại
			ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe", sach.MaCD);
			ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB", sach.MaNXB);
			return View(sach);
		}
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Edit(FormCollection f, HttpPostedFileBase fFileUpload)
		{
			var sach = db.SACHes.SingleOrDefault(n => n.MaSach == int.Parse(f["iMaSach"]));
			ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe", sach.MaCD);
			ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB", sach.MaNXB);
			if (ModelState.IsValid)
			{
				if (fFileUpload != null)//Kiểm tra để xác nhận cho thay đổi ảnh bìa
				{
					//Lấy tên file (Khai báo thư viện: System.IO)
					var sFileName = Path.GetFileName(fFileUpload.FileName);
					//Lấy đường dẫn lưu file
					var path = Path.Combine(Server.MapPath("~/Images"), sFileName);
					//Kiểm tra file đã tồn tại chưa
					if (!System.IO.File.Exists(path))
					{
						fFileUpload.SaveAs(path);
					}
					sach.AnhBia = sFileName;
				}
				//Lưu Sach vào CSDL
				sach.TenSach = f["sTenSach"];
				sach.MoTa = f["sMoTa"];

				sach.NgayCapNhat = Convert.ToDateTime(f["dNgayCapNhat"]);
				sach.SoLuongBan = int.Parse(f["iSoLuong"]);
				sach.GiaBan = decimal.Parse(f["mGiaBan"]);
				sach.MaCD = int.Parse(f["MaCD"]);
				sach.MaNXB = int.Parse(f["MaNXB"]);
				db.SubmitChanges();
				//Về lại trang Quản lý sách
				return RedirectToAction("Index");
			}
			return View(sach);
		}
	}
}