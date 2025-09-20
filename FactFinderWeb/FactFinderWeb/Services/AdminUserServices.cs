using FactFinderWeb.IServices;
using FactFinderWeb.Models;
using FactFinderWeb.ModelsView;
using FactFinderWeb.ModelsView.AdminMV;
using FactFinderWeb.Utils;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;

namespace FactFinderWeb.Services
{
    public class AdminUserServices
    {

        private ResellerBoyinawebFactFinderWebContext _context;
        private readonly long _userID;
        private readonly HttpContext _httpContext;

        public AdminUserServices(ResellerBoyinawebFactFinderWebContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContext = httpContextAccessor.HttpContext;
            var userIdStr = _httpContext.Session.GetString("AdminUserId");
            _userID = Convert.ToInt64(userIdStr);
        }

        public async Task<MVAdminProfile> AdminUserLogin(MVLoginAdmin userlogin)
        {
            //string Passwordhashed = UtilityHelperServices.PasswordHash(userlogin.Password);

            var user = _context.Set<TblFfAdminUser>()
                    .Where(o => o.Email == userlogin.Email && o.AccountStatus.ToLower() == "active")
                    .Select(o => new MVAdminProfile
                    {
                        Id = o.Id,
                        AdminUserEmail = o.Email,
                        AdminUserFullName = o.Name,
                        AdminUserRole = o.AdminRole,
                        Accesskey = o.Password
                    }
                    ).FirstOrDefault();
            if (user != null)
            {
                bool isValid = UtilityHelperServices.PasswordVerify(user.Accesskey, userlogin.Password);
                if (isValid)
                {
                    user.Accesskey = null; // Clear the password from the object
                }
                else
                {
                    user = null;
                }
            }
            return user;
        }



        public async Task<List<MVAdminProfile>> GetAdminList()
        { 
            var userList = await (from ruser in _context.TblFfAdminUsers 
                                  orderby ruser.Adminuserid descending
                                  select new MVAdminProfile
                                  {
                                      Id = ruser.Id,
                                      AdminUserFullName = ruser.Name,
                                      AdminUserEmail = ruser.Email,
                                      Mobile = ruser.Mobile,
                                        Adminuserid = ruser.Adminuserid,
                                        AdminRole = ruser.AdminRole,
                                        Department = ruser.Department,
                                        AccountStatus = ruser.AccountStatus,
                                        CreateDate = ruser.CreateDate,
                                        Accesskey = ruser.Accesskey
                                  }).ToListAsync();
                                  //}).Take(100).ToListAsync();
            return userList;
        }


        public async Task<List<SelectListItem>> GetAdvisorList()
        {
            var advisorListdata = await (from ruser in _context.TblFfAdminUsers
                                  where ruser.AccountStatus.ToLower() =="active"
                                  orderby ruser.Adminuserid descending
                                  select new AdvisorList
                                  {
                                      AdvisorId = ruser.Id.ToString(),
                                      AdvisorName = ruser.Name
                                      //AdminRole = ruser.AdminRole,
                                  }).ToListAsync();
            //}).Take(100).ToListAsync();

            var advisorLists = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Select" },
                };

            foreach (var advisor in advisorListdata) // 2. Dynamically add "Child Education" if needed
            {
                advisorLists.Add(new SelectListItem
                {
                    Value = advisor.AdvisorId,
                    Text = advisor.AdvisorName
                });
            }

            return advisorLists;
        }



        public string checkEmailExist(string email)
        {
            string ExistsUsername = _context.Set<TblFfAdminUser>()
                    .Where(o => o.Email == email)
                    .Select(o => o.Email).FirstOrDefault();

            return ExistsUsername;
        }



