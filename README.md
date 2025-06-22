# Agri-Energy Connect Platform - Prototype Web Application (PROG7311_POE)

## 1. Introduction

Welcome to the Agri-Energy Connect platform prototype. This web application serves as a functional model that demonstrates core interactions between farmers and Agri-Energy employees. The platform's aim is to facilitate managing agricultural products that are offered by farmers.

This prototype is built using:
- **Framework:** ASP.NET Core MVC (.NET 8)
- **Language:** C#
- **Database:** SQLite (via Entity Framework Core)
- **Authentication:** ASP.NET Core Identity
- **Primary Development Environment:** Visual Studio 2022 Community

## 2. Features Implemented

The prototype implements the following core functionalities:

**Common Features:**
- Secure user login and role-based access control.
- Data persistence in a relational database with pre-populated sample data for demonstration.

**Farmer Role Features:**
- **Product Management:**
  - Farmers can add new products to their profile and specify details like product name, category and production date.
  - Farmers can view a list of all products they have personally added.

**Employee Role Features:**
-**Farmer Management:**
  - Employees can create new farmer profiles (user accounts for farmers).
**Product Oversight:**
- Employees can view a comprehensive list of all products from all farmers.
- Employees can filter the list of all products based on:
    - Specific Farmer
    - Product Type (Category)
    - Date Range (Production Date)

## 3. Architectural Refinements (Incorporating Lecturer Feedback)

In response to feedback provided for the Part 2 submission, the application's architecture was refactored to improve separation of concerns.

- **Service Layer Abstraction:** All business logic related to user management (creating users, assigning roles, logging in/out) has been moved from the MVC controllers (`AccountController`, `EmployeeController`, `FarmerController`) into a dedicated `UserService`.
- **Cleaner Controllers:** The controllers no longer interact directly with ASP.NET Core Identity's `UserManager` or `SignInManager`. Instead, they now depend on the new `IUserService` interface, making their responsibility strictly to handle HTTP requests and responses. This change demonstrates an understanding and application of the service layer pattern as requested.

The core functionality of the application remains unchanged, but the internal code structure has been improved to align with best practices for maintainability and testability.

## 4. User Roles & Test Credentials

The system has two distinct user roles:

* **Employee:** Responsible for managing farmer accounts and overseeing product listings.
* **Farmer:** Can manage their own product offerings on the platform.

To test the prototype, you can use the following pre-seeded user accounts:

* **Employee:**
  * **Email:** `employee1@agrienergy.com`
  * **Password:** `Password.1`
* **Farmer (Seeded):**
  * **Email:** `farmer1@agrifarm.com`
  * **Password:** `Password.1`
    
*Note that employees can create additional farmer accounts through the "Employee Dashboard" after logging in. The password for these newly created farmers will be the one set by the employee during the creation process.*

## 5. Prerequisites (Development Environment Setup)

To set up and run this project on your local machine, you will need:

