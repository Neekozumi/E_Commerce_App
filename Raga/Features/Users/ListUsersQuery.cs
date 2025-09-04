using MediatR;

public class ListUsersQuery : IRequest<IEnumerable<Users>>
{
}

public class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, IEnumerable<Users>>
{
    private readonly UserManager<Users> _userManager;

    public ListUsersQueryHandler(UserManager<Users> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<Users>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(_userManager.Users.ToList());
    }
}
