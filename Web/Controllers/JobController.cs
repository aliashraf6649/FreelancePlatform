using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FreelancePlatform.Infrastructure.Services;
using FreelancePlatform.Web.ViewModels;
using FreelancePlatform.Application.Dtos;
using FreelancePlatform.Application.Common.Interfaces;
using FreelancePlatform.Application.Services;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FreelancePlatform.Infrastructure.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using FreelancePlatform.Domain.Enums;
using FreelancePlatform.Domain.Entities;

namespace FreelancePlatform.Web.Controllers
{
    [Authorize]
    public class JobController : Controller
    {
        private readonly IJobService _jobService;
        private readonly IOfferService _proposalService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<JobController> _logger;
        private readonly AppDbContext _context;

        public JobController(
            IJobService jobService,
            ICurrentUserService currentUserService,
            IOfferService proposalService,
            AppDbContext context,
            ILogger<JobController> logger)
        {
            _jobService = jobService;
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _proposalService = proposalService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize] // Ensure this attribute is present
        public IActionResult Create()
        {
            return View(new JobViewModel());
        }

        [Authorize] // Ensure this attribute is present
        [HttpPost]
        public async Task<IActionResult> Create(JobViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Add null checks for current user
            if (_currentUserService == null)
            {
                ModelState.AddModelError("", "User service not available");
                return View(model);
            }

            if (!_currentUserService.UserId.HasValue)
            {
                // This could happen if [Authorize] is missing or service is misconfigured
                return Challenge(); // Will redirect to login page
            }

            var jobDto = new JobDto
            {
                Title = model.Title,
                Description = model.Description,
                Budget = model.Budget,
                Deadline = model.Deadline,
            };

            try
            {
                var job = await _jobService.CreateJobAsync(jobDto, _currentUserService.UserId.Value);
                return RedirectToAction("Details", new { id = job.Id });
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error creating job");
                ModelState.AddModelError("", "An error occurred while creating the job");
                return View(model);
            }
        }


        [HttpGet]
public async Task<IActionResult> Details(int id)
{
    try
    {
        var job = await _jobService.GetJobByIdAsync(id);
        if (job == null)
        {
            return NotFound();
        }

        var proposals = await _proposalService.GetProposalsForJobAsync(id);
        var isClient = _currentUserService.UserId == job.ClientId;

        var viewModel = new JobViewModel.JobDetailsViewModel
        {
            Job = job,
            Proposals = proposals ?? new List<Offer>(),
            IsClient = isClient,
            CanSubmitProposal = !isClient && 
                               job.Status == JobStatus.Open && 
                               !(proposals?.Any(p => p.FreelancerId == _currentUserService.UserId) ?? false)
        };

        return View(viewModel);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error loading job details for ID {id}");
        return RedirectToAction("Error", "Home");
    }
}

        [HttpGet]
    public async Task<IActionResult> MyJobs()
    {
        var currentUserId = _currentUserService.UserId.Value;
        
        var jobs = await _context.Jobs
            .Include(j => j.Proposals)
            .Where(j => j.ClientId == currentUserId)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new JobViewModel
            {
                Id = j.Id,
                Title = j.Title,
                // Status = j.Status,
                Budget = j.Budget,
                // CreatedAt = j.CreatedAt,
                Deadline = j.Deadline,
                // ProposalCount = j.Proposals.Count
            })
            .ToListAsync();

        return View(jobs);
    }

        [HttpGet]
        public async Task<IActionResult> Browse()
        {
            var jobs = await _jobService.GetOpenJobsAsync();
            return View(jobs);
        }
    }
}