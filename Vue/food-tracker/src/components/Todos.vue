<template>
  <div class="container-fluid mt-4">
    <h1 class="h1 mb-4">Todos</h1>
    
    <b-alert :show="loading" variant="info">Loading...</b-alert>
    
    <b-alert :show="errorMessage" variant="danger" dismissible @dismissed="errorMessage = null">
      {{ errorMessage }}
    </b-alert>

    <b-card class="mb-4">
      <b-row align="center">
        <b-col md="4">
          <b-form-group label="Filter by Status" label-for="filter-isDone" label-cols-sm="4" class="mb-0">
            <b-form-select
              id="filter-isDone"
              v-model="filter.isDone"
              :options="filterOptions"
              @change="fetchTodos"
            ></b-form-select>
          </b-form-group>
        </b-col>
        <b-col md="4">
          <b-form-group label="Sort by" label-for="sort-by" label-cols-sm="3" class="mb-0">
            <b-row>
              <b-col>
                <b-form-select
                  id="sort-by"
                  v-model="sortBy"
                  :options="sortOptions"
                  @change="fetchTodos"
                ></b-form-select>
              </b-col>
              <b-col cols="auto">
                <b-button
                  :variant="sortDesc ? 'secondary' : 'outline-secondary'"
                  @click="toggleSort"
                >
                  <b-icon :icon="sortDesc ? 'arrow-up' : 'arrow-down'"></b-icon>
                </b-button>
              </b-col>
            </b-row>
          </b-form-group>
        </b-col>
        <b-col md="4" class="text-right">
          <b-button variant="primary" @click="openModal()">
            <b-icon icon="plus"></b-icon> New Todo
          </b-button>
        </b-col>
      </b-row>
    </b-card>

    <b-table
      :items="todos"
      :fields="tableFields"
      :busy="loading"
      striped
      hover
      class="mb-4"
    >
      <template slot="isDone" slot-scope="row">
        <b-form-checkbox
          :checked="row.item.isDone"
          @change="toggleTodoDone(row.item)"
        >
          <span :class="{ 'text-muted': row.item.isDone, 'text-decoration-line-through': row.item.isDone }">
            {{ row.item.isDone ? 'Completed' : 'Pending' }}
          </span>
        </b-form-checkbox>
      </template>

      <template slot="title" slot-scope="row">
        <span :class="{ 'text-muted': row.item.isDone, 'text-decoration-line-through': row.item.isDone }">
          {{ row.item.title }}
        </span>
      </template>

      <template slot="dueDate" slot-scope="row">
        <span v-if="row.item.dueDate" :class="isOverdue(row.item) ? 'text-danger font-weight-bold' : ''">
          {{ formatDate(row.item.dueDate) }}
        </span>
        <span v-else class="text-muted">Not set</span>
      </template>

      <template slot="createdAt" slot-scope="row">
        {{ formatDate(row.item.createdAt) }}
      </template>

      <template slot="actions" slot-scope="row">
        <b-button size="sm" variant="outline-primary" @click="openModal(row.item)" class="mr-2">
          Edit
        </b-button>
        <b-button size="sm" variant="outline-danger" @click="confirmDelete(row.item)">
          Delete
        </b-button>
      </template>
    </b-table>

    <b-pagination
      v-model="pagination.page"
      :total-rows="pagination.totalCount"
      :per-page="pagination.pageSize"
      @input="fetchTodos"
      align="center"
      class="mb-4"
    ></b-pagination>

    <b-modal
      :id="modalId"
      :title="editingTodo.id ? 'Edit Todo' : 'New Todo'"
      @ok="handleSave"
      @hidden="resetForm"
    >
      <b-form @submit.stop.prevent="handleSave">
        <b-form-group label="Title" label-for="todo-title">
          <b-form-input
            id="todo-title"
            v-model="formData.title"
            type="text"
            placeholder="Enter todo title"
            required
          ></b-form-input>
        </b-form-group>

        <b-form-group label="Due Date" label-for="todo-dueDate">
          <b-form-input
            id="todo-dueDate"
            v-model="formData.dueDate"
            type="date"
          ></b-form-input>
        </b-form-group>

        <b-form-group v-if="editingTodo.id" label="Status">
          <b-form-checkbox v-model="formData.isDone">
            Mark as completed
          </b-form-checkbox>
        </b-form-group>
      </b-form>
    </b-modal>

    <b-modal
      :id="deleteModalId"
      title="Confirm Delete"
      ok-variant="danger"
      @ok="handleDelete"
    >
      <p>Are you sure you want to delete this todo?</p>
      <p class="font-weight-bold">"{{ todoToDelete?.title }}"</p>
    </b-modal>
  </div>
</template>

<script>
import api from '@/TodosApiService'

