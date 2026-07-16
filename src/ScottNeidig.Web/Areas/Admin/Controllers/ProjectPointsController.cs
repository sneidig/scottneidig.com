using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScottNeidig.Web.Areas.Admin.Models;
using ScottNeidig.Web.Data.Entities;
using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Areas.Admin.Controllers;

/// <summary>
/// Points hang off a project, so the routes do too: /admin/projects/{projectId}/points.
/// The area's convention route can't express that nesting, hence attribute routes here.
/// </summary>
[Area("Admin")]
[Authorize]
[Route("admin/projects/{projectId:int}/points")]
public class ProjectPointsController : Controller
{
    private readonly IProjectPointService _points;
    private readonly IProjectService _projects;

    public ProjectPointsController(IProjectPointService points, IProjectService projects)
    {
        _points = points;
        _projects = projects;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int projectId, CancellationToken ct)
    {
        var project = await _projects.GetByIdAsync(projectId, ct);
        if (project is null)
        {
            return NotFound();
        }

        return View(await BuildListAsync(project, ct));
    }

    /// <summary>
    /// The add row posts back to the list URL. Binding is prefixed because the form fields
    /// are named NewPoint.*, and the ModelState keys have to match for the errors to render
    /// next to the inputs when we redisplay the page.
    /// </summary>
    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int projectId,
        [Bind(Prefix = nameof(ProjectPointsViewModel.NewPoint))] ProjectPointFormModel model,
        CancellationToken ct)
    {
        var project = await _projects.GetByIdAsync(projectId, ct);
        if (project is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            var invalid = await BuildListAsync(project, ct);
            invalid.NewPoint = model;
            return View(nameof(Index), invalid);
        }

        var point = ToEntity(model);
        point.ProjectId = projectId;

        await _points.CreateAsync(point, ct);

        // Redirect rather than re-render: a refresh after adding shouldn't add it twice.
        return RedirectToAction(nameof(Index), new { projectId });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Edit(int projectId, int id, CancellationToken ct)
    {
        var point = await _points.GetAsync(projectId, id, ct);
        if (point is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit point";

        return View("Form", new ProjectPointFormModel
        {
            Id = point.Id,
            ProjectId = projectId,
            Title = point.Title,
            Body = point.Body,
            SortOrder = point.SortOrder
        });
    }

    [HttpPost("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int projectId, int id, ProjectPointFormModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit point";

        // Route wins over anything posted, so a tampered form field can't retarget the update.
        model.Id = id;
        model.ProjectId = projectId;

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var point = ToEntity(model);
        point.Id = id;
        point.ProjectId = projectId;

        return await _points.UpdateAsync(point, ct)
            ? RedirectToAction(nameof(Index), new { projectId })
            : NotFound();
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int projectId, int id, CancellationToken ct) =>
        await _points.DeleteAsync(projectId, id, ct)
            ? RedirectToAction(nameof(Index), new { projectId })
            : NotFound();

    private static ProjectPoint ToEntity(ProjectPointFormModel model) => new()
    {
        Title = model.Title.Trim(),
        // The column is non-nullable; the form field is optional. Empty, not null.
        Body = model.Body?.Trim() ?? "",
        SortOrder = model.SortOrder
    };

    private async Task<ProjectPointsViewModel> BuildListAsync(Project project, CancellationToken ct)
    {
        ViewData["Title"] = $"Points — {project.Title}";

        return new ProjectPointsViewModel
        {
            ProjectId = project.Id,
            ProjectTitle = project.Title,
            Points = await _points.GetForProjectAsync(project.Id, ct),
            NewPoint = new ProjectPointFormModel
            {
                ProjectId = project.Id,
                SortOrder = await _points.GetNextSortOrderAsync(project.Id, ct)
            }
        };
    }
}
