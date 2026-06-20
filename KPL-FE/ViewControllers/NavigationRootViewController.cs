using KPL_FE.Models;
using System;
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
    }

    public void ReleaseIgnore() => _ignore = false;
}
