using MZXYOnlineJudge.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer;
using Webdiyer.WebControls.Mvc;

namespace MZXYOnlineJudge.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 分页时每页显示的个数
        /// </summary>
        private int pageColums = 10;

        public ActionResult Index()
        {
            OJEntities oj = new OJEntities();
            DateTime d1 = DateTime.Now.AddDays(1);
            DateTime d2 = DateTime.Now;
            DateTime d3 = DateTime.Now.AddDays(-1);
            DateTime d4 = DateTime.Now.AddDays(-2);
            DateTime d5 = DateTime.Now.AddDays(-3);
            DateTime d6 = DateTime.Now.AddDays(-4);
            DateTime d7 = DateTime.Now.AddDays(-5);
            DateTime d8 = DateTime.Now.AddDays(-6);

            ViewBag.day7 = oj.Solution.Where(s => s.uploadtime < d1 && s.uploadtime > d2).Count();
            ViewBag.day6 = oj.Solution.Where(s => s.uploadtime < d2 && s.uploadtime > d3).Count();
            ViewBag.day5 = oj.Solution.Where(s => s.uploadtime < d3 && s.uploadtime > d4).Count();
            ViewBag.day4 = oj.Solution.Where(s => s.uploadtime < d4 && s.uploadtime > d5).Count();
            ViewBag.day3 = oj.Solution.Where(s => s.uploadtime < d5 && s.uploadtime > d6).Count();
            ViewBag.day2 = oj.Solution.Where(s => s.uploadtime < d6 && s.uploadtime > d7).Count();
            ViewBag.day1 = oj.Solution.Where(s => s.uploadtime < d7 && s.uploadtime > d8).Count();
            return View();
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Login(Users user)
        {
            ViewBag.Lasturl = Request.Url;
            OJEntities ojentities = new OJEntities();
            Users u = ojentities.Users.Find(user.user_id);
            if (u==null||u.password!=user.password)
            {
                return Json(false);
            }
            Session["user_id"] = u.user_id;
            Session["nick"] = u.nick;
            return Json(true);
        }

        /// <summary>
        /// 注册页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Regist()
        {
            return View();
        }

        /// <summary>
        /// 注册页面
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Regist(Users user)
        {
            OJEntities oe = new OJEntities();
            Users findu = oe.Users.Find(user.user_id);
            if (findu!=null)
            {
                return Json(false);
            }
            oe.Users.Add(user);
            oe.SaveChanges();
            return Json(true);
        }

        /// <summary>
        /// 题目
        /// </summary>
        /// <returns></returns>
        public ActionResult Problem(int pageIndex = 1)
        {
            OJEntities oe = new OJEntities();
            var model = oe.Problem.OrderBy(a => a.problem_id).ToPagedList(pageIndex, pageColums);
            if (Request.IsAjaxRequest())
            {
                return PartialView("_ProblemList", model);
            }
            return View(model);
        }

        /// <summary>
        /// 题目详情
        /// </summary>
        /// <param name="problem_id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ProblemInfo(int problem_id)
        {
            OJEntities oJEntities = new OJEntities();
            Problem model = oJEntities.Problem.Find(problem_id);
            Session["problem_id"] = problem_id;
            return View(model);
        }



        /// <summary>
        /// 用户修改
        /// </summary>
        /// <returns></returns>
        public ActionResult UserEdit()
        {
            OJEntities oJEntities = new OJEntities();
            Users u = oJEntities.Users.Find(Session["user_id"]);
            return View(u);
        }

        [HttpPost]
        public ActionResult UserEdit(Users user)
        {
            OJEntities oJEntities = new OJEntities();
            Users u = oJEntities.Users.Find(Session["user_id"]);
            u.password = user.password;
            oJEntities.SaveChanges();
            Response.Write("<script>alert('修改成功！')</script>");
            return View(u);
        }

        /// <summary>
        /// 排名
        /// </summary>
        /// <returns></returns>
        public ActionResult Rank(int pageIndex = 1)
        {
            OJEntities oj = new OJEntities();
            var model = oj.Users.OrderByDescending(a => a.solved).ToPagedList(pageIndex, pageColums);
            if (Request.IsAjaxRequest())
            {
                return PartialView("_UserList", model);
            }
            return View(model);
        }

        /// <summary>
        /// 注销账号
        /// </summary>
        /// <returns></returns>
        public JsonResult Cancel()
        {
            Session.Clear();
            return Json(true);
        }



    }
}