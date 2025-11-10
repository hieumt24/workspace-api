
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories
{
    class WorkSpaceTypeRepository : IWorkSpaceTypeRepository
    {
        private readonly WorkSpaceContext _context;

        public WorkSpaceTypeRepository(WorkSpaceContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<List<WorkSpaceType>> GetAllWorkSpaceType()
        {
            return await _context.WorkSpaceTypes.ToListAsync();
        }

        public async Task<List<Domain.Entities.WorkSpace>> GetWorkSpacesByTypeId(int typeId)
        {
            return await _context.Workspaces
                                 .Where(ws => ws.WorkSpaceTypeId == typeId)
                                 .ToListAsync();
        }
    }
}
