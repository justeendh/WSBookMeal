using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookMeal.Models;
using WiseOffice;
using SocketCommunicate;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Text;

namespace BookMeal.Controllers
{
    public class HomeController : Controller
    {
        private bool CheckLogin()
        {
            USER_LOGIN userLogin = Session["USER_LOGIN"] as USER_LOGIN;
            if (userLogin == null) return false;
            else return true;
        }

        [HttpPost]
        public ActionResult Keepalive()
        {            
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult Loadtableaction(string actionid, Guid? sourceid)
        {
            EntitiesConnection dbContext = new EntitiesConnection();
            var lstMaKhuVuc = dbContext.KHU_VUC.OrderBy(a => a.MA_KHU_VUC).ToList();
            List<BAN_MAM> lstTables = new List<BAN_MAM>();
            switch(actionid)
            {
                case "CHUYEN_BAN":
                    lstTables = dbContext.BAN_MAM.Where(a => (a.DA_CO_KHACH == null || a.DA_CO_KHACH == false) && a.KEY_BAN_MAM != sourceid).ToList();
                    break;
                case "TACH_BAN":
                    lstTables = dbContext.BAN_MAM.Where(a => (a.DA_CO_KHACH == null || a.DA_CO_KHACH == false) && a.KEY_BAN_MAM != sourceid).ToList();
                    break;
                case "GHEP_BAN":
                    lstTables = dbContext.BAN_MAM.Where(a => a.DA_CO_KHACH == true && a.KEY_BAN_MAM != sourceid).ToList();
                    break;
                default:
                    break;
            }
            ViewBag.KhuVuc = lstMaKhuVuc;
            return PartialView("~/Views/_table_action.cshtml", lstTables);
        }

        [HttpPost]
        public ActionResult Submittableaction(string actionid, Guid? sourceid, Guid? selectedTableAction)
        {
            using (var dbContext = new EntitiesConnection())
            {
                using (var dbContextTransaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        BAN_MAM Source = dbContext.BAN_MAM.FirstOrDefault(a => a.KEY_BAN_MAM == sourceid);
                        HOA_DON HoaDon = dbContext.HOA_DON.FirstOrDefault(a => a.KEY_HOA_DON == Source.KEY_HOA_DON);
                        if (HoaDon == null) return Json(new { success = false, msg = "Bàn được chọn chưa có dữ liệu chọn món" });
                        BAN_MAM Target = dbContext.BAN_MAM.FirstOrDefault(a => a.KEY_BAN_MAM == selectedTableAction);
                        if (selectedTableAction == null) return Json(new { success = false, msg = "Vui lòng chọn một bàn để thực hiện !" });
                        switch (actionid)
                        {
                            case "CHUYEN_BAN":
                                Target.KEY_HOA_DON = Source.KEY_HOA_DON;
                                Target.DA_CO_KHACH = true;
                                Source.KEY_HOA_DON = null;
                                Source.DA_CO_KHACH = false;
                                HoaDon.KEY_BAN_MAM = Target.KEY_BAN_MAM;
                                dbContext.SaveChanges();
                                dbContextTransaction.Commit();
                                return Json(new { success = true });
                            //case "TACH_BAN":
                            //    lstTables = dbContext.BAN_MAM.Where(a => (a.DA_CO_KHACH == null || a.DA_CO_KHACH == false) && a.KEY_BAN_MAM != sourceid).ToList();
                            //    break;
                            case "GHEP_BAN":
                                HOA_DON HoaDonTarget = dbContext.HOA_DON.FirstOrDefault(a => a.KEY_HOA_DON == Target.KEY_HOA_DON);
                                List<BILL_MEAL> BillMealSource = dbContext.BILL_MEAL.Where(a => a.KEY_HOA_DON == HoaDon.KEY_HOA_DON).ToList();
                                short? MaxID = BillMealSource.Max(a => a.INDEX_SAP_XEP);
                                short newSTT = (short)(MaxID == null ? 1 : MaxID + 1);
                                if(HoaDonTarget != null)
                                {
                                    foreach (var itemBillMeal in BillMealSource)
                                    {
                                        itemBillMeal.KEY_HOA_DON = HoaDonTarget.KEY_HOA_DON;
                                        itemBillMeal.INDEX_SAP_XEP = newSTT;
                                        HoaDonTarget.TONG_PHAI_TRA += itemBillMeal.TIEN_PHAI_TRA;
                                        newSTT++;
                                    }
                                    dbContext.HOA_DON.Attach(HoaDon);
                                    dbContext.HOA_DON.Remove(HoaDon);
                                }
                                else
                                {
                                    HoaDon.KEY_BAN_MAM = Target.KEY_BAN_MAM;
                                    Target.KEY_HOA_DON = HoaDon.KEY_HOA_DON;
                                }

                                Source.KEY_HOA_DON = null;
                                Source.DA_CO_KHACH = false;
                                dbContext.SaveChanges();
                                dbContextTransaction.Commit();
                                return Json(new { success = true });
                            default:
                                dbContextTransaction.Rollback();
                                return Json(new { success = false, msg = "Không nhận được lệnh phù hợp" });
                        }
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return Json(new { success = false, msg = "Đã có lỗi xảy ra khi thực hiện" });
                    }
                }
            }
        }
        

        public ActionResult Index(string khuvuc)
        {
            if(!CheckLogin()) return Redirect("/user/");
            USER_LOGIN userLogin = Session["USER_LOGIN"] as USER_LOGIN;
            EntitiesConnection dbContext = new EntitiesConnection();
            var lstMaKhuVuc = dbContext.KHU_VUC.OrderBy(a => a.MA_KHU_VUC).ToList();
            var lstKhuVucView = dbContext.USER_ZONES.Where(a => a.KEY_USER_LOGIN == userLogin.KEY_USER_LOGIN);
            List<BAN_MAM> lstTables = null;
            if(lstKhuVucView != null)
            {
                var lstKhuVucViewable = lstKhuVucView.Select(a => a.MA_KHU_VUC).ToArray();
                lstMaKhuVuc = lstMaKhuVuc.Where(a => lstKhuVucViewable.Contains(a.MA_KHU_VUC)).ToList();
                if(khuvuc == null || khuvuc == "all") lstTables = dbContext.BAN_MAM.Where(a => lstKhuVucViewable.Contains(a.MA_KHU_VUC)).ToList();
                else lstTables = dbContext.BAN_MAM.Where(a => lstKhuVucViewable.Contains(a.MA_KHU_VUC) && a.MA_KHU_VUC == khuvuc).ToList();
            }
            else
            {
                lstMaKhuVuc = new List<KHU_VUC>();
                lstTables = new List<BAN_MAM>();
            }
            
            ViewBag.KhuVuc = lstMaKhuVuc;
            return View(lstTables);
        }

        public string Number()
        {
            EntitiesConnection dbContext = new EntitiesConnection();
            string SoChungTu = dbContext.HOA_DON.Max(a => a.MA_HOA_DON);
            if (string.IsNullOrEmpty(SoChungTu))
                SoChungTu = "0000001";
            else
            {
                long So;
                if (long.TryParse(SoChungTu, out So))
                {
                    So++;
                    SoChungTu = So.ToString("000000#");
                }
            }
            return SoChungTu;
        }

