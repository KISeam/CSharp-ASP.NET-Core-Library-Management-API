# Library Management API — Architecture Guide

## Clean Architecture Layers

```
┌─────────────────────────────────────────┐
│           API Layer (Controllers)        │  ← HTTP in/out, Auth middleware
├─────────────────────────────────────────┤
│        Application Layer (Services)      │  ← Business logic, DTOs, Validators
├─────────────────────────────────────────┤
│          Domain Layer (Entities)         │  ← Core models, Interfaces, Rules
├─────────────────────────────────────────┤
│     Infrastructure Layer (EF Core)       │  ← DB, Repos, External services
└─────────────────────────────────────────┘
```

## Role-Based Access
- **Admin**   → Full access: manage books, authors, members, view all borrows
- **Librarian** → Add/edit books & authors, manage borrow records
- **Member**  → View books, borrow/return, see own history

## Key Design Patterns Used
- Repository Pattern + Unit of Work
- Result Pattern (no raw exceptions across layers)
- CQRS-lite (separate read/write DTOs)
- Dependency Injection throughout
- Guard Clauses (fail fast)

## DSA Applied
- Binary Search on sorted ISBN list (BookIndex)
- Priority Queue for overdue fine calculation
- LRU Cache simulation via Dictionary + LinkedList
