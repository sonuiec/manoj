﻿using FactFinderWeb.Models;
using FactFinderWeb.ModelsView;
using FactFinderWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace FactFinderWeb.Controllers
{
	public class WealthController : Controller
    {
        private readonly ResellerBoyinawebFactFinderWebContext _context;
        private readonly AwarenessServices _AwarenessServices;
        private readonly WingsServices _WingsServices;
        private readonly KnowledgeThatMattersServices _KnowledgeThatMattersServices;
        private readonly long _userID;
        private readonly HttpContext _httpContext;
        private readonly ExecutionServices _executionServices;
        private readonly InvestServices _investServices;
        int updateRows = 0;

        public WealthController(ResellerBoyinawebFactFinderWebContext context, AwarenessServices awarenessServices, IHttpContextAccessor httpContextAccessor, WingsServices wingsServices, KnowledgeThatMattersServices knowledgeThatMattersServices, ExecutionServices executionServices, InvestServices investServices)
        {
            _context = context;
            _AwarenessServices = awarenessServices;
            _WingsServices = wingsServices;
            _KnowledgeThatMattersServices = knowledgeThatMattersServices;

            _httpContext = httpContextAccessor.HttpContext;
            var userIdStr = _httpContext.Session.GetString("UserId");
            _userID = Convert.ToInt64(userIdStr);
            _executionServices = executionServices;
            _investServices = investServices;
        }

    }
}
