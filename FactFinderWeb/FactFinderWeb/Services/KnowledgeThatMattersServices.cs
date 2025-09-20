using FactFinderWeb.IServices;
using FactFinderWeb.Models;
using FactFinderWeb.ModelsView;
using FactFinderWeb.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;

namespace FactFinderWeb.Services
{
    public class KnowledgeThatMattersServices
    {
        private ResellerBoyinawebFactFinderWebContext _context;
        private readonly long _userID;
        private readonly HttpContext _httpContext;

        public KnowledgeThatMattersServices(ResellerBoyinawebFactFinderWebContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContext = httpContextAccessor.HttpContext;
            var userIdStr = _httpContext.Session.GetString("UserId");
            _userID = Convert.ToInt64(userIdStr);
        }

        public async Task<MVKnowledgeRisk> KnowledgeThatMattersView()
        {
            var mvKnowledgeRisk = await _context.TblffKnowledgeRisks
                .Where(x => x.Profileid == _userID)
                .Select(x => new MVKnowledgeRisk
                {
                    Id = x.Id,
                    Profileid = x.Profileid,
                    RiskCapacity = x.RiskCapacity,
                    RiskRequirement = x.RiskRequirement,
                    RiskTolerance = x.RiskTolerance,
                    CreateDate = x.CreateDate,
                    UpdateDate = x.UpdateDate
                })
                .FirstOrDefaultAsync(); 

            return mvKnowledgeRisk;
        }

        public async Task<Int64> KnowledgeThatMattersAddUpdate(MVKnowledgeRisk mvKnowledge)
        {
            int AddorUpdate = 0;
            TblffKnowledgeRisk KnowledgeRisk = await _context.TblffKnowledgeRisks.FirstOrDefaultAsync(x => x.Profileid == _userID);
            if (KnowledgeRisk == null)
            {
                AddorUpdate = 1;
                TblffKnowledgeRisk TblffKnowledgeRiskvar = new TblffKnowledgeRisk();
                KnowledgeRisk = TblffKnowledgeRiskvar;
            }

            KnowledgeRisk.RiskCapacity = mvKnowledge.RiskCapacity;
            KnowledgeRisk.RiskRequirement = mvKnowledge.RiskRequirement;
            KnowledgeRisk.RiskTolerance = mvKnowledge.RiskTolerance;
            KnowledgeRisk.Profileid = _userID;
 
            if (AddorUpdate == 1)
            {
                KnowledgeRisk.CreateDate = DateTime.Now;
                KnowledgeRisk.UpdateDate = DateTime.Now;
                _context.TblffKnowledgeRisks.Add(KnowledgeRisk);
            }
            else
            {
                KnowledgeRisk.UpdateDate = DateTime.Now;
                _context.TblffKnowledgeRisks.Update(KnowledgeRisk);
            }

            return  await _context.SaveChangesAsync();
        }

        public async Task<Int64> KnowledgeThatMattersUpdate(MVKnowledgeRisk mvKnowledge)
        {

            var tblknowledge = new TblffKnowledgeRisk
            {
                Profileid = _userID,
                RiskCapacity = mvKnowledge.RiskCapacity,
                RiskRequirement = mvKnowledge.RiskRequirement,
                RiskTolerance = mvKnowledge.RiskTolerance,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
            };

            _context.TblffKnowledgeRisks.Update(tblknowledge);
            await _context.SaveChangesAsync();

            return tblknowledge.Id;
        }
    }
}
