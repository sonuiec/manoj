using FactFinderWeb.Models;
using FactFinderWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SystemTextJson = System.Text.Json.JsonSerializer;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

//using System.Text.Json;
using stJSON= System.Text.Json;
using System.Collections.Generic;
using FactFinderWeb.ModelsView;
//using NewtonsoftSerializer = Newtonsoft.Json.JsonConvert;

namespace FactFinderWeb.Utils
{
    public class JSONDataUtility
    {

        private readonly ResellerBoyinawebFactFinderWebContext _context;
        public JSONDataUtility(ResellerBoyinawebFactFinderWebContext context) => _context = context;


        List<object> selectedFormsdata = new List<object>();
        List<object> GoalsNameList = new List<object>();

        public async Task<string> GetAwareness(long profileId)
        {
            var jsonString= string.Empty;
            var profile = _context.TblffAwarenessProfileDetails.FirstOrDefault(p => p.Profileid == profileId);

            if (profile != null)
            {
                var spouse = _context.TblffAwarenessSpouses.FirstOrDefault(s => s.Profileid == profileId);
                var children = _context.TblffAwarenessChildren.Where(c => c.Profileid == profileId).ToList();
                var Assumptions = _context.TblffAwarenessAssumptions.Where(c => c.Profileid == profileId).FirstOrDefault();

                /* var profile = await _context.TblffAwarenessProfileDetails
                //.Include(p => p.Addresses)
                .Include(ps => _context.TblffAwarenessSpouses)//(p => p.Spouse)
                .Include(pc => _context.TblffAwarenessChildren)
                .Include(pff => _context.TblffAwarenessFamilyFinancials)
                //.Include(p => p.FamilyFinancial).FirstOrDefaultAsync(p => p.Profileid == profileId);*/

                if (profile == null) return null;// NotFound();

                var output = new
                {
                    awareness = new
                    {
                        planDetails = new
                        {
                            PlanType = profile.PlanType,
                            PlanYear = profile.PlanYear,
                            PlanDuration = profile.PlanDuration
                        },
                        personalDetails = new
                        {
                            Name = profile.Name,
                            Gender = profile.Gender,
                            Dob = profile.Dob,
                            Phone = profile.Phone,
                            Altphone = profile.Altphone,
                            Aadhaar = profile.Aadhaar,
                            Email = profile.Email,
                            SecEmail = profile.SecEmail,
                            Pan = profile.Pan,
                            Occupation = profile.Occupation,
                            address = new
                            {
                                residential = new
                                {
                                    address = profile.ResAddress,
                                    country = profile.ResCountry,
                                    countryCode= "",
                                    state = profile.ResState,
                                    stateCode  = "",
                                    city = profile.ResCity,
                                    pinCode = profile.ResPincode
                                },
                                permanent = new
                                {
                                     address = profile.PermAddress,
                                     country = profile.PermCountry,
                                     countryCode = "",
                                     state = profile.PermState,
                                     stateCode = "",
                                     city = profile.PermCity,
                                     pinCode = profile.PermPincode
                                },
                                office = new
                                {
                                    company=   profile.CompanyName,
                                    address = profile.CompanyAddress,
                                    country = profile.CompanyCountry,
                                    countryCode= "",
                                    state = profile.CompanyState,
                                    stateCode = "",
                                    city = profile.CompanyCity,
                                    pinCode = profile.CompanyPincode
                                },
                                isSameAddress = profile.IsSameAddress
                            }
                        },
                        familyFinancial = new
                        {
                            stock = profile.Stock,
                            income = profile.Income,
                            payment = profile.Payment,
                            holiday = profile.Holiday,
                            shopping = profile.Shopping
                        },
                        maritalDetails = new
                        {
                            profile.MaritalStatus,
                            spouseDetails = spouse == null ? null : new
                            {
                               spouseName  = spouse.SpouseName,
                               spouseGender  = spouse.SpouseGender,
                               spouseDateOfBirth  = spouse.SpouseDob,
                               spousePhone  = spouse.SpousePhone,
                               spouseAlternatePhone  = spouse.SpouseAltPhone,
                               spouseAadhaar  = spouse.SpouseAadhaar,
                               spouseEmail  = spouse.SpouseEmail,
                               spouseSecondaryEmail  = spouse.SpouseSecEmail,
                               spousePANNumber  = spouse.SpousePan,
                               spouseOccupation  = spouse.SpouseOccupation
                            },
                            haveChildren = profile.HaveChildren,
                            childrenDetails = children.Select(pc => new
                            {
                                id= pc.Id,
                                childName = pc.ChildName,
                                childGender = pc.ChildGender,
                                childDob = pc.ChildDob,
                                childPhone = pc.ChildPhone,
                                childAadhaar = pc.ChildAadhaar,
                                childEmail = pc.ChildEmail,
                                childPan = pc.ChildPan
                            }).ToList()
                        },
                        assumptions = Assumptions == null ? null : new
                        {
                            equity = Assumptions.Equity,
                            debt = Assumptions.Debt,
                            gold = Assumptions.Gold,
                            realEstateReturn = Assumptions.RealEstateReturn,
                            liquidFunds = Assumptions.LiquidFunds,
                            inflationRates = Assumptions.InflationRates,
                            educationInflation = Assumptions.EducationInflation,
                            applicantRetirement = Assumptions.ApplicantRetirement,
                            spouseRetirement = Assumptions.SpouseRetirement,
                            applicantLifeExpectancy = Assumptions.ApplicantLifeExpectancy,
                            spouseLifeExpectancy = Assumptions.SpouseLifeExpectancy
                        }
                    }
                };

//#if DEBUG

                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                  jsonString = SystemTextJson.Serialize(output, options);
            return JsonConvert.SerializeObject(output, Formatting.Indented);
                //return Content(jsonString, "application/json");
//#else
//                return Json(output);
//#endif

            }
                return jsonString;

        }

