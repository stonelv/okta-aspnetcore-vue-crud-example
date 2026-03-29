# CRUD Application with ASP.NET Core and Vue.js

This example shows how to build a basic CRUD application with ASP.NET Core and Vue.js

Please read [Build a Simple CRUD App with ASP.NET Core and Vue](https://developer.okta.com/blog/2018/08/27/build-crud-app-vuejs-netcore) to see how this app was created.

## New Todo Feature

This project has been extended with a Todo management feature.

### Backend API

The Todo API provides the following endpoints:

- `GET /api/todos` - Get todos with pagination, filtering, and sorting
- `GET /api/todos/{id}` - Get a specific todo by ID
- `POST /api/todos` - Create a new todo
- `PUT /api/todos/{id}` - Update an existing todo
- `DELETE /api/todos/{id}` - Delete a todo

All endpoints require authentication and only return data for the currently authenticated user.

### Frontend

The Todo feature includes:
- A dedicated `/todos` route with a Todo management page
- List view with pagination, filtering (by completion status), and sorting
- Add/Edit modal dialog
- Toggle todo completion status
- Delete with confirmation dialog

## Prerequisites

- [.NET Core 2.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/2.1) or later
- [Node.js](https://nodejs.org/) (for Vue.js frontend)
- [Okta Developer Account](https://developer.okta.com/) (for authentication)

## Running the Application

### 1. Configure Okta

Update the Okta configuration in:
- `AspNetCore/appsettings.json` (backend)
- `Vue/src/config.js` (frontend)

### 2. Run Backend

```bash
cd AspNetCore
dotnet restore
dotnet run
```

The backend will be available at `https://localhost:5001`.

### 3. Run Frontend

```bash
cd Vue
npm install
npm run dev
```

The frontend will be available at `http://localhost:8080`.

### 4. Database Migration

The project uses SQLite with Entity Framework Core. The database will be created automatically on first run.

To create a new migration after making changes to the model:

```bash
cd AspNetCore
dotnet ef migrations add TodoMigration
dotnet ef database update
```

## Testing the Todo Feature

### Backend Tests

Run the backend tests:

```bash
cd AspNetCore.Tests
dotnet test
```

The tests cover:
- User data isolation (users can only access their own todos)
- Filtering by completion status
- Creating todos with correct user association
- Handling non-existent resources

### Frontend Tests

Run the frontend tests:

```bash
cd Vue
npm run unit
```

The tests cover:
- Component structure
- Default data initialization

### Manual Testing

1. Start both backend and frontend
2. Log in with your Okta credentials
3. Navigate to the "Todos" page
4. Test the following functionality:
   - Create a new todo
   - Edit an existing todo
   - Toggle todo completion status
   - Filter by "All", "Active", or "Completed"
   - Sort by different fields
   - Delete a todo (with confirmation)
   - Verify pagination works with multiple items

## Help

Please post any questions as comments on the [blog post](https://developer.okta.com/blog/2018/08/27/build-crud-app-vuejs-netcore), or visit our [Okta Developer Forums](https://devforum.okta.com/). You can also email developers@okta.com if you'd like to create a support ticket.

## License

Apache 2.0, see [LICENSE](LICENSE).
