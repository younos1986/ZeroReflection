# Per-Controller TypeScript Generation

## Overview

The Swagger UI now includes **per-controller TypeScript generation**! Each controller group has its own "View TypeScript" button that generates a TypeScript service containing **only that controller's endpoints and models**.

## What's New

### ? Controller-Specific TypeScript Services

Each controller group now has a **"?? View TypeScript"** button that:
1. Generates a TypeScript service for **that controller only**
2. Includes **all models** used by that controller's endpoints (input and output)
3. Names the service class after the controller (e.g., `UsersControllerService`)
4. Downloads with a controller-specific filename (e.g., `userscontroller.service.ts`)

### Visual Changes

#### Before (No TypeScript Generation)
```
???????????????????????????????????????????????????????????????
? ?? ZeroReflection API Explorer                             ?
? [View Raw JSON]                                             ?
???????????????????????????????????????????????????????????????

???????????????????????????????????????????????????????????????
? UsersController               [5 endpoints]                 ?
?   (click to expand)                                         ?
???????????????????????????????????????????????????????????????
```

#### After (Per-Controller TypeScript Buttons)
```
???????????????????????????????????????????????????????????????
? ?? ZeroReflection API Explorer                             ?
? [View Raw JSON]                                             ?
???????????????????????????????????????????????????????????????

???????????????????????????????????????????????????????????????
? UsersController   [5 endpoints]    [?? View TypeScript] ?  ?
?   (click left to expand)                                    ?
???????????????????????????????????????????????????????????????

???????????????????????????????????????????????????????????????
? ProductController [1 endpoint]     [?? View TypeScript] ?  ?
?   (click left to expand)                                    ?
???????????????????????????????????????????????????????????????
```

## Usage Examples

### Example 1: Generate TypeScript for UsersController

**Step 1:** Click "?? View TypeScript" on UsersController

**Step 2:** Modal opens with generated TypeScript:

```typescript
/* Auto-generated TypeScript service from ZeroReflection API */
/* Generated at: 2025-01-15T10:30:00.000Z */
/* Controller: UsersController */

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

// ==================== DTOs ====================

/** UserModel model */
export interface UserModel {
  age: number;
  email: string;
  name: string;
}

/** CreateUserCommand model */
export interface CreateUserCommand {
  userModel: UserModel;
}

/** UpdateUserCommand model */
export interface UpdateUserCommand {
  id: string;
  userModel: UserModel;
}

/** DeleteUserCommand model */
export interface DeleteUserCommand {
  id: string;
}

// ==================== Service ====================

@Injectable({
  providedIn: 'root'
})
export class UsersControllerService {
  private baseUrl = '/api';

  constructor(private http: HttpClient) {}

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

**Step 3:** Click "?? Download File"
- Downloads as: `userscontroller.service.ts`

### Example 2: Generate TypeScript for ProductController

**Click** "?? View TypeScript" on ProductController

**Result:**

```typescript
/* Auto-generated TypeScript service from ZeroReflection API */
/* Generated at: 2025-01-15T10:30:00.000Z */
/* Controller: ProductController */

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

// ==================== DTOs ====================

/** ProductModel model */
export interface ProductModel {
  age: number;
  email: string;
  name: string;
}

/** CreateProductCommand model */
export interface CreateProductCommand {
  productModel: ProductModel;
}

// ==================== Service ====================

@Injectable({
  providedIn: 'root'
})
export class ProductControllerService {
  private baseUrl = '/api';

  constructor(private http: HttpClient) {}

  /**
   * Create a new product
   */
  createProduct(command: CreateProductCommand): Observable<void> {
    const url = `${this.baseUrl}/api/products`;
    return this.http.post<void>(url, command);
  }
}
```

**Download:** `productcontroller.service.ts`

## Key Features

### ? Scoped Models
Each controller service includes **only the models used by that controller**:
- **Input models** - Commands/queries from `[FromBody]` parameters
- **Output models** - Response types from endpoint returns
- **Nested models** - Models referenced by input/output models

### ? Controller-Specific Service Class
```typescript
// UsersController ? UsersControllerService
export class UsersControllerService { }

// ProductController ? ProductControllerService
export class ProductControllerService { }
```

### ? Smart File Naming
Downloads use controller-specific filenames:
- `userscontroller.service.ts`
- `productcontroller.service.ts`

### ? Modal Title Shows Context
```
?? TypeScript Service - UsersController
?? TypeScript Service - ProductController
```

## Benefits

### 1. **Modular Services**
Generate separate service files for each controller.

### 2. **Cleaner Organization**
```
src/
  services/
    userscontroller.service.ts    ? Only user-related
    productcontroller.service.ts  ? Only product-related
