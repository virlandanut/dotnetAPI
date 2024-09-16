using DotnetAPI.Data;
using DotnetAPI.Models;

namespace DotnetAPI.Helpers
{
    public class ReusableSql
    {
        private readonly DataContextDapper _dapper;

        public ReusableSql(IConfiguration configuration)
        {
            _dapper = new DataContextDapper(configuration);
        }

        public bool Upsert(UserComplete user)
        {
            string sql =
                @"EXEC TutorialAppSchema.spUser_Upsert
                @FirstName = @FirstNameParam, 
                @LastName = @LastNameParam, 
                @Email = @EmailParam, 
                @Gender = @GenderParam, 
                @Active = @ActiveParam,
                @JobTitle = @JobTitleParam,
                @Department = @DepartmentParam,
                @Salary = @SalaryParam,
                @UserId = @UserIdParam";

            return _dapper.ExecuteSql(
                sql,
                new
                {
                    FirstNameParam = user.FirstName,
                    LastNameParam = user.LastName,
                    EmailParam = user.Email,
                    GenderParam = user.Gender,
                    ActiveParam = user.Active.ToString(),
                    JobTitleParam = user.JobTitle,
                    DepartmentParam = user.Department,
                    SalaryParam = user.Salary,
                    UserIdParam = user.UserId,
                }
            );
        }
    }
}