        public async Task<List<MVADUserDetails>> GetuserList(string adminRole , int advisorID)
        {
            //var userList = await _context.TblFfRegisterUsers
            //.LeftJoin(_context.TblffAwarenessProfileDetails, p => p.UserId, u => u.Id, (p, u) => new { p, u })
            // MVADUserDetails userList = new MVADUserDetails();
            var userList = new List<MVADUserDetails>();
            if(adminRole == "advisor")
            {
                 userList = await (from ruser in _context.TblFfRegisterUsers
                                      join user in _context.TblffAwarenessProfileDetails on ruser.Id equals user.Profileid
                                      where user.Advisorid == advisorID
                                      orderby ruser.Createddate descending
                                      select new MVADUserDetails
                                      {
                                          Name = ruser.Name,
                                          planType = ruser.Plantype,
                                          planYear = user.PlanYear.ToString(),
                                          email = ruser.Email,
                                          mobile = ruser.Mobile,
                                          activestatus = ruser.Activestatus == "1" ? "Active" : "Deactive",
                                          createddate = ruser.Createddate,
                                          userFile = ruser.Ptx, //UserFile
                                          ProfileId = ruser.Id,
                                          Id = user.Id,
                                          advisorid = ruser.Advisorid
                                      }).ToListAsync();
            }
            else { 


             userList = await (from ruser in _context.TblFfRegisterUsers
                                  join user in _context.TblffAwarenessProfileDetails on ruser.Id equals user.Profileid
                                  orderby ruser.Createddate descending
                                  select new MVADUserDetails
                                  {
                                      Name = ruser.Name,
                                      planType = ruser.Plantype,
                                      planYear = user.PlanYear.ToString(),
                                      email = ruser.Email,
                                      mobile = ruser.Mobile,
                                      activestatus = ruser.Activestatus == "1" ? "Active" : "Deactive",
                                      createddate = ruser.Createddate,
                                      userFile = ruser.Ptx, //UserFile
                                      ProfileId = ruser.Id,
                                      Id = user.Id,
                                      advisorid = ruser.Advisorid
                                  }).ToListAsync();
            }
            return userList;

        }



        public async Task<UserProfileViewModel> GetUserDetail(long Userid)
        {
            //var userList = await _context.TblFfRegisterUsers
            //.LeftJoin(_context.TblffAwarenessProfileDetails, p => p.UserId, u => u.Id, (p, u) => new { p, u })
            // MVADUserDetails userList = new MVADUserDetails();
            var userList = await (from ruser in _context.TblFfRegisterUsers
                                  join user in _context.TblffAwarenessProfileDetails 
                                  on ruser.Id equals user.Profileid where ruser.Id == Userid
                                  orderby ruser.Createddate descending
                                  select new UserProfileViewModel
                                  {
                                      AdvisorName = user.AdvisorName,
                                      Advisorid = user.Advisorid,
                                      UserFullName = ruser.Name,
                                      UserPlan = ruser.Plantype,
                                      UserPlanYear = user.PlanYear,
                                      UserEmail = ruser.Email,
                                      UserEmailVerification = ruser.Emailverified,
                                      UserMobile = ruser.Mobile,
                                      UserActiveStatus = ruser.Activestatus,// == "1" ? "Active" : "Deactive"
                                      UserRegisterDate = ruser.Createddate,
                                      Userptx = ruser.Ptx, //user submitted =1 or admin locked = 2
                                      ProfileStatus = user.ProfileStatus, //user submitted =pending or admin locked = locked
                                      UId = ruser.Id
                                  }).FirstOrDefaultAsync();
            return userList;
        }



        public async Task<int> UserUpdate(UserProfileViewModel userProfileViewModel, string adminRole)
        {


            TblffAwarenessProfileDetail userprofile = await _context.TblffAwarenessProfileDetails.Where(x => x.Profileid == Convert.ToInt64(userProfileViewModel.UId)).FirstOrDefaultAsync();

            if(userprofile == null)
            {
                return 0; // User not found
            }

            if (adminRole.ToLower() != "advisor")
            {
                userprofile.Advisorid = userProfileViewModel.Advisorid;
                //userprofile.AdvisorName = userProfileViewModel.AdvisorName;
            }
            userprofile.ProfileStatus = userProfileViewModel.ProfileStatus;            
            _context.TblffAwarenessProfileDetails.Update(userprofile);

            TblFfRegisterUser userAccount = await _context.TblFfRegisterUsers.FirstOrDefaultAsync(x => x.Id == userProfileViewModel.UId);

            if (userAccount == null)
            {
                return 0; // User not found
            }
            else if (userProfileViewModel.UserActiveStatus != userAccount.Activestatus)
            {
                if (adminRole.ToLower() != "advisor")
                {
                    userAccount.Activestatus = userProfileViewModel.UserActiveStatus;  //profile locked by admin
                    _context.TblFfRegisterUsers.Update(userAccount);
                }
            }
            int resultCount = await _context.SaveChangesAsync();
            return resultCount;
        }


