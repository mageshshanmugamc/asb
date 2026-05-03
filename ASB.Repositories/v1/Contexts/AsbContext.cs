namespace ASB.Repositories.v1.Contexts
{
    using Microsoft.EntityFrameworkCore;
    using ASB.Repositories.v1.Entities;
    /// <summary>
    /// Represents the database context for the ASB application. This context is responsible for managing the connection to the database and providing access to the various entities such as users, roles, policies, and their relationships. It defines the DbSet properties for each entity and configures the relationships between them using the OnModelCreating method. The context also seeds initial data for an admin role, policy, and user to ensure that there is a default administrative account available when the application is first run.
    /// </summary>
   public class AsbContext : DbContext
    {
        public AsbContext(DbContextOptions<AsbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Policy> Policies => Set<Policy>();
        public DbSet<RolePolicy> RolePolicies => Set<RolePolicy>();
        public DbSet<UserGroupMapping> UserGroupMappings => Set<UserGroupMapping>();
        public DbSet<UserGroup> UserGroups => Set<UserGroup>();
        public DbSet<UserGroupRole> UserGroupRoles => Set<UserGroupRole>();
        public DbSet<Menu> Menus => Set<Menu>();
        public DbSet<RoleMenuPermission> RoleMenuPermissions => Set<RoleMenuPermission>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RolePolicy>().HasKey(rp => new { rp.RoleId, rp.PolicyId });
            modelBuilder.Entity<UserGroupMapping>().HasKey(ugm => new { ugm.UserId, ugm.UserGroupId });
            modelBuilder.Entity<UserGroupRole>().HasKey(ugr => new { ugr.UserGroupId, ugr.RoleId });
            modelBuilder.Entity<RoleMenuPermission>().HasKey(rmp => new { rmp.RoleId, rmp.MenuId });

            // Menu hierarchy
            modelBuilder.Entity<Menu>()
                .HasOne(m => m.ParentMenu)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.ParentMenuId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed Roles
            var adminRole   = new Role { Id = 1, Name = "Admin" };
            var viewerRole  = new Role { Id = 2, Name = "Viewer" };
            var managerRole = new Role { Id = 3, Name = "Manager" };
            var auditorRole = new Role { Id = 4, Name = "Auditor" };

            modelBuilder.Entity<Role>().HasData(adminRole, viewerRole, managerRole, auditorRole);

            // Seed Policies
            var fullAccessPolicy = new Policy { Id = 1, Name = "FullAccess", Description = "Grants full access to all resources.", Resource = "*", Action = "*" };
            var readOnlyPolicy   = new Policy { Id = 2, Name = "ReadOnly", Description = "Grants read-only access to resources.", Resource = "*", Action = "Read" };
            var manageUsersPolicy = new Policy { Id = 3, Name = "ManageUsers", Description = "Allows managing users and groups.", Resource = "User", Action = "Write" };
            var auditPolicy      = new Policy { Id = 4, Name = "AuditAccess", Description = "Allows viewing audit logs and reports.", Resource = "AuditLog", Action = "Read" };

            modelBuilder.Entity<Policy>().HasData(fullAccessPolicy, readOnlyPolicy, manageUsersPolicy, auditPolicy);

            // Seed RolePolicies
            modelBuilder.Entity<RolePolicy>().HasData(
                new RolePolicy { RoleId = 1, PolicyId = 1 }, // Admin → FullAccess
                new RolePolicy { RoleId = 2, PolicyId = 2 }, // Viewer → ReadOnly
                new RolePolicy { RoleId = 3, PolicyId = 2 }, // Manager → ReadOnly
                new RolePolicy { RoleId = 3, PolicyId = 3 }, // Manager → ManageUsers
                new RolePolicy { RoleId = 4, PolicyId = 2 }, // Auditor → ReadOnly
                new RolePolicy { RoleId = 4, PolicyId = 4 }  // Auditor → AuditAccess
            );

            // Seed Admin User
            var adminUser = new User { Id = 1, Username = "admin", Email = "admin@bank.com", PasswordHash = "hashedpassword" };
            modelBuilder.Entity<User>().HasData(adminUser);

            // Seed UserGroups
            modelBuilder.Entity<UserGroup>().HasData(
                new UserGroup { Id = 1, GroupName = "Admin" },
                new UserGroup { Id = 2, GroupName = "Manager" },
                new UserGroup { Id = 3, GroupName = "Auditor" },
                new UserGroup { Id = 4, GroupName = "Viewer" }
            );

            // Seed UserGroupRoles (group → role mapping)
            modelBuilder.Entity<UserGroupRole>().HasData(
                new UserGroupRole { UserGroupId = 1, RoleId = 1 }, // Admin group → Admin role
                new UserGroupRole { UserGroupId = 2, RoleId = 3 }, // Manager group → Manager role
                new UserGroupRole { UserGroupId = 3, RoleId = 4 }, // Auditor group → Auditor role
                new UserGroupRole { UserGroupId = 4, RoleId = 2 }  // Viewer group → Viewer role
            );

            // Seed UserGroupMapping (admin user → Admin group)
            modelBuilder.Entity<UserGroupMapping>().HasData(
                new UserGroupMapping { UserId = 1, UserGroupId = 1, AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), AssignedBy = "system" }
            );

            // Seed Menus (hierarchy)
            modelBuilder.Entity<Menu>().HasData(
                // Top-level
                new Menu { Id = 1, Name = "Dashboard", Route = "/dashboard", Icon = "dashboard", DisplayOrder = 1, ParentMenuId = null },
                new Menu { Id = 2, Name = "User Management", Route = "/users", Icon = "people", DisplayOrder = 2, ParentMenuId = null },
                new Menu { Id = 3, Name = "Reports", Route = "/reports", Icon = "bar_chart", DisplayOrder = 3, ParentMenuId = null },
                new Menu { Id = 4, Name = "Settings", Route = "/settings", Icon = "settings", DisplayOrder = 4, ParentMenuId = null },
                new Menu { Id = 5, Name = "Audit Logs", Route = "/audit", Icon = "history", DisplayOrder = 5, ParentMenuId = null },

                // User Management children
                new Menu { Id = 6, Name = "Users", Route = "/users/list", Icon = "person", DisplayOrder = 1, ParentMenuId = 2 },
                new Menu { Id = 7, Name = "Groups", Route = "/users/groups", Icon = "group", DisplayOrder = 2, ParentMenuId = 2 },
                new Menu { Id = 8, Name = "Roles", Route = "/users/roles", Icon = "admin_panel_settings", DisplayOrder = 3, ParentMenuId = 2 },
                new Menu { Id = 13, Name = "Policies", Route = "/users/policies", Icon = "policy", DisplayOrder = 4, ParentMenuId = 2 },

                // Reports children
                new Menu { Id = 9, Name = "Activity Report", Route = "/reports/activity", Icon = "assessment", DisplayOrder = 1, ParentMenuId = 3 },
                new Menu { Id = 10, Name = "Access Report", Route = "/reports/access", Icon = "security", DisplayOrder = 2, ParentMenuId = 3 },

                // Settings children
                new Menu { Id = 11, Name = "General", Route = "/settings/general", Icon = "tune", DisplayOrder = 1, ParentMenuId = 4 },
                new Menu { Id = 12, Name = "Security", Route = "/settings/security", Icon = "lock", DisplayOrder = 2, ParentMenuId = 4 }
            );

            // Seed RoleMenuPermissions
            // Admin → all menus
            modelBuilder.Entity<RoleMenuPermission>().HasData(
                new RoleMenuPermission { RoleId = 1, MenuId = 1 },
                new RoleMenuPermission { RoleId = 1, MenuId = 2 },
                new RoleMenuPermission { RoleId = 1, MenuId = 3 },
                new RoleMenuPermission { RoleId = 1, MenuId = 4 },
                new RoleMenuPermission { RoleId = 1, MenuId = 5 },
                new RoleMenuPermission { RoleId = 1, MenuId = 6 },
                new RoleMenuPermission { RoleId = 1, MenuId = 7 },
                new RoleMenuPermission { RoleId = 1, MenuId = 8 },
                new RoleMenuPermission { RoleId = 1, MenuId = 9 },
                new RoleMenuPermission { RoleId = 1, MenuId = 10 },
                new RoleMenuPermission { RoleId = 1, MenuId = 11 },
                new RoleMenuPermission { RoleId = 1, MenuId = 12 },
                new RoleMenuPermission { RoleId = 1, MenuId = 13 }
            );
            // Viewer → Dashboard only
            modelBuilder.Entity<RoleMenuPermission>().HasData(
                new RoleMenuPermission { RoleId = 2, MenuId = 1 }
            );
            // Manager → Dashboard, User Management, Settings
            modelBuilder.Entity<RoleMenuPermission>().HasData(
                new RoleMenuPermission { RoleId = 3, MenuId = 1 },
                new RoleMenuPermission { RoleId = 3, MenuId = 2 },
                new RoleMenuPermission { RoleId = 3, MenuId = 4 },
                new RoleMenuPermission { RoleId = 3, MenuId = 6 },
                new RoleMenuPermission { RoleId = 3, MenuId = 7 },
                new RoleMenuPermission { RoleId = 3, MenuId = 8 },
                new RoleMenuPermission { RoleId = 3, MenuId = 13 },
                new RoleMenuPermission { RoleId = 3, MenuId = 11 },
                new RoleMenuPermission { RoleId = 3, MenuId = 12 }
            );
            // Auditor → Dashboard, Reports, Audit Logs
            modelBuilder.Entity<RoleMenuPermission>().HasData(
                new RoleMenuPermission { RoleId = 4, MenuId = 1 },
                new RoleMenuPermission { RoleId = 4, MenuId = 3 },
                new RoleMenuPermission { RoleId = 4, MenuId = 5 },
                new RoleMenuPermission { RoleId = 4, MenuId = 9 },
                new RoleMenuPermission { RoleId = 4, MenuId = 10 }
            );
        }
    }

}