        public async Task<string> GetAwarenessJSON(long profileId)
        {
            var result = new
            {
                awaken = new
                {
                    awareness = BuildAwarenessSection(profileId),
                    wings = BuildWingsSection(profileId),
                    alertness = BuildAlertnessSection(profileId),
                    knowledge = BuildKnowledgeSection(profileId),
                    executePlan = BuildExecutePlanSection(profileId),
                    Invest = BuildInvestSection(profileId)
                }
            };

            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        private object BuildAwarenessSection(long profileId)
        {
            var profile = _context.TblffAwarenessProfileDetails.FirstOrDefault(p => p.Profileid == profileId);
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
                    dob = profile?.Dob ?? "",
                    phone = profile?.Phone ?? "",
                    altPhone = profile?.Altphone ?? "",
                    aadhaar = profile?.Aadhaar ?? "",
                    email = profile?.Email ?? "",
                    secEmail = profile?.SecEmail ?? "",
                    pan = profile?.Pan ?? "",
                    occupation = profile?.Occupation ?? "",
                    address = new
                    {
                        residential = new
                        {
                            address = profile.ResAddress?? "",
                            country = profile.ResCountry?? "",
                            countryCode = profile.ResCountry?? "",
                            state = profile.ResState?? "",
                            stateCode = profile.ResState?? "",
                            city = profile.ResCity?? "",
                            pinCode = profile.ResPincode
                        },
                        permanent = new
                        {
                            address = profile.PermAddress?? "",
                            country = profile.PermCountry?? "",
                            countryCode = profile.PermCountry?? "",
                            state = profile.PermState?? "",
                            stateCode = profile.PermState?? "",
                            city = profile.PermCity?? "",
                            pinCode = profile.PermPincode
                        },
                        office = new
                        {
                            company = profile.CompanyName?? "",
                            address = profile.CompanyAddress?? "",
                            country = profile.CompanyCountry?? "",
                            countryCode = profile.CompanyCountry?? "",
                            state = profile.CompanyState?? "",
                            stateCode = profile.CompanyState?? "",
                            city = profile.CompanyCity?? "",
                            pinCode = profile.CompanyPincode ?? ""
                        },
                        isSameAddress = profile.IsSameAddress
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
                        spouseGender = spouse.SpouseGender ?? "",
                        spouseDob = spouse.SpouseDob ?? "",
                        spousePhone = spouse.SpousePhone ?? "",
                        spouseAltPhone = spouse.SpouseAltPhone ?? "",
                        spouseAadhaar = spouse.SpouseAadhaar ?? "",
                        spouseEmail = spouse.SpouseEmail ?? "",
                        spouseSecEmail = spouse.SpouseSecEmail ?? "",
                        spousePan = spouse.SpousePan ?? "",
                        spouseOccupation = spouse.SpouseOccupation ?? ""
                    },
                    haveChildren = children.Any() ? "yes" : "no",
                    childrenDetails = children.Select(c => new
                    {
                        id = c.Id,
                        childName = c.ChildName ?? "",
                        childGender = c.ChildGender ?? "",
                        childDob = c.ChildDob ?? "",
                        childPhone = c.ChildPhone ?? "",
                        childAadhaar = c.ChildAadhaar ?? "",
                        childEmail = c.ChildEmail ?? "",
                        childPan = c.ChildPan ?? ""
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
                    //goalEndYear = goal.GoalEndYear ?? "",  // In case GoalEndYear is null, use an empty string
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
                    forms = new[]
                    {
                        new { id = 1, goal = "Emergency Fund" },
                        new { id = 2, goal = "Retirement Accumulation" },
                        new { id = 3, goal = "World Tour" },
                        new { id = 4, goal = "Purchase of Dream Car" },
                        new { id = 5, goal = "Purchase of Dream House" },
                        new { id = 6, goal = "Seed Capital for Business" },
                        new { id = 7, goal = "Charity" }
                    },
                    selectedForms = selectedFormsdata
                };
        }

        private object BuildAlertnessSection(long profileId)
        {
            var profileIdParam = new SqlParameter("@ProfileId", profileId);

            // Call stored procedure
            var jsonResult = _context.AlertnessJsonResult
                .FromSqlRaw("EXEC Sp_AlertnessAPIJshon @ProfileId", profileIdParam)
                .AsEnumerable()
                .FirstOrDefault();

            if (jsonResult == null || string.IsNullOrEmpty(jsonResult.AlertnessJson))
                return new { }; // return empty object if SP fails

            // Parse raw JSON string to dynamic object
            var alertnessData = JsonConvert.DeserializeObject<object>(jsonResult.AlertnessJson);
            return alertnessData;
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
                formData = new
                {
                    lifeInsurance = new { incomeUsedForFamily = "", fundsReturnRate = "", inflationRate = "" },
                    spouseDetails = new { Sliabilities = "" },
                    goalNames = new { GoalsNameList },
                    goalData = groupedGoals
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

        //-----for user FrontEnd


        public async Task<string> UserGetAwarenessJSON(long profileId)
        {
            var result = new
            {
                awaken = new
                {
                    awareness = UserBuildAwarenessSection(profileId),
                    wings = UserBuildWingsSection(profileId),
                    alertness = UserBuildAlertnessSection(profileId),
                    knowledge = UserBuildKnowledgeSection(profileId),
                    ExecutePlan = UserBuildExecutePlanSection(profileId),
                    Invest = UserBuildInvestSection(profileId)
                }
            };

            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        private object UserBuildAwarenessSection(long profileId)
        {
            var profile = _context.TblffAwarenessProfileDetails.FirstOrDefault(p => p.Profileid == profileId);
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
                    dob = profile?.Dob ?? "",
                    phone = profile?.Phone ?? "",
                    altPhone = profile?.Altphone ?? "",
                    aadhaar = profile?.Aadhaar ?? "",
                    email = profile?.Email ?? "",
                    secEmail = profile?.SecEmail ?? "",
                    pan = profile?.Pan ?? "",
                    occupation = profile?.Occupation ?? "",
                    address = new
                    {
                        residential = new
                        {
                            address = profile.ResAddress ?? "",
                            country = profile.ResCountry ?? "",
                            countryCode = profile.ResCountry ?? "",
                            state = profile.ResState ?? "",
                            stateCode = profile.ResState ?? "",
                            city = profile.ResCity ?? "",
                            pinCode = profile.ResPincode
                        },
                        permanent = new
                        {
                            address = profile.PermAddress ?? "",
                            country = profile.PermCountry ?? "",
                            countryCode = profile.PermCountry ?? "",
                            state = profile.PermState ?? "",
                            stateCode = profile.PermState ?? "",
                            city = profile.PermCity ?? "",
                            pinCode = profile.PermPincode
                        },
                        office = new
                        {
                            company = profile.CompanyName ?? "",
                            address = profile.CompanyAddress ?? "",
                            country = profile.CompanyCountry ?? "",
                            countryCode = profile.CompanyCountry ?? "",
                            state = profile.CompanyState ?? "",
                            stateCode = profile.CompanyState ?? "",
                            city = profile.CompanyCity ?? "",
                            pinCode = profile.CompanyPincode ?? ""
                        },
                        isSameAddress = profile.IsSameAddress
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
                        spouseGender = spouse.SpouseGender ?? "",
                        spouseDob = spouse.SpouseDob ?? "",
                        spousePhone = spouse.SpousePhone ?? "",
                        spouseAltPhone = spouse.SpouseAltPhone ?? "",
                        spouseAadhaar = spouse.SpouseAadhaar ?? "",
                        spouseEmail = spouse.SpouseEmail ?? "",
                        spouseSecEmail = spouse.SpouseSecEmail ?? "",
                        spousePan = spouse.SpousePan ?? "",
                        spouseOccupation = spouse.SpouseOccupation ?? "",

                        SpouseCmpAddress = spouse.SpouseCompanyAddress ?? "",
                        SpouseCmpCity = spouse.SpouseCompanyCity ?? "",
                        SpouseCmpCountry = spouse.SpouseCompanyCountry ?? "",
                        SpouseCmpName = spouse.SpouseCompanyName ?? "",
                        SpouseCmpPincode = spouse.SpouseCompanyPincode ?? "",
                        SpouseCmpState = spouse.SpouseCompanyState ?? ""

                    },
                    haveChildren = children.Any() ? "yes" : "no",
                    childrenDetails = children.Select(c => new
                    {
                        id = c.Id,
                        childName = c.ChildName ?? "",
                        childGender = c.ChildGender ?? "",
                        childDob = c.ChildDob ?? "",
                        childPhone = c.ChildPhone ?? "",
                        childAadhaar = c.ChildAadhaar ?? "",
                        childEmail = c.ChildEmail ?? "",
                        childPan = c.ChildPan ?? ""
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
        private object UserBuildWingsSection(long profileId)
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
            var WingsData = _context.TblffWings.Where(p => p.Profileid == profileId).OrderBy(u => u.GoalPriority)
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
                    //goalEndYear = goal.GoalEndYear ?? "",  // In case GoalEndYear is null, use an empty string
                    timeHorizon = goal.TimeHorizon,
                    newGoals = goal.NewGoals
                };
                var goalNames = new
                {
                    goal = goal.GoalName
                };
                GoalsNameList.Add(goalNames);  // Add each goal object to the selectedForms array
                selectedFormsdata.Add(goalObj);  // Add each goal object to the selectedForms array
            }

            return new
            {
                forms = new[]
                    {
                        new { id = 1, goal = "Emergency Fund" },
                        new { id = 2, goal = "Retirement Accumulation" },
                        new { id = 3, goal = "World Tour" },
                        new { id = 4, goal = "Purchase of Dream Car" },
                        new { id = 5, goal = "Purchase of Dream House" },
                        new { id = 6, goal = "Seed Capital for Business" },
                        new { id = 7, goal = "Charity" }
                    },
                selectedForms = selectedFormsdata
            };
        }

        private object UserBuildAlertnessSection(long profileId)
        {
            var profileIdParam = new SqlParameter("@ProfileId", profileId);

            // Call stored procedure
            var jsonResult = _context.AlertnessJsonResult
                .FromSqlRaw("EXEC Sp_AlertnessAPIJshonForPDF @ProfileId", profileIdParam)
                .AsEnumerable()
                .FirstOrDefault();

            if (jsonResult == null || string.IsNullOrEmpty(jsonResult.AlertnessJson))
                return new { }; // return empty object if SP fails

            // Parse raw JSON string to dynamic object
            var alertnessData = JsonConvert.DeserializeObject<object>(jsonResult.AlertnessJson);
            return alertnessData;
        }

        private object UserBuildKnowledgeSection(long profileId)
        {
            var kData = _context.TblffKnowledgeRisks.Where(p => p.Profileid == profileId).FirstOrDefault();

            return new
            {
                knowledge = new
                {
                    riskCapacity = string.IsNullOrEmpty(kData?.RiskCapacity) ? "" : kData.RiskCapacity,
                    riskRequirement = string.IsNullOrEmpty(kData?.RiskRequirement) ? "" : kData.RiskRequirement,
                    //plannerAssessmentOnRiskProfile = string.IsNullOrEmpty(kData?.PlannerAssessmentOnRiskProfile) ? "" : kData.PlannerAssessmentOnRiskProfile,
                    riskTolerance = string.IsNullOrEmpty(kData?.RiskTolerance) ? "" : kData.RiskTolerance,
                }
            };
            //var knowledge = new Dictionary<string, object>
            //{
            //    { "Risk Capacity", string.IsNullOrEmpty(kData?.RiskCapacity) ? "" : kData.RiskCapacity },
            //    { "Risk Requirement", string.IsNullOrEmpty(kData?.RiskRequirement) ? "" : kData.RiskRequirement },
            //    // { "Total Risk Profile Score", string.IsNullOrEmpty(kData?.TotalRiskProfileScore) ? "" : kData.TotalRiskProfileScore },
            //    // { "Planner Assessment On Risk Profile", string.IsNullOrEmpty(kData?.PlannerAssessmentOnRiskProfile) ? "" : kData.PlannerAssessmentOnRiskProfile },
            //    { "Risk Tolerance", string.IsNullOrEmpty(kData?.RiskTolerance) ? "" : kData.RiskTolerance }
            //};

            //return new Dictionary<string, object>
            //{
            //    { "knowledge", knowledge }
            //};

        }

        private object UserBuildExecutePlanSection(long profileId)
        {
            var executionData = _context.TblffWingsGoalStep5ExecutionData
                            .Where(p => p.Profileid == profileId)
                            .Select(u => new { u.GoalName, u.ExecutionDescription, u.ExecutionValue })
                            .ToList();
            //.Select(u => new { u.GoalName, u.ExecutionDescription, u.ExecutionValue })

            var groupedGoals = executionData
                    .GroupBy(e => e.GoalName)
                    .Select(group =>
                    {
                        var goalDict = new Dictionary<object, object>
                        {
                            ["Goal Name"] = group.Key
                        };

                        foreach (var item in group)
                        {
                            var keyWithSpace = item.ExecutionDescription?.Trim();  // Use label directly
                            if (!string.IsNullOrWhiteSpace(keyWithSpace) && !goalDict.ContainsKey(keyWithSpace))
                            {
                                goalDict[keyWithSpace] = item.ExecutionValue ?? "";
                            }
                        }
                        return goalDict;
                    }).ToList();


            //var groupedGoals = executionData
            //    .GroupBy(e => e.GoalName)
            //    .Select(group =>
            //    { 

            //        //var goalDict = new Dictionary<string, object>
            //        //{
            //        //    ["name"] = group.Key
            //        //};

            //        //foreach (var item in group)
            //        //{
            //        //    var key = GetJsonKey(item.ExecutionDescription);
            //        //    if (!goalDict.ContainsKey(key))
            //        //        goalDict[key] = item.ExecutionValue ?? "";
            //        //}
            //        ///***key need to space types also render in html

            //        return goalDict;
            //    }).ToList();
            //return new { goalData = groupedGoals };

            return new
            {
                formData = new
                {
                    //lifeInsurance = new { incomeUsedForFamily = "", fundsReturnRate = "", inflationRate = "" },
                    //spouseDetails = new { Sliabilities = "" },
                    //goalNames = new { GoalsNameList },
                    goalData = groupedGoals
                }
            };

        }
        private object UserBuildInvestSection(long profileId)
        {
            var InvestMasterDataUser = _context.TblffInvestWingsGoalMasters
                                    .Where(p => p.Profileid == profileId)
                                    .Select(u => new { u.Id, u.IntendedSipmonthly, u.AvailableLumpsum, u.MonthlySavings })
                                    .FirstOrDefault();

            var InvestDataUser = _context.TblffInvestWingsGoals
                .Where(p => p.Profileid == profileId)
                .Select(u => new
                {
                    //id = u.Id,
                    goal = u.GoalName ?? "",
                    //goalOptions = new List<object>(),
                    //typeOfFund = "",
                    //selectedFunds = "",
                    lumpSum = u.LumpsumAmount ?? 0,
                    SIP = u.Sipamount ?? 0,
                    //selectedFundsOptions = new List<object>()
                }).ToList();

            //var GeneralInsuranceDataUser = _context.TblffInvestWingsGoals
            //    .Where(p => p.Profileid == profileId)
            //    .Select(u => new { u.Id, u.GoalName, u.LumpsumAmount, u.Sipamount })
            //    .ToList();//?? new List<object>()

            //var LifeInsuranceDataUser = _context.TblffInvestWingsGoals
            //    .Where(p => p.Profileid == profileId)
            //    .Select(u => new { u.Id, u.GoalName, u.LumpsumAmount, u.Sipamount })
            //    .ToList();//?? new List<object>()

            return new
            {
                //wings = new { expectedReturns = new { GoalsNameList }, },
                //alertness = new { familyMonthlyIncomeTotal = "", familyMonthlyExpensesTotal = "" },
                //knowledge = new { selectedRiskProfile = "" },
                //executeWithPrecision = new
                //{
                //    hofInsurance = new
                //    {
                //        homeInsurance = new { currentCoverage = "", requiredCoverage = "", shortfall = "" },
                //        carInsurance = new { currentCoverage = "", requiredCoverage = "", shortfall = "" }
                //    }
                //},
                actNow = new
                {
                    savingsForInvestments = new { intendedSipMonthly = InvestMasterDataUser?.IntendedSipmonthly ?? 0, availableLumpsum = InvestMasterDataUser?.AvailableLumpsum ?? 0, MonthlySavings = InvestMasterDataUser?.MonthlySavings },
                    //lifeInsuranceReq = LifeInsuranceDataUser, // dynamically generated
                    //generalInsuranceReq = GeneralInsuranceDataUser, // dynamically generated
                    //actionPlanFinancialGoals = new
                    //{
                    //    goals = new { GoalsNameList },// new List<object>(),
                    //    total = new { totalLumpSum = "", totalFixorSipAmt = "", totalSipAmt = "", totalGapBtnSip = "" }
                    //},
                    //goalStatusReport = new { GoalsNameList },
                    //totalGoalStatusReport = new
                    //{
                    //    totalFutureValueCurrentCorpus = "",
                    //    totalFutureValueLumpSum = "",
                    //    totalFutureValueSip = "",
                    //    totalTotalCorpus = "",
                    //    totalReqCorpus = ""
                    //},
                    //goalsOptions = new { GoalsNameList },
                    InvestGoal = InvestDataUser // List of funds
                }
            };
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