* **.NET SDK:** Version 8.0.300 or a compatible .NET 8 SDK. You can download it from [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0).
* **Visual Studio 2022 Community Edition:** Ensure the "ASP.NET and web development" workload is installed. You can download Visual Studio Community from [https://visualstudio.microsoft.com/vs/community/](https://visualstudio.microsoft.com/vs/community/).
* **Git:** Required for cloning the repository. Download from [https://git-scm.com/downloads](https://git-scm.com/downloads).
* **(Optional) SQLite Database Browser:** A tool like "DB Browser for SQLite" ([https://sqlitebrowser.org/](https://sqlitebrowser.org/)) can be helpful for inspecting the `Local.db` database file directly, but it's not required to run the application.

## 6. Getting Started (Setup and Running the Prototype)

Follow these steps to get the prototype running:

1.  **Clone the Repository:**
    Open a terminal or command prompt and run the following command:
    ```bash
    git clone [https://github.com/ST10355049-Knipe/PROG7311_POE.git](https://github.com/ST10355049-Knipe/PROG7311_POE.git)
    ```
    Navigate into the cloned repository's main directory:
    ```bash
    cd PROG7311_POE 
    ```
    
2.  **Open the Project in Visual Studio:**
    * Navigate to the cloned repository folder (`PROG7311_POE`).
    * Open the solution file: `PROG7311_WebApp.sln`. 

3.  **Restore NuGet Packages:**
    * Visual Studio should automatically restore NuGet packages when the solution opens. If you see any errors related to missing packages, right-click on the solution in Solution Explorer and select "Restore NuGet Packages."

4.  **Apply Database Migrations:**
    * The database (SQLite) is managed using Entity Framework Core migrations. The database file (`Local.db`) will be created, and the schema applied, along with initial seed data when you run the migrations and then the application.
    * In Visual Studio, open the **Package Manager Console** (View > Other Windows > Package Manager Console or Tools > NuGet Package Manager > Package Manager Console).
    * Ensure the "Default project" dropdown in the Package Manager Console is set to your web application project: `PROG7311_WebApp`.
    * Run the following command:
        ```powershell
        Update-Database
        ```
    * This command will apply all pending migrations and create/update the `Local.db` file.

5.  **Run the Application:**
    * In Visual Studio, ensure your web application project (`PROG7311_WebApp`) is set as the startup project.
    * Press **F5** or click the "Start Debugging" button (looks like a green play icon).
    * Your default web browser will open, and the application should navigate to its home page.

## 7. Building the Prototype

* To build the project explicitly without running, you can select **Build > Build Solution** from the Visual Studio menu.

## 8. Database Information

* **Type:** SQLite
* **Database File:** `Local.db`
* **Location:** This file is created/updated by Entity Framework Core in the project's output directory when the application is run after migrations are applied.
* **Management:** The schema is managed by EF Core Migrations. Data is seeded by the `DbInitialiser` class on application startup after migrations are confirmed.

## 9. Project Structure Overview (Key Folders)

-   `/Controllers`: Contains MVC controllers handling incoming requests and setting up responses.
-   `/Views`: Contains CSHTML Razor views for rendering the user interface, organised by controller.
-   `/Models`: Contains data models (entities like `Product`, `ApplicationUser`) and view models used for passing data to views.
-   `/Services`: Contains service classes (like `ProductService` and `UserService`) encapsulating business logic and data access operations.
-   `/Data`: Contains `AppDbContext` for database interaction via Entity Framework Core and `DbInitialiser` for data seeding.
-   `/wwwroot`: Contains static client-side files such as CSS, JavaScript, and images.
-   `/Migrations`: Contains Entity Framework Core migration files detailing database schema changes over time.

## 10. Quick Run (Self-Contained Executable)

For users who do not have the .NET SDK or Visual Studio installed and want to quickly run the prototype. **Please note this is for Windows x64 only**.

**Note:** This is for demonstration purposes only. To see the code, please follow the "Getting Started" instructions to set up the project from the source.

**To run the self-contained version:**

1.  **Download and Extract:**
    * Download the `PROG7311_WebApp_Windows_x64_SelfContained.zip` file (if provided).
    * Extract all contents of the ZIP file to a new folder on your computer (e.g., `C:\AgriEnergyPrototype`).

2.  **Run the Application:**
    * Navigate into the folder where you extracted the files.
    * Find and double-click the `PROG7311_WebApp.exe` file.

3.  **Access in Browser:**
    * A console window will appear. This window is the Kestrel web server running the application. Do not close this window while using the application.
    * Wait for a message similar to `Now listening on: http://localhost:5000` or `Now listening on: https://localhost:5001` to appear in the console window.
    * Open your preferred web browser (e.g., Chrome, Firefox, Edge).
    * In the address bar, type the URL shown in the console window.

4.  **Using the Application:**
    * You can now use the application as described in the "Features Implemented" and "User Roles & Test Credentials" sections.
    * The application will use its own `Local.db` SQLite database file located within the extracted folder.

5.  **To Stop the Application:**
    * Simply close the console window that appeared when you ran `PROG7311_WebApp.exe`.

## 11. Troubleshooting

* **Database Errors on Startup / "Table not found":** Ensure you have successfully run the `Update-Database` command in the Package Manager Console after obtaining the code.
* **NuGet Package Issues:** If you encounter build errors related to missing references, try right-clicking the solution in Visual Studio's Solution Explorer and selecting "Restore NuGet Packages."
* **Ensure Correct .NET SDK:** Verify you have the .NET 8 SDK (version 8.0.300 or compatible) installed as listed in the Prerequisites.

## 12. References

Reference List:

1.  Anderson, R. et al. (2024) *Introduction to Identity on ASP.NET Core*. Microsoft Learn [Online]. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-9.0&tabs=visual-studio (Accessed: 10 May 2025).

2.  ByteHide (2023) *Mastering LINQ in C#: A Comprehensive Guide for Beginners and Beyond*. ByteHide Blog [Online]. Available at: https://www.bytehide.com/blog/linq-csharp (Accessed: 14 May 2025).

3.  DotNetTutorials.net (n.d.) *UserManager, SignInManager and RoleManager in ASP.NET Core Identity*. DotNetTutorials.net [Online]. Available at: https://dotnettutorials.net/lesson/usermanager-signinmanager-rolemanager-in-asp-net-core-identity/ (Accessed: 10 May 2025).

4.  Microsoft (2024) *Add, download, and delete custom user data to Identity in an ASP.NET Core project*. Microsoft Learn [Online]. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/add-user-data (Accessed: 10 May 2025)

5.  Microsoft (2025) *Data Seeding*. Microsoft Learn [Online]. Available at: https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding (Accessed: 11 May 2025).

6.  Microsoft (2024) *Dependency injection in ASP.NET Core*. Microsoft Learn [Online]. Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection (Accessed: 11 May 2025).

7.  Microsoft (2023) *Loading Related Data - Eager loading*. Microsoft Learn [Online]. Available at: https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager (Accessed: 10 May 2025). 

8.  Microsoft (2023) *Manage user data in ASP.NET Core Identity*. Microsoft Learn [Online]. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/manage-user-data (Accessed: 10 May 2025).

9.  Microsoft (2024) *Model validation in ASP.NET Core MVC and Razor Pages*. Microsoft Learn [Online]. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-9.0 (Accessed: 11 May 2025).

10. Microsoft (2023) *Overview of Entity Framework Core*. Microsoft Learn [Online]. Available at: https://learn.microsoft.com/en-us/ef/core/modeling/ (Accessed: 10 May 2025). 

11. Microsoft (2021) *Querying Data*. Microsoft Learn [Online]. Available at: https://learn.microsoft.com/en-us/ef/core/querying/ (Accessed: 11 May 2025).

12. Microsoft (2023) *Relationships*. Microsoft Learn [Online]. Available at: https://learn.microsoft.com/en-us/ef/core/modeling/relationships (Accessed: 12 May 2025).

13. J, Roshan. (2023) *Users and Roles Seeding in ASP.NET Core Identity with Entity Framework Core: A Step-by-Step Guide*. Medium, 13 August [Online]. Available at: https://medium.com/@roshanj100/users-and-roles-seeding-in-asp-net-core-identity-with-entity-framework-core-a-step-by-step-guide-28e6f76a18db (Accessed: 10 May 2025).

14. *(Conceptual Guidance) Various online tutorials and video resources (e.g., YouTube channels focusing on ASP.NET Core development) were consulted for general understanding of ASP.NET Core Identity concepts*



