// ── Constraints ──────────────────────────────────────────────────────
CREATE CONSTRAINT user_id_unique IF NOT EXISTS FOR (u:User) REQUIRE u.id IS UNIQUE;
CREATE CONSTRAINT user_email_unique IF NOT EXISTS FOR (u:User) REQUIRE u.email IS UNIQUE;
CREATE CONSTRAINT usergroup_id_unique IF NOT EXISTS FOR (g:UserGroup) REQUIRE g.id IS UNIQUE;
CREATE CONSTRAINT role_id_unique IF NOT EXISTS FOR (r:Role) REQUIRE r.id IS UNIQUE;
CREATE CONSTRAINT menu_id_unique IF NOT EXISTS FOR (m:Menu) REQUIRE m.id IS UNIQUE;
CREATE CONSTRAINT policy_id_unique IF NOT EXISTS FOR (p:Policy) REQUIRE p.id IS UNIQUE;

// ── Users ────────────────────────────────────────────────────────────
MERGE (u:User {id: 1})
SET u.username = 'admin', u.email = 'admin@bank.com', u.passwordHash = 'hashedpassword';

// ── UserGroups ───────────────────────────────────────────────────────
MERGE (:UserGroup {id: 1, groupName: 'Admin'});
MERGE (:UserGroup {id: 2, groupName: 'Manager'});
MERGE (:UserGroup {id: 3, groupName: 'Auditor'});
MERGE (:UserGroup {id: 4, groupName: 'Viewer'});

// ── Roles ────────────────────────────────────────────────────────────
MERGE (:Role {id: 1, name: 'Admin'});
MERGE (:Role {id: 2, name: 'Viewer'});
MERGE (:Role {id: 3, name: 'Manager'});
MERGE (:Role {id: 4, name: 'Auditor'});

// ── Policies ─────────────────────────────────────────────────────────
MERGE (:Policy {id: 1, name: 'FullAccess', description: 'Grants full access to all resources.', resource: '*', action: '*'});
MERGE (:Policy {id: 2, name: 'ReadOnly', description: 'Grants read-only access to resources.', resource: '*', action: 'Read'});
MERGE (:Policy {id: 3, name: 'ManageUsers', description: 'Allows managing users and groups.', resource: 'User', action: 'Write'});
MERGE (:Policy {id: 4, name: 'AuditAccess', description: 'Allows viewing audit logs and reports.', resource: 'AuditLog', action: 'Read'});

// ── Menus (top-level) ────────────────────────────────────────────────
MERGE (:Menu {id: 1, name: 'Dashboard', route: '/dashboard', icon: 'dashboard', displayOrder: 1});
MERGE (:Menu {id: 2, name: 'User Management', route: '/users', icon: 'people', displayOrder: 2});
MERGE (:Menu {id: 3, name: 'Reports', route: '/reports', icon: 'bar_chart', displayOrder: 3});
MERGE (:Menu {id: 4, name: 'Settings', route: '/settings', icon: 'settings', displayOrder: 4});
MERGE (:Menu {id: 5, name: 'Audit Logs', route: '/audit', icon: 'history', displayOrder: 5});

// ── Menus (children) ─────────────────────────────────────────────────
MERGE (:Menu {id: 6, name: 'Users', route: '/users/list', icon: 'person', displayOrder: 1, parentMenuId: 2});
MERGE (:Menu {id: 7, name: 'Groups', route: '/users/groups', icon: 'group', displayOrder: 2, parentMenuId: 2});
MERGE (:Menu {id: 8, name: 'Roles', route: '/users/roles', icon: 'admin_panel_settings', displayOrder: 3, parentMenuId: 2});
MERGE (:Menu {id: 13, name: 'Policies', route: '/users/policies', icon: 'policy', displayOrder: 4, parentMenuId: 2});
MERGE (:Menu {id: 9, name: 'Activity Report', route: '/reports/activity', icon: 'assessment', displayOrder: 1, parentMenuId: 3});
MERGE (:Menu {id: 10, name: 'Access Report', route: '/reports/access', icon: 'security', displayOrder: 2, parentMenuId: 3});
MERGE (:Menu {id: 11, name: 'General', route: '/settings/general', icon: 'tune', displayOrder: 1, parentMenuId: 4});
MERGE (:Menu {id: 12, name: 'Security', route: '/settings/security', icon: 'lock', displayOrder: 2, parentMenuId: 4});
MERGE (:Menu {id: 14, name: 'Menus', route: '/settings/menus', icon: 'menu', displayOrder: 3, parentMenuId: 4});

