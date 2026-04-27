# TypeScript/Angular Service Generator

## Overview

The Swagger UI now includes a **TypeScript Service Generator** that creates a ready-to-use Angular service with all your API endpoints, DTOs, and proper typing.

## Features

? **One-Click Generation** - Generate complete Angular service with a single click
? **Type-Safe DTOs** - All models exported as TypeScript interfaces  
? **HTTP Methods** - GET, POST, PUT, DELETE, PATCH all supported
? **Route Parameters** - Properly templated URL parameters
? **Query Parameters** - Optional query string parameters  
? **Request Bodies** - Typed request payloads
? **Response Types** - Properly typed Observable responses
? **JSDoc Comments** - Includes summaries from your controllers
? **Grouped by Controller** - Methods organized by controller
? **Angular HttpClient** - Uses standard Angular HTTP client
? **RxJS Observables** - Returns Observable for async operations

## How to Use

### 1. Open Swagger UI

Navigate to `http://localhost:5000/swagger`

### 2. Click "Generate TypeScript Service"

Click the **?? Generate TypeScript Service** button in the header.

### 3. Download Generated File

The browser will download `api.service.ts` automatically.

### 4. Add to Your Angular Project

Copy the file to your Angular project:
```bash
cp api.service.ts src/app/services/
```

### 5. Use in Your Components

```typescript
import { Component } from '@angular/core';
import { ApiService, UserModel } from './services/api.service';

@Component({
  selector: 'app-users',
  template: '...'
})
export class UsersComponent {
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

  createUser(user: UserModel) {
    this.api.createUser({ userModel: user }).subscribe(() => {
      this.loadUsers();
    });
  }

  deleteUser(id: string) {
    this.api.deleteUser(id).subscribe(() => {
      this.loadUsers();
    });
  }
}
```

## Generated Service Structure

### DTOs (Interfaces)

All unique types are extracted and exported as TypeScript interfaces:

```typescript
export interface UserModel {
  // TODO: Add properties based on your backend model
  [key: string]: any;
}

export interface CreateUserCommand {
  [key: string]: any;
}
```

**Note**: You'll need to fill in the actual properties based on your C# models. The generator provides the structure.

### Service Class

```typescript
@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = '/api'; // Configure your API base URL

  constructor(private http: HttpClient) {}

  // ==================== UsersController ====================

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
   * Get user by ID
   */
  getUser(id: string): Observable<UserModel> {
    const url = `${this.baseUrl}/api/users/${id}`;
    return this.http.get<UserModel>(url);
  }

  /**
   * Create a new user
   */
  createUser(command: CreateUserCommand): Observable<void> {
    const url = `${this.baseUrl}/api/users`;
    return this.http.post<void>(url, command);
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

## Type Mappings

The generator automatically maps C# types to TypeScript:

| C# Type | TypeScript Type |
|---------|----------------|
| `string` | `string` |
| `int`, `long`, `Int32`, `Int64` | `number` |
| `double`, `float`, `decimal` | `number` |
| `bool` | `boolean` |
| `DateTime` | `Date \| string` |
| `Guid` | `string` |
| `List<T>` | `T[]` |
| `Task<T>` | `Observable<T>` |
| `Unit` (void) | `void` |

## Customization

### Configure Base URL

Edit the `baseUrl` in the generated service:

```typescript
private baseUrl = 'https://api.yourapp.com'; // Your API URL
```

Or inject via environment:

```typescript
import { environment } from '../environments/environment';

export class ApiService {
  private baseUrl = environment.apiUrl;
  // ...
}
```

### Fill in DTO Properties

The generator creates interface stubs. Fill in the actual properties:

```typescript
// Before (generated)
export interface UserModel {
  [key: string]: any;
}

// After (customized)
export interface UserModel {
  id: string;
  name: string;
  email: string;
  age: number;
  createdAt: Date;
}
```

### Add Error Handling

Wrap calls with error handling:

```typescript
this.api.getUser(id).subscribe({
  next: (user) => console.log(user),
  error: (err) => console.error('Error loading user:', err)
});
```

Or use RxJS operators:

```typescript
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';

