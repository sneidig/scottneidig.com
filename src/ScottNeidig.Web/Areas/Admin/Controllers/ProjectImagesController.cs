using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ScottNeidig.Web.Areas.Admin.Models;
using ScottNeidig.Web.Configuration;
using ScottNeidig.Web.Data.Entities;
using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Areas.Admin.Controllers;

/// <summary>
/// Nested under a project like points: /admin/projects/{projectId}/images.
/// </summary>
[Area("Admin")]
[Authorize]
[Route("admin/projects/{projectId:int}/images")]
public class ProjectImagesController : Controller
{
    private readonly IProjectImageService _images;
    private readonly IProjectService _projects;
    private readonly ImageOptions _options;

    public ProjectImagesController(
        IProjectImageService images, IProjectService projects, IOptions<ImageOptions> options)
    {
        _images = images;
        _projects = projects;
        _options = options.Value;
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

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(
        int projectId,
        [Bind(Prefix = nameof(ProjectImagesViewModel.NewImage))] ProjectImageUploadModel model,
        CancellationToken ct)
    {
        var project = await _projects.GetByIdAsync(projectId, ct);
        if (project is null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await using var stream = model.File!.OpenReadStream();

            var result = await _images.UploadAsync(projectId, stream, model.File.Length, model.Caption, ct);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index), new { projectId });
            }

            AddUploadError(result.Failure);
        }

        var invalid = await BuildListAsync(project, ct);

        // The file input can't be repopulated: browsers won't let a server set its value,
        // for the obvious reason. Caption is kept so only the file has to be chosen again.
        invalid.NewImage = new ProjectImageUploadModel { ProjectId = projectId, Caption = model.Caption };

        return View(nameof(Index), invalid);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Edit(int projectId, int id, CancellationToken ct)
    {
        var image = await _images.GetAsync(projectId, id, ct);
        if (image is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit image";

        return View("Form", new ProjectImageFormModel
        {
            Id = image.Id,
            ProjectId = projectId,
            FileName = image.FileName,
            Caption = image.Caption,
            SortOrder = image.SortOrder,
            IsHero = image.IsHero
        });
    }

    [HttpPost("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int projectId, int id, ProjectImageFormModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit image";

        // Route wins over anything posted, so a tampered form field can't retarget the update.
        model.Id = id;
        model.ProjectId = projectId;

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var updated = await _images.UpdateAsync(new ProjectImage
        {
            Id = id,
            ProjectId = projectId,
            Caption = model.Caption.Trim(),
            SortOrder = model.SortOrder,
            IsHero = model.IsHero
        }, ct);

        return updated
            ? RedirectToAction(nameof(Index), new { projectId })
            : NotFound();
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int projectId, int id, CancellationToken ct) =>
        await _images.DeleteAsync(projectId, id, ct)
            ? RedirectToAction(nameof(Index), new { projectId })
            : NotFound();

    private void AddUploadError(UploadFailure failure)
    {
        var field = $"{nameof(ProjectImagesViewModel.NewImage)}.{nameof(ProjectImageUploadModel.File)}";

        ModelState.AddModelError(field, failure switch
        {
            UploadFailure.Empty => "That file is empty.",
            UploadFailure.TooLarge => $"That file is over the {_options.MaxUploadBytes / 1024 / 1024}MB limit.",
            UploadFailure.NotAnImage => "That file isn't an image the server can read. JPG, PNG or WEBP.",
            _ => "The upload failed."
        });
    }

    private async Task<ProjectImagesViewModel> BuildListAsync(Project project, CancellationToken ct)
    {
        ViewData["Title"] = $"Images — {project.Title}";

        return new ProjectImagesViewModel
        {
            ProjectId = project.Id,
            ProjectTitle = project.Title,
            Images = await _images.GetForProjectAsync(project.Id, ct),
            NewImage = new ProjectImageUploadModel { ProjectId = project.Id }
        };
    }
}
