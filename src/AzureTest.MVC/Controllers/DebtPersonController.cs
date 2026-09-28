using System;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
public class DebtPersonController : Controller
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public DebtPersonController(INoSqlUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var persons = await _unitOfWork.DebtPersons.GetAllAsync();
        var userPersons = persons.Where(p => p.UserId == _currentUser.Id).ToList();
        return View(userPersons);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DebtPerson person)
    {
        if (!ModelState.IsValid)
            return View(person);

        person.UserId = _currentUser.Id;
        await _unitOfWork.DebtPersons.AddAsync(person);
        await _unitOfWork.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var person = await _unitOfWork.DebtPersons.GetByIdAsync(id);
        if (person == null || person.UserId != _currentUser.Id)
            return NotFound();

        return View(person);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, DebtPerson model)
    {
        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var existingPerson = await _unitOfWork.DebtPersons.GetByIdAsync(id);
        if (existingPerson == null || existingPerson.UserId != _currentUser.Id)
            return NotFound();

        existingPerson.FullName = model.FullName;
        await _unitOfWork.DebtPersons.UpdateAsync(id, existingPerson);
        await _unitOfWork.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
