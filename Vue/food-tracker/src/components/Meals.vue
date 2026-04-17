<template>
  <div class="container-fluid mt-4">
    <h1 class="h1 mb-4">Meals</h1>
    
    <b-alert :show="loading" variant="info">Loading...</b-alert>
    
    <b-alert :show="errorMessage" variant="danger" dismissible @dismissed="errorMessage = null">
      {{ errorMessage }}
    </b-alert>

    <b-card class="mb-4">
      <b-row align="center" class="mb-3">
        <b-col md="4">
          <b-form-group label="Search" label-for="search-keyword" label-cols-sm="3" class="mb-0">
            <b-form-input
              id="search-keyword"
              v-model="search.keyword"
              type="text"
              placeholder="Search by name..."
              @keyup.enter="fetchMeals"
            ></b-form-input>
          </b-form-group>
        </b-col>
        <b-col md="5">
          <b-form-group label="Date Range" label-cols-sm="3" class="mb-0">
            <b-row>
              <b-col>
                <b-form-input
                  v-model="search.startDate"
                  type="date"
                  placeholder="Start Date"
                ></b-form-input>
              </b-col>
              <b-col class="text-center align-self-center">
                <span>to</span>
              </b-col>
              <b-col>
                <b-form-input
                  v-model="search.endDate"
                  type="date"
                  placeholder="End Date"
                ></b-form-input>
              </b-col>
            </b-row>
          </b-form-group>
        </b-col>
        <b-col md="3" class="text-right">
          <b-button variant="primary" @click="openModal()" class="mr-2">
            <b-icon icon="plus"></b-icon> New Meal
          </b-button>
          <b-button variant="secondary" @click="fetchMeals">
            <b-icon icon="search"></b-icon> Search
          </b-button>
        </b-col>
      </b-row>
      <b-row align="center">
        <b-col md="4">
          <b-form-group label="Sort by" label-for="sort-by" label-cols-sm="3" class="mb-0">
            <b-row>
              <b-col>
                <b-form-select
                  id="sort-by"
                  v-model="sortBy"
                  :options="sortOptions"
                  @change="fetchMeals"
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
        <b-col md="8" class="text-right">
          <b-button variant="outline-secondary" size="sm" @click="clearFilters">
            <b-icon icon="x"></b-icon> Clear Filters
          </b-button>
        </b-col>
      </b-row>
    </b-card>

    <b-table
      :items="meals"
      :fields="tableFields"
      :busy="loading"
      striped
      hover
      class="mb-4"
    >
      <template slot="name" slot-scope="row">
        <span>{{ row.item.name }}</span>
      </template>

      <template slot="calories" slot-scope="row">
        <span class="font-weight-bold">{{ row.item.calories }}</span>
        <span class="text-muted"> kcal</span>
      </template>

      <template slot="date" slot-scope="row">
        {{ formatDate(row.item.date) }}
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
      @input="fetchMeals"
      align="center"
      class="mb-4"
    ></b-pagination>

    <b-modal
      :id="modalId"
      :title="editingMeal.id ? 'Edit Meal' : 'New Meal'"
      @ok="handleSave"
      @hidden="resetForm"
    >
      <b-form @submit.stop.prevent="handleSave">
        <b-form-group label="Meal Name" label-for="meal-name">
          <b-form-input
            id="meal-name"
            v-model="formData.name"
            type="text"
            placeholder="Enter meal name"
            required
          ></b-form-input>
        </b-form-group>

        <b-form-group label="Calories" label-for="meal-calories">
          <b-form-input
            id="meal-calories"
            v-model.number="formData.calories"
            type="number"
            min="1"
            max="10000"
            placeholder="Enter calories"
            required
          ></b-form-input>
        </b-form-group>

        <b-form-group label="Date" label-for="meal-date">
          <b-form-input
            id="meal-date"
            v-model="formData.date"
            type="date"
            required
          ></b-form-input>
        </b-form-group>
      </b-form>
    </b-modal>

    <b-modal
      :id="deleteModalId"
      title="Confirm Delete"
      ok-variant="danger"
      @ok="handleDelete"
    >
      <p>Are you sure you want to delete this meal?</p>
      <p class="font-weight-bold">"{{ mealToDelete?.name }}" ({{ mealToDelete?.calories }} kcal)</p>
    </b-modal>
  </div>
</template>

<script>
import api from '@/MealsApiService'

