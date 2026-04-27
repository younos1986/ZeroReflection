# React TypeScript API Service Generator

## Overview

The Swagger UI now includes **React TypeScript service generation**! Each controller has both **Angular** and **React** buttons that generate framework-specific API clients.

## What's New

### ? Dual Framework Support

Each controller group now has TWO buttons:
- **?? Angular** - Generates Angular service with HttpClient and Observables
- **?? React** - Generates React API client with fetch and hooks

## UI Layout

### Controller Actions
```
?????????????????????????????????????????????????????
? UsersController  [5 endpoints]  [?? Angular] [?? React] ?
?????????????????????????????????????????????????????
```

## React Service Features

### ? Modern Fetch API
Uses native `fetch` instead of external dependencies

### ? Promise-Based
Returns `Promise<T>` instead of Observables

### ? React Hooks
Generates custom hooks for each endpoint with state management

### ? TypeScript Support
Full TypeScript types for all models and responses

### ? Error Handling
Built-in error handling and loading states

## Generated React Service Structure

### 1. **Types Section**
```typescript
// ==================== Types ====================

export interface UserModel {
  age: number;
  email: string;
  name: string;
}

export interface CreateUserCommand {
  userModel: UserModel;
}
```

### 2. **API Client Class**
```typescript
// ==================== API Client ====================

export class UsersControllerApi {
  private baseUrl: string = '/api';

  constructor(baseUrl?: string) {
    if (baseUrl) this.baseUrl = baseUrl;
  }

  /**
   * Create a new user
   */
  async createUser(command: CreateUserCommand): Promise<void> {
    let url = `${this.baseUrl}/api/users`;

    const response = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(command),
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return;
  }

  /**
   * Get user by ID
   */
  async getUser(id: string): Promise<UserModel> {
    let url = `${this.baseUrl}/api/users/${id}`;

    const response = await fetch(url, {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return await response.json();
  }

  /**
   * List all users
   */
  async listUsers(page?: number, pageSize?: number): Promise<UserModel[]> {
    let url = `${this.baseUrl}/api/users`;
    const params = new URLSearchParams();
    if (page !== undefined) {
      params.append('page', page.toString());
    }
    if (pageSize !== undefined) {
      params.append('pageSize', pageSize.toString());
    }
    const queryString = params.toString();
    if (queryString) url += `?${queryString}`;

    const response = await fetch(url, {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return await response.json();
  }
}
```

### 3. **React Hooks**
```typescript
// ==================== React Hooks ====================

const userscontrollerApi = new UsersControllerApi();

/**
 * Create a new user
 */
export function useCreateUser() {
  const [data, setData] = React.useState<void | null>(null);
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState<Error | null>(null);

  const execute = async (command: CreateUserCommand) => {
    try {
      setLoading(true);
      setError(null);
      const result = await userscontrollerApi.createUser(command);
      setData(result);
      return result;
    } catch (err) {
      const error = err instanceof Error ? err : new Error(String(err));
      setError(error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  return { data, loading, error, execute };
}

/**
 * Get user by ID
 */
export function useGetUser() {
  const [data, setData] = React.useState<UserModel | null>(null);
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState<Error | null>(null);

  const execute = async (id: string) => {
    try {
      setLoading(true);
      setError(null);
      const result = await userscontrollerApi.getUser(id);
      setData(result);
      return result;
    } catch (err) {
      const error = err instanceof Error ? err : new Error(String(err));
      setError(error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  return { data, loading, error, execute };
}

/**
 * List all users
 */
export function useListUsers() {
  const [data, setData] = React.useState<UserModel[] | null>(null);
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState<Error | null>(null);

  const execute = async (page?: number, pageSize?: number) => {
    try {
      setLoading(true);
      setError(null);
      const result = await userscontrollerApi.listUsers(page, pageSize);
      setData(result);
      return result;
    } catch (err) {
      const error = err instanceof Error ? err : new Error(String(err));
      setError(error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  return { data, loading, error, execute };
}
```

## Usage in React

### Method 1: Using React Hooks (Recommended)

```typescript
import React from 'react';
import { useGetUser, useListUsers, useCreateUser, UserModel, CreateUserCommand } from './userscontroller.api';

export function UsersComponent() {
  const { data: users, loading: listLoading, error: listError, execute: fetchUsers } = useListUsers();
  const { loading: createLoading, error: createError, execute: createUser } = useCreateUser();

  React.useEffect(() => {
    // Load users on mount
    fetchUsers(1, 10);
  }, []);

  const handleCreateUser = async () => {
    const command: CreateUserCommand = {
      userModel: {
        email: 'john@example.com',
        age: 30,
        name: 'John Doe'
      }
    };

    try {
      await createUser(command);
      // Refresh list
      fetchUsers(1, 10);
    } catch (err) {
      console.error('Failed to create user:', err);
    }
  };

  if (listLoading) return <div>Loading...</div>;
  if (listError) return <div>Error: {listError.message}</div>;

  return (
    <div>
      <h1>Users</h1>
      <button onClick={handleCreateUser} disabled={createLoading}>
        {createLoading ? 'Creating...' : 'Create User'}
      </button>
      <ul>
        {users?.map(user => (
          <li key={user.email}>
            {user.name} - {user.email} ({user.age})
          </li>
        ))}
      </ul>
    </div>
  );
}
```

### Method 2: Using API Client Directly

