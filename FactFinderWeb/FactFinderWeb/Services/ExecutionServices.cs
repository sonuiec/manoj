using FactFinderWeb.IServices;
using FactFinderWeb.Models;
using FactFinderWeb.ModelsView;
using FactFinderWeb.Utils;
using Microsoft.EntityFrameworkCore;

namespace FactFinderWeb.Services
{

    public class ExecutionServices
    {
        private ResellerBoyinawebFactFinderWebContext _context;
        private readonly long _userID;
        private readonly HttpContext _httpContext;
        int updateRows = 0;

        public ExecutionServices(ResellerBoyinawebFactFinderWebContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContext = httpContextAccessor.HttpContext;
            var userIdStr = _httpContext.Session.GetString("UserId");
            _userID = Convert.ToInt64(userIdStr);
        }

        public async Task<int> InvestCheckDataThenUpdate(long profileId)
        {
            // Check if any records exist for the given profileId
            bool dataExists = await _context.TblffWingsGoalStep5ExecutionData.AnyAsync(x => x.Profileid == profileId);

            if (!dataExists)
            {
                await ExecutionAddToTable();
            }
            else
            {
                await ExecutionUpdateToTable();
            }
            return 0;
        }
        public async Task<int> ExecutionAddToTable()
        {
            var userGoals = await _context.TblffWings.Where(w => w.Profileid == _userID).ToListAsync();
            foreach (var goal in userGoals)
            {
                var masterExecutions = await _context.TblffWingsGoalStep5ExecutionMasters
                                        .Where(m => m.GoalType == goal.GoalType).ToListAsync();

                foreach (var masterItem in masterExecutions)
                {
                    var newExecutionData = new TblffWingsGoalStep5ExecutionDatum
                    {
                        Profileid = goal.Profileid,
                        Step5ExecutionMasterid = masterItem.Id,
                        Goalid = (int)goal.Id,
                        GoalName = goal.GoalName,
                        ExecutionDescription = masterItem.ExecutionDescription,
                        ExecutionValueType = masterItem.ExecutionValueType,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    };
                    _context.TblffWingsGoalStep5ExecutionData.Add(newExecutionData);
                }
            }
            return await _context.SaveChangesAsync();             
        }

        public async Task<int> ExecutionUpdateToTable()
        {
            var existingExecutionData = await _context.TblffWingsGoalStep5ExecutionData
                                        .Where(w => w.Profileid == _userID).ToListAsync();

            var userGoals = await _context.TblffWings.Where(w => w.Profileid == _userID).ToListAsync();

            // Identify goals to add
            var goalsToAdd = userGoals.Where(incoming => !existingExecutionData
                                      .Any(existing => existing.Goalid == incoming.Id))
                                      .Select(incoming => new TblffWingsGoalStep5ExecutionDatum
                                      {Goalid = (int)incoming.Id}).ToList();

            // Identify goals to delete
            var goalsToDelete = existingExecutionData.Where(existing => !userGoals
                                                     .Any(incoming => incoming.Id == existing.Goalid)).ToList();

            if (goalsToAdd.Any())
            {
                foreach (var goal in userGoals)
                {
                    if(!goalsToAdd.Any(g => g.Goalid == (int)goal.Id)){continue;} 
                    // Skip if the goal is already in the add list
                    var masterExecutions = await _context.TblffWingsGoalStep5ExecutionMasters
                                                 .Where(m => m.GoalType == goal.GoalType).ToListAsync();

                    foreach (var masterItem in masterExecutions)
                    {
                        var newExecutionData = new TblffWingsGoalStep5ExecutionDatum
                        {
                            Profileid = goal.Profileid,
                            Step5ExecutionMasterid = masterItem.Id,
                            Goalid = (int)goal.Id,
                            GoalName = goal.GoalName,
                            ExecutionDescription = masterItem.ExecutionDescription,
                            ExecutionValueType = masterItem.ExecutionValueType,
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now,
                        };
                        _context.TblffWingsGoalStep5ExecutionData.Add(newExecutionData);
                    }
                }
            }

            if (goalsToDelete.Any()) { _context.TblffWingsGoalStep5ExecutionData.RemoveRange(goalsToDelete); }

                return await _context.SaveChangesAsync();
                /*
                            // Identify goals to update
                            var goalsToUpdate = existingExecutionData.Where(existing => incomingGoals.Any(incoming => incoming.Id == existing.Goalid)).ToList();
                            foreach (var goal in goalsToUpdate)
                            {
                                var incomingGoal = incomingGoals.First(x => x.Id == goal.Goalid);
                                goal.GoalName = incomingGoal.GoalName;
                                //goal.GoalType = incomingGoal.GoalType;
                                goal.UpdateDate = DateTime.Now;
                                // Update other fields as needed
                            }*/
        }

