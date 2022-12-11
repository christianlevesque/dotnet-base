using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autoinjector;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Repositories.Identity;

[ExcludeFromCodeCoverage]
[Service(ServiceLifetime.Scoped, typeof(IUserRepository))]
public class UserRepository : RepositoryBase<ApplicationUser, string>, IUserRepository
{
	public UserRepository(ApplicationDbContext context) : base(context) { }

	/// <inheritdoc />
	public override async Task<ApplicationUser> FindById(string id, Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>? include = null)
	{
		include ??= Include;
		var user = await include(Context.Set<ApplicationUser>())
			           .FirstOrDefaultAsync(e => e.Id.Equals(id));
		if (user is null)
		{
			throw KeyNotFound(id);
		}

		return user;
	}

	/// <inheritdoc />
	public override async Task<string> Create(ApplicationUser entity)
	{
		await Context.Set<ApplicationUser>()
		             .AddAsync(entity);
		await Context.SaveChangesAsync();
		return entity.Id;
	}

	/// <inheritdoc />
	protected override IQueryable<ApplicationUser> Search(IQueryable<ApplicationUser> results, string searchTerm)
	{
		return results.Where(u => u.UserName.Contains(searchTerm)
		                       || u.Email.Contains(searchTerm)
		                       || u.PhoneNumber.Contains(searchTerm));
	}

	/// <inheritdoc />
	protected override Expression<Func<ApplicationUser, object>> GetSortPredicate(string sortName) => sortName switch
	{
		nameof(ApplicationUser.UserName) => u => u.UserName,
		nameof(ApplicationUser.Email) => u => u.Email,
		_ => u => u.UserName
	};
}

public interface IUserRepository : IRepository<ApplicationUser, string>
{
}