        public async Task<Int64> AdminUserAdd(TblFfAdminUser adminUser)
        {
            var user = new TblFfAdminUser();
            string Passwordhashed = UtilityHelperServices.PasswordHash(adminUser.Password);
            adminUser.Password = Passwordhashed;
            _context.TblFfAdminUsers.Add(adminUser);

            int resultCount = await _context.SaveChangesAsync();

            return resultCount;
        }

        public async Task<bool> AddAdminUserAsync(AdminRegViewModel newUser)
        {
            try
            {
                bool emailExists = await _context.TblFfAdminUsers.AnyAsync(u => u.Email == newUser.Email);

                if (emailExists)
                {
                    return false; // Or throw/return error if needed
                }

                string hashedPassword = UtilityHelperServices.PasswordHash(newUser.Password);

                var entity = new TblFfAdminUser
                {
                    Name = newUser.Name,
                    Email = newUser.Email,
                    Password = hashedPassword,
                    AdminRole = newUser.AdminRole,
                    Department = newUser.Department, // Add if exists
                    Mobile = newUser.Mobile          // Add if exists
                };

                _context.TblFfAdminUsers.Add(entity);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<TblFfAdminUser> GetAdminUserDetail(long Userid)
        {
            var adminuserList = await (from ruser in _context.TblFfAdminUsers
                                       where ruser.Id == Userid 
                                  orderby ruser.CreateDate descending
                                  select new TblFfAdminUser
                                  {
                                      Id = ruser.Id,
                                      Name = ruser.Name,
                                      Email = ruser.Email,
                                      AdminRole = ruser.AdminRole,
                                      Department = ruser.Department,
                                      AccountStatus = ruser.AccountStatus,
                                      CreateDate = ruser.CreateDate,
                                      UpdateDate = ruser.UpdateDate
                                  }).FirstOrDefaultAsync();
            return adminuserList;
        }


        public async Task<int> UpdateAdminUserDetail(TblFfAdminUser adminuser)
        {
            var adminuserList = await _context.TblFfAdminUsers.Where(u => u.Id == adminuser.Id)
                                       .FirstOrDefaultAsync();

            if (adminuserList == null)
            {
                return 0; // User not found
            }

            adminuserList.AccountStatus = adminuser.AccountStatus;  
            adminuserList.AdminRole = adminuser.AdminRole;  
            adminuserList.UpdateDate = DateTime.UtcNow;
            _context.TblFfAdminUsers.Update(adminuserList);

            int resultCount = await _context.SaveChangesAsync();
            return resultCount;
        }


        public async Task<Int32> AdminChangepwd(long adminID, string oldPwd, string newPwd)
        {
            //string Passwordhashedold = UtilityHelperServices.PasswordHash(oldPwd);

            var adminuserData = await _context.TblFfAdminUsers.Where(u => u.Id == adminID).FirstOrDefaultAsync();
            // && u.Password == Passwordhashedold
            if (adminuserData == null)
            {
                return 0; // User not found
            }

            string Passwordhashed = UtilityHelperServices.PasswordHash(newPwd);
            adminuserData.Password = Passwordhashed;
            adminuserData.UpdateDate = DateTime.Now;
            _context.TblFfAdminUsers.Update(adminuserData);
            int resultCount = await _context.SaveChangesAsync();
            return resultCount;
        }

    }
}
