namespace Accyourate.Core.Repositories;

public interface IReadOnlyRepository<TEntity>
{
    IReadOnlyList<TEntity> GetAll();
    TEntity? GetById(int id);
}
