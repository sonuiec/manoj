using FactFinderWeb.IServices;
using FactFinderWeb.Models;
using FactFinderWeb.ModelsView;
using FactFinderWeb.Utils;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;
using System.Data;

namespace FactFinderWeb.Services
{
    public class WingsServices
    {

        private ResellerBoyinawebFactFinderWebContext _context;
        private readonly long _userID;
        private readonly HttpContext _httpContext;
        private readonly ExecutionServices _executionServices;
        private readonly InvestServices _investServices;
        int updateRows = 0;
        /*
 private static readonly List<WingsGoalSelect> StaticGoals = new()
        {
            new WingsGoalSelect { GoalValue = "Emergency Fund", GoalName = "Emergency Fund" },
            new WingsGoalSelect { GoalValue = "Retirement - Accumulation", GoalName = "Retirement - Accumulation" },
            new WingsGoalSelect { GoalValue = "Purchase of Dream Car", GoalName = "Purchase of Dream Car" },
            new WingsGoalSelect { GoalValue = "World Tour", GoalName = "World Tour" },
            new WingsGoalSelect { GoalValue = "Purchase of Dream Home", GoalName = "Purchase of Dream Home" },
            new WingsGoalSelect { GoalValue = "Seed Capital for Business", GoalName = "Seed Capital for Business" },
            new WingsGoalSelect { GoalValue = "Charity", GoalName = "Charity" },
        };
        */
        public WingsServices(ResellerBoyinawebFactFinderWebContext context, IHttpContextAccessor httpContextAccessor, ExecutionServices executionServices, InvestServices investServices)
        {
            _context = context;
            _httpContext = httpContextAccessor.HttpContext;
            _executionServices = executionServices;
            _investServices = investServices;
            var userIdStr = _httpContext.Session.GetString("UserId");
            _userID = Convert.ToInt64(userIdStr);
        }

        public async Task<WingsViewModel> WingsList()
        {
            var wingsViewModel = new WingsViewModel();
            var tblwings = await _context.TblffWings
                .Where(x => x.Profileid == _userID)
                .Select(x => new UserWings
                {
                    Id = x.Id,
                    Profileid = x.Profileid,
                    GoalType = x.GoalType,
                    GoalPriority = x.GoalPriority,
                    GoalName = x.GoalName,
                    GoalPlanYear = x.GoalPlanYear,
                    GoalStartYear = x.GoalStartYear,
                    GoalEndYear = x.GoalEndYear,
                    TimeHorizon = x.TimeHorizon,
                    CreateDate = x.CreateDate,
                    UpdateDate = x.UpdateDate,
                    NewGoals = x.NewGoals
                }).ToListAsync();


            var localStorageGoals = tblwings.OrderBy(g => g.GoalPriority).Select(g => new UserWingsLocalStorageGoalDto
            {
                priority = g.GoalPriority,
                goal = g.GoalName,
                planYear = g.GoalPlanYear,
                startYear = g.GoalStartYear,
                timeHorizon = g.TimeHorizon,
                goalEndYear = g.GoalEndYear,
                NewGoals = g.NewGoals
            }).ToList();

            wingsViewModel.UserWingsList = tblwings;     
            wingsViewModel.goalList = localStorageGoals;
            //wingsViewModel.ChildrenList=WingsChildrenList().Result.ChildrenList;
            return wingsViewModel;
        }

        public async Task<List<ChildrenList>> WingsChildrenList()
        {
            var tblChildren = new List<ChildrenList>();
             tblChildren = await _context.TblffAwarenessChildren
                .Where(x => x.Profileid == _userID)
                .Select(x => new ChildrenList
                {
                    ChildName = x.ChildName,
                    ChildAge = x.ChildDob,
                    ChildGender = x.ChildGender
                }).ToListAsync();
            //wingsViewModel.ChildrenList = tblChildren;            
            return tblChildren;
        }

