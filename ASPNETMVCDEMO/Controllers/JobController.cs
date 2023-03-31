using ASPNETMVCDEMO.Data;
using ASPNETMVCDEMO.Models;
using ASPNETMVCDEMO.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                return RedirectToAction("Index");
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
    }
}
