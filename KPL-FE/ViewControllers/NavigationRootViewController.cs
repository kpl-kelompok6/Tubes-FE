using KPL_FE.Models;
using ModernWpf.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KPL_FE.ViewControllers;

// MVVC: view calls into controller; view still owns WPF controls.
public sealed class NavigationRootViewController
{
    private bool _ignore;
    private readonly List<PageItem> _pages;

    public NavigationRootViewController(IEnumerable<PageItem> pages)
    {
        _pages = pages?.ToList() ?? throw new ArgumentNullException(nameof(pages));
    }

    public IReadOnlyList<PageItem> Pages => _pages;

    public Type? SelectedPageType { get; private set; }

    public Type? OnPagesListSelectionChanged(object? selectedValue, out bool shouldNavigate)
    {
        if (_ignore)
        {
            shouldNavigate = false;
            return null;
        }

        if (selectedValue is Type t)
        {
            SelectedPageType = t;
            shouldNavigate = true;
            return t;
        }

        shouldNavigate = false;
        return null;
    }

    public void OnRootFrameNavigated(object? content)
    {
        _ignore = true;
        SelectedPageType = content?.GetType();
        _ignore = false;
    }

    public IEnumerable? OnSearchTextChanged(string? rawText, AutoSuggestionBoxTextChangeReason reason)
    {
        if (reason != AutoSuggestionBoxTextChangeReason.UserInput) return null;

        var q = (rawText ?? "").Trim();
        if (q.Length == 0) return null;

        var tokens = q.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        var matches = _pages
            .Where(p => tokens.All(t => p.Title.IndexOf(t, StringComparison.CurrentCultureIgnoreCase) >= 0))
            .OrderByDescending(p => p.Title.StartsWith(q, StringComparison.CurrentCultureIgnoreCase))
            .ThenBy(p => p.Title)
            .ToList();

        return matches.Count > 0 ? matches : new[] { "No results found" };
    }

    public Type? OnSearchQuerySubmitted(object? chosenSuggestion, string? queryText)
    {
        if (chosenSuggestion is PageItem p)
        {
            SelectedPageType = p.PageType;
            return p.PageType;
        }

        if (!string.IsNullOrWhiteSpace(queryText))
        {
            var exact = _pages.FirstOrDefault(x => x.Title.Equals(queryText, StringComparison.OrdinalIgnoreCase));
            if (exact is not null)
            {
                SelectedPageType = exact.PageType;
                return exact.PageType;
            }
        }

        return null;
    }
}
