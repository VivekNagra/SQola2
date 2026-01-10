# TodoApp — Software Quality (OLA2)
## Please check ola3 branch for assignment 3 soloution. :)

This repository contains my OLA2 solution for the Software Quality course at Erhvervsakademi København. The project is a small .NET Todo backend where the main focus is demonstrating a realistic testing approach across unit, integration, and specification-based tests, supported by mutation testing.

---

## What the system does

The API supports the basic todo flows:

- create lists
- create tasks in a list
- update a task title
- mark a task completed / in-progress
- move a task to another list
- set or clear a deadline
- delete a task

I kept the API layer intentionally thin and pushed the decision-making into the application layer (`TodoService`), because that is where testing gives the most value.

---

## Solution structure

The solution is split into projects to separate concerns and keep the core logic testable:

- `Todo.Domain`  
  Entities and validation rules (normalization, constraints).
- `Todo.Application`  
  Use-case logic (`TodoService`) and abstractions (`IClock`, repository interfaces).
- `Todo.Infrastructure`  
  EF Core persistence (DbContext, repositories) and `SystemClock`.
- `Todo.Api`  
  Minimal API endpoints mapping HTTP requests to `TodoService`.

Tests are separated by level:

- `Todo.UnitTests`  
  Isolated tests of `TodoService` using test doubles (stub clock, fakes/spies).
- `Todo.IntegrationTests`  
  EF Core + SQLite in-memory tests verifying real persistence and repository integration.
- `Todo.SpecificationTests`  
  HTTP scenario test (Given/When/Then) using `WebApplicationFactory`.

---

## Tech stack

- .NET (SDK pinned using `global.json`)
- xUnit for testing
- EF Core + SQLite
- Stryker.NET for mutation testing

---

## How to run

### Build
```bash
dotnet build
```

## To run tests
```bash
dotnet test
```

## Mutation Testing
```bash
dotnet tool restore
dotnet stryker
```
The HTML report is generated under:

TodoApp/StrykerOutput/<timestamp>/reports/mutation-report.html

Latest mutation testing outcome during development:

55 mutants created

37 mutants tested

30 killed, 7 survived, 10 no coverage

mutation score: **63.83%**  
