﻿using System.Linq;
using System.Web;
using System.Xml.Linq;
using MiniMVC;
using Quartz;
using Quartz.Impl.Matchers;
using QuartzNetWebConsole.Views;

namespace QuartzNetWebConsole.Controllers {
    public class JobGroupController : Controller {
        private readonly IScheduler scheduler = Setup.Scheduler();

        public override void Execute(HttpContextBase context) {
            var group = context.Request.QueryString["group"];
            var jobNames = scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(group));
            var runningJobs = scheduler.GetCurrentlyExecutingJobs();
            var jobs = jobNames.Select(j => {
                var job = scheduler.GetJobDetail(j);
                var interruptible = typeof (IInterruptableJob).IsAssignableFrom(job.JobType);
                var jobContext = runningJobs.FirstOrDefault(r => r.JobDetail.Key.ToString() == job.Key.ToString());
                return new JobWithContext(job, jobContext, interruptible);
            });
            var paused = scheduler.IsJobGroupPaused(group);
            var thisUrl = context.Request.RawUrl;
            var highlight = context.Request.QueryString["highlight"];
            var view = Views.Views.JobGroup(group, paused, highlight, thisUrl, jobs);
            context.XDocument(Helpers.XHTML(view));
        }
    }
}