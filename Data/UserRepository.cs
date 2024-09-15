using DotnetAPI.Models;

namespace DotnetAPI.Data;

public class UserRepository : IUserRepository
{
    DataContextEF _entityFramework;

    public UserRepository(IConfiguration config)
    {
        _entityFramework = new DataContextEF(config);
    }

    public bool SaveChanges()
    {
        return _entityFramework.SaveChanges() > 0;
    }

    public void AddEntity<T>(T entityToAdd)
    {
        if (entityToAdd != null)
        {
            _entityFramework.Add(entityToAdd);
        }
    }

    public void RemoveEntity<T>(T entityToRemove)
    {
        if (entityToRemove != null)
        {
            _entityFramework.Remove(entityToRemove);
        }
    }

    public IEnumerable<User> GetUsers()
    {
        IEnumerable<User> users = _entityFramework.Users.ToList<User>();
        return users;
    }

    public User GetSingleUser(int userId)
    {
        User? user = _entityFramework
            .Users.Where(userInDatabase => userInDatabase.UserId == userId)
            .FirstOrDefault<User>();
        if (user != null)
        {
            return user;
        }

        throw new Exception("User does not exist!");
    }

    public UserSalary GetSingleUserSalary(int userId)
    {
        UserSalary? userSalary = _entityFramework
            .UserSalary.Where(salary => salary.UserId == userId)
            .FirstOrDefault<UserSalary>();
        if (userSalary != null)
        {
            return userSalary;
        }

        throw new Exception("Failed to find User Salary");
    }

    public UserJobInfo GetSingleUserJobInfo(int userId)
    {
        UserJobInfo? userJobInfo = _entityFramework
            .UserJobInfo.Where(jobInfo => jobInfo.UserId == userId)
            .FirstOrDefault<UserJobInfo>();
        if (userJobInfo != null)
        {
            return userJobInfo;
        }

        throw new Exception("Failed to get User Job Info");
    }
}
