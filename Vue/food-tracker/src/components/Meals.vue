<template>
  <div class="container-fluid mt-4">
    <h1 class="h1 mb-4">餐食记录</h1>
    
    <b-alert :show="loading" variant="info" dismissible>
      加载中...
    </b-alert>
    
    <b-alert :show="errorMessage" variant="danger" dismissible @dismissed="errorMessage = ''">
      {{ errorMessage }}
    </b-alert>
    
    <b-card class="mb-4">
      <b-row class="align-items-center">
        <b-col md="3">
          <b-form-group label="关键词搜索" label-for="keyword-input">
            <b-form-input
              id="keyword-input"
              type="text"
              v-model="searchParams.keyword"
              placeholder="搜索餐食名称..."
              @input="debouncedSearch"
            ></b-form-input>
          </b-form-group>
        </b-col>
        <b-col md="3">
          <b-form-group label="开始日期" label-for="start-date-input">
            <b-form-input
              id="start-date-input"
              type="date"
              v-model="searchParams.startDate"
              @input="loadMeals"
            ></b-form-input>
          </b-form-group>
        </b-col>
        <b-col md="3">
          <b-form-group label="结束日期" label-for="end-date-input">
            <b-form-input
              id="end-date-input"
              type="date"
              v-model="searchParams.endDate"
              @input="loadMeals"
            ></b-form-input>
          </b-form-group>
        </b-col>
        <b-col md="3" class="text-right">
          <b-btn variant="primary" @click="openModal()">
            <b-icon-plus></b-icon-plus> 新增餐食
          </b-btn>
          <b-btn variant="secondary" class="ml-2" @click="resetSearch">
            <b-icon-arrow-repeat></b-icon-arrow-repeat> 重置
          </b-btn>
        </b-col>
      </b-row>
    </b-card>
    
    <b-card>
      <div v-if="!loading && meals.length === 0" class="text-center py-5 text-muted">
        <p>暂无餐食记录</p>
        <b-btn variant="primary" @click="openModal()">添加第一条记录</b-btn>
      </div>
      
      <b-table
        v-if="meals.length > 0"
        :items="meals"
        :fields="fields"
        :busy="loading"
        striped
        hover
      >
        <template #cell(date)="row">
          {{ formatDate(row.item.date) }}
        </template>
        <template #cell(actions)="row">
          <b-btn variant="link" size="sm" @click="openModal(row.item)">
            编辑
          </b-btn>
          <b-btn variant="link" size="sm" class="text-danger" @click="confirmDelete(row.item)">
            删除
          </b-btn>
        </template>
      </b-table>
      
      <b-pagination
        v-if="totalPages > 1"
        v-model="currentPage"
        :total-rows="totalCount"
        :per-page="pageSize"
        align="center"
        class="mt-4"
        @input="loadMeals"
      ></b-pagination>
    </b-card>
    
    <b-modal
      v-model="showModal"
      :title="isEdit ? '编辑餐食' : '新增餐食'"
      @ok="handleSubmit"
      @hidden="resetForm"
    >
      <b-form @submit.stop.prevent="handleSubmit">
        <b-form-group
          id="input-group-name"
          label="餐食名称"
          label-for="input-name"
          :state="getFieldState('name')"
        >
          <b-form-input
            id="input-name"
            v-model="form.name"
            placeholder="请输入餐食名称"
            :state="getFieldState('name')"
          ></b-form-input>
          <b-form-invalid-feedback :state="getFieldState('name')">
            {{ formErrors.name }}
          </b-form-invalid-feedback>
        </b-form-group>
        
        <b-form-group
          id="input-group-calories"
          label="卡路里"
          label-for="input-calories"
          :state="getFieldState('calories')"
        >
          <b-form-input
            id="input-calories"
            type="number"
            v-model.number="form.calories"
            placeholder="请输入卡路里"
            :state="getFieldState('calories')"
            min="1"
            max="10000"
          ></b-form-input>
          <b-form-text>
            卡路里范围：1-10000
          </b-form-text>
          <b-form-invalid-feedback :state="getFieldState('calories')">
            {{ formErrors.calories }}
          </b-form-invalid-feedback>
        </b-form-group>
        
        <b-form-group
          id="input-group-date"
          label="日期"
          label-for="input-date"
          :state="getFieldState('date')"
        >
          <b-form-input
            id="input-date"
            type="date"
            v-model="form.date"
            :state="getFieldState('date')"
          ></b-form-input>
          <b-form-invalid-feedback :state="getFieldState('date')">
            {{ formErrors.date }}
          </b-form-invalid-feedback>
        </b-form-group>
      </b-form>
    </b-modal>
    
    <b-modal
      v-model="showDeleteModal"
      title="确认删除"
      ok-variant="danger"
      ok-title="删除"
      cancel-title="取消"
      @ok="handleDelete"
    >
      <p>确定要删除餐食 "<strong>{{ deleteItem && deleteItem.name }}</strong>" 吗？</p>
      <p class="text-muted small">此操作不可撤销。</p>
    </b-modal>
  </div>
</template>

<script>
import mealsApi from '@/api/meals'