export default {
  name: 'Todos',
  data() {
    return {
      loading: false,
      errorMessage: null,
      todos: [],
      pagination: {
        page: 1,
        pageSize: 10,
        totalCount: 0
      },
      filter: {
        isDone: null
      },
      sortBy: 'CreatedAt',
      sortDesc: true,
      filterOptions: [
        { value: null, text: 'All' },
        { value: false, text: 'Pending' },
        { value: true, text: 'Completed' }
      ],
      sortOptions: [
        { value: 'CreatedAt', text: 'Created Date' },
        { value: 'Title', text: 'Title' },
        { value: 'DueDate', text: 'Due Date' },
        { value: 'IsDone', text: 'Status' }
      ],
      tableFields: [
        { key: 'isDone', label: 'Status' },
        { key: 'title', label: 'Title' },
        { key: 'dueDate', label: 'Due Date' },
        { key: 'createdAt', label: 'Created' },
        { key: 'actions', label: 'Actions' }
      ],
      modalId: 'todo-modal',
      deleteModalId: 'delete-modal',
      editingTodo: {},
      formData: {
        id: null,
        title: '',
        isDone: false,
        dueDate: ''
      },
      todoToDelete: null
    }
  },
  async created() {
    await this.fetchTodos()
  },
  methods: {
    async fetchTodos() {
      this.loading = true
      this.errorMessage = null
      
      try {
        const params = {
          page: this.pagination.page,
          pageSize: this.pagination.pageSize,
          sortBy: this.sortBy,
          sortDesc: this.sortDesc
        }
        
        if (this.filter.isDone !== null) {
          params.isDone = this.filter.isDone
        }
        
        const result = await api.getList(params)
        this.todos = result.items
        this.pagination.totalCount = result.totalCount
        this.pagination.pageSize = result.pageSize
      } catch (error) {
        this.errorMessage = 'Failed to load todos. Please try again.'
        console.error('Error fetching todos:', error)
      } finally {
        this.loading = false
      }
    },
    
    toggleSort() {
      this.sortDesc = !this.sortDesc
      this.fetchTodos()
    },
    
    async toggleTodoDone(todo) {
      try {
        const updatedTodo = {
          ...todo,
          isDone: !todo.isDone
        }
        await api.update(todo.id, updatedTodo)
        await this.fetchTodos()
      } catch (error) {
        this.errorMessage = 'Failed to update todo status.'
        console.error('Error updating todo:', error)
      }
    },
    
    openModal(todo = null) {
      if (todo) {
        this.editingTodo = { ...todo }
        this.formData = {
          id: todo.id,
          title: todo.title,
          isDone: todo.isDone,
          dueDate: todo.dueDate ? this.formatDateForInput(todo.dueDate) : ''
        }
      } else {
        this.editingTodo = {}
        this.formData = {
          id: null,
          title: '',
          isDone: false,
          dueDate: ''
        }
      }
      this.$root.$emit('bv::show::modal', this.modalId)
    },
    
    resetForm() {
      this.formData = {
        id: null,
        title: '',
        isDone: false,
        dueDate: ''
      }
      this.editingTodo = {}
    },
    
    async handleSave() {
      if (!this.formData.title.trim()) {
        this.errorMessage = 'Title is required.'
        return
      }
      
      try {
        const todoData = {
          title: this.formData.title.trim(),
          isDone: this.formData.isDone,
          dueDate: this.formData.dueDate ? new Date(this.formData.dueDate).toISOString() : null
        }
        
        if (this.formData.id) {
          await api.update(this.formData.id, todoData)
        } else {
          await api.create(todoData)
        }
        
        this.$root.$emit('bv::hide::modal', this.modalId)
        await this.fetchTodos()
      } catch (error) {
        this.errorMessage = 'Failed to save todo. Please try again.'
        console.error('Error saving todo:', error)
      }
    },
    
    confirmDelete(todo) {
      this.todoToDelete = todo
      this.$root.$emit('bv::show::modal', this.deleteModalId)
    },
    
    async handleDelete() {
      if (!this.todoToDelete) return
      
      try {
        await api.delete(this.todoToDelete.id)
        this.$root.$emit('bv::hide::modal', this.deleteModalId)
        this.todoToDelete = null
        await this.fetchTodos()
      } catch (error) {
        this.errorMessage = 'Failed to delete todo. Please try again.'
        console.error('Error deleting todo:', error)
      }
    },
    
    formatDate(dateString) {
      if (!dateString) return ''
      const date = new Date(dateString)
      return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      })
    },
    
    formatDateForInput(dateString) {
      if (!dateString) return ''
      const date = new Date(dateString)
      const year = date.getFullYear()
      const month = String(date.getMonth() + 1).padStart(2, '0')
      const day = String(date.getDate()).padStart(2, '0')
      return `${year}-${month}-${day}`
    },
    
    isOverdue(todo) {
      if (!todo.dueDate || todo.isDone) return false
      const dueDate = new Date(todo.dueDate)
      const today = new Date()
      today.setHours(0, 0, 0, 0)
      return dueDate < today
    }
  }
}
</script>

<style scoped>
.text-decoration-line-through {
  text-decoration: line-through;
}
</style>
