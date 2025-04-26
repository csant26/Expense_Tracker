using Expense_Tracker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Expense_Tracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ExpenseTrackerDbContext context;

        public DashboardController(ExpenseTrackerDbContext context)
        {
            this.context = context;
        }
        public async Task<IActionResult> Index()
        {

            //Date
            var StartDate = DateTime.Now.AddDays(-6);
            var EndDate = DateTime.Now;

            var filteredTransactions = await context.Transactions
                .Include(x => x.Category)
                .Where(d => d.TransactionDate <= EndDate && d.TransactionDate >= StartDate)
                .ToListAsync();

            //Income
            var totalIncome = filteredTransactions
                .Where(x => x.Category.Type == "Income")
                .Sum(x => x.Amount);
            ViewBag.TotalIncome = totalIncome.ToString("C0");
            
            //Expense
            var totalExpense = filteredTransactions
                .Where(x => x.Category.Type == "Expense")
                .Sum(x => x.Amount);
            ViewBag.TotalExpense = totalExpense.ToString("C0");


            //Balance
            var balance = totalIncome - totalExpense;
            CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("en-US");
            cultureInfo.NumberFormat.CurrencyNegativePattern = 1;
            ViewBag.TotalBalance = String.Format(cultureInfo, "{0:C0}", balance);


            //Doughtnut Chart Data
            ViewBag.DoughnutChartData = filteredTransactions
                .Where(x => x.Category.Type == "Expense")
                .GroupBy(c => c.CategoryId)
                .Select(a => new
                {
                    title = a.First().Category.TitleWIcon,
                    amount = a.Sum(j => j.Amount),
                    formattedAmount = a.Sum(j => j.Amount).ToString("C0")
                })
                .OrderByDescending(x=>x.amount)
                .ToList();


            //Spline Chart Data - Income vs Expense

            //Income
            List<SplineChartData> incomeSummary = filteredTransactions
                .Where(x => x.Category.Type == "Income")
                .GroupBy(d => d.TransactionDate)
                .Select(a => new SplineChartData
                {
                    day = a.First().TransactionDate.ToString("dd-MMM"),
                    income = a.Sum(i => i.Amount)
                })
                .ToList();    

            //Expense
            List<SplineChartData> expenseSummary = filteredTransactions
                .Where(x => x.Category.Type == "Expense")
                .GroupBy(d => d.TransactionDate)
                .Select(a => new SplineChartData
                {
                    day = a.First().TransactionDate.ToString("dd-MMM"),
                    expense = a.Sum(i => i.Amount)
                })
                .ToList();

            //Combine income and expense
            string[] last7days = Enumerable.Range(0, 7)
                .Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();

            ViewBag.SplineChartData = from day in last7days

                                      join income in incomeSummary on day equals income.day into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()

                                      join expense in expenseSummary on day equals expense.day into dayExpenseJoined
                                      from expense in dayExpenseJoined.DefaultIfEmpty()

                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income,
                                          expense = expense == null ? 0 : expense.expense
                                      };

            //Recent Transactions
            ViewBag.RecentTransactions = context.Transactions
                .OrderByDescending(x=>x.TransactionDate)
                .Take(5)
                .ToList();


            return View();
        }
    }
    public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;
    }
}