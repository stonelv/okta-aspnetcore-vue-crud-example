<template>
  <div class="container-fluid mt-4">
    <h1 class="h1">Todo List</h1>
    <b-alert :show="loading" variant="info">Loading...</b-alert>
    
    <b-row class="mb-3">
      <b-col>
        <b-button variant="primary" @click="showCreateModal">+ New Todo</b-button>
      </b-col>
      <b-col lg="4">
        <b-form-group label="Filter by Status" label-for="filter-status" label-cols-sm="4" label-size="sm">
          <b-form-select id="filter-status" v-model="filter.isDone" @change="loadTodos" size="sm">
            <option :value="null">All</option>
            <option :value="false">Pending</option>
            <option :value="true">Completed</option>
          </b-form-select>
        </b-form-group>
      </b-col>
      <b-col lg="4">
        <b-form-group label="Sort by" label-for="sort-by" label-cols-sm="3" label-size="sm">
          <b-row>
            <b-col>
              <b-form-select id="sort-by" v-model="sort.by" @change="loadTodos" size="sm">
                <option value="CreatedAt">Created Date</option>
                <option value="Title">Title</option>
                <option value="DueDate">Due Date</option>
                <option value="IsDone">Status</option>
              </b-form-select>
            </b-col>
            <b-col sm="3">
              <b-button variant="outline-secondary" size="sm" @click="toggleSortDirection">
                {{ sort.desc ? '↓' : '↑' }}
              </b-button>
            </b-col>
          </b-row>
        </b-form-group>
      </b-col>
    </b-row>

    <b-row>
      <b-col>
        <b-table 
          :items="todos" 
          :fields="fields" 
          :busy="loading"
          @row-clicked="onRowClicked"
          hover
          striped
        >
          <template v-slot:cell(isDone)="row">
            <b-form-checkbox 
              v-model="row.item.isDone" 
              @change="toggleTodoStatus(row.item)"
            >
              {{ row.item.isDone ? 'Completed' : 'Pending' }}
            </b-form-checkbox>
          </template>
          <template v-slot:cell(dueDate)="row">
            {{ formatDate(row.item.dueDate) }}
          </template>
          <template v-slot:cell(actions)="row">
            <b-button size="sm" variant="info" @click.stop="editTodo(row.item)">Edit</b-button>
            <b-button size="sm" variant="danger" @click.stop="deleteTodo(row.item)" class="ml-1">Delete</b-button>
          </template>
        </b-table>

        <b-pagination 
          v-model="pagination.page" 
          :total-rows="pagination.totalCount" 
          :per-page="pagination.pageSize"
          @change="loadTodos"
          align="center"
          class="mt-3"
        />
      </b-col>
    </b-row>

    <b-modal 
      v-model="modal.show" 
      :title="modal.isEdit ? 'Edit Todo' : 'New Todo'"
      @hide="resetModal"
    >
      <b-form @submit.prevent="saveTodo">
        <b-form-group label="Title" label-for="todo-title">
          <b-form-input 
            id="todo-title" 
            v-model="modal.model.title" 
            required 
            :state="validateTitle()"
          ></b-form-input>
          <b-form-invalid-feedback :state="validateTitle()">
            Title is required
          </b-form-invalid-feedback>
        </b-form-group>

        <b-form-group label="Due Date" label-for="todo-duedate">
          <b-form-input 
            id="todo-duedate" 
            v-model="modal.model.dueDate" 
            type="date"
          ></b-form-input>
        </b-form-group>

        <b-form-group label="Status">
          <b-form-checkbox v-model="modal.model.isDone">
            Completed
          </b-form-checkbox>
        </b-form-group>

        <div class="text-right">
          <b-button variant="secondary" @click="modal.show = false" class="mr-2">Cancel</b-button>
          <b-button variant="primary" type="submit" :disabled="!validateTitle()">Save</b-button>
        </div>
      </b-form>
    </b-modal>

    <b-modal v-model="deleteModal.show" title="Confirm Delete" ok-variant="danger" ok-title="Delete" @ok="confirmDelete">
      <p>Are you sure you want to delete this todo?</p>
      <p class="text-danger"><strong>{{ deleteModal.todo?.title }}</strong></p>
    </b-modal>
  </div>
</template>

<script>
import api from '@/TodosApiService';