        public async Task<int> ExecutionUpdateToTableSubmit(ExecutionWithPrecisionModelView executionWithPrecisionModelView)
        { 
            foreach (var executionData in executionWithPrecisionModelView.wingsGoalStep5ExecutionDataList)
            {
                var dataToUpdate = await _context.TblffWingsGoalStep5ExecutionData
                                .FirstOrDefaultAsync(x =>  x.Id == executionData.Id && x.Profileid == _userID);
                if (dataToUpdate != null)
                {
                    dataToUpdate.ExecutionValue = executionData.ExecutionValue;
                    dataToUpdate.UpdateDate = DateTime.Now;
                    _context.TblffWingsGoalStep5ExecutionData.Update(dataToUpdate);
                }
            }
            int updateRows = 0;
            updateRows = await _context.SaveChangesAsync();
            return updateRows;
        }


        public async Task<ExecutionWithPrecisionModelView> GetExecutionData(long profileId)
        {
            ExecutionWithPrecisionModelView executionMV = new ExecutionWithPrecisionModelView();
            executionMV.wingsGoalStep5ExecutionDataList = await (
                        from exec in _context.TblffWingsGoalStep5ExecutionData
                        join wings in _context.TblffWings  on exec.Goalid equals (int)wings.Id
                        where exec.Profileid == profileId && wings.Profileid == profileId
                        orderby wings.GoalPriority
                        select new WingsGoalStep5ExecutionDataMV
                        {
                            Id = exec.Id,
                            Profileid = exec.Profileid,
                            Step5ExecutionMasterid = exec.Step5ExecutionMasterid,
                            Goalid = exec.Goalid,
                            GoalName = wings.GoalName, // from joined table
                            ExecutionDescription = exec.ExecutionDescription,
                            ExecutionValue = exec.ExecutionValue,
                            CreateDate = exec.CreateDate,
                            UpdateDate = exec.UpdateDate, // Optional: add GoalPriority for sorting or display
                            GoalPriority = wings.GoalPriority
                            //        Id = x.Id,
                            //Profileid = x.Profileid,
                            //Step5ExecutionMasterid = x.Step5ExecutionMasterid,
                            //Goalid = x.Goalid,
                            //GoalName = x.GoalName,
                            //ExecutionDescription = x.ExecutionDescription,
                            //ExecutionValue = x.ExecutionValue,
                            //CreateDate = x.CreateDate,
                            //UpdateDate = x.UpdateDate
                        }).ToListAsync();
            /*   await _context.TblffWingsGoalStep5ExecutionData.Where(x => x.Profileid == profileId)
                .Select(x => new WingsGoalStep5ExecutionData*/
            return executionMV;
        }
    }
}

/*
public async Task<Int64> WingsAddExecutionData(TblffWingsGoalStep5ExecutionDatum wings)
{
    var tblwings = new TblffWingsGoalStep5ExecutionDatum();

    //tblwings.Id = wings.Id;
    tblwings.Profileid = _userID;
    tblwings.Goalid = wings.Goalid;
    tblwings.GoalName = wings.GoalName;
    tblwings.Step5ExecutionMasterid = wings.Step5ExecutionMasterid;
    tblwings.ExecutionDescription = wings.ExecutionDescription;
    tblwings.ExecutionValue = wings.ExecutionValue;
    tblwings.CreateDate = DateTime.Now;
    tblwings.UpdateDate = DateTime.Now;
    _context.TblffWingsGoalStep5ExecutionData.Add(wings);
    await _context.SaveChangesAsync();
    return wings.Id;
}


public async Task<Int64> WingsUpdateExecutionData(TblffWingsGoalStep5ExecutionDatum wings)
{
    var tblwings = new TblffWingsGoalStep5ExecutionDatum();

    tblwings.Id = wings.Id;
    tblwings.Step5ExecutionMasterid = wings.Step5ExecutionMasterid;
    tblwings.ExecutionDescription = wings.ExecutionDescription;
    tblwings.ExecutionValue = wings.ExecutionValue;
    tblwings.CreateDate = DateTime.Now;
    tblwings.UpdateDate = DateTime.Now;
    _context.TblffWingsGoalStep5ExecutionData.Update(wings);
    await _context.SaveChangesAsync();
    return wings.Id;
}

public async Task<Int64> WingsUpdateExecutionDataForWings(List<WingsGoalStep5ExecutionData> wingsGoalStep5ExecutionDatas1)
{
    //var wingsGoalStep5ExecutionDataList = new List<WingsGoalStep5ExecutionData>();
    foreach (var wingsGoalExecuation in wingsGoalStep5ExecutionDatas1)
    {
        var wingsGoalStep5ExecutionData = new TblffWingsGoalStep5ExecutionDatum();
        wingsGoalStep5ExecutionData.Profileid = _userID;
        //wingsGoalStep5ExecutionData.Goalid = goal.Goalid;
        wingsGoalStep5ExecutionData.GoalName = wingsGoalExecuation.GoalName;
        wingsGoalStep5ExecutionData.ExecutionDescription = null;
        wingsGoalStep5ExecutionData.ExecutionValue = null;
        wingsGoalStep5ExecutionData.CreateDate = DateTime.Now;
        wingsGoalStep5ExecutionData.UpdateDate = DateTime.Now;
        _context.TblffWingsGoalStep5ExecutionData.Add(wingsGoalStep5ExecutionData);
    //_context.TblffWingsGoalStep5ExecutionData.Add(wingsGoalStep5ExecutionDataList);
    }

    Int64 updateRows = 0;
    updateRows = await _context.SaveChangesAsync();
    return updateRows;
}
*/