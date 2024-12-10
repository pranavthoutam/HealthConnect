namespace HealthConnect.Repositories
{
    public class UserRepository:IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserRepository(ApplicationDbContext context, UserManager<User> user)
        {
            _context = context; 
            _userManager = user;
        }


    }
}
