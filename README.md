# Page Stack Navigation for Blazor

[![NuGet](https://img.shields.io/nuget/v/Avenged.Radzen.PageStack.svg)](https://www.nuget.org/packages/Avenged.Radzen.PageStack/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Page Stack Navigation** provides a simple way to stack views on top of each other in Blazor applications using Radzen. It's ideal for scenarios where you want to open components like modals or overlay pages while maintaining navigation state, without relying on the default Blazor routing.

---

## 🚀 Installation

Install via NuGet:

```bash
dotnet add package Avenged.Radzen.PageStack
```

Or via the NuGet Package Manager in Visual Studio.

---

## ✨ Features

- Stack-based navigation for Blazor using Radzen components.
- Easy-to-use API: `PushAsync<T>()` and `Pop()`.
- Maintains state across stacked pages.
- Seamless integration with existing Radzen layouts.
- Works well with modal-style or layered navigation scenarios.

---

## 📦 Package Info

- **Package ID**: `Avenged.Radzen.PageStack`
- **Version**: `1.0.0`
- **License**: MIT
- **Authors**: Avenged
- **Tags**: `blazor`, `components`, `view-stack`, `razor-components`, `navigation`, `page-stack`, `ui`, `modal`, `layered-navigation`, `openmode`, `blazor-library`, `dotnet`, `.net`
- **Repository**: [GitHub](https://github.com/Avenged/Avenged.Radzen.PageStack)

---

## 🛠️ Usage Example

### `MainLayout.razor`

```razor
@inherits LayoutComponentBase

<HeadContent>
    <RadzenTheme Theme="material" />
</HeadContent>

<RadzenLayout>
    <RadzenSidebar>
        <RadzenPanelMenu>
            <RadzenPanelMenuItem Text="Home" Path="/" Icon="home" />
        </RadzenPanelMenu>
    </RadzenSidebar>
    
    <Avenged.Radzen.PageStack.PageStack>
        <div class="rz-p-4">
            @Body
        </div>
    </Avenged.Radzen.PageStack.PageStack>
</RadzenLayout>
```

---

### `Home.razor`

```razor
@page "/"

<PageTitle>Home</PageTitle>

<h1>Hello, world!</h1>

Welcome to your new app.

<RadzenButton Text="Open page" Click="@(() => PageStack.PushAsync<Component>())" />

@code {
    [CascadingParameter]
    public PageStack PageStack { get; set; } = null!;
}
```

---

### `Component.razor`

```razor
<h3>Component</h3>

<RadzenButton Text="Close page" Click="@(() => PageStack.Pop())" />

@code {
    [CascadingParameter]
    public PageStack PageStack { get; set; } = null!;
}
```

---

## 📄 License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).

---

## 💬 Feedback & Contributions

Feel free to open issues or submit pull requests on [GitHub](https://github.com/Avenged/Avenged.Radzen.PageStack).
