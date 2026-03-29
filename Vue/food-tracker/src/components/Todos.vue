<template>
  <div class="container-fluid mt-4">
    <h1 class="h1">Todos</h1>
    <b-alert :show="loading" variant="info">Loading...</b-alert>
    <b-alert :show="error" variant="danger">{{ error }}</b-alert>

    <!-- 筛选和排序 -->
    <b-row class="mb-3">
      <b-col md="4">
        <b-form-group label="筛选状态">
          <b-form-select v-model="filter.isDone" @change="loadTodos">
            <option :value="null">全部</option>
            <option :value="false">未完成</option>
            <option :value="true">已完成</option>
          </b-form-select>
        </b-form-group>
      </b-col>
      <b-col md="4">
        <b-form-group label="排序方式">
          <b-form-select v-model="sortBy" @change="loadTodos">
            <option value="CreatedAt">创建时间</option>
            <option value="Title">标题</option>
            <option value="DueDate">截止日期</option>
            <option value="IsDone">完成状态</option>
          </b-form-select>
        </b-form-group>
      </b-col>
      <b-col md="4">
        <b-form-group label="排序方向">
          <b-form-select v-model="sortOrder" @change="loadTodos">
            <option value="desc">降序</option>
            <option value="asc">升序</option>
          </b-form-select>
        </b-form-group>
      </b-col>
    </b-row>

    <b-row>
      <b-col>
        <table class="table table-striped">
          <thead>
          <tr>
            <th style="width: 50px;">状态</th>
            <th>标题</th>
            <th>截止日期</th>
            <th>创建时间</th>
            <th style="width: 150px;">操作</th>
          </tr>
          </thead>
          <tbody>
          <tr v-for="todo in todos" :key="todo.id" :class="{ 'text-muted': todo.isDone }">
            <td>
              <b-form-checkbox v-model="todo.isDone" @change="toggleTodo(todo)" />
            </td>
            <td :class="{ 'text-decoration-line-through': todo.isDone }">{{ todo.title }}</td>
            <td>
              <span v-if="todo.dueDate">
                {{ formatDate(todo.dueDate) }}
                <span v-if="isOverdue(todo)" class="text-danger ml-1">(已过期)</span>
              </span>
              <span v-else class="text-muted">-</span>
            </td>
            <td>{{ formatDate(todo.createdAt) }}</td>
            <td class="text-right">
              <b-btn size="sm" variant="link" @click="editTodo(todo)">编辑</b-btn>
              <b-btn size="sm" variant="link" class="text-danger" @click="deleteTodo(todo)">删除</b-btn>
            </td>
          </tr>
          </tbody>
        </table>

        <!-- 分页 -->
        <b-pagination
          v-model="currentPage"
          :total-rows="totalCount"
          :per-page="pageSize"
          @change="loadTodos"
          class="justify-content-center"
        />
      </b-col>
      <b-col lg="3">
        <b-card :title="(editingTodo.id ? '编辑待办' : '新建待办')">
          <form @submit.prevent="saveTodo">
            <b-form-group label="标题">
              <b-form-input
                type="text"
                v-model="editingTodo.title"
                required
              ></b-form-input>
            </b-form-group>
            <b-form-group label="截止日期">
              <b-form-input
                type="date"
                v-model="editingTodo.dueDate"
              ></b-form-input>
            </b-form-group>
            <div>
              <b-btn type="submit" variant="primary" class="mr-2">保存</b-btn>
              <b-btn type="button" variant="secondary" @click="resetForm">取消</b-btn>
            </div>
          </form>
        </b-card>
      </b-col>
    </b-row>

    <!-- 删除确认模态框 -->
    <b-modal
      id="deleteModal"
      title="确认删除"
      @ok="confirmDelete"
      ok-variant="danger"
    >
      <p>确定要删除待办事项 "{{ deletingTodo && deletingTodo.title }}" 吗？</p>
    </b-modal>
  </div>
</template>

<script>
  import api from '@/TodosApiService';

  export default {
    data() {
      return {
        loading: false,
        error: null,
        todos: [],
        editingTodo: {},
        deletingTodo: null,
        filter: {
          isDone: null
        },
        sortBy: 'CreatedAt',
        sortOrder: 'desc',
        currentPage: 1,
        pageSize: 10,
        totalCount: 0
      };
    },
    async created() {
      await this.loadTodos();
    },
    methods: {
      async loadTodos() {
        this.loading = true;
        this.error = null;

        try {
          const params = {
            page: this.currentPage,
            pageSize: this.pageSize,
            sortBy: this.sortBy,
            sortOrder: this.sortOrder
          };

          if (this.filter.isDone !== null) {
            params.isDone = this.filter.isDone;
          }

          const response = await api.getAll(params);
          this.todos = response.items;
          this.totalCount = response.totalCount;
        } catch (err) {
          this.error = '加载待办事项失败，请稍后重试';
          console.error(err);
        } finally {
          this.loading = false;
        }
      },

      async toggleTodo(todo) {
        try {
          await api.update(todo.id, todo);
        } catch (err) {
          this.error = '更新待办事项失败';
          todo.isDone = !todo.isDone;
          console.error(err);
        }
      },

      editTodo(todo) {
        this.editingTodo = Object.assign({}, todo);
      },

      async saveTodo() {
        try {
          if (this.editingTodo.id) {
            await api.update(this.editingTodo.id, this.editingTodo);
          } else {
            await api.create(this.editingTodo);
          }
          this.resetForm();
          await this.loadTodos();
        } catch (err) {
          this.error = '保存待办事项失败';
          console.error(err);
        }
      },

      deleteTodo(todo) {
        this.deletingTodo = todo;
        this.$bvModal.show('deleteModal');
      },

      async confirmDelete() {
        try {
          await api.delete(this.deletingTodo.id);
          await this.loadTodos();
        } catch (err) {
          this.error = '删除待办事项失败';
          console.error(err);
        } finally {
          this.deletingTodo = null;
        }
      },

      resetForm() {
        this.editingTodo = {};
      },

      formatDate(dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString();
      },

      isOverdue(todo) {
        if (!todo.dueDate || todo.isDone) return false;
        return new Date(todo.dueDate) < new Date();
      }
    }
  };
</script>