export default {
  name: 'Meals',
  data() {
    return {
      loading: false,
      errorMessage: null,
      meals: [],
      pagination: {
        page: 1,
        pageSize: 10,
        totalCount: 0
      },
      search: {
        keyword: '',
        startDate: '',
        endDate: ''
      },
      sortBy: 'Date',
      sortDesc: true,
      sortOptions: [
        { value: 'Date', text: 'Date' },
        { value: 'Name', text: 'Name' },
        { value: 'Calories', text: 'Calories' }
      ],
      tableFields: [
        { key: 'name', label: 'Meal Name' },
        { key: 'calories', label: 'Calories' },
        { key: 'date', label: 'Date' },
        { key: 'actions', label: 'Actions' }
      ],
      modalId: 'meal-modal',
      deleteModalId: 'delete-meal-modal',
      editingMeal: {},
      formData: {
        id: null,
        name: '',
        calories: null,
        date: ''
      },
      mealToDelete: null
    }
  },
  async created() {
    await this.fetchMeals()
  },
  methods: {
    async fetchMeals() {
      this.loading = true
      this.errorMessage = null
      
      try {
        const params = {
          page: this.pagination.page,
          pageSize: this.pagination.pageSize,
          sortBy: this.sortBy,
          sortDesc: this.sortDesc
        }
        
        if (this.search.keyword.trim()) {
          params.keyword = this.search.keyword.trim()
        }
        
        if (this.search.startDate) {
          params.startDate = this.search.startDate
        }
        
        if (this.search.endDate) {
          params.endDate = this.search.endDate
        }
        
        const result = await api.getList(params)
        this.meals = result.items
        this.pagination.totalCount = result.totalCount
        this.pagination.pageSize = result.pageSize
      } catch (error) {
        this.errorMessage = error.userMessage || error.message || 'Failed to load meals. Please try again.'
        console.error('Error fetching meals:', error)
      } finally {
        this.loading = false
      }
    },
    
    toggleSort() {
      this.sortDesc = !this.sortDesc
      this.fetchMeals()
    },
    
    clearFilters() {
      this.search = {
        keyword: '',
        startDate: '',
        endDate: ''
      }
      this.sortBy = 'Date'
      this.sortDesc = true
      this.pagination.page = 1
      this.fetchMeals()
    },
    
    openModal(meal = null) {
      if (meal) {
        this.editingMeal = { ...meal }
        this.formData = {
          id: meal.id,
          name: meal.name,
          calories: meal.calories,
          date: this.formatDateForInput(meal.date)
        }
      } else {
        this.editingMeal = {}
        this.formData = {
          id: null,
          name: '',
          calories: null,
          date: this.formatDateForInput(new Date())
        }
      }
      this.$root.$emit('bv::show::modal', this.modalId)
    },
    
    resetForm() {
      this.formData = {
        id: null,
        name: '',
        calories: null,
        date: ''
      }
      this.editingMeal = {}
    },
    
    async handleSave() {
      if (!this.formData.name || !this.formData.name.trim()) {
        this.errorMessage = 'Meal name is required.'
        return
      }
      
      if (!this.formData.calories || this.formData.calories < 1) {
        this.errorMessage = 'Calories must be at least 1.'
        return
      }
      
      if (!this.formData.date) {
        this.errorMessage = 'Date is required.'
        return
      }
      
      try {
        const mealData = {
          name: this.formData.name.trim(),
          calories: parseInt(this.formData.calories),
          date: new Date(this.formData.date).toISOString()
        }
        
        if (this.formData.id) {
          await api.update(this.formData.id, mealData)
        } else {
          await api.create(mealData)
        }
        
        this.$root.$emit('bv::hide::modal', this.modalId)
        await this.fetchMeals()
      } catch (error) {
        this.errorMessage = error.userMessage || error.message || 'Failed to save meal. Please try again.'
        console.error('Error saving meal:', error)
      }
    },
    
    confirmDelete(meal) {
      this.mealToDelete = meal
      this.$root.$emit('bv::show::modal', this.deleteModalId)
    },
    
    async handleDelete() {
      if (!this.mealToDelete) return
      
      try {
        await api.delete(this.mealToDelete.id)
        this.$root.$emit('bv::hide::modal', this.deleteModalId)
        this.mealToDelete = null
        await this.fetchMeals()
      } catch (error) {
        this.errorMessage = error.userMessage || error.message || 'Failed to delete meal. Please try again.'
        console.error('Error deleting meal:', error)
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
    }
  }
}
</script>

<style scoped>
</style>