```typescript
import { UsersControllerApi, CreateUserCommand } from './userscontroller.api';

const usersApi = new UsersControllerApi();

export async function loadUsers() {
  try {
    const users = await usersApi.listUsers(1, 10);
    console.log('Users:', users);
    return users;
  } catch (error) {
    console.error('Failed to load users:', error);
    throw error;
  }
}

export async function createNewUser() {
  const command: CreateUserCommand = {
    userModel: {
      email: 'jane@example.com',
      age: 25,
      name: 'Jane Smith'
    }
  };

  try {
    await usersApi.createUser(command);
    console.log('User created successfully');
  } catch (error) {
    console.error('Failed to create user:', error);
    throw error;
  }
}
```

## Key Features

### ? No External Dependencies
Uses native `fetch` API - no need for axios or other HTTP libraries

### ? Built-in State Management
React hooks include:
- `data` - The response data
- `loading` - Loading state
- `error` - Error state
- `execute` - Function to trigger the API call

### ? Type Safety
Full TypeScript support:
```typescript
// Autocomplete for all properties
const user: UserModel = {
  email: 'test@example.com', // ? string
  age: 30,                    // ? number
  name: 'Test User'           // ? string
};
```

### ? Error Handling
```typescript
const { error, execute } = useCreateUser();

try {
  await execute(command);
} catch (err) {
  // Error is automatically set in hook state
  console.error(error);
}
```

### ? Loading States
```typescript
const { loading, execute } = useListUsers();

// Show loading UI
if (loading) {
  return <Spinner />;
}
```

## File Naming

### Angular Service
Downloads as: `userscontroller.service.ts`

### React API
Downloads as: `userscontroller.api.ts`

## Comparison: Angular vs React

### Angular Service
```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UsersControllerService {
  constructor(private http: HttpClient) {}
  
  createUser(command: CreateUserCommand): Observable<void> {
    return this.http.post<void>(url, command);
  }
}
```

**Uses:**
- HttpClient (Angular dependency)
- Observables (RxJS)
- Injectable decorator

### React API
```typescript
export class UsersControllerApi {
  async createUser(command: CreateUserCommand): Promise<void> {
    const response = await fetch(url, {
      method: 'POST',
      body: JSON.stringify(command),
    });
    return await response.json();
  }
}

export function useCreateUser() {
  const [loading, setLoading] = useState(false);
  const execute = async (command: CreateUserCommand) => {
    setLoading(true);
    await userscontrollerApi.createUser(command);
    setLoading(false);
  };
  return { loading, execute };
}
```

**Uses:**
- Native fetch API
- Promises
- React hooks

## Benefits

### 1. **Framework Choice**
Choose the right service for your project:
- **Angular** ? Angular service with HttpClient
- **React** ? React API with fetch and hooks

### 2. **No Setup Required**
React services work out-of-the-box:
- No axios configuration
- No HTTP library setup
- Just import and use

### 3. **Modern React Patterns**
- Custom hooks for state management
- Async/await syntax
- TypeScript support

### 4. **Consistent APIs**
Both frameworks generate the same models and endpoints, just with framework-specific implementations.

## Example: Full React Component

```typescript
import React from 'react';
import { 
  useListUsers, 
  useCreateUser, 
  useUpdateUser, 
  useDeleteUser,
  UserModel,
  CreateUserCommand 
} from './userscontroller.api';

export function UserManagement() {
  const { data: users, loading, error, execute: loadUsers } = useListUsers();
  const { execute: createUser } = useCreateUser();
  const { execute: updateUser } = useUpdateUser();
  const { execute: deleteUser } = useDeleteUser();

  const [formData, setFormData] = React.useState({
    name: '',
    email: '',
    age: 0
  });

  React.useEffect(() => {
    loadUsers(1, 100);
  }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    const command: CreateUserCommand = {
      userModel: formData
    };
    await createUser(command);
    loadUsers(1, 100);
    setFormData({ name: '', email: '', age: 0 });
  };

  const handleDelete = async (id: string) => {
    await deleteUser(id);
    loadUsers(1, 100);
  };

  if (loading) return <div className="loading">Loading users...</div>;
  if (error) return <div className="error">Error: {error.message}</div>;

  return (
    <div className="user-management">
      <h1>User Management</h1>
      
      <form onSubmit={handleCreate}>
        <input 
          value={formData.name}
          onChange={e => setFormData({...formData, name: e.target.value})}
          placeholder="Name"
        />
        <input 
          value={formData.email}
          onChange={e => setFormData({...formData, email: e.target.value})}
          placeholder="Email"
        />
        <input 
          type="number"
          value={formData.age}
          onChange={e => setFormData({...formData, age: Number(e.target.value)})}
          placeholder="Age"
        />
        <button type="submit">Create User</button>
      </form>

      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Age</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {users?.map(user => (
            <tr key={user.email}>
              <td>{user.name}</td>
              <td>{user.email}</td>
              <td>{user.age}</td>
              <td>
                <button onClick={() => handleDelete(user.email)}>Delete</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
```

## Summary

The React TypeScript generator provides:
1. ? **Modern fetch-based API client** - No external dependencies
2. ? **Custom React hooks** - Built-in state management
3. ? **Full TypeScript support** - Type-safe models and methods
4. ? **Error handling** - Automatic error state management
5. ? **Loading states** - Built-in loading indicators
6. ? **Promise-based** - Clean async/await syntax

**Now you can generate API services for both Angular AND React projects!** ????