        public async Task<List<SelectListItem>> WingsBindSelect()
        {
               // var wingsViewModel = new WingsViewModel();
                var tblChildren = await _context.TblffAwarenessChildren
               .Where(x => x.Profileid == _userID)
               .Select(x => new ChildrenList
               {
                   ChildName = x.ChildName,
                   ChildAge = x.ChildDob,
                   ChildGender = x.ChildGender
               }).ToListAsync();

                var goalOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Select" },
                    new SelectListItem { Value = "Emergency Fund", Text = "Emergency Fund" },
                    new SelectListItem { Value = "Retirement - Accumulation", Text = "Retirement - Accumulation" }
                };
                
                foreach (var child in tblChildren) // 2. Dynamically add "Child Education" if needed
                {
                    goalOptions.Add(new SelectListItem
                    {
                        Value = "Child Education - " + child.ChildName,
                        Text = "Child Education - " + child.ChildName
                    });
                } 
                
                goalOptions.AddRange(new List<SelectListItem>// 3. Next 3 options
                {
                    new SelectListItem { Value = "Purchase of Dream Car", Text = "Purchase of Dream Car" },
                    new SelectListItem { Value = "World Tour", Text = "World Tour" },
                    new SelectListItem { Value = "Purchase of Dream Home", Text = "Purchase of Dream Home" }
                });

                
                foreach (var child in tblChildren) // 4. Dynamically add "Child Marriage" if needed
                {
                    goalOptions.Add(new SelectListItem
                    {
                        Value = "Child Marriage - " + child.ChildName,
                        Text = "Child Marriage - " + child.ChildName
                    });
                } 
                
                goalOptions.AddRange(new List<SelectListItem> // 5. Remaining options
                {
                    new SelectListItem { Value = "Seed Capital for Business", Text = "Seed Capital for Business" },
                    new SelectListItem { Value = "Charity", Text = "Charity" }
                });

                //wingsViewModel.GoalOptions = goalOptions;
                return goalOptions;
        }

        public async Task<int> WingsAddDeleteGoal(List<UserWingsUI> submittedGoals)
        {
            // Get all existing goals for this user UserWingsLocalStorageGoalDto
            var existingGoals = _context.TblffWings.Where(g => g.Profileid == _userID).ToList();

            // Track changes
            var goalsToAdd = new List<TblffWing>();
            var goalsToUpdate = new List<TblffWing>();
            var goalsToDelete = new List<TblffWing>();
            
            foreach (var submitted in submittedGoals)// Identify added or updated goals
            {
                var existing = existingGoals.FirstOrDefault(g => g.GoalName == submitted.goal);

                if (existing != null)
                {                    
                    if (existing.GoalPriority != submitted.priority)// Goal exists: update priority if changed
                    {
                        existing.GoalPriority = submitted.priority;
                        existing.UpdateDate = DateTime.Now;
                        goalsToUpdate.Add(existing);
                    } 
                }
                else // New goal: add it
                {                    
                    var newGoal = new TblffWing
                    {
                        Profileid = _userID,
                        GoalPriority = submitted.priority,
                        GoalName = submitted.goal,
                        GoalPlanYear = submitted.planYear,
                        GoalStartYear = submitted.startYear,
                        GoalEndYear = submitted.goalEndYear,
                        TimeHorizon = submitted.timeHorizon,
                        NewGoals = submitted.NewGoals,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                        GoalType = submitted.NewGoals == 1 ? "custom" : WingsSetGoalType(submitted.goal)
                    };
                    goalsToAdd.Add(newGoal);
                }
            }

            // Identify deleted goals (in DB but not in submitted list)
            var submittedGoalNames = submittedGoals.Select(g => g.goal).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var existing in existingGoals)
            {
                if (!submittedGoalNames.Contains(existing.GoalName))
                {
                    goalsToDelete.Add(existing);
                }
            }

            // Apply DB changes
            if (goalsToDelete.Any()) _context.TblffWings.RemoveRange(goalsToDelete);
            if (goalsToUpdate.Any()) _context.TblffWings.UpdateRange(goalsToUpdate);
            if (goalsToAdd.Any()) _context.TblffWings.AddRange(goalsToAdd);
                        