export default {
  data() {
    return {
      loading: false,
      todos: [],
      fields: [
        { key: 'isDone', label: 'Status', sortable: false },
        { key: 'title', label: 'Title', sortable: false },
        { key: 'dueDate', label: 'Due Date', sortable: false },
        { key: 'createdAt', label: 'Created', sortable: false },
        { key: 'actions', label: 'Actions' }
      ],
      pagination: {
        page: 1,
        pageSize: 10,
        totalCount: 0
      },
      filter: {
        isDone: null
      },
      sort: {
        by: 'CreatedAt',
        desc: true
      },
      modal: {
        show: false,
        isEdit: false,
        model: {}
      },
      deleteModal: {
        show: false,
        todo: null
      }
    };
  },
  async created() {
    await this.loadTodos();
  },
  methods: {
    async loadTodos() {
      this.loading = true;
      try {
        const params = {
          page: this.pagination.page,
          pageSize: this.pagination.pageSize,
          sortBy: this.sort.by,
          sortDesc: this.sort.desc
        };
        
        if (this.filter.isDone !== null) {
          params.isDone = this.filter.isDone;
        }

        const result = await api.getAll(params);
        this.todos = result.items;
        this.pagination.totalCount = result.totalCount;
      } catch (error) {
        console.error('Error loading todos:', error);
        this.$bvToast.toast('Failed to load todos', {
          title: 'Error',
          variant: 'danger',
          solid: true
        });
      } finally {
        this.loading = false;
      }
    },
    showCreateModal() {
      this.modal.isEdit = false;
      this.modal.model = {
        title: '',
        dueDate: null,
        isDone: false
      };
      this.modal.show = true;
    },
    editTodo(todo) {
      this.modal.isEdit = true;
      this.modal.model = Object.assign({}, todo);
      if (this.modal.model.dueDate) {
        this.modal.model.dueDate = this.formatDateForInput(this.modal.model.dueDate);
      }
      this.modal.show = true;
    },
    async saveTodo() {
      try {
        if (this.modal.isEdit) {
          await api.update(this.modal.model.id, this.modal.model);
          this.$bvToast.toast('Todo updated successfully', {
            title: 'Success',
            variant: 'success',
            solid: true
          });
        } else {
          await api.create(this.modal.model);
          this.$bvToast.toast('Todo created successfully', {
            title: 'Success',
            variant: 'success',
            solid: true
          });
        }
        this.modal.show = false;
        await this.loadTodos();
      } catch (error) {
        console.error('Error saving todo:', error);
        this.$bvToast.toast('Failed to save todo', {
          title: 'Error',
          variant: 'danger',
          solid: true
        });
      }
    },
    async toggleTodoStatus(todo) {
      try {
        await api.update(todo.id, todo);
        this.$bvToast.toast('Todo status updated', {
          title: 'Success',
          variant: 'success',
          solid: true
        });
      } catch (error) {
        console.error('Error updating todo status:', error);
        todo.isDone = !todo.isDone;
        this.$bvToast.toast('Failed to update todo status', {
          title: 'Error',
          variant: 'danger',
          solid: true
        });
      }
    },
    deleteTodo(todo) {
      this.deleteModal.todo = todo;
      this.deleteModal.show = true;
    },
    async confirmDelete() {
      if (!this.deleteModal.todo) return;
      
      try {
        await api.delete(this.deleteModal.todo.id);
        this.$bvToast.toast('Todo deleted successfully', {
          title: 'Success',
          variant: 'success',
          solid: true
        });
        await this.loadTodos();
      } catch (error) {
        console.error('Error deleting todo:', error);
        this.$bvToast.toast('Failed to delete todo', {
          title: 'Error',
          variant: 'danger',
          solid: true
        });
      }
      this.deleteModal.show = false;
    },
    resetModal() {
      this.modal.model = {};
    },
    toggleSortDirection() {
      this.sort.desc = !this.sort.desc;
      this.loadTodos();
    },
    onRowClicked(item) {
      this.editTodo(item);
    },
    validateTitle() {
      return this.modal.model.title && this.modal.model.title.trim().length > 0;
    },
    formatDate(dateStr) {
      if (!dateStr) return '-';
      const date = new Date(dateStr);
      return date.toLocaleDateString();
    },
    formatDateForInput(dateStr) {
      if (!dateStr) return '';
      const date = new Date(dateStr);
      return date.toISOString().split('T')[0];
    }
  }
};
</script>
