using System;
using System.Linq.Expressions;
using SharedLibrary.Dtos;

namespace AuthServer.Core.Services
{
	public interface IGenericService<TEntity, TDto>
        where TEntity: class
        where TDto: class
	{
        Task<Response<TDto>> GetByIdAsync(int Id);

        Task<Response<IEnumerable<TDto>>> GetAllAsync();

        Task<Response<IEnumerable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate);

        Task<Response<TDto>> AddAsync(TDto dto);

        Task<Response<NoDataDto>> Remove(int Id);

        Task<Response<NoDataDto>> Update(TDto dto, int Id);
    }
}

