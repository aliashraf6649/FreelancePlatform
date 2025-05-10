using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreelancePlatform.Domain.Entities;
using FreelancePlatform.Application.Dtos;

namespace FreelancePlatform.Application.Common.Interfaces
{
    public interface IJobService
    {
        Task<Job> CreateJobAsync(JobDto jobDto, int clientId);
        Task<IEnumerable<Job>> GetOpenJobsAsync();
        Task<Job> GetJobByIdAsync(int id);
        Task<IEnumerable<Job>> GetJobsByClientAsync(int clientId);
    }
}
