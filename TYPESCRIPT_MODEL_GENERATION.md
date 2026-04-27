# TypeScript Model Generation Guide

## Overview

The `TypeScriptServiceEmitter` now generates proper TypeScript interfaces with actual properties instead of placeholder `[key: string]: any` interfaces. It analyzes C# model classes using Roslyn and extracts their properties, types, and nullability to create strongly-typed TypeScript definitions.

## How It Works

### 1. Property Analysis
The generator inspects C# classes and extracts:
- **Property names** (converted to camelCase)
- **Property types** (mapped to TypeScript equivalents)
- **Nullability** (converted to optional `?` in TypeScript)
- **Required modifier** (non-optional properties in TypeScript)

### 2. Type Mapping

#### Primitive Types
| C# Type | TypeScript Type |
|---------|----------------|
| `string` | `string` |
| `int`, `long`, `short`, `byte` | `number` |
| `double`, `float`, `decimal` | `number` |
| `bool` | `boolean` |
| `DateTime`, `DateTimeOffset`, `DateOnly` | `string` (ISO format) |
| `Guid` | `string` |
| `TimeSpan`, `TimeOnly` | `string` |

#### Collection Types
- `List<T>`, `IEnumerable<T>`, `ICollection<T>` ? `T[]`
- `T[]` ? `T[]`
- `Dictionary<TKey, TValue>` ? `{ [key: string]: any }`

#### Complex Types
Custom classes are generated as separate interfaces.

## Example Generated Output

### C# Models

```csharp
// AotSample/Models/ViewModels/UserModel.cs
namespace AotSample.Models.ViewModels;

public class UserModel
{
    public required string Email { get; set; } 
    public required int Age { get; set; }
    public required string Name { get; set; }
}

// AotSample/Commands/CreateUserCommand.cs
public class CreateUserCommand : IRequest<Unit>
{
    public required UserModel UserModel { get; set; }
}

// AotSample/Commands/UpdateUserCommand.cs
public class UpdateUserCommand : IRequest<Unit>
{
    public string Id { get; set; } = string.Empty;
    public required UserModel UserModel { get; set; }
}

// AotSample/Commands/DeleteUserCommand.cs
public class DeleteUserCommand : IRequest<Unit>
{
    public required string Id { get; set; }
}
```

### Controller Endpoints

```csharp
[ApiController("api/users")]
public class UsersController
{
    [HttpPost(Summary = "Create a new user")]
    public async Task<Unit> CreateUser([FromBody] CreateUserCommand command, CancellationToken ct);

    [HttpGet("{id}", Summary = "Get user by ID")]
    public async Task<UserModel> GetUser([FromRoute] string id, CancellationToken ct);

    [HttpGet(Summary = "List all users")]
    public async Task<List<UserModel>> ListUsers([FromQuery] int page, [FromQuery] int pageSize, CancellationToken ct);

    [HttpPut("{id}", Summary = "Update user")]
    public async Task<Unit> UpdateUser([FromRoute] string id, [FromBody] UpdateUserCommand command, CancellationToken ct);

    [HttpDelete("{id}", Summary = "Delete user")]
    public async Task<Unit> DeleteUser([FromRoute] string id, CancellationToken ct);
}
```

### Generated TypeScript

