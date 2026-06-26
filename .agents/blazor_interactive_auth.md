# Blazor Interactive Routing & Authentication State Restoration

This document describes the design patterns and solutions implemented to resolve dynamic role-based navigation rendering and session recovery race conditions.

## 1. Enabling Interactive Routing & Layouts

By default, .NET 8/9 Blazor Web Apps use Static SSR for layouts and routing, even if individual pages declare interactive render modes. This causes components in the layout (like `NavMenu.razor`) to remain static and fail to re-render when the user's login state changes.

### Solution
Set the render mode globally on the `<Routes />` component in `App.razor` with pre-rendering disabled:
```html
<Routes @rendermode="@(new InteractiveServerRenderMode(prerender: false))" />
```
*Note: Disabling pre-rendering avoids `NavigationException` errors being thrown by the `NavigationManager` when redirecting unauthenticated users during the initial server-side render phase.*

---

## 2. Preventing Session Recovery Race Conditions

When restoring a user session from browser storage (e.g. `sessionStorage`) in `MainLayout.razor`'s `OnInitializedAsync`, the asynchronous JS Interop call yields execution. This allows child page components to initialize before the user context has been restored, causing them to incorrectly redirect the user to `/login`.

### Solution
Introduce a loading state (`_isInitializing = true`) in `MainLayout.razor` and conditionally defer rendering the page content:
```razor
@if (_isInitializing)
{
    <!-- Loading spinner / skeleton -->
}
else
{
    @Body
}
```
In `OnInitializedAsync`, restore the session and set the flag to false in a `finally` block:
```csharp
private bool _isInitializing = true;

protected override async Task OnInitializedAsync()
{
    try
    {
        var json = await JS.InvokeAsync<string?>("sessionStorage.getItem", "pcUser");
        // Deserialize and set user...
    }
    catch
    {
        // Handle pre-render phase or JS unavailable
    }
    finally
    {
        _isInitializing = false;
    }
}
```
This guarantees child pages are only initialized after the user's authentication state has been fully resolved.