this.api.getUser(id).pipe(
  catchError(err => {
    console.error(err);
    return of(null);
  })
).subscribe(user => {
  // Handle user or null
});
```

## Angular Setup

### 1. Import HttpClientModule

In your `app.module.ts`:

```typescript
import { HttpClientModule } from '@angular/common/http';

@NgModule({
  imports: [
    HttpClientModule,
    // ... other imports
  ]
})
export class AppModule { }
```

### 2. Provide ApiService

The service uses `providedIn: 'root'`, so it's automatically available. No need to add to providers.

### 3. Inject and Use

```typescript
constructor(private api: ApiService) {}
```

## Advanced Usage

### Interceptors

Add HTTP interceptors for authentication, logging, etc.:

```typescript
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler } from '@angular/common/http';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler) {
    const token = localStorage.getItem('token');
    if (token) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    }
    return next.handle(req);
  }
}
```

### Retry Logic

```typescript
import { retry } from 'rxjs/operators';

this.api.getUser(id).pipe(
  retry(3)
).subscribe(user => {
  // ...
});
```

### Loading States

```typescript
export class UsersComponent {
  loading = false;
  users: UserModel[] = [];

  loadUsers() {
    this.loading = true;
    this.api.listUsers(1, 10).subscribe({
      next: (users) => {
        this.users = users;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }
}
```

## Benefits

? **Time Saving** - No manual typing of API calls
? **Type Safety** - Compile-time checking of API calls  
? **IntelliSense** - Full autocomplete in your IDE
? **Consistency** - Service matches your backend exactly
? **Maintainability** - Regenerate when API changes
? **Documentation** - JSDoc comments from your controllers

## Workflow

1. **Develop Backend** - Add/modify controllers in C#
2. **Build Project** - Endpoints auto-generated
3. **Open Swagger UI** - Review your API
4. **Generate Service** - Click button, download file
5. **Update Angular** - Replace old service
6. **Customize DTOs** - Fill in property details
7. **Use in Components** - Type-safe API calls!

## Example: Complete Angular Component

```typescript
import { Component, OnInit } from '@angular/core';
import { ApiService, UserModel, CreateUserCommand } from '../services/api.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-users',
  template: `
    <div class="users-container">
      <h2>Users</h2>
      
      <form [formGroup]="userForm" (ngSubmit)="createUser()">
        <input formControlName="name" placeholder="Name">
        <input formControlName="email" placeholder="Email">
        <input formControlName="age" type="number" placeholder="Age">
        <button type="submit">Create User</button>
      </form>
      
      <div *ngFor="let user of users" class="user-card">
        <h3>{{ user.name }}</h3>
        <p>{{ user.email }}</p>
        <button (click)="deleteUser(user.id)">Delete</button>
      </div>
    </div>
  `
})
export class UsersComponent implements OnInit {
  users: UserModel[] = [];
  userForm: FormGroup;

  constructor(
    private api: ApiService,
    private fb: FormBuilder
  ) {
    this.userForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      age: [0, Validators.required]
    });
  }

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.api.listUsers(1, 100).subscribe(users => {
      this.users = users;
    });
  }

  createUser() {
    if (this.userForm.valid) {
      const command: CreateUserCommand = {
        userModel: this.userForm.value
      };
      
      this.api.createUser(command).subscribe(() => {
        this.userForm.reset();
        this.loadUsers();
      });
    }
  }

  deleteUser(id: string) {
    if (confirm('Delete this user?')) {
      this.api.deleteUser(id).subscribe(() => {
        this.loadUsers();
      });
    }
  }
}
```

## Tips

1. **Keep DTOs in Sync** - Regenerate service when backend changes
2. **Version Your Service** - Commit generated files to git
3. **Document Changes** - Add comments when customizing DTOs
4. **Use Interfaces** - Prefer interfaces over classes for DTOs
5. **Handle Errors** - Always add error handling to subscriptions
6. **Unsubscribe** - Use `takeUntil` or `async` pipe to prevent memory leaks

## Future Enhancements

Potential future additions:
- Generate DTOs from C# models automatically
- Include validation rules from C# attributes
- Generate mock data for testing
- Create NgRx effects/actions
- Support for file uploads/downloads

## License

Part of ZeroReflection - MIT License