```typescript
/* Auto-generated TypeScript service from ZeroReflection API */
/* Generated at: 2025-01-15T10:30:00Z */

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

// ==================== DTOs ====================

/** UserModel model from AotSample.Models.ViewModels */
export interface UserModel {
  age: number;
  email: string;
  name: string;
}

/** CreateUserCommand model from AotSample.Commands */
export interface CreateUserCommand {
  userModel: UserModel;
}

/** UpdateUserCommand model from AotSample.Commands */
export interface UpdateUserCommand {
  id: string;
  userModel: UserModel;
}

/** DeleteUserCommand model from AotSample.Commands */
export interface DeleteUserCommand {
  id: string;
}

// ==================== Service ====================

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = '/api'; // Configure your API base URL

  constructor(private http: HttpClient) {}

  // ==================== UsersController ====================

  /**
   * Create a new user
   */
  createUser(command: CreateUserCommand): Observable<void> {
    const url = `${this.baseUrl}/api/users`;
    return this.http.post<void>(url, command);
  }

  /**
   * Get user by ID
   */
  getUser(id: string): Observable<UserModel> {
    const url = `${this.baseUrl}/api/users/${id}`;
    return this.http.get<UserModel>(url);
  }

  /**
   * List all users
   */
  listUsers(page?: number, pageSize?: number): Observable<UserModel[]> {
    const url = `${this.baseUrl}/api/users`;
    let params = new HttpParams();
    if (page !== undefined) {
      params = params.set('page', page.toString());
    }
    if (pageSize !== undefined) {
      params = params.set('pageSize', pageSize.toString());
    }
    return this.http.get<UserModel[]>(url, { params });
  }

  /**
   * Update user
   */
  updateUser(id: string, command: UpdateUserCommand): Observable<void> {
    const url = `${this.baseUrl}/api/users/${id}`;
    return this.http.put<void>(url, command);
  }

  /**
   * Delete user
   */
  deleteUser(id: string): Observable<void> {
    const url = `${this.baseUrl}/api/users/${id}`;
    return this.http.delete<void>(url);
  }
}
```

## Features

### ? Strongly-Typed Properties
- All model properties are properly typed
- No more `any` types unless necessary

### ? Optional vs Required
- C# nullable types (`string?`) become optional TypeScript properties (`prop?`)
- C# `required` properties become non-optional TypeScript properties

### ? Nested Models
- Complex types are automatically detected
- Nested interfaces are generated recursively
- Avoids duplicates through tracking

### ? CamelCase Conversion
- C# PascalCase properties ? TypeScript camelCase
- Example: `Email` ? `email`, `UserModel` ? `userModel`

### ? Collection Support
- Arrays and Lists properly handled
- Generic types are unwrapped and mapped

## Usage in Angular

```typescript
import { Component } from '@angular/core';
import { ApiService, UserModel, CreateUserCommand } from './api.service';

@Component({
  selector: 'app-user-list',
  template: `
    <div *ngFor="let user of users">
      {{ user.name }} - {{ user.email }}
    </div>
  `
})
export class UserListComponent {
  users: UserModel[] = [];

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.api.listUsers(1, 10).subscribe(users => {
      this.users = users;
    });
  }

  createUser() {
    const command: CreateUserCommand = {
      userModel: {
        email: 'test@example.com',
        age: 30,
        name: 'John Doe'
      }
    };

    this.api.createUser(command).subscribe(() => {
      this.loadUsers();
    });
  }
}
```

## Benefits

1. **Type Safety**: Full IntelliSense support in TypeScript/Angular
2. **No Manual Sync**: Models auto-update when C# changes
3. **Reduced Errors**: Compile-time checking catches type mismatches
4. **Better DX**: Autocomplete for all properties and methods
5. **Documentation**: Generated interfaces serve as API documentation

## Current Status

?? **Note**: TypeScript generation is currently disabled in the source generator because source generators cannot perform file I/O operations. The `TypeScriptServiceEmitter` class contains all the logic, but it needs to be called from a separate build step or tool.

### Alternative Approaches

1. **MSBuild Task**: Create a custom MSBuild task that runs after compilation
2. **CLI Tool**: Create a separate CLI tool that reads the compilation and generates TypeScript
3. **Analyzer Tool**: Use the Roslyn analyzer API in a standalone tool
4. **Build Event**: Add a post-build event that runs a TypeScript generation executable

## Example Integration

You could create a console app that uses the `TypeScriptServiceEmitter`:

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ZeroReflection.ApiGenerator.Emit;
using ZeroReflection.ApiGenerator.Models;

var compilation = /* load your compilation */;
var endpoints = /* extract endpoint definitions */;

var typescript = TypeScriptServiceEmitter.GenerateTypeScriptService(endpoints, compilation);
File.WriteAllText("api.service.ts", typescript);
```

This way, the TypeScript generation happens outside the source generator context where file I/O is allowed.
