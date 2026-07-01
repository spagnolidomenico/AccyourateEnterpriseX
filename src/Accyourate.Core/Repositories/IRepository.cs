namespace Accyourate.Core.Repositories;

public interface IRepository<TEntity>
{
    IReadOnlyList<TEntity> GetAll();
    TEntity? GetById(int id);
    int Create(TEntity entity);
    void Update(TEntity entity);
    void Delete(int id);
}
