using MZXYOnlineJudge.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer;
using Webdiyer.WebControls.Mvc;
namespace MZXYOnlineJudge.Controllers
{
    public class JudgeController : Controller
    {
        /// <summary>
        /// 存放代码的地址
        /// </summary>
        private static string CODEPATH = @"D:\毕业设计\OnlineJudgeServer\OJServer\bin\Debug\test";


        /// <summary>
        /// 提交代码
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult UpLoadCode(Code param)
        {

            if (Session["user_id"] == null)
            {
                return Json(false);
            }
            string user_id = Session["user_id"].ToString();
            int problem_id = Convert.ToInt32(Session["problem_id"]);

            // 写入用户题库
            if (!Directory.Exists(CODEPATH + "/" + user_id + "/" + problem_id))
            {
                Directory.CreateDirectory(CODEPATH + "/" + user_id + "/" + problem_id);
            }
            string srpath = CODEPATH + "/" + user_id + "/" + problem_id + "/a.c";
            StreamWriter sr = new StreamWriter(srpath, false);
            sr.Write(param.text);
            sr.Flush();
            sr.Close();

            // 写入数据库
            OJEntities oj = new OJEntities();
            Solution solution = new Solution();
            solution.problem_id = problem_id;
            solution.user_id = user_id;
            solution.status = "wait";
            solution.uploadtime = DateTime.Now;
            oj.Solution.Add(solution);
            oj.SaveChanges();
            Solution s;
            while (true)
            {
                oj.Dispose();
                oj = new OJEntities();
                s = oj.Solution.Find(solution.solution_id);
                if (s.status == "judged")
                {
                    if (s.result == 1) // 1是成功
                    {
                        return Json("1");//"\"result\":\"1\"");
                    }
                    else if (s.result == 0) // 编译失败
                    {
                        return Json("0");//"\"result\":\"0\"");
                    }
                    else if (s.result == -1) // 结果错误
                    {
                        return Json("-1");
                    }
                }
            }
        }

        /// <summary>
        /// 用户详细信息
        /// </summary>
        /// <returns></returns>
        public ActionResult UserInfo()
        {
            Solution solution = new Solution();
            solution.user_id = Session["user_id"].ToString();
            OJEntities oJEntities = new OJEntities();
            List<Solution> model = oJEntities.Solution.Where<Solution>(s => s.user_id == solution.user_id).ToList();
            ViewBag.success = model.Where(s => s.result == 1).Count();
            ViewBag.error = model.Where(s => s.result == -1).Count();
            ViewBag.warning = model.Where(s => s.result == 0).Count();
            //Session[""] = 
            model.Reverse();
            var mdl =model.ToPagedList(1,1000);//OrderBy(a=>a.user_id).ToPagedList(1,1000);
            return View(mdl);
        }

        public ActionResult CJudgeRecard(int pageIndex = 1)
        {
            Solution solution = new Solution();
            //solution.user_id = Session["user_id"].ToString();
            OJEntities oJEntities = new OJEntities();
            List<Solution> model = oJEntities.Solution.ToList();// Where<Solution>(s => s.user_id == solution.user_id).ToList();
            ViewBag.success = model.Where(s => s.result == 1).Count();
            ViewBag.error = model.Where(s => s.result == -1).Count();
            ViewBag.warning = model.Where(s => s.result == 0).Count();
            //Session[""] = 
            model.Reverse();
            var mdl = model.ToPagedList(pageIndex, 20);//OrderBy(a=>a.user_id).ToPagedList(1,1000);
            if (Request.IsAjaxRequest())
            {
                return PartialView("_ProblemList", mdl);
            }
            return View(mdl);
        }
    }
}