export default {
  name: 'Meals',
  data () {
    return {
      loading: false,
      errorMessage: '',
      meals: [],
      totalCount: 0,
      totalPages: 0,
      currentPage: 1,
      pageSize: 10,
      searchParams: {
        keyword: '',
        startDate: '',
        endDate: ''
      },
      fields: [
        { key: 'name', label: '餐食名称', sortable: true },
        { key: 'calories', label: '卡路里', sortable: true },
        { key: 'date', label: '日期', sortable: true },
        { key: 'actions', label: '操作' }
      ],
      showModal: false,
      isEdit: false,
      form: {
        id: '',
        name: '',
        calories: null,
        date: ''
      },
      formErrors: {
        name: '',
        calories: '',
        date: ''
      },
      showDeleteModal: false,
      deleteItem: null,
      searchTimer: null
    }
  },
  async created () {
    await this.loadMeals()
  },
  methods: {
    async loadMeals () {
      this.loading = true
      this.errorMessage = ''
      
      try {
        const params = {
          page: this.currentPage,
          pageSize: this.pageSize
        }
        
        if (this.searchParams.keyword) {
          params.keyword = this.searchParams.keyword
        }
        if (this.searchParams.startDate) {
          params.startDate = this.searchParams.startDate
        }
        if (this.searchParams.endDate) {
          params.endDate = this.searchParams.endDate
        }
        
        const result = await mealsApi.getList(params)
        this.meals = result.items || []
        this.totalCount = result.totalCount || 0
        this.totalPages = result.totalPages || 0
      } catch (error) {
        this.errorMessage = error.message || '加载数据失败'
      } finally {
        this.loading = false
      }
    },
    
    debouncedSearch () {
      if (this.searchTimer) {
        clearTimeout(this.searchTimer)
      }
      this.searchTimer = setTimeout(() => {
        this.currentPage = 1
        this.loadMeals()
      }, 300)
    },
    
    resetSearch () {
      this.searchParams = {
        keyword: '',
        startDate: '',
        endDate: ''
      }
      this.currentPage = 1
      this.loadMeals()
    },
    
    openModal (item = null) {
      this.isEdit = !!item
      this.resetForm()
      
      if (item) {
        this.form.id = item.id
        this.form.name = item.name
        this.form.calories = item.calories
        this.form.date = this.formatDateForInput(item.date)
      } else {
        this.form.date = this.formatDateForInput(new Date())
      }
      
      this.showModal = true
    },
    
    resetForm () {
      this.form = {
        id: '',
        name: '',
        calories: null,
        date: ''
      }
      this.formErrors = {
        name: '',
        calories: '',
        date: ''
      }
    },
    
    validateForm () {
      let isValid = true
      this.formErrors = {
        name: '',
        calories: '',
        date: ''
      }
      
      if (!this.form.name || this.form.name.trim() === '') {
        this.formErrors.name = '餐食名称不能为空'
        isValid = false
      } else if (this.form.name.length > 100) {
        this.formErrors.name = '餐食名称不能超过100个字符'
        isValid = false
      }
      
      if (this.form.calories === null || this.form.calories === undefined || this.form.calories === '') {
        this.formErrors.calories = '卡路里不能为空'
        isValid = false
      } else if (this.form.calories < 1 || this.form.calories > 10000) {
        this.formErrors.calories = '卡路里必须在1-10000之间'
        isValid = false
      }
      
      if (!this.form.date) {
        this.formErrors.date = '日期不能为空'
        isValid = false
      }
      
      return isValid
    },
    
    getFieldState (fieldName) {
      if (this.formErrors[fieldName]) {
        return false
      }
      return null
    },
    
    async handleSubmit (bvModalEvt) {
      bvModalEvt.preventDefault()
      
      if (!this.validateForm()) {
        return
      }
      
      this.loading = true
      this.showModal = false
      
      try {
        const data = {
          id: this.form.id,
          name: this.form.name.trim(),
          calories: this.form.calories,
          date: new Date(this.form.date).toISOString()
        }
        
        if (this.isEdit) {
          await mealsApi.update(this.form.id, data)
          this.errorMessage = ''
        } else {
          await mealsApi.create(data)
          this.errorMessage = ''
        }
        
        await this.loadMeals()
      } catch (error) {
        this.errorMessage = error.message || '操作失败'
        this.showModal = true
      } finally {
        this.loading = false
      }
    },
    
    confirmDelete (item) {
      this.deleteItem = item
      this.showDeleteModal = true
    },
    
    async handleDelete () {
      if (!this.deleteItem) return
      
      this.loading = true
      
      try {
        await mealsApi.delete(this.deleteItem.id)
        await this.loadMeals()
      } catch (error) {
        this.errorMessage = error.message || '删除失败'
      } finally {
        this.loading = false
        this.showDeleteModal = false
        this.deleteItem = null
      }
    },
    
    formatDate (dateStr) {
      if (!dateStr) return ''
      const date = new Date(dateStr)
      return date.toLocaleDateString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
      })
    },
    
    formatDateForInput (dateStr) {
      if (!dateStr) return ''
      const date = new Date(dateStr)
      const year = date.getFullYear()
      const month = String(date.getMonth() + 1).padStart(2, '0')
      const day = String(date.getDate()).padStart(2, '0')
      return `${year}-${month}-${day}`
    }
  }
}
</script>