        public ActionResult Open(Guid? tableid)
        {
            if (!CheckLogin()) return Redirect("/user/");
            EntitiesConnection dbContext = new EntitiesConnection();
            BAN_MAM itemBanMam = dbContext.BAN_MAM.FirstOrDefault(a => a.KEY_BAN_MAM == tableid);
            if (itemBanMam != null)
            {
                if (itemBanMam.DA_CO_KHACH != null && itemBanMam.DA_CO_KHACH.Value && itemBanMam.KEY_HOA_DON != null)
                {
                    return RedirectToAction("Index");
                }
            }

            USER_LOGIN userLogin = Session["USER_LOGIN"] as USER_LOGIN;
            Guid KeyHoaDon = Guid.NewGuid();
            HOA_DON itemHoaDon = new HOA_DON();
            itemHoaDon.KEY_HOA_DON = KeyHoaDon;
            itemHoaDon.KEY_BAN_MAM = tableid;
            itemHoaDon.HSO_CLIENT_CARDS = 0;
            itemHoaDon.VAL_LOAI_COUPON = 0;
            itemHoaDon.TIEN_THU_VAO = 0;
            itemHoaDon.TONG_KHUYEN_MAI = 0;
            itemHoaDon.LAN_IN_PHIEU = 0;
            itemHoaDon.TONG_GIO_USE = 0;
            itemHoaDon.TONG_TIEN_BO = 0;
            itemHoaDon.TONG_TANG_GIA = 0;
            itemHoaDon.DA_THU_TIEN = false;
            itemHoaDon.DA_CHE_BIEN = false;
            itemHoaDon.KEY_USER_LOGIN = userLogin.KEY_USER_LOGIN;
            Session["DA_PHA_CHE_COBO_" + tableid.ToString()] = false;
            Session[string.Format("KEY_HOA_DON_{0}", tableid.ToString())] = itemHoaDon;
            Session["MON_SELECTED_" + tableid.ToString()] = new List<BILL_MEAL>();
            Session["COMBO_SELECTED_" + tableid.ToString()] = null;
            return RedirectToAction("Bill", new { isopentable = true, tableid = tableid });
        }

        public ActionResult Update(Guid? billid, Guid? parent, bool? childBill)
        {

            USER_LOGIN userLogin = Session["USER_LOGIN"] as USER_LOGIN;
            if (userLogin == null) return Redirect("/user/");

            EntitiesConnection dbContext = new EntitiesConnection();
            HOA_DON itemHoaDon = dbContext.HOA_DON.FirstOrDefault(a => a.KEY_HOA_DON == billid);
            if (itemHoaDon == null)
            {
                TempData["ErrorMsg"] = "Bill bạn vừa cập nhật đã được thu ngân gộp vào bill chính. Vui lòng kiểm tra lại";
                return RedirectToAction("update", new { billid = parent });
            }
            BAN_MAM itemBanMam = dbContext.BAN_MAM.FirstOrDefault(a => a.KEY_BAN_MAM == itemHoaDon.KEY_BAN_MAM);
            if (itemBanMam != null)
            {
                if ((itemBanMam.DA_CO_KHACH == null || !itemBanMam.DA_CO_KHACH.Value) && itemBanMam.KEY_HOA_DON == null)
                {
                    return RedirectToAction("Index");
                }
            }

            Session[string.Format("KEY_HOA_DON_{0}", itemHoaDon.KEY_BAN_MAM.ToString())] = itemHoaDon;
            List<BILL_MEAL> listSaved = dbContext.BILL_MEAL.Where(a => a.KEY_HOA_DON == billid).ToList();            
            COMB_SET ComboSaved = dbContext.COMB_SET.FirstOrDefault(a => a.KEY_COMB_SET == itemHoaDon.KEY_COMB_SET);
            
            if(Session["DA_PHA_CHE_COBO_" + itemHoaDon.KEY_BAN_MAM.ToString()] == null) 
                Session["DA_PHA_CHE_COBO_" + itemHoaDon.KEY_BAN_MAM.ToString()] = itemHoaDon.KEY_COMB_SET != null ? true : false;
            Session["MON_SELECTED_" + itemHoaDon.KEY_BAN_MAM.ToString()] = listSaved.OrderBy(a => a.INDEX_SAP_XEP).ToList();
            Session["COMBO_SELECTED_" + itemHoaDon.KEY_BAN_MAM.ToString()] = ComboSaved;
            return RedirectToAction("Bill", Request.QueryString.ToRouteValues(new { }));
        }

        [HttpPost]
        public ActionResult Loadlistmonhang(Guid? nhomid, Guid? tableid)
        {
            EntitiesConnection dbContext = new EntitiesConnection();
            List<MON_HANG> itemMonHang = new List<MON_HANG>();
            if(nhomid != null) itemMonHang = dbContext.MON_HANG.Where(a => a.KEY_NHOM_MON == nhomid).OrderBy(a => a.TEN_MON_HANG).ToList();
            else itemMonHang = dbContext.MON_HANG.OrderBy(a => a.TEN_MON_HANG).ToList();
            ViewBag.MonSelected = Session["MON_SELECTED_" + tableid.ToString()];
            ViewBag.KeyBanMam = tableid;
            return PartialView("~/Views/_listmonhang.cshtml", itemMonHang);
        }

        [HttpPost]
        public ActionResult Loadlistmonhangselected(Guid? tableid)
        {
            EntitiesConnection dbContext = new EntitiesConnection();
            List<MON_HANG> itemMonHang = new List<MON_HANG>();
            List<BILL_MEAL> lstMonSelected = Session["MON_SELECTED_" + tableid.ToString()] as List<BILL_MEAL>;
            lstMonSelected.RemoveAll(a => a.SO_LUONG_MON == 0);
            var listKeyMonSelected = lstMonSelected.Select(a => a.KEY_MON_HANG).ToArray();
            itemMonHang = dbContext.MON_HANG.Where(a => listKeyMonSelected.Contains(a.KEY_MON_HANG)).OrderBy(a => a.TEN_MON_HANG).ToList();
            ViewBag.MonSelected = lstMonSelected;
            ViewBag.KeyBanMam = tableid;
            return PartialView("~/Views/_listmonhang_bookselected.cshtml", itemMonHang);
        }

        [HttpPost]
        public ActionResult Loadlistcombo(Guid? tableid)
        {
            EntitiesConnection dbContext = new EntitiesConnection();
            Dictionary<string, MON_HANG> lstTenMonSelected = new Dictionary<string, MON_HANG>();
            var itemCombo = dbContext.COMB_SET.OrderBy(a => a.TEN_COMB_SET).ToList();
            var itemComboMonHang = dbContext.COMB_MON.ToList();
            foreach(var itemComboMon in itemComboMonHang)
            {
                MON_HANG monhang = dbContext.MON_HANG.FirstOrDefault(a => a.KEY_MON_HANG == itemComboMon.KEY_MON_HANG);
                if (monhang != null) lstTenMonSelected.Add(monhang.KEY_MON_HANG.ToString(), monhang);
            }
            ViewBag.ComboSelected = Session["COMBO_SELECTED_" + tableid.ToString()];
            ViewBag.lstComboMonHangSelected = itemComboMonHang;
            ViewBag.ChitietMonSelected = lstTenMonSelected;
            ViewBag.KeyBanMam = tableid;
            return PartialView("~/Views/_listcombo.cshtml", itemCombo);
        }

