using System.Threading.Tasks;

namespace Domain.Base.DomainRepository
{
    public interface IDomainRepository<TElem, TElemId> where TElem : class
    {
        TElem GetById(TElemId id);
        void Save(TElem elem);

        Task<TElem> GetByIdAsync(TElemId id);
        Task SaveAsync(TElem elem);

        TElem GetNewAggregate();
    }
}
