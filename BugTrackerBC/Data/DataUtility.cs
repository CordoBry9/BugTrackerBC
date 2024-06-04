
using Bogus;
using BugTrackerBC.Data;
using BugTrackerBC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Globalization;

public static class DataUtility
{
    private static int company1Id;
    private static int company2Id;
    private static int company3Id;
    private static int company4Id;
    private static int company5Id;

    private static int portfolioId;
    private static int blogId;
    private static int bugtrackerId;
    private static int movieId;
    private static int addressbookId;

    private static Faker faker = new();


    public static string? GetConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);
    }

    public static string BuildConnectionString(string databaseUrl)
    {
        //Provides an object representation of a uniform resource identifier (URI) and easy access to the parts of the URI.
        var databaseUri = new Uri(databaseUrl);
        var userInfo = databaseUri.UserInfo.Split(':');
        //Provides a simple way to create and manage the contents of connection strings used by the NpgsqlConnection class.
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = databaseUri.Host,
            Port = databaseUri.Port,
            Username = userInfo[0],
            Password = userInfo[1],
            Database = databaseUri.LocalPath.TrimStart('/'),
            SslMode = SslMode.Prefer,
        };
        return builder.ToString();
    }

    public static async Task ManageDataAsync(IServiceProvider svcProvider)
    {
        //Service: An instance of RoleManager
        await using var dbContextSvc = svcProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext();
        //Service: An instance of RoleManager
        var roleManagerSvc = svcProvider.GetRequiredService<RoleManager<IdentityRole>>();
        //Service: An instance of the UserManager
        var userManagerSvc = svcProvider.GetRequiredService<UserManager<ApplicationUser>>();
        //Migration: This is the programmatic equivalent to Update-Database
        await dbContextSvc.Database.MigrateAsync();

        await SeedRolesAsync(roleManagerSvc);
        await SeedDefaultCompaniesAsync(dbContextSvc);
        await SeedDefaultUsersAsync(userManagerSvc);
        await SeedDemoUsersAsync(userManagerSvc);
        await SeedDefaultProjectsAsync(dbContextSvc);
        await SeedDefaultTicketsAsync(dbContextSvc, userManagerSvc);

        await dbContextSvc.DisposeAsync();
    }


    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        //Seed Roles
        await roleManager.CreateAsync(new IdentityRole(nameof(Roles.Admin)));
        await roleManager.CreateAsync(new IdentityRole(nameof(Roles.ProjectManager)));
        await roleManager.CreateAsync(new IdentityRole(nameof(Roles.Developer)));
        await roleManager.CreateAsync(new IdentityRole(nameof(Roles.Submitter)));
        await roleManager.CreateAsync(new IdentityRole(nameof(Roles.DemoUser)));
    }

    public static async Task SeedDefaultCompaniesAsync(ApplicationDbContext context)
    {
        try
        {
            IList<Company> defaultcompanies = new List<Company>() {
            new Company() { Name = "Company1", Description="This is default Company 1" },
            new Company() { Name = "Company2", Description="This is default Company 2" },
            new Company() { Name = "Company3", Description="This is default Company 3" },
            new Company() { Name = "Company4", Description="This is default Company 4" },
            new Company() { Name = "Company5", Description="This is default Company 5" }
        };

            var dbCompanies = context.Companies.Select(c => c.Name).ToList();
            await context.Companies.AddRangeAsync(defaultcompanies.Where(c => !dbCompanies.Contains(c.Name)));
            await context.SaveChangesAsync();

            //Get company Ids
            company1Id = context.Companies.FirstOrDefault(p => p.Name == "Company1")!.Id;
            company2Id = context.Companies.FirstOrDefault(p => p.Name == "Company2")!.Id;
            company3Id = context.Companies.FirstOrDefault(p => p.Name == "Company3")!.Id;
            company4Id = context.Companies.FirstOrDefault(p => p.Name == "Company4")!.Id;
            company5Id = context.Companies.FirstOrDefault(p => p.Name == "Company5")!.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Companies.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }

    }

    public static async Task SeedDefaultUsersAsync(UserManager<ApplicationUser> userManager)
    {
        //Seed Default Admin User
        var defaultUser = new ApplicationUser
        {
            UserName = "btadmin1@bugtracker.com",
            Email = "btadmin1@bugtracker.com",
            FirstName = faker.Name.FirstName(),
            LastName = faker.Name.LastName(),
            EmailConfirmed = true,
            CompanyId = company1Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Abc&123!");
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.Admin));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Admin User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }

        //Seed Default Admin User
        defaultUser = new ApplicationUser
        {
            UserName = "btadmin2@bugtracker.com",
            Email = "btadmin2@bugtracker.com",
            FirstName = faker.Name.FirstName(),
            LastName = faker.Name.LastName(),
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Abc&123!");
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.Admin));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Admin User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Default ProjectManager1 User
        defaultUser = new ApplicationUser
        {
            UserName = "ProjectManager1@bugtracker.com",
            Email = "ProjectManager1@bugtracker.com",
            FirstName = faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male),
            LastName = faker.Name.LastName(),
            EmailConfirmed = true,
            CompanyId = company1Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Abc&123!");
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.ProjectManager));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default ProjectManager1 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Default ProjectManager2 User
        defaultUser = new ApplicationUser
        {
            UserName = "ProjectManager2@bugtracker.com",
            Email = "ProjectManager2@bugtracker.com",
            FirstName = faker.Name.FirstName(Bogus.DataSets.Name.Gender.Female),
            LastName = faker.Name.LastName(),
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Abc&123!");
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.ProjectManager));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default ProjectManager2 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Default Developer1 User
        defaultUser = new ApplicationUser
        {
            UserName = "Developer1@bugtracker.com",
            Email = "Developer1@bugtracker.com",
            FirstName = faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male),
            LastName = faker.Name.LastName(),
            EmailConfirmed = true,
            CompanyId = company1Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Abc&123!");
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.Developer));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Developer1 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Default Developer2 User
        defaultUser = new ApplicationUser
        {
            UserName = "Developer2@bugtracker.com",
            Email = "Developer2@bugtracker.com",
            FirstName = faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male),
            LastName = faker.Name.LastName(),
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Abc&123!");
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.Developer));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Developer2 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Default Developer3 User
        defaultUser = new ApplicationUser
        {
            UserName = "Developer3@bugtracker.com",
            Email = "Developer3@bugtracker.com",
            FirstName = faker.Name.FirstName(Bogus.DataSets.Name.Gender.Female),
            LastName = faker.Name.LastName(),
            EmailConfirmed = true,
            CompanyId = company1Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Abc&123!");
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.Developer));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Developer3 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Default Developer4 User
        defaultUser = new ApplicationUser
        {
            UserName = "Developer4@bugtracker.com",
            Email = "Developer4@bugtracker.com",
            FirstName = faker.Name.FirstName(Bogus.DataSets.Name.Gender.Female),
            LastName = faker.Name.LastName(),
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Abc&123!");
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.Developer));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Developer4 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Default Developer5 User
        defaultUser = new ApplicationUser
        {
            UserName = "Developer5@bugtracker.com",
            Email = "Developer5@bugtracker.com",
            FirstName = faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male),
            LastName = faker.Name.LastName(),
            EmailConfirmed = true,
            CompanyId = company1Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Abc&123!");
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.Developer));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Developer5 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }

        //Seed Default Developer6 User
        defaultUser = new ApplicationUser
        {
            UserName = "Developer6@bugtracker.com",
            Email = "Developer6@bugtracker.com",
            FirstName = faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male),
            LastName = faker.Name.LastName(),
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Abc&123!");
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.Developer));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Developer5 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }

        //Seed Default Submitter1 User
        defaultUser = new ApplicationUser
        {
            UserName = "Submitter1@bugtracker.com",
            Email = "Submitter1@bugtracker.com",
            FirstName = faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male),
            LastName = faker.Name.LastName(),
            EmailConfirmed = true,
            CompanyId = company1Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Abc&123!");
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.Submitter));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Submitter1 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Default Submitter2 User
        defaultUser = new ApplicationUser
        {
            UserName = "Submitter2@bugtracker.com",
            Email = "Submitter2@bugtracker.com",
            FirstName = faker.Name.FirstName(Bogus.DataSets.Name.Gender.Female),
            LastName = faker.Name.LastName(),
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Abc&123!");
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.Submitter));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Submitter2 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }

    }

    public static async Task SeedDemoUsersAsync(UserManager<ApplicationUser> userManager)
    {
        //Seed Demo Admin User
        var defaultUser = new ApplicationUser
        {
            UserName = "demoadmin@bugtracker.com",
            Email = "demoadmin@bugtracker.com",
            FirstName = "Demo",
            LastName = "Admin",
            EmailConfirmed = true,
            CompanyId = company1Id
        };
        try
        {
            //Test database to see if user already exists
            var user = await userManager.FindByEmailAsync(defaultUser.Email);

            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Abc&123!");
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.Admin));
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.DemoUser));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Demo Admin User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Demo ProjectManager User
        defaultUser = new ApplicationUser
        {
            UserName = "demopm@bugtracker.com",
            Email = "demopm@bugtracker.com",
            FirstName = "Demo",
            LastName = "ProjectManager",
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Abc&123!");
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.ProjectManager));
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.DemoUser));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Demo ProjectManager1 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Demo Developer User
        defaultUser = new ApplicationUser
        {
            UserName = "demodev@bugtracker.com",
            Email = "demodev@bugtracker.com",
            FirstName = "Demo",
            LastName = "Developer",
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Abc&123!");
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.Developer));
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.DemoUser));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Demo Developer1 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Demo Submitter User
        defaultUser = new ApplicationUser
        {
            UserName = "demosub@bugtracker.com",
            Email = "demosub@bugtracker.com",
            FirstName = "Demo",
            LastName = "Submitter",
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Abc&123!");
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.Submitter));
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.DemoUser));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Demo Submitter User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Demo New User
        defaultUser = new ApplicationUser
        {
            UserName = "demonew@bugtracker.com",
            Email = "demonew@bugtracker.com",
            FirstName = "Demo",
            LastName = "NewUser",
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Abc&123!");
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.Submitter));
                await userManager.AddToRoleAsync(defaultUser, nameof(Roles.DemoUser));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Demo New User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }
    }

    public static async Task SeedDefaultProjectsAsync(ApplicationDbContext context)
    {
        try
        {
            IList<Project> projects = new List<Project>() {
             new Project()
             {
                 CompanyId = company1Id,
                 Name = "Build a Personal Porfolio",
                 Description="Single page HTML, CSS & JavaScript page. Serves as a landing page for candidates and contains a bio and links to all applications and challenges.",
                 Created = DateTimeOffset.Now - TimeSpan.FromDays(7 * 10),
                 StartDate = DateTimeOffset.Now - TimeSpan.FromDays(7 * 9),
                 EndDate = DateTimeOffset.Now + TimeSpan.FromDays(7 * 8),
                 Priority = ProjectPriority.Low
             },
             new Project()
             {
                 CompanyId = company2Id,
                 Name = "Build personal Blogging Platform",
                 Description= "A custom built web application using .NET with Blazor, a Postgres database, and deployed in a Railway container. The app is designed to create, update and maintain a live blog site.",
                 Created = DateTimeOffset.Now - TimeSpan.FromDays(7 * 5),
                 StartDate = DateTimeOffset.Now - TimeSpan.FromDays(7 * 3),
                 EndDate = DateTimeOffset.Now + TimeSpan.FromDays(7 * 2),
                 Priority = ProjectPriority.Medium
             },
             new Project()
             {
                 CompanyId = company1Id,
                 Name = "Build an Issue Tracking Web Application",
                 Description="A custom designed .NET application with Postgres database. The application is a multi-tennent application designed to track projects and their progress using a ticket system. Implemented with identity and user roles, tickets are maintained in projects which are maintained by users in the role of Project Manager. Each project has a team and team members.",
                 Created = DateTimeOffset.Now - TimeSpan.FromDays(7),
                 StartDate = DateTimeOffset.Now - TimeSpan.FromDays(7),
                 EndDate = DateTimeOffset.Now + TimeSpan.FromDays(7 * 4),
                 Priority = ProjectPriority.High
             },
             new Project()
             {
                 CompanyId = company2Id,
                 Name = "Build an Address Book Web Application",
                 Description="A custom designed .NET application with Postgres database.  This is an application to serve as a rolodex of contacts for a given user and allows users to send emails to individual contacts or entire categories of contacts.",
                 Created = DateTimeOffset.Now - TimeSpan.FromDays(7 * 5),
                 StartDate = DateTimeOffset.Now - TimeSpan.FromDays(7 * 4),
                 EndDate = DateTimeOffset.Now + TimeSpan.FromDays(7),
                 Priority = ProjectPriority.Low
             },
            new Project()
             {
                 CompanyId = company1Id,
                 Name = "Build a Movie Information Web Application",
                 Description="A custom designed .NET application with Blazor. An API based application allows users to browse popular movies and retrieve detailed information.",
                 Created = DateTimeOffset.Now - TimeSpan.FromDays(7 * 6),
                 StartDate = DateTimeOffset.Now - TimeSpan.FromDays(7 * 5),
                 EndDate = DateTimeOffset.Now - TimeSpan.FromDays(7 * 4),
                 Priority = ProjectPriority.High
             }
        };

            var dbProjects = context.Projects.Select(c => c.Name).ToList();
            await context.Projects.AddRangeAsync(projects.Where(c => !dbProjects.Contains(c.Name)));
            await context.SaveChangesAsync();


            //Get company Ids
            portfolioId = context.Projects.FirstOrDefault(p => p.Name == "Build a Personal Porfolio")!.Id;
            blogId = context.Projects.FirstOrDefault(p => p.Name == "Build personal Blogging Platform")!.Id;
            bugtrackerId = context.Projects.FirstOrDefault(p => p.Name == "Build an Issue Tracking Web Application")!.Id;
            movieId = context.Projects.FirstOrDefault(p => p.Name == "Build a Movie Information Web Application")!.Id;
            addressbookId = context.Projects.FirstOrDefault(p => p.Name == "Build an Address Book Web Application")!.Id;

        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Projects.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }
    }

    public static async Task SeedDefaultTicketsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        //Get admin Ids
        string company1AdminId = (await userManager.FindByEmailAsync("btadmin1@bugtracker.com"))!.Id;
        string company2AdminId = (await userManager.FindByEmailAsync("btadmin2@bugtracker.com"))!.Id;

        var portfolio = (await context.Projects.FindAsync(portfolioId))!;
        var blog = (await context.Projects.FindAsync(blogId))!;
        var tracker = (await context.Projects.FindAsync(bugtrackerId))!;
        var movie = (await context.Projects.FindAsync(movieId))!;
        var addressBook = (await context.Projects.FindAsync(addressbookId))!;

        try
        {
            IList<Ticket> tickets = new List<Ticket>() {
                        //PORTFOLIO
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(portfolio.StartDate, portfolio.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : portfolio.EndDate), ProjectId = portfolioId, Priority = TicketPriority.Low, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(portfolio.StartDate, portfolio.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : portfolio.EndDate), ProjectId = portfolioId, Priority = TicketPriority.Medium, Status = TicketStatus.New, Type = TicketType.ChangeRequest},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(portfolio.StartDate, portfolio.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : portfolio.EndDate), ProjectId = portfolioId, Priority = TicketPriority.High, Status = TicketStatus.InDevelopment, Type = TicketType.Enhancement},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(portfolio.StartDate, portfolio.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : portfolio.EndDate), ProjectId = portfolioId, Priority = TicketPriority.Urgent, Status = TicketStatus.Testing, Type = TicketType.Defect},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(portfolio.StartDate, portfolio.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : portfolio.EndDate), ProjectId = portfolioId, Priority = TicketPriority.Low, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(portfolio.StartDate, portfolio.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : portfolio.EndDate), ProjectId = portfolioId, Priority = TicketPriority.Medium, Status = TicketStatus.New, Type = TicketType.ChangeRequest},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(portfolio.StartDate, portfolio.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : portfolio.EndDate), ProjectId = portfolioId, Priority = TicketPriority.High, Status = TicketStatus.InDevelopment, Type = TicketType.Enhancement},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(portfolio.StartDate, portfolio.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : portfolio.EndDate), ProjectId = portfolioId, Priority = TicketPriority.Urgent, Status = TicketStatus.Testing, Type = TicketType.Defect},
                        //BLOG
                        new Ticket() {SubmitterUserId = company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(blog.StartDate, blog.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : blog.EndDate), ProjectId = blogId, Priority = TicketPriority.Low, Status = TicketStatus.New, Type = TicketType.Defect},
                        new Ticket() {SubmitterUserId = company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(blog.StartDate, blog.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : blog.EndDate), ProjectId = blogId, Priority = TicketPriority.Medium, Status = TicketStatus.InDevelopment, Type = TicketType.Enhancement},
                        new Ticket() {SubmitterUserId = company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(blog.StartDate, blog.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : blog.EndDate), ProjectId = blogId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.ChangeRequest},
                        new Ticket() {SubmitterUserId = company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(blog.StartDate, blog.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : blog.EndDate), ProjectId = blogId, Priority = TicketPriority.Urgent, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId = company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(blog.StartDate, blog.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : blog.EndDate), ProjectId = blogId, Priority = TicketPriority.Low, Status = TicketStatus.InDevelopment,  Type = TicketType.Defect},
                        new Ticket() {SubmitterUserId = company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(blog.StartDate, blog.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : blog.EndDate), ProjectId = blogId, Priority = TicketPriority.Medium, Status = TicketStatus.New,  Type = TicketType.Enhancement},
                        new Ticket() {SubmitterUserId = company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(blog.StartDate, blog.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : blog.EndDate), ProjectId = blogId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.ChangeRequest},
                        new Ticket() {SubmitterUserId = company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(blog.StartDate, blog.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : blog.EndDate), ProjectId = blogId, Priority = TicketPriority.Urgent, Status = TicketStatus.InDevelopment,  Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId = company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(blog.StartDate, blog.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : blog.EndDate), ProjectId = blogId, Priority = TicketPriority.Low, Status = TicketStatus.New,  Type = TicketType.Defect},
                        new Ticket() {SubmitterUserId = company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(blog.StartDate, blog.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : blog.EndDate), ProjectId = blogId, Priority = TicketPriority.Medium, Status = TicketStatus.New, Type = TicketType.Enhancement},
                        new Ticket() {SubmitterUserId = company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(blog.StartDate, blog.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : blog.EndDate), ProjectId = blogId, Priority = TicketPriority.High, Status = TicketStatus.InDevelopment,  Type = TicketType.ChangeRequest},
                        new Ticket() {SubmitterUserId = company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(blog.StartDate, blog.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : blog.EndDate), ProjectId = blogId, Priority = TicketPriority.Urgent, Status = TicketStatus.New,  Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId = company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(blog.StartDate, blog.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : blog.EndDate), ProjectId = blogId, Priority = TicketPriority.Low, Status = TicketStatus.New, Type = TicketType.Defect},
                        new Ticket() {SubmitterUserId = company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(blog.StartDate, blog.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : blog.EndDate), ProjectId = blogId, Priority = TicketPriority.Medium, Status = TicketStatus.InDevelopment,  Type = TicketType.Enhancement},
                        new Ticket() {SubmitterUserId = company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(blog.StartDate, blog.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : blog.EndDate), ProjectId = blogId, Priority = TicketPriority.High, Status = TicketStatus.New,  Type = TicketType.ChangeRequest},
                        new Ticket() {SubmitterUserId = company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(blog.StartDate, blog.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : blog.EndDate), ProjectId = blogId, Priority = TicketPriority.Urgent, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId = company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(blog.StartDate, blog.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : blog.EndDate), ProjectId = blogId, Priority = TicketPriority.High, Status = TicketStatus.InDevelopment,  Type = TicketType.NewDevelopment},
                        //BUGTRACKER                                                                                                                         
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(tracker.StartDate, tracker.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : tracker.EndDate), ProjectId = bugtrackerId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        //MOVIE
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.Low, Status = TicketStatus.New, Type = TicketType.Defect},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.Medium, Status = TicketStatus.InDevelopment, Type = TicketType.Enhancement},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.ChangeRequest},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.Urgent, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.Low, Status = TicketStatus.InDevelopment,  Type = TicketType.Defect},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.Medium, Status = TicketStatus.New,  Type = TicketType.Enhancement},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.High, Status = TicketStatus.New, Type = TicketType.ChangeRequest},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.Urgent, Status = TicketStatus.InDevelopment,  Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.Low, Status = TicketStatus.New,  Type = TicketType.Defect},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.Medium, Status = TicketStatus.New, Type = TicketType.Enhancement},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.High, Status = TicketStatus.InDevelopment,  Type = TicketType.ChangeRequest},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.Urgent, Status = TicketStatus.New,  Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.Low, Status = TicketStatus.New, Type = TicketType.Defect},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.Medium, Status = TicketStatus.InDevelopment,  Type = TicketType.Enhancement},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.High, Status = TicketStatus.New,  Type = TicketType.ChangeRequest},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.Urgent, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.High, Status = TicketStatus.InDevelopment,  Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.Medium, Status = TicketStatus.InDevelopment,  Type = TicketType.Enhancement},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.High, Status = TicketStatus.New,  Type = TicketType.ChangeRequest},
                        new Ticket() {SubmitterUserId = company1AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(movie.StartDate, movie.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : movie.EndDate), ProjectId = movieId, Priority = TicketPriority.Urgent, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        //ADDRESSBOOK
                        new Ticket() {SubmitterUserId=company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(addressBook.StartDate, addressBook.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : addressBook.EndDate), ProjectId = addressbookId, Priority = TicketPriority.Low, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(addressBook.StartDate, addressBook.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : addressBook.EndDate), ProjectId = addressbookId, Priority = TicketPriority.Medium, Status = TicketStatus.New, Type = TicketType.ChangeRequest},
                        new Ticket() {SubmitterUserId=company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(addressBook.StartDate, addressBook.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : addressBook.EndDate), ProjectId = addressbookId, Priority = TicketPriority.High, Status = TicketStatus.InDevelopment, Type = TicketType.Enhancement},
                        new Ticket() {SubmitterUserId=company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(addressBook.StartDate, addressBook.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : addressBook.EndDate), ProjectId = addressbookId, Priority = TicketPriority.Urgent, Status = TicketStatus.Testing, Type = TicketType.Defect},
                        new Ticket() {SubmitterUserId=company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(addressBook.StartDate, addressBook.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : addressBook.EndDate), ProjectId = addressbookId, Priority = TicketPriority.Low, Status = TicketStatus.New, Type = TicketType.NewDevelopment},
                        new Ticket() {SubmitterUserId=company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(addressBook.StartDate, addressBook.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : addressBook.EndDate), ProjectId = addressbookId, Priority = TicketPriority.Medium, Status = TicketStatus.New, Type = TicketType.ChangeRequest},
                        new Ticket() {SubmitterUserId=company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(addressBook.StartDate, addressBook.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : addressBook.EndDate), ProjectId = addressbookId, Priority = TicketPriority.High, Status = TicketStatus.InDevelopment, Type = TicketType.Enhancement},
                        new Ticket() {SubmitterUserId=company2AdminId, Title = faker.Company.Bs().Titleize(), Description = faker.Lorem.Paragraph(), Created = faker.Date.BetweenOffset(addressBook.StartDate, addressBook.EndDate > DateTimeOffset.Now ? DateTimeOffset.Now : addressBook.EndDate), ProjectId = addressbookId, Priority = TicketPriority.Urgent, Status = TicketStatus.Testing, Type = TicketType.Defect},


        };

            int[] projectIds = [portfolioId, blogId, bugtrackerId, movieId, addressbookId];
            foreach (var projectId in projectIds)
            {
                if (!context.Tickets.Any(t => t.ProjectId == projectId))
                {
                    context.Tickets.AddRange(tickets.Where(t => t.ProjectId == projectId));
                }
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Tickets.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }
    }


    private static readonly TextInfo _textInfo = new CultureInfo("en-US").TextInfo;
    private static string Titleize(this string input) => _textInfo.ToTitleCase(input);
}