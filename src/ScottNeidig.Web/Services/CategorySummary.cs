namespace ScottNeidig.Web.Services;

/// <summary>
/// A category plus the number of projects using it, projected in a single query.
/// A DTO rather than an entity, so it's safe to hand straight to a view without
/// dragging EF tracking or navigation properties along.
/// </summary>
/// <param name="ProjectCount">
/// Shown before deleting, so it's clear how much work is about to be unfiled.
/// </param>
public record CategorySummary(int Id, string Name, string Slug, int SortOrder, int ProjectCount);
