# CRUD Application with ASP.NET Core and Vue.js

This example shows how to build a basic CRUD application with ASP.NET Core and Vue.js, including food tracking and todo management features.

## Features

- **Food Tracking**: CRUD operations for food records
- **Todo Management**: Complete todo list management with pagination, filtering, and sorting
- **Okta Authentication**: Secure user authentication with Okta
- **User Isolation**: Each user can only access their own data

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download)
- [Node.js and npm](https://nodejs.org/)
- [Okta Developer Account](https://developer.okta.com/)

## Running the Application

### 1. Configure Okta

1. Sign up for a free [Okta Developer Account](https://developer.okta.com/)
2. Create a new SPA application in Okta
3. Update the configuration files:
   - Backend: `AspNetCore/appsettings.json` (update Okta settings)
   - Frontend: `Vue/food-tracker/src/config.js` (update Okta client ID)

### 2. Run the Backend API

```bash
cd AspNetCore
dotnet restore
dotnet run
```

The API will be available at `https://localhost:5001`

### 3. Run the Frontend Application

```bash
cd Vue/food-tracker
npm install
npm run dev
```

The frontend will be available at `http://localhost:8080`

## Todo Feature Verification

### Access the Todo Page
1. Log in to the application
2. Click on "Todos" in the navigation menu
3. You will see the todo management interface

### Key Features to Test

#### 1. Create a Todo
- Click the "新增任务" (Add Task) button
- Enter a title and optional due date
- Click "保存" (Save)
- The new todo will appear in the list

#### 2. View and Filter Todos
- The list shows all your todos with pagination (10 items per page)
- Use the "筛选状态" (Filter Status) dropdown to filter by:
  - 全部 (All)
  - 未完成 (Incomplete)
  - 已完成 (Completed)

#### 3. Sort Todos
- Click on column headers to sort:
  - 标题 (Title)
  - 状态 (Status)
  - 截止日期 (Due Date)
  - 创建时间 (Created At)

#### 4. Edit a Todo
- Click the "编辑" (Edit) button on any todo
- Modify the title, due date, or status
- Click "保存" (Save) to update

#### 5. Mark as Complete
- Click the checkbox in the "已完成" (Completed) column to toggle completion status

#### 6. Delete a Todo
- Click the "删除" (Delete) button
- Confirm the deletion in the modal dialog
- The todo will be permanently removed

## API Endpoints

### Todo API

- `GET /api/todos` - Get todos with pagination, filtering, and sorting
  - Query parameters: `page`, `pageSize`, `isDone`, `sortBy`, `sortOrder`
- `GET /api/todos/{id}` - Get a specific todo
- `POST /api/todos` - Create a new todo
- `PUT /api/todos/{id}` - Update an existing todo
- `DELETE /api/todos/{id}` - Delete a todo

## Testing

### Run Backend Tests

```bash
cd AspNetCore.Tests
dotnet test
```

### Run Frontend Tests

```bash
cd Vue/food-tracker
npm run test
```

## Database

The application uses SQLite database by default. The database file (`foodtracker.db`) will be created automatically when the application starts.

To apply migrations:
```bash
cd AspNetCore
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Help

Please post any questions as comments on the [blog post](https://developer.okta.com/blog/2018/08/27/build-crud-app-vuejs-netcore), or visit our [Okta Developer Forums](https://devforum.okta.com/). You can also email developers@okta.com if you'd like to create a support ticket.

## License

Apache 2.0, see [LICENSE](LICENSE).