        [HttpPost]
        public ActionResult Selectcomboset(Guid? tableid, Guid? KeyComboSet)
        {
            EntitiesConnection dbContext = new EntitiesConnection();
            HOA_DON itemHoaDon = Session[string.Format("KEY_HOA_DON_{0}", tableid.ToString())] as HOA_DON;
            itemHoaDon.KEY_COMB_SET = KeyComboSet;
            Session[string.Format("KEY_HOA_DON_{0}", tableid.ToString())] = itemHoaDon;  
            COMB_SET ComboSelected = dbContext.COMB_SET.FirstOrDefault(a => a.KEY_COMB_SET == KeyComboSet);
            List<COMB_MON> lstComBoMon = new List<COMB_MON>();
            List<BILL_MEAL> lstBillMealComboSet = new List<BILL_MEAL>();
            if(ComboSelected != null)
            {
                List<BILL_MEAL> lstBillMeal = Session["MON_SELECTED_" + tableid.ToString()] as List<BILL_MEAL>;
                lstComBoMon = dbContext.COMB_MON.Where(a => a.KEY_COMB_SET == ComboSelected.KEY_COMB_SET).ToList();
                foreach(var itemComboMon in lstComBoMon)
                {
                    MON_HANG monhangItem = dbContext.MON_HANG.FirstOrDefault(a => a.KEY_MON_HANG == itemComboMon.KEY_MON_HANG);

                    short? maxSTT = lstBillMeal.Max(a => a.INDEX_SAP_XEP);
                    short newSTT = (short)((maxSTT == null) ? 1 : maxSTT.Value + 1);
                    BILL_MEAL billmealItemComboSet = new BILL_MEAL();
                    billmealItemComboSet.KEY_BILL_MEAL = Guid.NewGuid();
                    billmealItemComboSet.KEY_MON_HANG = monhangItem.KEY_MON_HANG;
                    billmealItemComboSet.MON_HANG = monhangItem;
                    billmealItemComboSet.DON_GIA_MON = monhangItem.DON_GIA_MUC1;
                    billmealItemComboSet.SO_LUONG_MON = itemComboMon.SO_LUONG_MON;
                    billmealItemComboSet.INDEX_SAP_XEP = newSTT;
                    billmealItemComboSet.NOT_COOK_MIX = string.Format("Món từ Combo {0}", ComboSelected.TEN_COMB_SET);
                    billmealItemComboSet.COOK_VA_MIXE = false;
                    lstBillMealComboSet.Add(billmealItemComboSet);
                }
            }
            Session["COMBO_MON_SELECTED_" + tableid.ToString()] = lstBillMealComboSet;
            Session["COMBO_SELECTED_" + tableid.ToString()] = ComboSelected;
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult Unselectcomboset(Guid? tableid)
        {
            EntitiesConnection dbContext = new EntitiesConnection();
            COMB_SET itemComboSet = Session["COMBO_SELECTED_" + tableid.ToString()] as COMB_SET;            
            //if(itemComboSet != null)
            //{
            //    List<COMB_MON> lstComBoMon = dbContext.COMB_MON.Where(a => a.KEY_COMB_SET == itemComboSet.KEY_COMB_SET).ToList();
            //    foreach(var itemComboMon in lstComBoMon)
            //    {
            //        MON_HANG monhangItem = dbContext.MON_HANG.FirstOrDefault(a => a.KEY_MON_HANG == itemComboMon.KEY_MON_HANG);

            //        short? maxSTT = lstBillMeal.Max(a => a.INDEX_SAP_XEP);
            //        short newSTT = (short)((maxSTT == null) ? 1 : maxSTT.Value + 1);
            //        BILL_MEAL billmealItemComboSet = new BILL_MEAL();
            //        billmealItemComboSet.KEY_BILL_MEAL = Guid.NewGuid();
            //        billmealItemComboSet.KEY_MON_HANG = monhangItem.KEY_MON_HANG;
            //        billmealItemComboSet.MON_HANG = monhangItem;
            //        billmealItemComboSet.DON_GIA_MON = monhangItem.DON_GIA_MUC1;
            //        billmealItemComboSet.SO_LUONG_MON = 1;
            //        billmealItemComboSet.INDEX_SAP_XEP = newSTT;
            //        billmealItemComboSet.NOT_COOK_MIX = null;
            //        billmealItemComboSet.COOK_VA_MIXE = false;
            //        lstBillMealComboSet.Add(billmealItemComboSet);
            //    }
            //    lstBillMeal.AddRange(lstBillMealComboSet);
            //}

            HOA_DON itemHoaDon = Session[string.Format("KEY_HOA_DON_{0}", tableid.ToString())] as HOA_DON;
            itemHoaDon.KEY_COMB_SET = null;
            Session["COMBO_SELECTED_" + tableid.ToString()] = null;
            Session[string.Format("KEY_HOA_DON_{0}", tableid.ToString())] = itemHoaDon;
            return Json(new { success = true });
        }

        private static string convertToUnSign3(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        [HttpPost]
        public ActionResult Searchmonhang(Guid? nhomid, string query, Guid? tableid)
        {
            EntitiesConnection dbContext = new EntitiesConnection();
            List<MON_HANG> itemMonHang = new List<MON_HANG>();
            //if (nhomid != null) itemMonHang = dbContext.MON_HANG.Where(a => a.KEY_NHOM_MON == nhomid).OrderBy(a => a.TEN_MON_HANG).ToList();
            itemMonHang = Session["LIST_MON_DB"] as List<MON_HANG>;
            if (!string.IsNullOrEmpty(query)) itemMonHang = itemMonHang.Where(a => convertToUnSign3(a.TEN_MON_HANG.ToLower()).Contains(convertToUnSign3(query.ToLower()))).ToList();
            ViewBag.MonSelected = Session["MON_SELECTED_" + tableid.ToString()];
            ViewBag.KeyBanMam = tableid;
            return PartialView("~/Views/_listmonhang.cshtml", itemMonHang);
        }

        [HttpPost]
        public ActionResult Listmonselected(Guid? tableid, Guid? billid, bool? isopentable)
        {
            EntitiesConnection dbContext = new EntitiesConnection();
            
            List<BILL_TACH> lstBillTach = null;
            List<HOA_DON> lstChildHoaDon = null;
            Guid? ParentBill = null;
            if (billid != null)
            {
                var ItemBillTach = dbContext.BILL_TACH.FirstOrDefault(a => a.KEY_BILL_TACH == billid);
                if (ItemBillTach != null)
                {
                    ParentBill = ItemBillTach.KEY_HOA_DON;
                }
                
                if(ParentBill == null)
                {
                    lstBillTach = dbContext.BILL_TACH.Where(a => a.KEY_HOA_DON == billid).ToList();
                    Guid[] KeyHoaDonTachs = lstBillTach.Select(a => a.KEY_BILL_TACH).ToArray();
                    if (KeyHoaDonTachs != null && KeyHoaDonTachs.Count() > 0)
                    {
                        lstChildHoaDon = dbContext.HOA_DON.Where(a => KeyHoaDonTachs.Contains(a.KEY_HOA_DON)).ToList();
                    }
                }
                else
                {
                    lstBillTach = dbContext.BILL_TACH.Where(a => a.KEY_HOA_DON == ParentBill && a.KEY_HOA_DON != billid).ToList();
                    Guid[] KeyHoaDonTachs = lstBillTach.Select(a => a.KEY_BILL_TACH).ToArray();
                    if (KeyHoaDonTachs != null && KeyHoaDonTachs.Count() > 0)
                    {
                        lstChildHoaDon = dbContext.HOA_DON.Where(a => KeyHoaDonTachs.Contains(a.KEY_HOA_DON)).ToList();
                    }
                }
            }
            ViewBag.BillID = billid;
            ViewBag.ParentBill = ParentBill;
            ViewBag.lstBillTach = lstBillTach;
            ViewBag.lstChildHoaDon = lstChildHoaDon;

            if (isopentable == null && !isopentable.Value)
            {
                HOA_DON itemHoaDon = dbContext.HOA_DON.FirstOrDefault(a => a.KEY_HOA_DON == billid);
                BAN_MAM itemBanMam = dbContext.BAN_MAM.FirstOrDefault(a => a.KEY_BAN_MAM == itemHoaDon.KEY_BAN_MAM);
                if (itemBanMam != null)
                {
                    if ((itemBanMam.DA_CO_KHACH == null || !itemBanMam.DA_CO_KHACH.Value) && itemBanMam.KEY_HOA_DON == null)
                    {
                        return RedirectToAction("Index");
                    }
                }

                Session[string.Format("KEY_HOA_DON_{0}", itemHoaDon.KEY_BAN_MAM.ToString())] = itemHoaDon;
                List<BILL_MEAL> listSaved = dbContext.BILL_MEAL.Where(a => a.KEY_HOA_DON == billid).ToList();
                COMB_SET ComboSaved = dbContext.COMB_SET.FirstOrDefault(a => a.KEY_COMB_SET == itemHoaDon.KEY_COMB_SET);

                if (Session["DA_PHA_CHE_COBO_" + itemHoaDon.KEY_BAN_MAM.ToString()] == null)
                    Session["DA_PHA_CHE_COBO_" + itemHoaDon.KEY_BAN_MAM.ToString()] = itemHoaDon.KEY_COMB_SET != null ? true : false;
                Session["MON_SELECTED_" + itemHoaDon.KEY_BAN_MAM.ToString()] = listSaved.OrderBy(a => a.INDEX_SAP_XEP).ToList();
                Session["COMBO_SELECTED_" + itemHoaDon.KEY_BAN_MAM.ToString()] = ComboSaved;
            }
            List<BILL_MEAL> lstBillMeal = Session["MON_SELECTED_" + tableid.ToString()] as List<BILL_MEAL>;            
            return PartialView("~/Views/_listmonhang_selected.cshtml", lstBillMeal);
        }

        [HttpPost]
        public ActionResult Addbillmealitem(Guid? KeyBanMam, Guid? KeyMonHang, int? Quantity, string NoteBillMeal)
        {
            EntitiesConnection dbContext = new EntitiesConnection();
            MON_HANG monhangItem = dbContext.MON_HANG.FirstOrDefault(a => a.KEY_MON_HANG == KeyMonHang);
            List<BILL_MEAL> lstBillMeal = Session["MON_SELECTED_" + KeyBanMam.ToString()] as List<BILL_MEAL>;
            BILL_MEAL billmealItem = null;

            //New note for bill meal
            if(!string.IsNullOrEmpty(NoteBillMeal))
            {
                short? maxSTT = lstBillMeal.Max(a => a.INDEX_SAP_XEP);
                short newSTT = (short)((maxSTT == null) ? 1 : maxSTT.Value + 1);

                billmealItem = new BILL_MEAL();
                billmealItem.KEY_BILL_MEAL = Guid.NewGuid();
                billmealItem.KEY_MON_HANG = KeyMonHang;
                billmealItem.SO_LUONG_MON = Quantity;
                billmealItem.INDEX_SAP_XEP = newSTT;
                billmealItem.NOT_COOK_MIX = NoteBillMeal;
                billmealItem.TIEN_VIET_MON = monhangItem.DON_GIA_MUC1 * Quantity;
                billmealItem.TIEN_GIAM_GIA = 0;
                billmealItem.TIEN_PHAI_TRA = billmealItem.TIEN_VIET_MON - billmealItem.TIEN_GIAM_GIA;
                billmealItem.COOK_VA_MIXE = false;
                lstBillMeal.Add(billmealItem);
            }

            //Note note
            else
            {
                billmealItem = lstBillMeal.FirstOrDefault(a => a.KEY_MON_HANG == KeyMonHang && (a.COOK_VA_MIXE == null || !a.COOK_VA_MIXE.Value) && string.IsNullOrEmpty(a.NOT_COOK_MIX));
                if(billmealItem == null)
                {
                    short? maxSTT = lstBillMeal.Max(a => a.INDEX_SAP_XEP);
                    short newSTT = (short)((maxSTT == null) ? 1 : maxSTT.Value + 1);

                    billmealItem = new BILL_MEAL();
                    billmealItem.KEY_BILL_MEAL = Guid.NewGuid();
                    billmealItem.KEY_MON_HANG = monhangItem.KEY_MON_HANG;
                    billmealItem.SO_LUONG_MON = Quantity;
                    billmealItem.INDEX_SAP_XEP = newSTT;
                    billmealItem.TIEN_VIET_MON = monhangItem.DON_GIA_MUC1 * Quantity;
                    billmealItem.TIEN_GIAM_GIA = 0;
                    billmealItem.TIEN_PHAI_TRA = billmealItem.TIEN_VIET_MON - billmealItem.TIEN_GIAM_GIA;
                    billmealItem.COOK_VA_MIXE = false;
                    lstBillMeal.Add(billmealItem);
                }
                else
                {
                    billmealItem.SO_LUONG_MON++;
                }
            }            
            Session["MON_SELECTED_" + KeyBanMam.ToString()] = lstBillMeal.OrderBy(a => a.INDEX_SAP_XEP).ToList();
            return Json(new { success = true, quantity = billmealItem.SO_LUONG_MON });
        }

        [HttpPost]
        public ActionResult Addmonhang(Guid? tableid, Guid? keymonhang, string KeyNote)
        {
            EntitiesConnection dbContext = new EntitiesConnection();
            MON_HANG monhangItem = dbContext.MON_HANG.FirstOrDefault(a => a.KEY_MON_HANG == keymonhang);
            List<BILL_MEAL> lstBillMeal = Session["MON_SELECTED_" + tableid.ToString()] as List<BILL_MEAL>;
            BILL_MEAL billmealItem = null;
            if (KeyNote.ToLower().CompareTo("nonote") == 0) billmealItem = lstBillMeal.FirstOrDefault(a => a.KEY_MON_HANG == keymonhang && (a.COOK_VA_MIXE == null || !a.COOK_VA_MIXE.Value));
            else
            {
                string NoteSearch = clsSecuritys.Decrypt(KeyNote, "", true);
                billmealItem = lstBillMeal.FirstOrDefault(a => a.KEY_MON_HANG == keymonhang && (a.COOK_VA_MIXE == null || !a.COOK_VA_MIXE.Value) && a.NOT_COOK_MIX != null && a.NOT_COOK_MIX.CompareTo(NoteSearch) == 0);
            }
            if(billmealItem == null)
            {
                short? maxSTT = lstBillMeal.Max(a => a.INDEX_SAP_XEP);
                short newSTT = (short)((maxSTT == null) ? 1 : maxSTT.Value + 1);
                string NoteSearch = clsSecuritys.Decrypt(KeyNote, "", true);
                billmealItem = new BILL_MEAL();
                billmealItem.KEY_BILL_MEAL = Guid.NewGuid();
                billmealItem.KEY_MON_HANG = monhangItem.KEY_MON_HANG;
                billmealItem.SO_LUONG_MON = 1;
                billmealItem.INDEX_SAP_XEP = newSTT;
                billmealItem.NOT_COOK_MIX = NoteSearch;                
                billmealItem.TIEN_VIET_MON = monhangItem.DON_GIA_MUC1 * 1;
                billmealItem.TIEN_GIAM_GIA = 0;
                billmealItem.TIEN_PHAI_TRA = billmealItem.TIEN_VIET_MON - billmealItem.TIEN_GIAM_GIA;
                billmealItem.COOK_VA_MIXE = false;
                lstBillMeal.Add(billmealItem);
            }
            else
            {
                billmealItem.SO_LUONG_MON++;
                
                billmealItem.TIEN_VIET_MON = monhangItem.DON_GIA_MUC1 * billmealItem.SO_LUONG_MON;
                billmealItem.TIEN_GIAM_GIA = 0;
                billmealItem.TIEN_PHAI_TRA = billmealItem.TIEN_VIET_MON - billmealItem.TIEN_GIAM_GIA;
            }
            Session["MON_SELECTED_" + tableid.ToString()] = lstBillMeal.OrderBy(a => a.INDEX_SAP_XEP).ToList();
            return Json(new { success = true, quantity = billmealItem.SO_LUONG_MON });
        }

        [HttpPost]
        public ActionResult Minusmonhang(Guid? tableid, Guid? keymonhang, string KeyNote)
        {
            EntitiesConnection dbContext = new EntitiesConnection();
            MON_HANG monhangItem = dbContext.MON_HANG.FirstOrDefault(a => a.KEY_MON_HANG == keymonhang);
            List<BILL_MEAL> lstBillMeal = Session["MON_SELECTED_" + tableid.ToString()] as List<BILL_MEAL>;
            BILL_MEAL billmealItem = null;
            if (KeyNote.ToLower().CompareTo("nonote") == 0) billmealItem = lstBillMeal.FirstOrDefault(a => a.KEY_MON_HANG == keymonhang && (a.COOK_VA_MIXE == null || !a.COOK_VA_MIXE.Value));
            else
            {
                string NoteSearch = clsSecuritys.Decrypt(KeyNote, "", true);
                billmealItem = lstBillMeal.FirstOrDefault(a => a.KEY_MON_HANG == keymonhang && (a.COOK_VA_MIXE == null || !a.COOK_VA_MIXE.Value) && a.NOT_COOK_MIX != null && a.NOT_COOK_MIX.CompareTo(NoteSearch) == 0);
            }
            if (billmealItem != null)
            {
                if (billmealItem.SO_LUONG_MON > 0)
                {
                    billmealItem.SO_LUONG_MON--;
                    
                    billmealItem.TIEN_VIET_MON = monhangItem.DON_GIA_MUC1 * billmealItem.SO_LUONG_MON;
                    billmealItem.TIEN_GIAM_GIA = 0;
                    billmealItem.TIEN_PHAI_TRA = billmealItem.TIEN_VIET_MON - billmealItem.TIEN_GIAM_GIA;
                }
            }
            Session["MON_SELECTED_" + tableid.ToString()] = lstBillMeal.OrderBy(a => a.INDEX_SAP_XEP).ToList();
            return Json(new { success =  true, quantity = billmealItem != null ? billmealItem.SO_LUONG_MON : 0 });
        }

        public ActionResult Bill(Guid? billid, bool? isopentable, Guid? tableid)
        {
            if (!CheckLogin()) return Redirect("/user/");

            EntitiesConnection dbContext = new EntitiesConnection();
            HOA_DON itemHD = null;
            BILL_TACH itemBillTachChild = null;
            List<BILL_TACH> lstBillTach = null;
            List<HOA_DON> lstChildHoaDon = null;
            Dictionary<string, int> dictToTalMonBillTach = null;
            Dictionary<string, int> dictDaPhucVuBillTach = null;
            Dictionary<string, int> dictChuaPhucVuBillTach = null;
            bool IsOpenTable = false;
            if (isopentable != null && isopentable.Value)
            {
                itemHD = new HOA_DON();
                itemBillTachChild = null;
                itemHD.KEY_HOA_DON = Guid.NewGuid();
                itemHD.KEY_BAN_MAM = tableid.Value;
                itemHD.BAN_MAM = dbContext.BAN_MAM.FirstOrDefault(a => a.KEY_BAN_MAM == tableid);
                itemHD.TONG_TANG_GIA = 0;
                itemHD.TONG_PHAI_TRA = 0;
                itemHD.TONG_KHUYEN_MAI = 0;
                
                itemHD.HSO_CLIENT_CARDS = 0;
                itemHD.VAL_LOAI_COUPON = 0;
                itemHD.TIEN_THU_VAO = 0;
                itemHD.TONG_KHUYEN_MAI = 0;
                itemHD.LAN_IN_PHIEU = 0;
                itemHD.TONG_GIO_USE = 0;
                itemHD.TONG_TIEN_BO = 0;
                itemHD.TONG_TANG_GIA = 0;
                itemHD.DA_THU_TIEN = false;
                itemHD.DA_CHE_BIEN = false;

                lstBillTach = new List<BILL_TACH>();
                lstChildHoaDon = new List<HOA_DON>();
                dictToTalMonBillTach = new Dictionary<string, int>();
                dictDaPhucVuBillTach = new Dictionary<string, int>();
                dictChuaPhucVuBillTach = new Dictionary<string, int>();
                IsOpenTable = true;
            }
            else
            {
                itemHD =  dbContext.HOA_DON.FirstOrDefault(a => a.KEY_HOA_DON == billid);
                lstBillTach = dbContext.BILL_TACH.Where(a => a.KEY_HOA_DON == billid).OrderBy(a => a.COD_BILL_TACH).ToList();
                itemBillTachChild = dbContext.BILL_TACH.FirstOrDefault(a => a.KEY_BILL_TACH == billid);
                Guid[] KeyHoaDonTachs = lstBillTach.Select(a => a.KEY_BILL_TACH).ToArray();
                if (KeyHoaDonTachs != null && KeyHoaDonTachs.Count() > 0)
                {
                    lstChildHoaDon = dbContext.HOA_DON.Where(a => KeyHoaDonTachs.Contains(a.KEY_HOA_DON)).OrderBy(a => a.MA_HOA_DON).ToList();
                    
                    dictToTalMonBillTach = dbContext.BILL_MEAL
                        .Where(a => a.KEY_HOA_DON != null && KeyHoaDonTachs.Contains(a.KEY_HOA_DON.Value))
                        .GroupBy(a => a.KEY_HOA_DON).Select(group => new { KEY_HOA_DON = group.Key, SO_LUONG_MON = group.Count() })
                        .ToDictionary(a => a.KEY_HOA_DON.Value.ToString(), a => a.SO_LUONG_MON);
                    dictDaPhucVuBillTach = dbContext.BILL_MEAL
                        .Where(a => a.KEY_HOA_DON != null && KeyHoaDonTachs.Contains(a.KEY_HOA_DON.Value) && (a.COOK_VA_MIXE != null && a.COOK_VA_MIXE == true))
                        .GroupBy(a => a.KEY_HOA_DON).Select(group => new { KEY_HOA_DON = group.Key, SO_LUONG_MON = group.Count() })
                        .ToDictionary(a => a.KEY_HOA_DON.Value.ToString(), a => a.SO_LUONG_MON);
                    dictChuaPhucVuBillTach = dbContext.BILL_MEAL
                        .Where(a => a.KEY_HOA_DON != null && KeyHoaDonTachs.Contains(a.KEY_HOA_DON.Value) && (a.COOK_VA_MIXE == null || a.COOK_VA_MIXE == false))
                        .GroupBy(a => a.KEY_HOA_DON).Select(group => new { KEY_HOA_DON = group.Key, SO_LUONG_MON = group.Count() })
                        .ToDictionary(a => a.KEY_HOA_DON.Value.ToString(), a => a.SO_LUONG_MON);
                }
                else
                {
                    lstChildHoaDon = new List<HOA_DON>();
                    dictToTalMonBillTach = new Dictionary<string, int>();
                    dictDaPhucVuBillTach = new Dictionary<string, int>();
                    dictChuaPhucVuBillTach = new Dictionary<string, int>();
                }

                IsOpenTable = false;
            }
            List<BILL_MEAL> lstBillMeal = Session["MON_SELECTED_" + itemHD.KEY_BAN_MAM.ToString()] as List<BILL_MEAL>;
            if (lstBillMeal == null) return RedirectToAction("Update", new { billid = billid });
            bool CanCancel = true;
            foreach(var item in lstBillMeal)
            {
                if(item.COOK_VA_MIXE != null && item.COOK_VA_MIXE.Value)
                {
                    CanCancel = false;
                    break;
                }
            }
            ViewBag.lstBillTach = lstBillTach;
            ViewBag.lstChildHoaDon = lstChildHoaDon;
            ViewBag.dictToTalMonBillTach = dictToTalMonBillTach;
            ViewBag.dictDaPhucVuBillTach = dictDaPhucVuBillTach;
            ViewBag.dictChuaPhucVuBillTach = dictChuaPhucVuBillTach;
            ViewBag.itemBillTachChild = itemBillTachChild;
            ViewBag.CanCancel = CanCancel;
            ViewBag.IsOpenTable = IsOpenTable;
            return View(itemHD);
        }

        public ActionResult Movetobill(Guid? keybillmeal, Guid? targetbill, Guid? parent, bool? childBill)
        {
            using (var dbContext = new EntitiesConnection())
            {
                using (var dbContextTransaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        Guid KeySource = new Guid(Request.QueryString["billid"].ToString());
                        BILL_MEAL itemBillMeal = dbContext.BILL_MEAL.FirstOrDefault(a => a.KEY_BILL_MEAL == keybillmeal);
                        HOA_DON HDTarget = dbContext.HOA_DON.FirstOrDefault(a => a.KEY_HOA_DON == targetbill);
                        if (HDTarget != null) HDTarget.TONG_PHAI_TRA += itemBillMeal.TIEN_PHAI_TRA;
                        HOA_DON Source = dbContext.HOA_DON.FirstOrDefault(a => a.KEY_HOA_DON == KeySource);
                        if (Source != null) Source.TONG_PHAI_TRA -= itemBillMeal.TIEN_PHAI_TRA;
                        itemBillMeal.KEY_HOA_DON = targetbill;
                        dbContext.SaveChanges();
                        dbContextTransaction.Commit();
                        TempData["SuccessMsg"] = "Đã chuyển món thành công";
                    }
                    catch
                    {
                        dbContextTransaction.Rollback();
                        TempData["ErrorMsg"] = "Chuyển món không thành công. Vui lòng thử lại sau.";
                    }
                }
            }
            return RedirectToAction("update", Request.QueryString.ToRouteValues(new { keybillmeal = "", targetbill = "", parent = parent, childBill = childBill }));
        }


        [HttpPost]
        public ActionResult Checktabledone(Guid? tableid)
        {
            EntitiesConnection dbContext = new EntitiesConnection();
            BAN_MAM item = dbContext.BAN_MAM.FirstOrDefault(a => a.KEY_BAN_MAM == tableid);
            if(item != null)
            {
                if((item.DA_CO_KHACH == null || !item.DA_CO_KHACH.Value) && item.KEY_HOA_DON == null)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false });
                }
            }
            return Json(new { success = true });
        }

        public ActionResult Bookmeal(Guid? billid, bool? isopentable, Guid? tableid, Guid? parent)
        {
            if (!CheckLogin()) return Redirect("/user/");

            EntitiesConnection dbContext = new EntitiesConnection();
            var itemBillTachChild = dbContext.BILL_TACH.FirstOrDefault(a => a.KEY_BILL_TACH == billid);
            if (isopentable != null && !isopentable.Value)
            {
                if (billid != null)
                {
                    HOA_DON itemHoaDonCheck = dbContext.HOA_DON.FirstOrDefault(a => a.KEY_HOA_DON == billid);
                    if (itemHoaDonCheck == null)
                    {
                        TempData["ErrorMsg"] = "Bill bạn vừa cập nhật đã được thu ngân gộp vào bill chính. Vui lòng kiểm tra lại";
                        return RedirectToAction("update", new { billid = parent });
                    }
                    if (itemHoaDonCheck.DA_THU_TIEN != null && itemHoaDonCheck.DA_THU_TIEN.Value)
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            BAN_MAM item = dbContext.BAN_MAM.FirstOrDefault(a => a.KEY_BAN_MAM == tableid);
            List<NHOM_MON> itemNhomMon = dbContext.NHOM_MON.Where(a => a.BAN_NHOM_MON == true).OrderBy(a => a.TEN_NHOM_MON).ToList();
            List<MON_HANG> itemMonHang = dbContext.MON_HANG.OrderBy(a => a.TEN_MON_HANG).ToList();
            Session["LIST_MON_DB"] = itemMonHang;
            ViewBag.MonSelected = Session["MON_SELECTED_" + tableid.ToString()];
            ViewBag.lstNhomMon = itemNhomMon;
            ViewBag.lstMonHang = itemMonHang;
            ViewBag.BAN_MAM = item;
            ViewBag.itemBillTachChild = itemBillTachChild;
            ViewBag.IsOpenTable = isopentable != null ? isopentable.Value : false ;
            return View();
        }

        protected decimal TongPhaiTra(List<BILL_MEAL> lstBillMeal)
        {
            if (lstBillMeal != null)
            {
                decimal TongPhaiTra = 0; // dung de tinh tong tien combo
                foreach (BILL_MEAL entbh in lstBillMeal)
                {
                    TongPhaiTra += entbh.TIEN_PHAI_TRA.Value;
                }

                return TongPhaiTra;
            }
            return 0;
        }


        public ActionResult Savechonmon(Guid? tableid, bool? isopentable, Guid? billid, Guid? parent, bool? childBill)
        {
            if (!CheckLogin()) return Redirect("/user/");

            USER_LOGIN userLogin = Session["USER_LOGIN"] as USER_LOGIN;
            EntitiesConnection dbContext = new EntitiesConnection();

            

            HOA_DON itemHoaDon = Session[string.Format("KEY_HOA_DON_{0}", tableid.ToString())] as HOA_DON;

            if (isopentable != null && !isopentable.Value)
            {
                if (itemHoaDon.KEY_HOA_DON != null)
                {
                    HOA_DON itemHoaDonCheck = dbContext.HOA_DON.FirstOrDefault(a => a.KEY_HOA_DON == itemHoaDon.KEY_HOA_DON);
                    if (itemHoaDonCheck == null)
                    {
                        TempData["ErrorMsg"] = "Bill bạn vừa cập nhật đã được thu ngân gộp vào bill chính. Vui lòng kiểm tra lại";
                        return RedirectToAction("update", new { billid = parent });
                    }
                    if (itemHoaDonCheck != null)
                    {
                        if (itemHoaDonCheck.DA_THU_TIEN != null && itemHoaDonCheck.DA_THU_TIEN.Value)
                        {
                            return RedirectToAction("Index");
                        }
                    }
                }
            }

            HOA_DON itemHoaDonToSave = null;

            decimal TongTienTra = 0;
            
            List<BILL_MEAL> lstBillMeal = Session["MON_SELECTED_" + tableid.ToString()] as List<BILL_MEAL>;
            if(lstBillMeal != null)
            {
                foreach(var itemBillMeal in lstBillMeal)
                {
                    if(itemBillMeal.SO_LUONG_MON > 0)
                    {
                        MON_HANG itemMonHang = dbContext.MON_HANG.FirstOrDefault(a => a.KEY_MON_HANG == itemBillMeal.KEY_MON_HANG);
                        itemBillMeal.DON_GIA_MON = itemMonHang.DON_GIA_MUC1;
                        TongTienTra += itemBillMeal.DON_GIA_MON.Value * itemBillMeal.SO_LUONG_MON.Value;
                    }
                }
            }
            
            List<BILL_MEAL> lstBillMealComboSet = Session["COMBO_MON_SELECTED_" + tableid.ToString()] as List<BILL_MEAL>;
            int countSelect = (lstBillMeal != null) ? lstBillMeal.Count : 0;
            int countSelectCombo = (lstBillMealComboSet != null) ? lstBillMealComboSet.Count : 0;
            if (countSelect + countSelectCombo == 0)
            {
                TempData["ErrorMsg"] = "Vui lòng chọn món trước khi lưu";
                if (isopentable != null && isopentable.Value) return RedirectToAction("Bill", new { isopentable = true, tableid = tableid });
                else return RedirectToAction("Update", new { billid = itemHoaDon.KEY_HOA_DON });
            }

            if(lstBillMealComboSet != null)
            {
                foreach(var itemBillMeal in lstBillMealComboSet)
                {
                    if(itemBillMeal.SO_LUONG_MON > 0)
                    {
                        MON_HANG itemMonHang = dbContext.MON_HANG.FirstOrDefault(a => a.KEY_MON_HANG == itemBillMeal.KEY_MON_HANG);
                        itemBillMeal.DON_GIA_MON = itemMonHang.DON_GIA_MUC1;
                        if (itemHoaDon.KEY_COMB_SET != null) TongTienTra += itemBillMeal.DON_GIA_MON.Value * itemBillMeal.SO_LUONG_MON.Value;
                    }
                }
            }

            using (var context = new EntitiesConnection())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        CAU_HINH itemCauHinh = context.CAU_HINH.FirstOrDefault();
                        if (isopentable != null && isopentable.Value)
                        {
                            itemHoaDonToSave = itemHoaDon;
                            itemHoaDonToSave.KEY_HOA_DON = itemHoaDon.KEY_HOA_DON;
                            itemHoaDonToSave.KEY_USER_LOGIN = userLogin.KEY_USER_LOGIN;
                            itemHoaDonToSave.MA_HOA_DON = Number();
                            itemHoaDonToSave.NGAY_HOA_DON = DateTime.Now;
                            itemHoaDonToSave.NGAY_GIO_BAN = DateTime.Now;
                            itemHoaDonToSave.NGAY_GIO_VAO = DateTime.Now;
                            itemHoaDonToSave.NGAY_GIO_OUT = DateTime.Now;
                            itemHoaDonToSave.TONG_PHAI_TRA = TongTienTra;
                            itemHoaDonToSave.KEY_COMB_SET = itemHoaDon.KEY_COMB_SET;
                            
                            itemHoaDonToSave.HSO_CLIENT_CARDS = 0;
                            itemHoaDonToSave.VAL_LOAI_COUPON = 0;
                            itemHoaDonToSave.TIEN_THU_VAO = 0;
                            itemHoaDonToSave.TONG_KHUYEN_MAI = 0;
                            itemHoaDonToSave.LAN_IN_PHIEU = 0;
                            itemHoaDonToSave.TONG_GIO_USE = 0;
                            itemHoaDonToSave.TONG_TIEN_BO = 0;
                            itemHoaDonToSave.TONG_TANG_GIA = 0;
                            itemHoaDonToSave.DA_THU_TIEN = false;
                            itemHoaDonToSave.DA_CHE_BIEN = false;
                            if(itemCauHinh != null)
                            {
                                itemHoaDon.DIEM_QUI_UOC = itemCauHinh.DIEM_QUI_UOC;
                                itemHoaDon.DIEM_QUI_DOI = itemCauHinh.DIEM_QUI_DOI;
                            }
                            else
                            {
                                itemHoaDon.DIEM_QUI_UOC = 0;
                                itemHoaDon.DIEM_QUI_DOI = 0;
                            }
                            context.HOA_DON.Add(itemHoaDonToSave);
                        }
                        else
                        {
                            itemHoaDonToSave = context.HOA_DON.FirstOrDefault(a => a.KEY_HOA_DON == itemHoaDon.KEY_HOA_DON);
                            itemHoaDonToSave.KEY_COMB_SET = itemHoaDon.KEY_COMB_SET;
                            itemHoaDonToSave.TONG_PHAI_TRA = TongTienTra;
                            
                            itemHoaDonToSave.HSO_CLIENT_CARDS = 0;
                            itemHoaDonToSave.VAL_LOAI_COUPON = 0;
                            itemHoaDonToSave.TIEN_THU_VAO = 0;
                            itemHoaDonToSave.TONG_KHUYEN_MAI = 0;
                            itemHoaDonToSave.LAN_IN_PHIEU = 0;
                            itemHoaDonToSave.TONG_GIO_USE = 0;
                            itemHoaDonToSave.TONG_TIEN_BO = 0;
                            itemHoaDonToSave.TONG_TANG_GIA = 0;
                            itemHoaDonToSave.DA_THU_TIEN = false;
                            itemHoaDonToSave.DA_CHE_BIEN = false;
                            if(itemCauHinh != null)
                            {
                                itemHoaDon.DIEM_QUI_UOC = itemCauHinh.DIEM_QUI_UOC;
                                itemHoaDon.DIEM_QUI_DOI = itemCauHinh.DIEM_QUI_DOI;
                            }
                            else
                            {
                                itemHoaDon.DIEM_QUI_UOC = 0;
                                itemHoaDon.DIEM_QUI_DOI = 0;
                            }
                        }

                        bool HoaDonDaPhaChe = true;
                        foreach (var item in lstBillMeal)
                        {
                            BILL_MEAL itemBillMeal = null;
                            if (item.KEY_BILL_MEAL == Guid.Empty) itemBillMeal = null;
                            else itemBillMeal = context.BILL_MEAL.FirstOrDefault(a => a.KEY_MON_HANG == item.KEY_MON_HANG && a.KEY_HOA_DON == itemHoaDon.KEY_HOA_DON && a.KEY_BILL_MEAL == item.KEY_BILL_MEAL);
                            if (itemBillMeal != null && itemBillMeal.COOK_VA_MIXE == true) continue;
                            if(itemBillMeal != null)
                            {
                                HoaDonDaPhaChe = false;
                                if(itemBillMeal != null && item.SO_LUONG_MON == 0)
                                {
                                    context.BILL_MEAL.Attach(itemBillMeal);
                                    context.BILL_MEAL.Remove(itemBillMeal);
                                }
                                else
                                {
                                    if (itemBillMeal.SO_LUONG_MON != item.SO_LUONG_MON)
                                    {
                                        itemBillMeal.SO_LUONG_MON = item.SO_LUONG_MON;
                                        itemBillMeal.NOT_COOK_MIX = item.NOT_COOK_MIX;
                                        itemBillMeal.TIEN_VIET_MON = itemBillMeal.DON_GIA_MON * itemBillMeal.SO_LUONG_MON;
                                        itemBillMeal.TIEN_GIAM_GIA = 0;
                                        itemBillMeal.TIEN_PHAI_TRA = itemBillMeal.TIEN_VIET_MON - itemBillMeal.TIEN_GIAM_GIA;
                                    }
                                }

                            }
                            else
                            {
                                HoaDonDaPhaChe = false;
                                if(item.SO_LUONG_MON > 0)
                                {
                                    BILL_MEAL billmealAdd = new BILL_MEAL();
                                    billmealAdd.KEY_BILL_MEAL = item.KEY_BILL_MEAL;
                                    billmealAdd.KEY_MON_HANG = item.KEY_MON_HANG;
                                    billmealAdd.SO_LUONG_MON = item.SO_LUONG_MON;
                                    billmealAdd.DON_GIA_MON = item.DON_GIA_MON;
                                    billmealAdd.KEY_HOA_DON = itemHoaDonToSave.KEY_HOA_DON;
                                    billmealAdd.TIEN_VIET_MON = item.DON_GIA_MON * item.SO_LUONG_MON;
                                    billmealAdd.NOT_COOK_MIX = item.NOT_COOK_MIX;

                                    billmealAdd.TIEN_VIET_MON = billmealAdd.DON_GIA_MON * billmealAdd.SO_LUONG_MON;
                                    billmealAdd.TIEN_GIAM_GIA = 0;
                                    billmealAdd.TIEN_PHAI_TRA = billmealAdd.TIEN_VIET_MON - billmealAdd.TIEN_GIAM_GIA;
                                    billmealAdd.COOK_VA_MIXE = false;
                                    billmealAdd.THEM_BO_SUNG = false;
                                    billmealAdd.INDEX_SAP_XEP = item.INDEX_SAP_XEP;
                                    context.BILL_MEAL.Add(billmealAdd);
                                }
                            }
                        }
                        
                        itemHoaDonToSave.DA_CHE_BIEN = HoaDonDaPhaChe;

                        if (lstBillMealComboSet != null)
                        {
                            if(itemHoaDon.KEY_COMB_SET == null)
                            {
                                foreach (var item in lstBillMealComboSet)
                                {
                                    BILL_MEAL itemBillMeal = context.BILL_MEAL.FirstOrDefault(a => a.KEY_MON_HANG == item.KEY_MON_HANG && a.KEY_HOA_DON == itemHoaDon.KEY_HOA_DON && a.KEY_BILL_MEAL == item.KEY_BILL_MEAL);
                                    if (itemBillMeal != null && itemBillMeal.COOK_VA_MIXE == false)
                                    {
                                        context.BILL_MEAL.Attach(itemBillMeal);
                                        context.BILL_MEAL.Remove(itemBillMeal);
                                    }
                                }
                                
                                
                            }
                            else
                            {
                                foreach (var item in lstBillMealComboSet)
                                {
                                    BILL_MEAL itemBillMeal = context.BILL_MEAL.FirstOrDefault(a => a.KEY_MON_HANG == item.KEY_MON_HANG && a.KEY_HOA_DON == itemHoaDon.KEY_HOA_DON && a.KEY_BILL_MEAL == item.KEY_BILL_MEAL);
                                    if (itemBillMeal == null)
                                    {
                                        if(item.SO_LUONG_MON > 0)
                                        {
                                            BILL_MEAL billmealAdd = new BILL_MEAL();
                                            billmealAdd.KEY_BILL_MEAL = item.KEY_BILL_MEAL;
                                            billmealAdd.KEY_MON_HANG = item.KEY_MON_HANG;
                                            billmealAdd.SO_LUONG_MON = item.SO_LUONG_MON;
                                            billmealAdd.DON_GIA_MON = item.DON_GIA_MON;
                                            billmealAdd.KEY_HOA_DON = itemHoaDonToSave.KEY_HOA_DON;
                                            billmealAdd.TIEN_VIET_MON = item.DON_GIA_MON * item.SO_LUONG_MON;
                                            billmealAdd.NOT_COOK_MIX = item.NOT_COOK_MIX;

                                            billmealAdd.TIEN_VIET_MON = billmealAdd.DON_GIA_MON * billmealAdd.SO_LUONG_MON;
                                            billmealAdd.TIEN_GIAM_GIA = 0;
                                            billmealAdd.TIEN_PHAI_TRA = billmealAdd.TIEN_VIET_MON - billmealAdd.TIEN_GIAM_GIA;
                                            billmealAdd.COOK_VA_MIXE = false;
                                            billmealAdd.THEM_BO_SUNG = false;
                                            billmealAdd.INDEX_SAP_XEP = item.INDEX_SAP_XEP;
                                            context.BILL_MEAL.Add(billmealAdd);
                                        }
                                    }
                                }
                            }
                            
                        }

                        if (childBill == null || !childBill.Value)
                        {
                            BAN_MAM itemBanMamSave = context.BAN_MAM.FirstOrDefault(a => a.KEY_BAN_MAM == tableid);
                            itemBanMamSave.KEY_HOA_DON = itemHoaDon.KEY_HOA_DON;
                            itemBanMamSave.DA_CO_KHACH = true;
                        }

                        context.SaveChanges();
                        dbContextTransaction.Commit();
                        if(itemHoaDon.KEY_COMB_SET == null) Session["COMBO_MON_SELECTED_" + tableid.ToString()] = new List<BILL_MEAL>();
                        TempData["SuccessMsg"] = "Lưu thông tin đặt bàn thành công !";
                        return RedirectToAction("Update", Request.QueryString.ToRouteValues(new { billid = itemHoaDon.KEY_HOA_DON, isopentable = "", tableid = "" }));    
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMsg"] = "Đã có lỗi xảy ra khi lưu thông tin đặt bàn. Vui lòng thử lại sau";
                        dbContextTransaction.Rollback();
                        if (isopentable != null && isopentable.Value) return RedirectToAction("Bill", new { isopentable = true, tableid = tableid });                  
                        else return RedirectToAction("Update", new { billid = itemHoaDon.KEY_HOA_DON });  
                    }
                }
            }

        }
        

        private bool CheckValid(HOA_DON entHoaDon)
        {
            if (entHoaDon.NGAY_HOA_DON.Value.Date.CompareTo(new DateTime(1900, 1, 1)) < 0)
            {
                TempData["ErrorMsg"] = "Ngày ghi sổ phải lớn hơn ngày 01/01/1900. Vui lòng nhập lại!";
                return false;
            }
            if (entHoaDon.NGAY_HOA_DON.Value.Date.CompareTo(new DateTime(2079, 12, 31)) > 0)
            {
                TempData["ErrorMsg"] = "Ngày ghi sổ phải nhỏ hơn ngày 31/12/2079. Vui lòng nhập lại!";
                return false;
            }

            if (string.IsNullOrEmpty(entHoaDon.MA_HOA_DON))
            {
                TempData["ErrorMsg"] = "Số chứng từ là dữ liệu bắt buộc nhập. Vui lòng nhập lại!";
                return false;
            }

            if (!entHoaDon.KEY_BAN_MAM.HasValue)
            {
                TempData["ErrorMsg"] = "Hãy chọn BÀN / ROOM / KEY trước khi hoàn tất. Vui lòng nhập lại!";
                return false;
            }
            
            return true;
        }

        public ActionResult Phanauxuatmon(Guid? billid)
        {
            USER_LOGIN userLogin = Session["USER_LOGIN"] as USER_LOGIN;
            if (userLogin == null) return Redirect("/user/");
            using (var context = new EntitiesConnection())
            {
                HOA_DON _entHoaDon = context.HOA_DON.FirstOrDefault(a => a.KEY_HOA_DON == billid);
                if (_entHoaDon != null)
                {
                    if (_entHoaDon.DA_THU_TIEN != null && _entHoaDon.DA_THU_TIEN.Value)
                    {
                        return RedirectToAction("Index");
                    }
                }
                try
                {
                    string HostPrint = ConfigurationManager.AppSettings["HostPrint"];
                    string PortHostPrint = ConfigurationManager.AppSettings["PortHostPrint"];
                    AsynchronousClient client = new AsynchronousClient();
                    if (client.StartClient(HostPrint, int.Parse(PortHostPrint)))
                    {
                        //Update pha che & bep & xuat kho
                        client.SendData(_entHoaDon.KEY_HOA_DON.ToString());
                        client.Close();
                        TempData["SuccessMsg"] = "Đã đưa vào pha chế thành công !";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "Đưa vào pha chế không thành công ! Vui lòng thử lại sau.";
                        return Redirect("/home/update?billid=" + _entHoaDon.KEY_HOA_DON.ToString());
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMsg"] = "Đưa vào pha chế không thành công ! Vui lòng thử lại sau.";
                    return Redirect("/home/update?billid=" + _entHoaDon.KEY_HOA_DON.ToString());
                }
            }

        }


        public ActionResult Thanhtoanxong(Guid? billid)
        {            
            if (!CheckLogin()) return Redirect("/user/");

            using (var context = new EntitiesConnection())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    HOA_DON _entHoaDon = context.HOA_DON.FirstOrDefault(a => a.KEY_HOA_DON == billid);
                    if (_entHoaDon != null)
                    {
                        if (_entHoaDon.DA_THU_TIEN != null || _entHoaDon.DA_THU_TIEN.Value)
                        {
                            return RedirectToAction("Index");
                        }
                    }

                    BAN_MAM _entBanMam = context.BAN_MAM.FirstOrDefault(a => a.KEY_BAN_MAM == _entHoaDon.KEY_BAN_MAM);
                    try
                    {
                        _entHoaDon.DA_THU_TIEN = true;
                        _entBanMam.DA_CO_KHACH = false;
                        _entBanMam.KEY_HOA_DON = null;

                        context.SaveChanges();
                        dbContextTransaction.Commit();
                        TempData["SuccessMsg"] = string.Format("Thanh toán hoá đơn số {0} tại bàn {1} - {2} thành công !", _entHoaDon.MA_HOA_DON, _entBanMam.TEN_BAN_MAM, _entBanMam.KHU_VUC.TEN_KHU_VUC);
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMsg"] = "Thanh toán không thành công ! Vui lòng thử lại sau.";
                        dbContextTransaction.Rollback();
                        return RedirectToAction("Update", new { billid = _entHoaDon.KEY_HOA_DON });
                    }
                }
            }

        }


        public ActionResult Huyban(Guid? tableid)
        {            
            if (!CheckLogin()) return Redirect("/user/");

            using (var context = new EntitiesConnection())
            {
                try
                {
                    BAN_MAM _entBanMam = context.BAN_MAM.FirstOrDefault(a => a.KEY_BAN_MAM == tableid);
                    Session["DA_PHA_CHE_COBO_" + tableid.ToString()] = false;
                    Session[string.Format("KEY_HOA_DON_{0}", tableid.ToString())] = null;
                    Session["MON_SELECTED_" + tableid.ToString()] = new List<BILL_MEAL>();
                    Session["COMBO_SELECTED_" + tableid.ToString()] = null;
                    TempData["SuccessMsg"] = string.Format("Đã huỷ bàn {0} - {1} thành công !", _entBanMam.TEN_BAN_MAM, _entBanMam.KHU_VUC.TEN_KHU_VUC);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMsg"] = "Huỷ bàn không thành công ! Vui lòng thử lại sau.";
                    return RedirectToAction("Open", new { tableid = tableid });
                }
            }

        }

        public ActionResult Huybanchuathanhtoan(Guid? billid)
        {            
            if (!CheckLogin()) return Redirect("/user/");

            using (var context = new EntitiesConnection())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    List<BILL_MEAL> lstBillMEal = context.BILL_MEAL.Where(a => a.KEY_HOA_DON == billid).ToList();
                    HOA_DON _entHoaDon = context.HOA_DON.FirstOrDefault(a => a.KEY_HOA_DON == billid);
                    BAN_MAM _entBanMam = context.BAN_MAM.FirstOrDefault(a => a.KEY_BAN_MAM == _entHoaDon.KEY_BAN_MAM);
                    try
                    {
                        foreach(var Item in lstBillMEal)
                        {
                            context.BILL_MEAL.Attach(Item);
                            context.BILL_MEAL.Remove(Item);
                        }
                        context.HOA_DON.Attach(_entHoaDon);
                        context.HOA_DON.Remove(_entHoaDon);
                        _entBanMam.DA_CO_KHACH = false;
                        _entBanMam.KEY_HOA_DON = null;

                        context.SaveChanges();
                        dbContextTransaction.Commit();
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMsg"] = "Thanh toán không thành công ! Vui lòng thử lại sau.";
                        dbContextTransaction.Rollback();
                        return RedirectToAction("Update", new { billid = _entHoaDon.KEY_HOA_DON });
                    }
                }
            }

        }

    }
}