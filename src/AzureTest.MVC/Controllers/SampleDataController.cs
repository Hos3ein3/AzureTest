using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Allow anyone to hit this for testing purposes
public class SampleDataController : ControllerBase
{
    [HttpGet("revenue")]
    public IActionResult GetRevenueData()
    {
        // Generates random revenue data for the last 6 months
        var random = new Random();
        var data = new List<object>();
        
        for (int i = 5; i >= 0; i--)
        {
            var monthName = DateTime.Now.AddMonths(-i).ToString("MMM yyyy");
            data.Add(new
            {
                title = monthName,
                value = random.Next(10000, 50000) // Random revenue between 10k and 50k
            });
        }
        
        return Ok(data);
    }
    
    [HttpGet("users")]
    public IActionResult GetUserGrowthData()
    {
        var random = new Random();
        var data = new List<object>
        {
            new { name = "Monday", amount = random.Next(10, 50) },
            new { name = "Tuesday", amount = random.Next(20, 80) },
            new { name = "Wednesday", amount = random.Next(50, 150) },
            new { name = "Thursday", amount = random.Next(30, 90) },
            new { name = "Friday", amount = random.Next(80, 200) },
            new { name = "Saturday", amount = random.Next(150, 300) },
            new { name = "Sunday", amount = random.Next(100, 250) }
        };
        
        return Ok(data);
    }
    
    [HttpGet("server-load")]
    public IActionResult GetServerLoad()
    {
        var random = new Random();
        var data = new List<object>();
        for (int i = 0; i < 24; i++)
        {
            data.Add(new { label = $"{i}:00", value = random.Next(20, 95) });
        }
        return Ok(data);
    }

    [HttpGet("support-tickets")]
    public IActionResult GetSupportTickets()
    {
        var random = new Random();
        return Ok(new List<object>
        {
            new { label = "Open", value = random.Next(10, 50) },
            new { label = "In Progress", value = random.Next(5, 30) },
            new { label = "Resolved", value = random.Next(100, 300) },
            new { label = "Closed", value = random.Next(500, 1000) }
        });
    }

    [HttpGet("traffic-sources")]
    public IActionResult GetTrafficSources()
    {
        var random = new Random();
        return Ok(new List<object>
        {
            new { label = "Direct", value = random.Next(200, 500) },
            new { label = "Organic Search", value = random.Next(400, 800) },
            new { label = "Social", value = random.Next(100, 300) },
            new { label = "Referral", value = random.Next(50, 150) }
        });
    }

    [HttpGet("conversion-rate")]
    public IActionResult GetConversionRate()
    {
        var random = new Random();
        var data = new List<object>();
        var days = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        foreach (var day in days)
        {
            data.Add(new { label = day, value = Math.Round(random.NextDouble() * (5.5 - 1.2) + 1.2, 2) });
        }
        return Ok(data);
    }

    [HttpGet("customer-satisfaction")]
    public IActionResult GetCustomerSatisfaction()
    {
        var random = new Random();
        return Ok(new List<object>
        {
            new { label = "Very Satisfied", value = random.Next(40, 60) },
            new { label = "Satisfied", value = random.Next(20, 40) },
            new { label = "Neutral", value = random.Next(10, 20) },
            new { label = "Unsatisfied", value = random.Next(1, 10) },
            new { label = "Very Unsatisfied", value = random.Next(1, 5) }
        });
    }

    [HttpGet("monthly-expenses")]
    public IActionResult GetMonthlyExpenses()
    {
        var random = new Random();
        var data = new List<object>();
        for (int i = 5; i >= 0; i--)
        {
            var monthName = DateTime.Now.AddMonths(-i).ToString("MMM");
            data.Add(new { label = monthName, value = random.Next(5000, 20000) });
        }
        return Ok(data);
    }

    [HttpGet("product-sales")]
    public IActionResult GetProductSales()
    {
        var random = new Random();
        return Ok(new List<object>
        {
            new { label = "Basic Plan", value = random.Next(100, 300) },
            new { label = "Pro Plan", value = random.Next(200, 500) },
            new { label = "Enterprise Plan", value = random.Next(10, 50) }
        });
    }

    [HttpGet("churn-rate")]
    public IActionResult GetChurnRate()
    {
        var random = new Random();
        var data = new List<object>();
        for (int i = 1; i <= 12; i++)
        {
            data.Add(new { label = $"Month {i}", value = Math.Round(random.NextDouble() * 5.0, 1) });
        }
        return Ok(data);
    }

    [HttpGet("active-sessions")]
    public IActionResult GetActiveSessions()
    {
        var random = new Random();
        var data = new List<object>();
        for (int i = 0; i < 10; i++)
        {
            data.Add(new { label = $"{i * 10}m ago", value = random.Next(100, 1000) });
        }
        return Ok(data);
    }

    [HttpGet("bug-reports")]
    public IActionResult GetBugReports()
    {
        var random = new Random();
        return Ok(new List<object>
        {
            new { label = "Critical", value = random.Next(0, 5) },
            new { label = "High", value = random.Next(5, 15) },
            new { label = "Medium", value = random.Next(10, 30) },
            new { label = "Low", value = random.Next(20, 50) }
        });
    }
}
