using ASPNETMVCDEMO.Data;
using ASPNETMVCDEMO.Models;
using ASPNETMVCDEMO.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace ASPNETMVCDEMO.Controllers
{
    public class JobController : Controller
    {
        private readonly MVCDemoDbContext mvcDemoDbContext;

        public JobController(MVCDemoDbContext mvcDemoDbContext)
        {
            this.mvcDemoDbContext = mvcDemoDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index() 
        {
            ViewBag.SuccessMessage = TempData["successMessage"] as string;
            ViewBag.FailureMessage = TempData["failureMessage"] as string;
            var jobs = await mvcDemoDbContext.Jobs.ToListAsync();
            return View(jobs);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddJobViewModel addJobRequest)
        {
            var job = new Job()
            {
                Id = Guid.NewGuid(),
                Name = addJobRequest.Name,
                Url = addJobRequest.Url,
                Description = addJobRequest.Description,
                Interval = addJobRequest.Interval,
                DateOfLastStart = null
            };

            await mvcDemoDbContext.Jobs.AddAsync(job);
            await mvcDemoDbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> View(Guid id)
        {
            var job = await mvcDemoDbContext.Jobs.FirstOrDefaultAsync(x => x.Id == id);

            if (job == null)
            {
                return NotFound();
            }

            var viewModel = new UpdateJobViewModel() {
                Id = job.Id,
                Name = job.Name,
                Url = job.Url,
                Description = job.Description,
                Interval = job.Interval,
                DateOfLastStart = job.DateOfLastStart
            };

            return await Task.Run(() => View("View", viewModel));
        }


        [HttpPost]
        public async Task<IActionResult> View(UpdateJobViewModel model)
        {
            var job = await mvcDemoDbContext.Jobs.FindAsync(model.Id);

            if (job != null) {
                job.Name = model.Name;
                job.Description = model.Description;
                job.Interval = model.Interval;
                job.Url = model.Url;
                await mvcDemoDbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");   
        }

        [HttpPost]
        public async Task<IActionResult> Delete(UpdateJobViewModel model)
        {
            var job = await mvcDemoDbContext.Jobs.FindAsync(model.Id);

            if (job != null)
            {
                mvcDemoDbContext.Jobs.Remove(job);
                await mvcDemoDbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Start(Guid id)
        {
            var job = await mvcDemoDbContext.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            var httpClient = new HttpClient();
            var url = job.Url;
            job.DateOfLastStart = DateTime.Now;

            try
            {
                var response = await httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                var statusCode = (int)response.StatusCode;

                var jobExecutionHistory = new JobExecutionHistory
                {
                    ExecutionDate = DateTime.Now,
                    StatusCode = statusCode,
                    Response = responseBody,
                    JobId = job.Id,
                    Success = response.IsSuccessStatusCode
                };
                mvcDemoDbContext.JobExecutionHistory.Add(jobExecutionHistory);
                await mvcDemoDbContext.SaveChangesAsync();

                var message = $"Job '{job.Name}' was executed successfuly!";
                TempData["successMessage"] = message;
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                // connectivity error
                var jobExecutionHistory = new JobExecutionHistory
                {
                    ExecutionDate = DateTime.Now,
                    StatusCode = 0, // Set status code to 0 for connection failure
                    Response = $"Connectivity error! [{ex.Message}]",
                    JobId = job.Id,
                    Success = false
                };
                mvcDemoDbContext.JobExecutionHistory.Add(jobExecutionHistory);
                await mvcDemoDbContext.SaveChangesAsync();

                var message = $"Job '{job.Name}' failed!";
                TempData["failureMessage"] = message;
            }
            catch (HttpRequestException ex) when (ex.InnerException is null)
            {
                // malrformed url
                var jobExecutionHistory = new JobExecutionHistory
                {
                    ExecutionDate = DateTime.Now,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Response = $"Url malrformed! [{ex.Message}]",
                    JobId = job.Id,
                    Success = false
                };
                mvcDemoDbContext.JobExecutionHistory.Add(jobExecutionHistory);
                await mvcDemoDbContext.SaveChangesAsync();

                var message = $"Job '{job.Name}' failed!";
                TempData["failureMessage"] = message;
            }
            catch (Exception ex)
            {
                var jobExecutionHistory = new JobExecutionHistory
                {
                    ExecutionDate = DateTime.Now,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Response = $"Exception thrown! [{ex.Message}]",
                    JobId = job.Id,
                    Success = false
                };
                mvcDemoDbContext.JobExecutionHistory.Add(jobExecutionHistory);
                await mvcDemoDbContext.SaveChangesAsync();

                var message = $"Job '{job.Name}' failed!";
                TempData["failureMessage"] = message;
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> History(Guid id)
        {
            var job = await mvcDemoDbContext.Jobs.FirstOrDefaultAsync(j => j.Id == id);
            if (job == null)
            {
                return NotFound();
            }

            var history = await mvcDemoDbContext.JobExecutionHistory
                .Where(x => x.JobId == id).OrderByDescending(x => x.ExecutionDate).Take(10).ToListAsync();

            var historyModel = new HistoryViewModel()
            {
                JobId = job.Id,
                JobName = job.Name,
                JobExecutionHistoryList = history.Select(x => new JobExecutionHistoryViewModel()
                {
                    ExecutionDate = x.ExecutionDate,
                    StatusCode = x.StatusCode,
                    Response = x.Response,
                    Success = x.Success
                }).ToList()
            };

            return View(historyModel);
        }
    }
}
