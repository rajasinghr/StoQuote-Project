﻿using IEXTrading.DataAccess;
using IEXTrading.Infrastructure.IEXTradingHandler;
using IEXTrading.Models;
using IEXTrading.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MVCTemplate.Controllers
{
    public class HomeController : Controller
    {
        public ApplicationDbContext dbContext;

        public HomeController(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public IActionResult Index()
        {

            HashSet<string> profileList = new HashSet<string>(dbContext.ProfileStocks.Select((e => e.profileName)).ToArray());
            ViewData["profiles"] = profileList;
            HashSet<string> symbolList = new HashSet<string>(dbContext.ProfileStocks.Where(e => e.profileName.Equals(profileList.First())).Select((e => e.symbol)).ToArray());
            List<Company> companyList = new List<Company>();
            Company company = null;
            foreach (string symbol in symbolList)
            {
                company = new Company();
                company.symbol = symbol;
                companyList.Add(company);
            }
            List<Quote> quotes = GetQuotes(companyList);

            //List<Quote> quotes = GetQuotes()
            return View(quotes);
        }

        public JsonResult GetProfileQuotes(string profileName)
        {
            HashSet<string> profileList = new HashSet<string>(dbContext.ProfileStocks.Select((e => e.profileName)).ToArray());
            ViewData["profiles"] = profileList;
            List<ProfileStock> pr = dbContext.ProfileStocks.Where(e => e.profileName.Equals(profileName)).ToList();

            HashSet<string> symbolList = new HashSet<string>(dbContext.ProfileStocks.Where(e => e.profileName.Equals(profileName)).Select((e => e.symbol)).ToArray());
            //ViewData["profiles"] = profileList;
            List<Company> companyList = new List<Company>();
            Company company = null;
            foreach (string symbol in symbolList)
            {
                company = new Company();
                company.symbol = symbol;
                companyList.Add(company);
            }
            List<Quote> quotes = GetQuotes(companyList);
            return Json(quotes);
        }

        public List<string> GetProfiles()
        {
            return new HashSet<string>(dbContext.ProfileStocks.Select((e => e.profileName)).ToArray()).ToList();
        }

        /**
         * The Symbols action calls the GetSymbols method that returns a list of Companies.
         * This list of Companies is passed to the Symbols View.
        **/


        public IActionResult Symbols()
        {
            HashSet<string> profileList = new HashSet<string>(dbContext.ProfileStocks.Select((e => e.profileName)).ToArray());
            ViewData["profiles"] = profileList;
            //Set ViewBag variable first
            ViewBag.dbSucessComp = 0;
            IEXHandler webHandler = new IEXHandler();
            List<Company> companies = webHandler.GetSymbols().Take(1000).ToList();
            ViewData["profiles"] = ViewData["profiles"];
            //foreach (Company company in companies)
            //{
            //    //Database will give PK constraint violation error when trying to insert record with existing PK.
            //    //So add company only if it doesnt exist, check existence using symbol (PK)
            //    if (dbContext.Companies.Where(c => c.symbol.Equals(company.symbol)).Count() == 0)
            //    {
            //        dbContext.Companies.Add(company);
            //    }
            //}
            //dbContext.SaveChanges();

            //Save comapnies in TempData
            //TempData["Companies"] = JsonConvert.SerializeObject(companies.Take(5));
            //List<CompanyStrategyValue> quotes = webHandler.GetQuotes(companies);

            return View(companies);
            //return PartialView("_symbolDropdown",companies);
        }

        public IActionResult Quotes()
        {
            //Set ViewBag variable first
            ViewBag.dbSucessComp = 0;
            IEXHandler webHandler = new IEXHandler();
            List<Quote> quotes = webHandler.GetQuotes(webHandler.GetSymbols()).Take(5).ToList();

            //Save comapnies in TempData
            TempData["Quotes"] = JsonConvert.SerializeObject(quotes.Take(5));
            //List<CompanyStrategyValue> quotes = webHandler.GetQuotes(companies);

            return View(quotes);
        }




        /**
         * The Chart action calls the GetChart method that returns 1 year's equities for the passed symbol.
         * A ViewModel CompaniesEquities containing the list of companies, prices, volumes, avg price and volume.
         * This ViewModel is passed to the Chart view.
        **/
        public IActionResult Chart(string symbol)
        {
            //Set ViewBag variable first
            ViewBag.dbSuccessChart = 0;
            List<Equity> equities = new List<Equity>();
            if (symbol != null)
            {
                IEXHandler webHandler = new IEXHandler();
                equities = webHandler.GetChart(symbol);
                equities = equities.OrderBy(c => c.date).ToList(); //Make sure the data is in ascending order of date.
            }

            CompaniesEquities companiesEquities = getCompaniesEquitiesModel(equities);

            return View(companiesEquities);
        }

        /**
         * The Refresh action calls the ClearTables method to delete records from a or all tables.
         * Count of current records for each table is passed to the Refresh View.
        **/
        public IActionResult Refresh(string tableToDel)
        {
            ClearTables(tableToDel);
            Dictionary<string, int> tableCount = new Dictionary<string, int>();
            tableCount.Add("Companies", dbContext.Companies.Count());
            tableCount.Add("Charts", dbContext.Equities.Count());
            tableCount.Add("Quotes", dbContext.Equities.Count());
            return View(tableCount);
        }

        /**
         * Saves the Symbols in database.
        **/
        public IActionResult PopulateSymbols()
        {
            List<Company> companies = JsonConvert.DeserializeObject<List<Company>>(TempData["Companies"].ToString());
            foreach (Company company in companies)
            {
                //Database will give PK constraint violation error when trying to insert record with existing PK.
                //So add company only if it doesnt exist, check existence using symbol (PK)
                if (dbContext.Companies.Where(c => c.symbol.Equals(company.symbol)).Count() == 0)
                {
                    dbContext.Companies.Add(company);
                }
            }
            dbContext.SaveChanges();
            ViewBag.dbSuccessComp = 1;
            return View("Symbols", companies);
        }

        public IActionResult PopulateQuotes()
        {
            List<Quote> quotes = JsonConvert.DeserializeObject<List<Quote>>(TempData["Quotes"].ToString());
            foreach (Quote quote in quotes)
            {
                //Database will give PK constraint violation error when trying to insert record with existing PK.
                //So add company only if it doesnt exist, check existence using symbol (PK)
                if (dbContext.Quotes.Where(c => c.symbol.Equals(quote.symbol)).Count() == 0)
                {
                    dbContext.Quotes.Add(quote);
                }
            }
            dbContext.SaveChanges();
            ViewBag.dbSuccessComp = 1;
            return View("Quotes", quotes);
        }


        public IActionResult ShowTop5Stock()
        {
            IEXHandler webHandler = new IEXHandler();
            List<Company> companies = webHandler.GetSymbols();

            //TempData["Companies"] = JsonConvert.SerializeObject(companies);
            List<StockWithValue> quotes = webHandler.GetTop5Picks(companies);
            TempData["Quotes"] = JsonConvert.SerializeObject(quotes.Take(5));
            return View(quotes);
        }

        public IActionResult About()
        {
            return View();
        }

        /**
         * Saves the equities in database.
        **/
        public IActionResult SaveCharts(string symbol)
        {
            IEXHandler webHandler = new IEXHandler();
            List<Equity> equities = webHandler.GetChart(symbol);
            //List<Equity> equities = JsonConvert.DeserializeObject<List<Equity>>(TempData["Equities"].ToString());
            foreach (Equity equity in equities)
            {
                if (dbContext.Equities.Where(c => c.date.Equals(equity.date)).Count() == 0)
                {
                    dbContext.Equities.Add(equity);
                }
            }

            dbContext.SaveChanges();
            ViewBag.dbSuccessChart = 1;

            CompaniesEquities companiesEquities = getCompaniesEquitiesModel(equities);

            return View("Chart", companiesEquities);
        }

        /**
         * Deletes the records from tables.
        **/
        public void ClearTables(string tableToDel)
        {
            if ("all".Equals(tableToDel))
            {
                //First remove equities and then the companies
                dbContext.Equities.RemoveRange(dbContext.Equities);
                dbContext.Companies.RemoveRange(dbContext.Companies);
            }
            else if ("Companies".Equals(tableToDel))
            {
                //Remove only those that don't have Equity stored in the Equitites table
                dbContext.Companies.RemoveRange(dbContext.Companies
                                                         .Where(c => c.Equities.Count == 0)
                                                                      );
            }
            else if ("Charts".Equals(tableToDel))
            {
                dbContext.Equities.RemoveRange(dbContext.Equities);
            }
            dbContext.SaveChanges();
        }

        /**
         * Returns the ViewModel CompaniesEquities based on the data provided.
         **/
        public CompaniesEquities getCompaniesEquitiesModel(List<Equity> equities)
        {
            List<Company> companies = dbContext.Companies.ToList();

            if (equities.Count == 0)
            {
                return new CompaniesEquities(companies, null, "", "", "", 0, 0);
            }

            Equity current = equities.Last();
            string dates = string.Join(",", equities.Select(e => e.date));
            string prices = string.Join(",", equities.Select(e => e.high));
            string volumes = string.Join(",", equities.Select(e => e.volume / 1000000)); //Divide vol by million
            float avgprice = equities.Average(e => e.high);
            double avgvol = equities.Average(e => e.volume) / 1000000; //Divide volume by million
            return new CompaniesEquities(companies, equities.Last(), dates, prices, volumes, avgprice, avgvol);
        }
    }
}