﻿using SachOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SachOnline.Controllers
{
    public class GioHangController : Controller
    {
        // GET: GioHang
        //public ActionResult Index()
        //{
        //    return View();
        //}
        private dbSachOnlineDataContext db = new dbSachOnlineDataContext("Data Source=DESKTOP-33LF4EI\\SQLEXPRESS;Initial Catalog=SachOnline;Integrated Security=True");

        public List<GioHang> LayGioHang()
        {
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang == null)
            {
                lstGioHang = new List<GioHang>();
                Session["GioHang"] = lstGioHang;
            }
            return lstGioHang;
        }

        public ActionResult ThemGioHang(int ms, string url)
        {
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sp = lstGioHang.Find(n => n.iMaSach == ms);
            if (sp == null)
            {
                sp = new GioHang(ms);
                lstGioHang.Add(sp);
            }
            else
            {
                sp.iSoLuong++;
            }
            return Redirect(url);
        }

        private int TongSoLuong()
        {
            int iTongSoLuong = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang != null)
            {
                iTongSoLuong = lstGioHang.Sum(n => n.iSoLuong);
            }
            return iTongSoLuong;
        }

        private double TongTien()
        {
            double dTongTien = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if(lstGioHang != null)
            {
                dTongTien = lstGioHang.Sum(n => n.dThanhTien);
            }
            return dTongTien;
        }
        
        public ActionResult GioHang()
        {
            List<GioHang> lstGioHang = LayGioHang();
            if (lstGioHang.Count == 0)
            {
                return RedirectToAction("Index", "SachOnline");
            }
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();
            return View(lstGioHang);
        }

        public ActionResult GioHangPartial()
        {
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();
            return PartialView();
        }
		public ActionResult XoaSPKhoiGioHang(int iMaSach)
		{
			var gioHang = LayGioHang();
			var sanPham = gioHang.SingleOrDefault(n => n.iMaSach == iMaSach);
			if (sanPham != null)
			{
				gioHang.Remove(sanPham);
				if (gioHang.Count == 0)
				{
					return RedirectToAction("Index", "SachOnline");
				}
			}
			return RedirectToAction("GioHang");
		}
		public ActionResult CapNhatGioHang(int iMaSach, FormCollection f)
		{
			var gioHang = LayGioHang();
			var sanPham = gioHang.SingleOrDefault(n => n.iMaSach == iMaSach);
			if (sanPham != null)
			{
				sanPham.iSoLuong = int.Parse(f["txtSoLuong"].ToString());
			}
			return RedirectToAction("GioHang");
		}
		public ActionResult XoaGioHang()
		{
			var gioHang = LayGioHang();
			gioHang.Clear();
			return RedirectToAction("Index", "SachOnline");
		}
		[HttpGet]
		public ActionResult DatHang()
		{
			if (Session["TaiKhoan"] == null || Session["TaiKhoan"].ToString() == "")
			{
				return RedirectToAction("DangNhap", "User");
			}
			if (Session["GioHang"] == null)
			{
				return RedirectToAction("Index", "SachOnline");
			}
			List<GioHang> lstGioHang = LayGioHang();
			ViewBag.TongSoLuong = TongSoLuong();
			ViewBag.TongTien = TongTien();
			return View(lstGioHang);
		}
		[HttpPost]
		public ActionResult DatHang(FormCollection f)
		{
			DONDATHANG ddh = new DONDATHANG();
			KHACHHANG kh = (KHACHHANG)Session["TaiKhoan"];
			List<GioHang> lstGioHang = LayGioHang();
			ddh.MaKH = kh.MaKH;
			ddh.NgayDat = DateTime.Now;
			var NgayGiao = String.Format("{0:MM/dd/yyyy}", f["NgayGiao"]);
			ddh.NgayGiao = DateTime.Parse(NgayGiao);
			ddh.TinhTrangGiaoHang = 1;
			ddh.DaThanhToan = false;
			db.DONDATHANGs.InsertOnSubmit(ddh);
			db.SubmitChanges();
			// Thêm chi tiết đơn hàng
			foreach (var item in lstGioHang)
			{
				CHITIETDATHANG ctdh = new CHITIETDATHANG();
				ctdh.MaDonHang = ddh.MaDonHang;
				ctdh.MaSach = item.iMaSach;
				ctdh.SoLuong = item.iSoLuong;
				ctdh.DonGia = (decimal)item.dDonGia;
				db.CHITIETDATHANGs.InsertOnSubmit(ctdh);
			}
			db.SubmitChanges();
			Session["GioHang"] = null;
			return RedirectToAction("XacNhanDonHang", "GioHang");
		}
		// XacNhanDonHang
		public ActionResult XacNhanDonHang()
		{
			return View();
		}
	}
}