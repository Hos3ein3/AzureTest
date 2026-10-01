using AzureTest.Application.Abstractions;


using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace AzureTest.MVC.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class SampleDataController : ControllerBase
    {
        [HttpGet("server-load")]
        public IActionResult GetServerLoad()
        {
            var data = new List<object>
            {
                new { title = "00:00", value = 45 },
                new { title = "04:00", value = 30 },
                new { title = "08:00", value = 75 },
                new { title = "12:00", value = 90 },
                new { title = "16:00", value = 85 },
                new { title = "20:00", value = 60 }
            };
            return Ok(data);
        }

        [HttpGet("support-tickets")]
        public IActionResult GetSupportTickets()
        {
            var data = new List<object>
            {
                new { title = "Open", value = 120 },
                new { title = "In Progress", value = 45 },
                new { title = "Closed", value = 300 }
            };
            return Ok(data);
        }

        [HttpGet("traffic-sources")]
        public IActionResult GetTrafficSources()
        {
            var data = new List<object>
            {
                new { title = "Direct", value = 35 },
                new { title = "Organic Search", value = 40 },
                new { title = "Referral", value = 15 },
                new { title = "Social", value = 10 }
            };
            return Ok(data);
        }

        [HttpGet("conversion-rate")]
        public IActionResult GetConversionRate()
        {
            var data = new List<object>
            {
                new { title = "Mon", value = 2.4 },
                new { title = "Tue", value = 3.1 },
                new { title = "Wed", value = 2.8 },
                new { title = "Thu", value = 3.5 },
                new { title = "Fri", value = 4.2 },
                new { title = "Sat", value = 2.1 },
                new { title = "Sun", value = 1.9 }
            };
            return Ok(data);
        }

        [HttpGet("customer-satisfaction")]
        public IActionResult GetCustomerSatisfaction()
        {
            var data = new List<object>
            {
                new { title = "5 Stars", value = 150 },
                new { title = "4 Stars", value = 80 },
                new { title = "3 Stars", value = 30 },
                new { title = "2 Stars", value = 15 },
                new { title = "1 Star", value = 5 }
            };
            return Ok(data);
        }

        [HttpGet("monthly-expenses")]
        public IActionResult GetMonthlyExpenses()
        {
            var data = new List<object>
            {
                new { title = "Jan", value = 12000 },
                new { title = "Feb", value = 14000 },
                new { title = "Mar", value = 11000 },
                new { title = "Apr", value = 15000 },
                new { title = "May", value = 18000 }
            };
            return Ok(data);
        }

        [HttpGet("product-sales")]
        public IActionResult GetProductSales()
        {
            var data = new List<object>
            {
                new { title = "Product A", value = 450 },
                new { title = "Product B", value = 320 },
                new { title = "Product C", value = 610 },
                new { title = "Product D", value = 280 }
            };
            return Ok(data);
        }

        [HttpGet("churn-rate")]
        public IActionResult GetChurnRate()
        {
            var data = new List<object>
            {
                new { title = "Q1", value = 5.2 },
                new { title = "Q2", value = 4.8 },
                new { title = "Q3", value = 3.5 },
                new { title = "Q4", value = 3.1 }
            };
            return Ok(data);
        }

        [HttpGet("active-sessions")]
        public IActionResult GetActiveSessions()
        {
            var data = new List<object>
            {
                new { title = "Morning", value = 1200 },
                new { title = "Afternoon", value = 2500 },
                new { title = "Evening", value = 3100 },
                new { title = "Night", value = 800 }
            };
            return Ok(data);
        }

        [HttpGet("bug-reports")]
        public IActionResult GetBugReports()
        {
            var data = new List<object>
            {
                new { title = "UI/UX", value = 45 },
                new { title = "Performance", value = 12 },
                new { title = "Security", value = 3 },
                new { title = "Functionality", value = 28 },
                new { title = "Other", value = 8 }
            };
            return Ok(data);
        }
    }
}
