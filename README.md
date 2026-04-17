# CRUD Application with ASP.NET Core and Vue.js

This example shows how to build a basic CRUD application with ASP.NET Core and Vue.js, including Food Records and Todos features.

Please read [Build a Simple CRUD App with ASP.NET Core and Vue](https://developer.okta.com/blog/2018/08/27/build-crud-app-vuejs-netcore) to see how this app was created.

## Features

### Food Records (Original)
- Basic CRUD operations for food tracking
- Okta authentication

### Todos (New Feature)
- Complete Todo management system
- User-specific data isolation (each user only sees their own todos)
- Pagination support
- Filtering by completion status (All / Pending / Completed)
- Sorting by multiple fields (Created Date, Title, Due Date, Status)
- Modal dialogs for create/edit operations
- Delete confirmation
- Due date overdue indicator (red text for past due items)

## Tech Stack

- **Backend**: ASP.NET Core 2.1, Entity Framework Core, SQLite
- **Frontend**: Vue 2.5, Vue Router, Bootstrap-Vue, Axios
- **Authentication**: Okta OAuth 2.0 / OpenID Connect

## Project Structure

```
.
├── AspNetCore/                 # Backend ASP.NET Core
│   ├── Controllers/
│   │   ├── FoodRecordsController.cs   # Original food records API
│   │   └── TodosController.cs         # New todos API
│   ├── Todo.cs                        # Todo entity
│   ├── PagedResult.cs                 # Pagination response DTO
│   ├── FoodRecord.cs
│   ├── ApplicationDbContext.cs
│   ├── Startup.cs
│   ├── Program.cs
│   └── AspNetCore.csproj
├── AspNetCore.Tests/           # Backend tests
│   ├── TodosControllerTests.cs
│   └── AspNetCore.Tests.csproj
└── Vue/food-tracker/          # Frontend Vue app
    ├── src/
    │   ├── components/
    │   │   ├── FoodRecords.vue
    │   │   └── Todos.vue              # New todos page
    │   ├── router/
    │   │   └── index.js
    │   ├── __tests__/
    │   │   └── TodosApiService.spec.js # API service tests
    │   ├── FoodRecordsApiService.js
    │   ├── TodosApiService.js         # New todos API service
    │   ├── App.vue
    │   └── main.js
    └── package.json
```

## API Endpoints

### Todos API (`/api/todos`)

| Method | Endpoint | Description | Query Parameters |
|--------|----------|-------------|------------------|
| GET | `/api/todos` | Get paginated todos | `page`, `pageSize`, `isDone`, `sortBy`, `sortDesc` |
| GET | `/api/todos/{id}` | Get single todo by ID | - |
| POST | `/api/todos` | Create new todo | - |
| PUT | `/api/todos/{id}` | Update existing todo | - |
| DELETE | `/api/todos/{id}` | Delete todo | - |

**Query Parameters for GET /api/todos:**
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10)
- `isDone`: Filter by completion status (optional: true/false)
- `sortBy`: Sort field (CreatedAt, Title, DueDate, IsDone)
- `sortDesc`: Sort descending (true/false, default: true)

**Response Format (Paginated):**
```json
{
  "items": [...],
  "totalCount": 100,
  "page": 1,
  "pageSize": 10,
  "totalPages": 10
}
```

## Prerequisites

- .NET Core SDK 2.1 or later
- Node.js 8.0 or later
- Okta Developer Account (for authentication)

## Running the Application

### 1. Backend Setup

```bash
cd AspNetCore

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the backend (http://localhost:5000)
dotnet run
```

The backend will automatically create the SQLite database on first run using `Database.EnsureCreated()`.

### 2. Frontend Setup

```bash
cd Vue/food-tracker

# Install dependencies
npm install

# Run development server (http://localhost:8080)
npm run dev
```

### 3. Access the Application

1. Open your browser and navigate to `http://localhost:8080`
2. Click "Login" to authenticate with Okta
3. After login, you can access:
   - **Food Records**: `/food-records`
   - **Todos**: `/todos`

## Using the Todos Feature

### Viewing Todos
1. Click "Todos" in the navigation bar
2. Use the **Status Filter** to show All / Pending / Completed todos
3. Use the **Sort** dropdown and direction button to sort by:
   - Created Date
   - Title
   - Due Date
   - Status

### Creating a Todo
1. Click the "New Todo" button
2. Enter a title (required)
3. Optionally set a due date
4. Click "OK" to save

### Editing a Todo
1. Click the "Edit" button on any todo
2. Modify the title, status, or due date
3. Click "OK" to save changes

### Toggling Completion
- Click the checkbox in the "Status" column to quickly mark as complete/incomplete

### Deleting a Todo
1. Click the "Delete" button
2. Confirm deletion in the modal dialog

### Pagination
- Use the pagination controls at the bottom of the list to navigate between pages

## Running Tests

### Backend Tests

```bash
cd AspNetCore.Tests

# Build and run tests
dotnet test
```

**Note**: The test project targets .NET Core 2.1, which may have compatibility issues on modern systems. The tests verify:
- User isolation (users can only access their own todos)
- Filtering by completion status
- Creating todos with correct UserId assignment
- Security: accessing other users' todos returns 404

### Frontend Tests

```bash
cd Vue/food-tracker

# Run Jest tests
npm test
```

Tests cover the `TodosApiService.js` methods:
- Authentication header injection
- GET list with query parameters
- GET by ID
- POST/PUT/DELETE operations

## Configuration

### Backend Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Database.db"
  },
  "Okta": {
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "Authority": "https://dev-xxxxxx.oktapreview.com/oauth2/default"
  }
}
```

### Frontend Okta Configuration (`src/router/index.js`)

```javascript
Vue.use(Auth, {
  issuer: 'https://dev-xxxxxx.oktapreview.com/oauth2/default',
  client_id: 'your-client-id',
  redirect_uri: 'http://localhost:8080/implicit/callback',
  scope: 'openid profile email'
})
```

## Data Model

### Todo Entity

| Property | Type | Description |
|----------|------|-------------|
| Id | string | Unique identifier (GUID) |
| Title | string | Todo title (required) |
| IsDone | bool | Completion status |
| DueDate | DateTime? | Optional due date |
| CreatedAt | DateTime | Creation timestamp (UTC) |
| UserId | string | Owning user's Okta ID |

**User Isolation**: All API queries automatically filter by `UserId`, ensuring users only access their own data.

## Help

Please post any questions as comments on the [blog post](https://developer.okta.com/blog/2018/08/27/build-crud-app-vuejs-netcore), or visit our [Okta Developer Forums](https://devforum.okta.com/). You can also email developers@okta.com if you'd like to create a support ticket.

## License

Apache 2.0, see [LICENSE](LICENSE).
