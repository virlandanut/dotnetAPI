using AutoMapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserEFController : ControllerBase
{
    readonly IUserRepository _userRepository;
    readonly IMapper _mapper;

    public UserEFController(IConfiguration config, IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _mapper = new Mapper(
            new MapperConfiguration(config =>
            {
                config.CreateMap<UserToAddDto, User>();
            })
        );
    }

    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        IEnumerable<User> users = _userRepository.GetUsers();
        return users;
    }

    [HttpGet("GetSingleUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        return _userRepository.GetSingleUser(userId);
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        User? userInDatabase = _userRepository.GetSingleUser(user.UserId);

        if (userInDatabase != null)
        {
            userInDatabase.FirstName = user.FirstName;
            userInDatabase.LastName = user.LastName;
            userInDatabase.Email = user.Email;
            userInDatabase.Gender = user.Gender;
            userInDatabase.Active = user.Active;
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Failed to Update User");
        }

        throw new Exception("Failed to Get User");
    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        User userDb = _mapper.Map<User>(user);
        _userRepository.AddEntity<User>(userDb);
        if (_userRepository.SaveChanges())
        {
            return Ok();
        }

        throw new Exception("Failed to Add User");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        User? userInDatabase = _userRepository.GetSingleUser(userId);
        if (userInDatabase != null)
        {
            _userRepository.RemoveEntity<User>(userInDatabase);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Failed to Delete User");
        }

        throw new Exception("Failed to Get User");
    }

    [HttpGet("GetSingleUserJobInfo/{userId}")]
    public UserJobInfo GetSingleUserJobInfo(int userId)
    {
        UserJobInfo? userJobInfo = _userRepository.GetSingleUserJobInfo(userId);
        if (userJobInfo != null)
        {
            return userJobInfo;
        }

        throw new Exception("Failed to get User Job Info");
    }

    [HttpPut("EditUserJobInfo")]
    public IActionResult EditUserJobInfo(UserJobInfo userJobInfo)
    {
        UserJobInfo? userJobInfoInDatabase = _userRepository.GetSingleUserJobInfo(
            userJobInfo.UserId
        );

        if (userJobInfoInDatabase != null)
        {
            userJobInfoInDatabase.JobTitle = userJobInfo.JobTitle;
            userJobInfoInDatabase.Department = userJobInfo.Department;
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Failed to Update User Job Info");
        }

        throw new Exception("Failed to Get User Job Info");
    }

    [HttpPost("AddUserJobInfo")]
    public IActionResult AddUserJobInfo(UserJobInfo userJobInfo)
    {
        _userRepository.AddEntity<UserJobInfo>(userJobInfo);
        if (_userRepository.SaveChanges())
        {
            return Ok();
        }

        throw new Exception("Failed to Add User Job Info");
    }

    [HttpDelete("RemoveUserJobInfo/{userId}")]
    public IActionResult RemoveUserJobInfo(int userId)
    {
        UserJobInfo? userJobInfoInDatabase = _userRepository.GetSingleUserJobInfo(userId);
        if (userJobInfoInDatabase != null)
        {
            _userRepository.RemoveEntity<UserJobInfo>(userJobInfoInDatabase);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Failed to Delete User Job Info");
        }

        throw new Exception("Failed to get User Job Info");
    }

    [HttpGet("GetSingleUserSalary/{userId}")]
    public UserSalary GetSingleUserSalary(int userId)
    {
        return _userRepository.GetSingleUserSalary(userId);
    }

    [HttpPut("EditUserSalary")]
    public IActionResult EditUserSalary(UserSalary userSalary)
    {
        UserSalary? userSalaryInDatabase = _userRepository.GetSingleUserSalary(userSalary.UserId);
        if (userSalaryInDatabase != null)
        {
            userSalaryInDatabase.Salary = userSalary.Salary;
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Failed to Update Salary");
        }

        throw new Exception("Failed to Get User Salary");
    }

    [HttpPost("AddUserSalary")]
    public IActionResult AddUserSalary(UserSalary userSalary)
    {
        _userRepository.AddEntity<UserSalary>(userSalary);
        if (_userRepository.SaveChanges())
        {
            return Ok();
        }

        throw new Exception("Failed to Add User Salary");
    }

    [HttpDelete("RemoveUserSalary/{userId}")]
    public IActionResult RemoveUserSalary(int userId)
    {
        UserSalary? userSalaryInDatabase = _userRepository.GetSingleUserSalary(userId);

        if (userSalaryInDatabase != null)
        {
            _userRepository.RemoveEntity<UserSalary>(userSalaryInDatabase);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Failed to Delete User Salary");
        }
        throw new Exception("Failed to Get User Salary");
    }
}