            return await _context.SaveChangesAsync(); // Save changes
        }

        private string WingsSetGoalType(string goalType)
        {
            string goalsdata_GoalType = "custom"; // Default to custom if no match found

            if (goalType == "Emergency Fund")
            {
                goalsdata_GoalType = "Emergency-Fund";
            }
            else if (goalType == "Retirement - Accumulation")
            {
                goalsdata_GoalType = "Retirement-Accumulation";
            }
            else if (goalType == "Purchase of Dream Car")
            {
                goalsdata_GoalType = "Purchase-of-Dream-Car";
            }
            else if (goalType == "World Tour")
            {
                goalsdata_GoalType = "World-Tour";
            }
            else if (goalType == "Purchase of Dream Home")
            {
                goalsdata_GoalType = "Purchase-of-Dream-Home";
            }
            else if (goalType == "Seed Capital for Business")
            {
                goalsdata_GoalType = "Seed-Capital-for-Business";
            }
            else if (goalType == "Charity")
            {
                goalsdata_GoalType = "Charity";
            }
            else if (goalType.Contains("Education"))
            {
                goalsdata_GoalType = "Education";
            }
            else if (goalType.Contains("Marriage"))
            {
                goalsdata_GoalType = "Marriage";
            }
            else
            {
                goalsdata_GoalType = "custom";
            }

            return goalsdata_GoalType;
        }

        public async Task<Int64> WingsAdd(TblffWing wings)
        {
            var tblwings = new TblffWing();

            tblwings.Id = wings.Id;
            tblwings.Profileid = _userID;
            tblwings.GoalType = wings.GoalType;
            tblwings.GoalPriority = wings.GoalPriority;
            tblwings.GoalName = wings.GoalName;
            tblwings.GoalPlanYear = wings.GoalPlanYear;
            tblwings.GoalStartYear = wings.GoalStartYear;
            tblwings.GoalEndYear = wings.GoalEndYear;
            tblwings.TimeHorizon = wings.TimeHorizon;
            tblwings.CreateDate = DateTime.Now;
            tblwings.UpdateDate = DateTime.Now;
            tblwings.NewGoals = wings.NewGoals;
            _context.TblffWings.Add(wings);
            await _context.SaveChangesAsync();
            return wings.Id;
        }
        
        public async Task<Int64> WingsUpdate(TblffWing wings)
        {
            var tblwings = new TblffWing();
            tblwings.Id = wings.Id;
            tblwings.Profileid = wings.Profileid;
            tblwings.GoalType = wings.GoalType;
            tblwings.GoalPriority = wings.GoalPriority;
            tblwings.GoalName = wings.GoalName;
            tblwings.GoalPlanYear = wings.GoalPlanYear;
            tblwings.GoalStartYear = wings.GoalStartYear;
            tblwings.GoalEndYear = wings.GoalEndYear;
            tblwings.TimeHorizon = wings.TimeHorizon;
            tblwings.UpdateDate = DateTime.Now;
            tblwings.NewGoals = wings.NewGoals;
            _context.TblffWings.Add(wings);
            await _context.SaveChangesAsync();

            if (wings.Id == 0)
            {
                //_executionServices.WingsUpdateExecutionDataForWings(InvesttblList);
                //_investServices.WingsUpdateInvestDataForWings(wings);
            }
            else
            {
                _context.TblffWings.Update(wings);
            }
            return wings.Id;
        }


        public async Task<ApplicantDataDto?> GetApplicantDataAsync(long profileId)
        {
            var sql = "EXEC spApplicantData @profileid";

            var param = new SqlParameter("@profileid", SqlDbType.BigInt)
            {
                Value = profileId
            };

            var result = await _context.ApplicantDataDto
                .FromSqlRaw(sql, param)
                .AsNoTracking()
                .ToListAsync();

            return result.FirstOrDefault();
        }

    }
}
