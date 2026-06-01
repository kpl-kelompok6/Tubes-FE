using System;

namespace KPL_FE.Controllers;

// MVVC: controllers own app actions. Views call viewcontrollers; viewcontrollers can call controllers.
public sealed class NavigationController
{
    public Type? ResolveNavigationTarget(object? selectedValue)
    {
        return selectedValue as Type;
    }
}
