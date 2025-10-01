 
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SystemTextJson = System.Text.Json.JsonSerializer;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

//using System.Text.Json;
using stJSON= System.Text.Json;
using System.Collections.Generic;
using FactFinderWeb.API.Models;
using Newtonsoft.Json;

//using NewtonsoftSerializer = Newtonsoft.Json.JsonConvert;

namespace FactFinderWeb.API.Utils
{
    public class JSONDataUtility
    {

        private readonly FactFinderDbContext _context;
        public JSONDataUtility(FactFinderDbContext context) => _context = context;


        List<object> selectedFormsdata = new List<object>();
        List<object> GoalsNameList = new List<object>();
        public static long ConvertPhoneToNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return 0;

            // Remove any non-numeric characters (like +, -, spaces)
            string digits = new string(phone.Where(char.IsDigit).ToArray());

            if (long.TryParse(digits, out long result))
                return result;

            return 0; // fallback if parsing fails
        }
 

        public async Task<string> GetAwarenessJSON(long profileId)
        {
            var result = new
            {
                data = new
                {
                    awareness = BuildAwarenessSection(profileId),
                    wings = BuildWingsSection(profileId),
                     alertness = BuildAlertnessSection(profileId),
                    knowledge = BuildKnowledgeSection(profileId),
                    executePlan = BuildExecutePlanSection(profileId),
                    invest = BuildInvestSection(profileId)
                }
            };

            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        private object BuildAwarenessSection(long profileId)
        {
            var profile = _context.TblffAwarenessProfileDetails.FirstOrDefault(p => p.ProfileId == profileId);
            var spouse = _context.TblffAwarenessSpouses.FirstOrDefault(s => s.Profileid == profileId);
            var children = _context.TblffAwarenessChildren.Where(c => c.Profileid == profileId).ToList();
            var Assumptions = _context.TblffAwarenessAssumptions.Where(c => c.Profileid == profileId).FirstOrDefault();
            //var familyFinancial = _context.TblffAwarenessFamilyFinancials.FirstOrDefault(f => f.Profileid == profileId);


            return new
            {
                planDetails = new
                {
                    planType = profile?.PlanType ?? "",
                    planYear = profile?.PlanYear.ToString() ?? "",
                    planDuration = profile?.PlanDuration ?? "",
                },
                personalDetails = new
                {
                    name = profile?.Name ?? "",
                    gender = profile?.Gender ?? "",
                    dob = DateTime.TryParse(profile?.Dob, out var d) ? d.ToString("yyyy-MM-dd") : "",
                    phone = ConvertPhoneToNumber(profile?.Phone ?? ""),
                    altPhone = ConvertPhoneToNumber(profile?.Altphone ?? ""),
                    aadhaar = profile?.Aadhaar ?? "",
                    email = profile?.Email ?? "",
                    secEmail = profile?.SecEmail ?? "",
                    pan = profile?.Pan ?? "",
                    occupation = profile?.Occupation ?? "",
                    address = new
                    {
                        isSameAddress = profile.IsSameAddress,
                        residential = new
                        {
                           // address = profile.ResAddress?? "",
                            country = profile.ResCountry?? "",
                            countryCode = profile.ResCountry?? "",
                            state = profile.ResState?? "",
                            stateCode = profile.ResState?? "",
                            city = profile.ResCity?? "",
                           // pinCode = profile.ResPincode
                        },
                        permanent = new
                        {
                            //address = profile.PermAddress?? "",
                            country = profile.PermCountry?? "",
                            countryCode = profile.PermCountry?? "",
                            state = profile.PermState?? "",
                            stateCode = profile.PermState?? "",
                            city = profile.PermCity?? "",
                            //pinCode = profile.PermPincode
                        },
                        office = new
                        {
                           // company = profile.CompanyName?? "",
                           // address = profile.CompanyAddress?? "",
                            country = profile.CompanyCountry?? "",
                            countryCode = profile.CompanyCountry?? "",
                            state = profile.CompanyState?? "",
                            stateCode = profile.CompanyState?? "",
                            city = profile.CompanyCity?? "",
                           // pinCode = profile.CompanyPincode ?? ""
                        },
                       
                    }
                },
                familyFinancial = new
                {
                    stock = profile?.Stock ?? "",
                    income = profile?.Income ?? "",
                    payment = profile?.Payment ?? "",
                    holiday = profile?.Holiday ?? "",
                    shopping = profile?.Shopping ?? ""
                },
                maritalDetails = new
                {
                    maritalStatus = profile?.MaritalStatus ?? "",
                    spouseDetails = spouse == null ? null : new
                    {
                        spouseName = spouse.SpouseName ?? "",
                       // spouseGender = spouse.SpouseGender ?? "",
                        spouseDob = DateTime.TryParse(spouse.SpouseDob, out var sd) ? sd.ToString("yyyy-MM-dd") : "",
                        spousePhone = ConvertPhoneToNumber(spouse.SpousePhone ?? ""),
                      ///  spouseAltPhone = ConvertPhoneToNumber(spouse.SpouseAltPhone ?? ""),
                       // spouseAadhaar = spouse.SpouseAadhaar ?? "",
                        spouseEmail = spouse.SpouseEmail ?? "",
                       // spouseSecEmail = spouse.SpouseSecEmail ?? "",
                        spousePan = spouse.SpousePan ?? "",
                        spouseOccupation = spouse.SpouseOccupation ?? ""
                    },
                    haveChildren = children.Any() ? "Yes" : "No",
                    childrenDetails = children.Select(c => new
                    {
                        id = c.Id,
                        childName = c.ChildName ?? "",
                        childGender = c.ChildGender ?? "",
                        childDob = DateTime.TryParse(c.ChildDob, out var cd) ? cd.ToString("yyyy-MM-dd") : "",
                        childPhone = ConvertPhoneToNumber(c.ChildPhone ?? ""),
                       // childAadhaar = c.ChildAadhaar ?? "",
                       // childEmail = c.ChildEmail ?? "",
                       // childPan = c.ChildPan ?? ""
                    }).ToList()
                },
                assumptions = new
                {
                    equity = Assumptions?.Equity ?? 0,
                    debt = Assumptions?.Debt ?? 0,
                    gold = Assumptions?.Gold ?? 0,
                    realEstateReturn = Assumptions?.RealEstateReturn ?? 0,
                    liquidFunds = Assumptions?.LiquidFunds ?? 0,
                    inflationRates = Assumptions?.InflationRates ?? 0,
                    educationInflation = Assumptions?.EducationInflation ?? 0,
                    applicantRetirement = Assumptions?.ApplicantRetirement ?? 0,
                    spouseRetirement = Assumptions?.SpouseRetirement ?? 0,
                    applicantLifeExpectancy = Assumptions?.ApplicantLifeExpectancy ?? 0,
                    spouseLifeExpectancy = Assumptions?.SpouseLifeExpectancy ?? 0,
                }
            };
        }
        private object BuildWingsSection(long profileId)
        {
            /*// {
            //     "id": 4,
            //     "priority": 3, //--- it denotes on this particular priority the goal will be
            //     "goal": "Purchase of Dream Car",
            //     "goalStartYear": 2056,
            //     "goalPlanYear": 2025, // it is non input field which will be referenced from awareness
            //     "goalEndYear" : "",
            //     "timeHorizon": 31, // it is also autopopulated field
            //     "newGoals": false // --- it tells whether it is newlyadded goal which can be added dynamically through the application
            // }*/
            var WingsData = _context.TblffWings.Where(p => p.Profileid == profileId)
                                   .Select(u => new { u.Id, u.GoalPriority, u.GoalName, u.GoalStartYear, u.GoalPlanYear, u.GoalEndYear, u.TimeHorizon, u.NewGoals }).ToList();

            ////var selectedFormsdata = new List<object>();

            foreach (var goal in WingsData)
            {
                var goalObj = new
                {
                    id = goal.Id,
                    priority = goal.GoalPriority,
                    goal = goal.GoalName,
                    goalStartYear = goal.GoalStartYear,
                    goalPlanYear = goal.GoalPlanYear,
                    goalEndYear = goal.GoalEndYear ,
                    timeHorizon = goal.TimeHorizon,
                    newGoals = goal.NewGoals
                };
                var goalNames = new { 
                    goal = goal.GoalName
                };
                GoalsNameList.Add(goalNames);  // Add each goal object to the selectedForms array
                selectedFormsdata.Add(goalObj);  // Add each goal object to the selectedForms array
            }
            return new
            {
                forms = WingsData.Select(item => new
                {
                    id = item.Id,
                    goal = item.GoalName
                }).ToList(),

                selectedForms = selectedFormsdata
            };
        }

        private object BuildAlertnessSection(long profileId)
        {
            var profileIdParam = new SqlParameter("@ProfileId", profileId);

            var jsonResult = _context.Database
                .SqlQueryRaw<AlertnessResult>("EXEC Sp_AlertnessAPIJshon @ProfileId", profileIdParam)
                .AsEnumerable()
                .FirstOrDefault();

            if (jsonResult == null || string.IsNullOrEmpty(jsonResult.AlertnessJson))
                return new { }; // return empty object if SP fails

            // Parse raw JSON string to a dynamic object
            var alertnessData = JsonConvert.DeserializeObject<object>(jsonResult.AlertnessJson);

            return alertnessData;
        }

        public class AlertnessResult
        {
            public string? AlertnessJson { get; set; }  // column alias in SP should match this
        }
        private object BuildKnowledgeSection(long profileId)
        {
            var kData = _context.TblffKnowledgeRisks.Where(p => p.Profileid == profileId).FirstOrDefault();

            return new
            {
                knowledge = new
                {
            riskCapacity = string.IsNullOrEmpty(kData?.RiskCapacity) ? "" : kData.RiskCapacity,
            riskRequirement = string.IsNullOrEmpty(kData?.RiskRequirement) ? "" : kData.RiskRequirement,
            totalRiskProfileScore = string.IsNullOrEmpty(kData?.TotalRiskProfileScore) ? "" : kData.TotalRiskProfileScore,
                    plannerAssessmentOnRiskProfile = string.IsNullOrEmpty(kData?.PlannerAssessmentOnRiskProfile) ? "" : kData.PlannerAssessmentOnRiskProfile,
            riskTolerance = string.IsNullOrEmpty(kData?.RiskTolerance) ? "" : kData.RiskTolerance,
                }
            };
        }

        private object BuildExecutePlanSection(long profileId)
        {
            var executionData = _context.TblffWingsGoalStep5ExecutionData
                .Where(p => p.Profileid == profileId)
                .Select(u => new { u.GoalName, u.ExecutionDescription, u.ExecutionValue })
                .ToList();

            var groupedGoals = executionData
                .GroupBy(e => e.GoalName)
                .Select(group =>
                {
                    var goalDict = new Dictionary<string, object>
                    {
                        ["name"] = group.Key
                    };

                    foreach (var item in group)
                    {
                        var key = GetJsonKey(item.ExecutionDescription);
                        if (!goalDict.ContainsKey(key))
                            goalDict[key] = item.ExecutionValue ?? "";
                    }
                    return goalDict;
                }).ToList();
            //return new { goalData = groupedGoals };

            return new
            {
                executePlan = new
                {
                    lifeInsurance = new { incomeUsedForFamily = "", fundsReturnRate = "", inflationRate = "" },
                    spouseDetails = new { Sliabilities = "" },
                    goalNames = new { GoalsNameList },
                    goalData = groupedGoals,
                    _id = profileId
                }
            };

        }
        private object BuildInvestSection(long profileId)
        {/*
        id: 2,
          earmarkedForGoal: "",
          goalOptions: [{ value: "", label: "Select Goal" }],  // these will be dynamically populated based on the selected goals in wings
          typeOfFund: "",
          selectedFunds: "",
          lumpSum: "",
          SIP: "",
          selectedFundsOptions: [{ value: "", label: "Select Fund" }],*/
            /*
            var InvestMasterDataUser = _context.TblffInvestWingsGoalMasters.Where(p => p.Profileid == profileId)
                          .Select(u => new { u.Id, u.IntendedSipmonthly, u.AvailableLumpsum }).FirstOrDefault();
            var InvestDataUser = _context.TblffInvestWingsGoals.Where(p => p.Profileid == profileId)
                          .Select(u => new { 
                             id  = u.Id,
                              earmarkedForGoal = u.GoalName ?? "", 
                              goalOptions = new List<object>(),
                              typeOfFund ="",
                              selectedFunds ="",
                              lumpSum = u.LumpsumAmount ?? 0,
                              SIP = u.Sipamount ?? 0,
                              selectedFundsOptions = new List<object>()
                          }).ToList();
            var GeneralInsuranceDataUser = _context.TblffInvestWingsGoals.Where(p => p.Profileid == profileId)
                         .Select(u => new { u.Id, u.GoalName, u.LumpsumAmount, u.Sipamount }).ToList();
            var LifeInsuranceDataUser = _context.TblffInvestWingsGoals.Where(p => p.Profileid == profileId)
                         .Select(u => new { u.Id, u.GoalName, u.LumpsumAmount, u.Sipamount }).ToList();*/
            var InvestMasterDataUser = _context.TblffInvestWingsGoalMasters
    .Where(p => p.Profileid == profileId)
    .Select(u => new { u.Id, u.IntendedSipmonthly, u.AvailableLumpsum })
    .FirstOrDefault();

            var InvestDataUser = _context.TblffInvestWingsGoals
                .Where(p => p.Profileid == profileId)
                .Select(u => new
                {
                    id = u.Id,
                    earmarkedForGoal = u.GoalName ?? "",
                    goalOptions = new List<object>(),
                    typeOfFund = "",
                    selectedFunds = "",
                    lumpSum = u.LumpsumAmount ?? 0,
                    SIP = u.Sipamount ?? 0,
                    selectedFundsOptions = new List<object>()
                }).ToList() ;

            var GeneralInsuranceDataUser = _context.TblffInvestWingsGoals
                .Where(p => p.Profileid == profileId)
                .Select(u => new { u.Id, u.GoalName, u.LumpsumAmount, u.Sipamount })
                .ToList();//?? new List<object>()

            var LifeInsuranceDataUser = _context.TblffInvestWingsGoals
                .Where(p => p.Profileid == profileId)
                .Select(u => new { u.Id, u.GoalName, u.LumpsumAmount, u.Sipamount })
                .ToList();//?? new List<object>()

            return new
            {
                wings = new { expectedReturns = new { GoalsNameList }, },
                alertness = new { familyMonthlyIncomeTotal = "", familyMonthlyExpensesTotal = "" },
                knowledge = new { selectedRiskProfile = "" },
                executeWithPrecision = new
                {
                    hofInsurance = new
                    {
                        homeInsurance = new { currentCoverage = "", requiredCoverage = "", shortfall = "" },
                        carInsurance = new { currentCoverage = "", requiredCoverage = "", shortfall = "" }
                    }
                },
                actNow = new
                {
                    savingsForInvestments = new { intendedSipMonthly = InvestMasterDataUser?.IntendedSipmonthly ?? 0, availableLumpsum = InvestMasterDataUser?.AvailableLumpsum ?? 0, },
                    lifeInsuranceReq = LifeInsuranceDataUser , // dynamically generated
                    generalInsuranceReq = GeneralInsuranceDataUser, // dynamically generated
                    //generalInsuranceReq = new { GeneralInsuranceDataUser }, // dynamically generated
                    //lifeInsuranceReq =  new List<object>() , // dynamically generated
                    //generalInsuranceReq =  new List<object>() , // dynamically generated
                    actionPlanFinancialGoals = new
                    {
                        goals = new { GoalsNameList },// new List<object>(),
                        total = new { totalLumpSum = "", totalFixorSipAmt = "", totalSipAmt = "", totalGapBtnSip = "" }
                    },
                    goalStatusReport = new { GoalsNameList },
                    totalGoalStatusReport = new
                    {
                        totalFutureValueCurrentCorpus = "",
                        totalFutureValueLumpSum = "",
                        totalFutureValueSip = "",
                        totalTotalCorpus = "",
                        totalReqCorpus = ""
                    },
                    goalsOptions = new { GoalsNameList },
                    earmarked = InvestDataUser // List of funds
                }
            };
        }

        public string GenerateGoalJson(string goalName, string goalDescription, string goalValue)
        {
            // Create an anonymous object that represents the goal data

            var goalData = new
            {
                goalName = goalName,
                goalDescription = goalDescription,
                goalValue = goalValue
            };


            var goalJson = new object();
            /*
            switch (goalName)
            {
                case "Emergency Fund":
                    goalJson = new
                    {
                        name = goalName,
                        monthsRequired = goal.MonthsRequired ?? "",
                        availableEmergencyFund = goal.AvailableEmergencyFund ?? "",
                        annualAmountForFund = goal.AnnualAmountForFund ?? "",
                        expectedAnnualIncrease = goal.ExpectedAnnualIncrease ?? "",
                        monthlySipTopup = goal.MonthlySipTopup ?? ""
                    };
                    break;

                case "Retirement Accumulation":
                    goalJson = new
                    {
                        name = goal.Name,
                        monthlyRetirementExpenses = goal.MonthlyRetirementExpenses ?? "",
                        expectedReturnOnPortfolio = goal.ExpectedReturnOnPortfolio ?? "",
                        expectedInflationPostRetirement = goal.ExpectedInflationPostRetirement ?? "",
                        existingInvestments = goal.ExistingInvestments ?? "",
                        annualContribution = goal.AnnualContribution ?? "",
                        expectedAvgReturnInvestments = goal.ExpectedAvgReturnInvestments ?? "",
                        expectedReturnOnProperty = goal.ExpectedReturnOnProperty ?? "",
                        valueOfSharesMFSForRetirement = goal.ValueOfSharesMFSForRetirement ?? "",
                        expectedAnnualIncreaseInvestment = goal.ExpectedAnnualIncreaseInvestment ?? "",
                        monthlySIPAmountTopup = goal.MonthlySIPAmountTopup ?? ""
                    };
                    break;

                case "Child1 higher education":
                    goalJson = new
                    {
                        name = goal.Name,
                        durationHigherEducation = goal.DurationHigherEducation ?? "",
                        ageTimeFundingMF = goal.AgeTimeFundingMF ?? "",
                        expectedInflation = goal.ExpectedInflation ?? "",
                        yearlyExpenseHigherEducation = goal.YearlyExpenseHigherEducation ?? "",
                        presentValueFundsEarmarked = goal.PresentValueFundsEarmarked ?? "",
                        expectedReturnFundsEarmarked = goal.ExpectedReturnFundsEarmarked ?? "",
                        expectedReturnPropertyAsset = goal.ExpectedReturnPropertyAsset ?? "",
                        expectedAnnualIncreaseInvestmentPercentage = goal.ExpectedAnnualIncreaseInvestmentPercentage ?? "",
                        monthlySIPAmountWithTopup10PercentPerAnnum = goal.MonthlySIPAmountWithTopup10PercentPerAnnum ?? "",
                        childId = goal.ChildId ?? ""
                    };
                    break;

                case "Purchase of Dream Car":
                    goalJson = new
                    {
                        name = goal.Name,
                        anticipatedInflation = goal.AnticipatedInflation ?? "",
                        cost = goal.Cost ?? "",
                        fundsEarmarked = goal.FundsEarmarked ?? "",
                        expectedReturnFundsEarmarked = goal.ExpectedReturnFundsEarmarked ?? "",
                        expectedAnnualIncreaseInvestment = goal.ExpectedAnnualIncreaseInvestment ?? "",
                        monthlySIPTopup = goal.MonthlySipTopup ?? ""
                    };
                    break;

                // Add more cases here for other goal types...

                default:
                    // Handle default case or unknown goal names
                    goalJson = new
                    {
                        name = goal.Name,
                        description = "No specific description available.",
                        value = "N/A"
                    };
                    break;
            }
            */
            // Return the JSON result
            //return Json(goalData, JsonRequestBehavior.AllowGet);
            return "";
        }

        public static string GetJsonKey(string label)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "No of Months for which Emergency Fund may be Required", "monthsRequired" },
                { "Amount availble for Emergency Fund", "availableEmergencyFund" },
                { "Anunal Amount Going Towards Creating Emergency Fund", "annualAmountForFund" }, 
                { "Available Funds meant for this goal", "availableFundsMeantForThisGoal" }, 
                { "Monthly Retirement Expenses @ Current Cost", "monthlyRetirementExpenses" }, 
                { "Existing Investments in EPF, Supper Annuation, PPF, NPS etc.", "ExistingInvestmentsEPFSupperAnnuationPPFNPS" }, //
                { "Annual Contribution", "expectedAnnualIncrease" }, 
                { "Expected Average Return in EPF, Supper Annuation, PPF, NPS etc.", "expectedReturnOnPortfolio" }, 
                { "Value of Shares and MFs meant for retirement", "valueOfSharesMFSForRetirement" }, 
                { "Any other Investments / Assets meant for this goal", "existingInvestments" }, 
                { "Children Name", "name" }, 
                { "Duration of Higher Education", "durationHigherEducation" }, 
                { "Age of  at the start Higher Education", "ageOfAtTheStartHigherEducation" }, 
                { "Yearly Expense for Higher Education @ Current Cost", "yearlyExpenseHigherEducation" }, 
                { "Present Value of Funds Earmarked for this Goal", "presentValueFundsEarmarked" }, 
                { "Expected Return on Funds Earmarked for this Goal", "expectedReturnFundsEarmarked" }, 
                { "Cost of Dream Car @ Current Cost", "cost" }, 
                { "Year of World Travel", "YearofWorldTravel" }, //
                { "Repeat After Every(Years)", "repeatYears" }, 
                { "Cost of World Tour  @ Current Cost", "cost" }, 
                { "Child's Name", "childName" }, 
                { "Expected Age for Marriage", "expectedAgeForMarriage" }, 
                { "Period left for Marriage", "periodleftforMarriage" }, 
                { "Peiod left for Marriage", "periodleftforMarriage" }, 
                { "Expense for Marriage @ Current Cost", "expenseForMarriageAtCurrentCost" }, 
                { "Cost of Dream Home @ Current Cost", "seedCost" }, 
                { "Cost of Seed Capital @ Current Cost", "cost" }, 
                { "Cost of Charity @ Current Cost", "cost" }, 
                { "Cost of Custom Goal @ Current Cost", "cost" }

                // Add more mappings here
            };

            return map.TryGetValue(label, out var key) ? key : ToCamelCase(label); // fallback
        }
        public static string ToCamelCase(string label)
        {        // Example: "Age of at the start Higher Education" → "ageOfAtTheStartHigherEducation"
            if (string.IsNullOrWhiteSpace(label))
                return label;

            var words = label.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower());

            var camel = string.Concat(words);
            return char.ToLower(camel[0]) + camel.Substring(1);
        }

         


    }


    //public static class JsonResultExtensions
    //{
    //    public static string ToJsonString(this object obj)
    //    {
    //        var options = new JsonSerializerOptions { WriteIndented = true };
    //        return JsonSerializer.Serialize(obj, options);
    //    }
    //}
    // need to add in dbcontext
    //public DbSet<AlertnessJsonResult> AlertnessJsonResult { get; set; }
    //modelBuilder.Entity<AlertnessJsonResult>().HasNoKey();
    public class AlertnessJsonResult
    {
        public string? AlertnessJson { get; set; }
    }

}
