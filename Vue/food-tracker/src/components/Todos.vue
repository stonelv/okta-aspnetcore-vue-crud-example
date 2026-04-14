<template>
  <div class="container-fluid mt-4">
    <h1 class="h1">Todos</h1>
    
    <b-alert :show="loading" variant="info">Loading...</b-alert>
    <b-alert :show="error" variant="danger" dismissible @dismissed="error = null">{{ error }}</b-alert>

    <b-row class="mb-3">
      <b-col>
        <b-btn variant="primary" @click="openModal()">
          <b-icon-plus></b-icon-plus> New Todo
        </b-btn>
      </b-col>
      <b-col md="3">
        <b-form-select v-model="filterIsDone" @change="loadTodos">
          <option :value="null">All</option>
          <option :value="true">Completed</option>
          <option :value="false">Incomplete</option>
        </b-form-select>
      </b-col>
    </b-row>

    <b-table
      :items="todos"
      :fields="fields"
      :sort-by.sync="sortBy"
      :sort-desc.sync="sortDesc"
      @sort-changed="loadTodos"
      striped
      hover
    >
      <template v-slot:cell(isDone)="data">
        <b-form-checkbox
          :checked="data.item.isDone"
          @change="toggleDone(data.item)"
        ></b-form-checkbox>
      </template>

      <template v-slot:cell(dueDate)="data">
        <span :class="{ 'text-danger': isOverdue(data.item) }">
          {{ formatDate(data.item.dueDate) }}
        </span>
      </template>

      <template v-slot:cell(actions)="data">
        <b-btn size="sm" variant="link" @click="openModal(data.item)">Edit</b-btn>
        <b-btn size="sm" variant="link" class="text-danger" @click="confirmDelete(data.item)">Delete</b-btn>
      </template>
    </b-table>

    <b-row class="mt-3">
      <b-col>
        <b-pagination
          v-model="currentPage"
          :total-rows="totalCount"
          :per-page="pageSize"
          @input="loadTodos"
          align="center"
        ></b-pagination>
      </b-col>
    </b-row>

    <b-modal
      v-model="showModal"
      :title="editingTodo ? 'Edit Todo' : 'New Todo'"
      @ok="handleSubmit"
      @hidden="resetForm"
    >
      <b-form @submit.prevent="handleSubmit">
        <b-form-group label="Title">
          <b-form-input
            v-model="form.title"
            type="text"
            placeholder="Enter todo title"
            :state="form.title ? true : null"
            required
          ></b-form-input>
        </b-form-group>

        <b-form-group label="Due Date">
          <b-form-input
            v-model="form.dueDate"
            type="date"
          ></b-form-input>
        </b-form-group>

        <b-form-group v-if="editingTodo">
          <b-form-checkbox v-model="form.isDone">
            Mark as completed
          </b-form-checkbox>
        </b-form-group>
      </b-form>
    </b-modal>

    <b-modal
      v-model="showDeleteModal"
      title="Confirm Delete"
      ok-variant="danger"
      @ok="handleDelete"
    >
      <p>Are you sure you want to delete this todo?</p>
      <p class="font-weight-bold">{{ deletingTodo?.title }}</p>
    </b-modal>
  </div>
</template>

<script>
import api from '@/TodosApiService'

export default {
  data() {
    return {
      loading: false,
      error: null,
      todos: [],
      totalCount: 0,
      currentPage: 1,
      pageSize: 10,
      filterIsDone: null,
      sortBy: 'createdAt',
      sortDesc: true,
      showModal: false,
      showDeleteModal: false,
      editingTodo: null,
      deletingTodo: null,
      form: {
        title: '',
        dueDate: '',
        isDone: false
      }
    }
  },
  computed: {
    fields() {
      return [
        { key: 'isDone', label: 'Done', sortable: true },
        { key: 'title', label: 'Title', sortable: true },
        { key: 'dueDate', label: 'Due Date', sortable: true },
        { key: 'createdAt', label: 'Created', sortable: true },
        { key: 'actions', label: 'Actions' }
      ]
    }
  },
  async created() {
    await this.loadTodos()
  },
  methods: {
    async loadTodos() {
      this.loading = true
      this.error = null

      try {
        const params = {
          page: this.currentPage,
          pageSize: this.pageSize,
          sortBy: this.sortBy,
          sortDesc: this.sortDesc
        }

        if (this.filterIsDone !== null) {
          params.isDone = this.filterIsDone
        }

        const result = await api.getAll(params)
        this.todos = result.items
        this.totalCount = result.totalCount
      } catch (err) {
        this.error = 'Failed to load todos. Please try again.'
        console.error(err)
      } finally {
        this.loading = false
      }
    },
    openModal(todo = null) {
      this.editingTodo = todo
      if (todo) {
        this.form = {
          title: todo.title,
          dueDate: todo.dueDate ? this.formatDateForInput(todo.dueDate) : '',
          isDone: todo.isDone
        }
      } else {
        this.resetForm()
      }
      this.showModal = true
    },
    resetForm() {
      this.form = {
        title: '',
        dueDate: '',
        isDone: false
      }
      this.editingTodo = null
    },
    async handleSubmit() {
      if (!this.form.title.trim()) {
        this.error = 'Title is required'
        return
      }

      try {
        const data = {
          title: this.form.title,
          isDone: this.form.isDone,
          dueDate: this.form.dueDate ? new Date(this.form.dueDate).toISOString() : null
        }

        if (this.editingTodo) {
          await api.update(this.editingTodo.id, data)
        } else {
          await api.create(data)
        }

        this.showModal = false
        await this.loadTodos()
      } catch (err) {
        this.error = 'Failed to save todo. Please try again.'
        console.error(err)
      }
    },
    confirmDelete(todo) {
      this.deletingTodo = todo
      this.showDeleteModal = true
    },
    async handleDelete() {
      try {
        await api.delete(this.deletingTodo.id)
        this.showDeleteModal = false
        await this.loadTodos()
      } catch (err) {
        this.error = 'Failed to delete todo. Please try again.'
        console.error(err)
      }
    },
    async toggleDone(todo) {
      try {
        await api.update(todo.id, {
          ...todo,
          isDone: !todo.isDone
        })
        await this.loadTodos()
      } catch (err) {
        this.error = 'Failed to update todo. Please try again.'
        console.error(err)
      }
    },
    formatDate(dateStr) {
      if (!dateStr) return '-'
      const date = new Date(dateStr)
      return date.toLocaleDateString()
    },
    formatDateForInput(dateStr) {
      if (!dateStr) return ''
      const date = new Date(dateStr)
      return date.toISOString().split('T')[0]
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