```

### 3. **Smaller Files**
Each service file contains only what's needed for that controller.

### 4. **Better Team Workflow**
Different developers can work on different controllers without conflicts.

### 5. **Selective Import**
Import only the services you need:
```typescript
import { UsersControllerService } from './services/userscontroller.service';
// Don't need ProductController? Don't import it!
```

## Model Inclusion Logic

The generator extracts models from:

### Input Models (Request)
- ? `[FromBody]` parameters
  - Example: `CreateUserCommand`, `UpdateUserCommand`
- ? Complex types in route/query parameters (rare)

### Output Models (Response)
- ? Return types from controller methods
  - Example: `UserModel`, `List<UserModel>`
- ? Types wrapped in `Task<T>` or `List<T>`

### Nested Models
- ? Models referenced by input/output models
  - Example: If `CreateUserCommand` has a `UserModel` property, both are included

### Excluded
- ? Primitive types (`string`, `int`, etc.) - mapped inline
- ? System types (`DateTime` ? `string`, `Guid` ? `string`)
- ? Framework types (`CancellationToken`, `Unit`)

## UI Changes

### Controller Header Layout

```
???????????????????????????????????????????????????????????????
? ??????????????????????????????  ??????????????????????????? ?
? ? UsersController            ?  ? [?? View TypeScript]   ? ?
? ? [5 endpoints]              ?  ?                         ? ?
? ??????????????????????????????  ??????????????????????????? ?
?    ? Clickable to expand            ? Button (isolated)     ?
???????????????????????????????????????????????????????????????
```

### Interaction
- **Click controller name/count** ? Expands/collapses endpoints
- **Click "View TypeScript" button** ? Opens modal with TypeScript
- **Button stops propagation** ? Doesn't toggle expansion

## Technical Implementation

### Client-Side Generation
TypeScript is generated **in the browser** using JavaScript:

```javascript
async function generateControllerTypeScript(controllerName, controllerEndpoints) {
  // 1. Extract unique types from controller's endpoints
  // 2. Generate model interfaces
  // 3. Generate service class with methods
  // 4. Return complete TypeScript string
}
```

### Type Extraction
```javascript
// From each endpoint:
- endpoint.ResponseType ? Output model
- endpoint.Parameters[].Type ? Input models
- Filters out system types
- Deduplicates model names
```

### Service Generation
```javascript
// For each endpoint:
- Generate method signature
- Map parameters (route ? required, query ? optional, body ? typed)
- Generate HTTP call (GET/POST/PUT/DELETE)
- Include JSDoc comments
```

## Comparison

### Per-Controller TypeScript Service (Only Option)
```typescript
// userscontroller.service.ts
export class UsersControllerService {
  // Only user endpoints
  createUser(...) { }
  getUser(...) { }
  // Only user-related models
}

// productcontroller.service.ts
export class ProductControllerService {
  // Only product endpoints
  createProduct(...) { }
  // Only product-related models
}
```

Each controller gets its own focused, modular TypeScript service file.

## Developer Workflow

### Generate TypeScript for Specific Controller
```
1. Find the controller you need (e.g., UsersController)
2. Click "View TypeScript" button on that controller
3. Get focused service with only user endpoints
4. Copy or download as userscontroller.service.ts
5. Use in your Angular app
```

### Generate Multiple Controllers
```
1. Click "View TypeScript" on UsersController
2. Download userscontroller.service.ts
3. Click "View TypeScript" on ProductController
4. Download productcontroller.service.ts
5. Now you have separate, focused service files
```

## Best Practices

### When to Use Per-Controller Generation
? **Large APIs** - Many controllers/endpoints
? **Team Development** - Different devs work on different areas
? **Modular Architecture** - Separate concerns
? **Focused Services** - Each controller gets its own TypeScript service

This approach keeps your frontend services as modular as your backend controllers.

## Future Enhancements

Possible improvements:
- [ ] Actual type introspection (read C# properties)
- [ ] Generate model properties instead of `any`
- [ ] Add syntax highlighting to modal
- [ ] Generate multiple files as a ZIP download
- [ ] Export to different frameworks (React, Vue, etc.)

---

## Summary

Per-controller TypeScript generation provides:
1. ? **Focused services** - One file per controller
2. ? **Scoped models** - Only models used by that controller
3. ? **Better organization** - Modular service architecture
4. ? **Flexible workflow** - Choose global or per-controller
5. ? **Smart naming** - Controller-specific filenames

**Your TypeScript services are now as modular as your C# controllers!** ??