// ── Menu hierarchy (CHILD_OF) ────────────────────────────────────────
MATCH (child:Menu {id: 6}), (parent:Menu {id: 2}) MERGE (child)-[:CHILD_OF]->(parent);
MATCH (child:Menu {id: 7}), (parent:Menu {id: 2}) MERGE (child)-[:CHILD_OF]->(parent);
MATCH (child:Menu {id: 8}), (parent:Menu {id: 2}) MERGE (child)-[:CHILD_OF]->(parent);
MATCH (child:Menu {id: 13}), (parent:Menu {id: 2}) MERGE (child)-[:CHILD_OF]->(parent);
MATCH (child:Menu {id: 9}), (parent:Menu {id: 3}) MERGE (child)-[:CHILD_OF]->(parent);
MATCH (child:Menu {id: 10}), (parent:Menu {id: 3}) MERGE (child)-[:CHILD_OF]->(parent);
MATCH (child:Menu {id: 11}), (parent:Menu {id: 4}) MERGE (child)-[:CHILD_OF]->(parent);
MATCH (child:Menu {id: 12}), (parent:Menu {id: 4}) MERGE (child)-[:CHILD_OF]->(parent);
MATCH (child:Menu {id: 14}), (parent:Menu {id: 4}) MERGE (child)-[:CHILD_OF]->(parent);

// ── User → UserGroup (MEMBER_OF) ────────────────────────────────────
MATCH (u:User {id: 1}), (g:UserGroup {id: 1})
MERGE (u)-[:MEMBER_OF {assignedAt: datetime('2026-01-01T00:00:00Z'), assignedBy: 'system'}]->(g);

// ── UserGroup → Role (HAS_ROLE) ──────────────────────────────────────
MATCH (g:UserGroup {id: 1}), (r:Role {id: 1}) MERGE (g)-[:HAS_ROLE]->(r);
MATCH (g:UserGroup {id: 2}), (r:Role {id: 3}) MERGE (g)-[:HAS_ROLE]->(r);
MATCH (g:UserGroup {id: 3}), (r:Role {id: 4}) MERGE (g)-[:HAS_ROLE]->(r);
MATCH (g:UserGroup {id: 4}), (r:Role {id: 2}) MERGE (g)-[:HAS_ROLE]->(r);

// ── Role → Policy (HAS_POLICY) ──────────────────────────────────────
MATCH (r:Role {id: 1}), (p:Policy {id: 1}) MERGE (r)-[:HAS_POLICY]->(p);
MATCH (r:Role {id: 2}), (p:Policy {id: 2}) MERGE (r)-[:HAS_POLICY]->(p);
MATCH (r:Role {id: 3}), (p:Policy {id: 2}) MERGE (r)-[:HAS_POLICY]->(p);
MATCH (r:Role {id: 3}), (p:Policy {id: 3}) MERGE (r)-[:HAS_POLICY]->(p);
MATCH (r:Role {id: 4}), (p:Policy {id: 2}) MERGE (r)-[:HAS_POLICY]->(p);
MATCH (r:Role {id: 4}), (p:Policy {id: 4}) MERGE (r)-[:HAS_POLICY]->(p);

// ── Role → Menu (HAS_MENU_PERMISSION) ───────────────────────────────
// Admin (role 1) → all menus
MATCH (r:Role {id: 1}), (m:Menu {id: 1}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 1}), (m:Menu {id: 2}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 1}), (m:Menu {id: 3}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 1}), (m:Menu {id: 4}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 1}), (m:Menu {id: 5}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 1}), (m:Menu {id: 6}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 1}), (m:Menu {id: 7}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 1}), (m:Menu {id: 8}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 1}), (m:Menu {id: 9}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 1}), (m:Menu {id: 10}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 1}), (m:Menu {id: 11}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 1}), (m:Menu {id: 12}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 1}), (m:Menu {id: 13}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 1}), (m:Menu {id: 14}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);

// Viewer (role 2) → Dashboard only
MATCH (r:Role {id: 2}), (m:Menu {id: 1}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);

// Manager (role 3) → Dashboard, User Management, Settings + children
MATCH (r:Role {id: 3}), (m:Menu {id: 1}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 3}), (m:Menu {id: 2}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 3}), (m:Menu {id: 4}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 3}), (m:Menu {id: 6}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 3}), (m:Menu {id: 7}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 3}), (m:Menu {id: 8}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 3}), (m:Menu {id: 13}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 3}), (m:Menu {id: 11}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 3}), (m:Menu {id: 12}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 3}), (m:Menu {id: 14}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);

// Auditor (role 4) → Dashboard, Reports, Audit Logs + children
MATCH (r:Role {id: 4}), (m:Menu {id: 1}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 4}), (m:Menu {id: 3}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 4}), (m:Menu {id: 5}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 4}), (m:Menu {id: 9}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
MATCH (r:Role {id: 4}), (m:Menu {id: 10}) MERGE (r)-[:HAS_MENU_PERMISSION {permissionLevel: 'View'}]->(m);
