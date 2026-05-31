using Domain.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interface
{
    public interface IApplicationDbContext
    {
        DbSet<StudentBasicInfo> StudentBasicInfos { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
