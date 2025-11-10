using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories
{
    public interface IWorkSpaceTypeRepository
    {
        Task<List<WorkSpaceType>> GetAllWorkSpaceType();
        Task<List<WorkSpace.Domain.Entities.WorkSpace>> GetWorkSpacesByTypeId(int typeId);
    }
}
