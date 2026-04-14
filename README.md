# CRUD Application with ASP.NET Core and Vue.js

This example shows how to build a basic CRUD application with ASP.NET Core and Vue.js

Please read [Build a Simple CRUD App with ASP.NET Core and Vue](https://developer.okta.com/blog/2018/08/27/build-crud-app-vuejs-netcore) to see how this app was created.

## Features

- **Food Records**: Basic CRUD operations for tracking food records
- **Todos**: Advanced todo management with pagination, filtering, sorting, and user isolation

## Prerequisites

- .NET Core 2.1 SDK
- Node.js (>= 4.0.0)
- npm (>= 3.0.0)
- Okta Developer Account (for authentication)

## Running the Application

### Backend (ASP.NET Core)

1. Navigate to the AspNetCore directory:
```bash
cd AspNetCore
```

2. Run the backend server:
```bash
dotnet run
```

The backend will start on `http://localhost:5000`

### Frontend (Vue.js)

1. Navigate to the Vue/food-tracker directory:
```bash
cd Vue/food-tracker
```

2. Install dependencies:
```bash
npm install
```

3. Run the development server:
```bash
npm run dev
```

The frontend will start on `http://localhost:8080`

## Todo Feature - API Endpoints

All Todo API endpoints require authentication (JWT token). Users can only access their own todos.

### GET /api/todos
Get paginated list of todos with filtering and sorting.

**Query Parameters:**
- `page` (default: 1) - Page number
- `pageSize` (default: 10) - Items per page
- `isDone` (optional) - Filter by completion status (true/false)
- `sortBy` (default: CreatedAt) - Sort field (Title, DueDate, IsDone, CreatedAt)
- `sortDesc` (default: true) - Sort direction

**Response:**
```json
{
  "items": [...],
  "totalCount": 100,
  "page": 1,
  "pageSize": 10,
  "totalPages": 10
}
```

### GET /api/todos/{id}
Get a single todo by ID.

### POST /api/todos
Create a new todo.

**Request Body:**
```json
{
  "title": "Buy groceries",
  "isDone": false,
  "dueDate": "2026-04-15T00:00:00Z"
}
```

### PUT /api/todos/{id}
Update an existing todo.

### DELETE /api/todos/{id}
Delete a todo.

## Todo Feature - Frontend

Access the Todo page at `http://localhost:8080/todos` (requires login).

**Features:**
- View todos in a table with pagination
- Filter by completion status (All/Completed/Incomplete)
- Sort by clicking column headers
- Create new todos via modal dialog
- Edit existing todos
- Toggle completion status with checkbox
- Delete todos with confirmation dialog
- Overdue todos highlighted in red

## Database

The application uses SQLite with Entity Framework Core. The database will be automatically created on first run.

**Tables:**
- `FoodRecords` - Existing food tracking data
- `Todos` - New todo management data with UserId for isolation

## Authentication

Authentication is handled by Okta. Users must log in to access:
- `/food-records`
- `/todos`

Each user's data is isolated - users can only see and modify their own todos.

## Verification Steps

1. **Start Backend:**
   ```bash
   cd AspNetCore
   dotnet run
   ```
   Verify: `http://localhost:5000` is running

2. **Start Frontend:**
   ```bash
   cd Vue/food-tracker
   npm install
   npm run dev
   ```
   Verify: `http://localhost:8080` is running

3. **Test Todo Feature:**
   - Click "Login" and authenticate with Okta
   - Navigate to "Todos" in the navigation bar
   - Click "New Todo" to create a todo
   - Verify the todo appears in the list
   - Toggle the checkbox to mark as complete
   - Use the filter dropdown to filter by status
   - Click column headers to sort
   - Edit a todo using the "Edit" link
   - Delete a todo using the "Delete" link (confirm in dialog)
   - Verify pagination works with multiple todos

## Help

Please post any questions as comments on the [blog post](https://developer.okta.com/blog/2018/08/27/build-crud-app-vuejs-netcore), or visit our [Okta Developer Forums](https://devforum.okta.com/). You can also email developers@okta.com if you'd like to create a support ticket.

## License

Apache 2.0, see [LICENSE](LICENSE).
