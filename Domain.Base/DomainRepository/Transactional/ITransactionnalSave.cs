using System.Threading.Tasks;

namespace Domain.Base.DomainRepository.Transactional
{
    public interface ITransactionnalSave<TElem, TElemId> where TElem : class
    {
        TElem Save(TElem elem, IUnitOfWork<TElem, TElemId> uow);
        Task<TElem> SaveAsync(TElem elem, IUnitOfWork<TElem, TElemId> uow);
    }
}