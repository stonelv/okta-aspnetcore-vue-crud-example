import { shallowMount, createLocalVue } from '@vue/test-utils'
import Todos from '@/components/Todos.vue'

const localVue = createLocalVue()

jest.mock('@/TodosApiService', () => ({
  getAll: jest.fn(),
  create: jest.fn(),
  update: jest.fn(),
  delete: jest.fn()
}))

import api from '@/TodosApiService'

describe('Todos.vue', () => {
  let wrapper

  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('renders correctly with initial state', () => {
    api.getAll.mockResolvedValue({
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 10
    })

    wrapper = shallowMount(Todos, {
      localVue,
      stubs: ['b-alert', 'b-row', 'b-col', 'b-btn', 'b-form-select', 'b-table', 'b-pagination', 'b-modal', 'b-form', 'b-form-group', 'b-form-input', 'b-form-checkbox']
    })

    expect(wrapper.find('h1').text()).toBe('Todos')
  })

  it('loads todos on mount', async () => {
    const mockTodos = [
      { id: '1', title: 'Test Todo 1', isDone: false, dueDate: null, createdAt: '2026-04-14T00:00:00Z', userId: 'user1' },
      { id: '2', title: 'Test Todo 2', isDone: true, dueDate: '2026-04-15T00:00:00Z', createdAt: '2026-04-13T00:00:00Z', userId: 'user1' }
    ]

    api.getAll.mockResolvedValue({
      items: mockTodos,
      totalCount: 2,
      page: 1,
      pageSize: 10
    })

    wrapper = shallowMount(Todos, {
      localVue,
      stubs: ['b-alert', 'b-row', 'b-col', 'b-btn', 'b-form-select', 'b-table', 'b-pagination', 'b-modal', 'b-form', 'b-form-group', 'b-form-input', 'b-form-checkbox']
    })

    await wrapper.vm.$nextTick()
    await wrapper.vm.$nextTick()

    expect(api.getAll).toHaveBeenCalledWith({
      page: 1,
      pageSize: 10,
      sortBy: 'createdAt',
      sortDesc: true
    })
    expect(wrapper.vm.todos).toEqual(mockTodos)
    expect(wrapper.vm.totalCount).toBe(2)
  })

  it('formats date correctly', () => {
    wrapper = shallowMount(Todos, {
      localVue,
      stubs: ['b-alert', 'b-row', 'b-col', 'b-btn', 'b-form-select', 'b-table', 'b-pagination', 'b-modal', 'b-form', 'b-form-group', 'b-form-input', 'b-form-checkbox']
    })

    expect(wrapper.vm.formatDate(null)).toBe('-')
    
    const testDate = '2026-04-14T12:00:00Z'
    const formatted = wrapper.vm.formatDate(testDate)
    expect(formatted).not.toBe('-')
    expect(formatted).toBeDefined()
  })

  it('checks if todo is overdue', () => {
    wrapper = shallowMount(Todos, {
      localVue,
      stubs: ['b-alert', 'b-row', 'b-col', 'b-btn', 'b-form-select', 'b-table', 'b-pagination', 'b-modal', 'b-form', 'b-form-group', 'b-form-input', 'b-form-checkbox']
    })

    const pastTodo = {
      id: '1',
      title: 'Overdue Todo',
      isDone: false,
      dueDate: '2020-01-01T00:00:00Z',
      createdAt: '2020-01-01T00:00:00Z'
    }

    const futureTodo = {
      id: '2',
      title: 'Future Todo',
      isDone: false,
      dueDate: '2030-01-01T00:00:00Z',
      createdAt: '2020-01-01T00:00:00Z'
    }

    const completedTodo = {
      id: '3',
      title: 'Completed Todo',
      isDone: true,
      dueDate: '2020-01-01T00:00:00Z',
      createdAt: '2020-01-01T00:00:00Z'
    }

    const noDueDateTodo = {
      id: '4',
      title: 'No Due Date Todo',
      isDone: false,
      dueDate: null,
      createdAt: '2020-01-01T00:00:00Z'
    }

    expect(wrapper.vm.isOverdue(pastTodo)).toBe(true)
    expect(wrapper.vm.isOverdue(futureTodo)).toBe(false)
    expect(wrapper.vm.isOverdue(completedTodo)).toBe(false)
    expect(wrapper.vm.isOverdue(noDueDateTodo)).toBe(false)
  })

  it('opens modal for new todo', () => {
    wrapper = shallowMount(Todos, {
      localVue,
      stubs: ['b-alert', 'b-row', 'b-col', 'b-btn', 'b-form-select', 'b-table', 'b-pagination', 'b-modal', 'b-form', 'b-form-group', 'b-form-input', 'b-form-checkbox']
    })

    expect(wrapper.vm.showModal).toBe(false)
    expect(wrapper.vm.editingTodo).toBeNull()

    wrapper.vm.openModal()

    expect(wrapper.vm.showModal).toBe(true)
    expect(wrapper.vm.editingTodo).toBeNull()
    expect(wrapper.vm.form.title).toBe('')
    expect(wrapper.vm.form.isDone).toBe(false)
  })

  it('opens modal for editing todo', () => {
    wrapper = shallowMount(Todos, {
      localVue,
      stubs: ['b-alert', 'b-row', 'b-col', 'b-btn', 'b-form-select', 'b-table', 'b-pagination', 'b-modal', 'b-form', 'b-form-group', 'b-form-input', 'b-form-checkbox']
    })

    const existingTodo = {
      id: '1',
      title: 'Existing Todo',
      isDone: true,
      dueDate: '2026-04-15T00:00:00Z',
      createdAt: '2026-04-14T00:00:00Z'
    }

    wrapper.vm.openModal(existingTodo)

    expect(wrapper.vm.showModal).toBe(true)
    expect(wrapper.vm.editingTodo).toEqual(existingTodo)
    expect(wrapper.vm.form.title).toBe('Existing Todo')
    expect(wrapper.vm.form.isDone).toBe(true)
  })

  it('resets form when modal is hidden', () => {
    wrapper = shallowMount(Todos, {
      localVue,
      stubs: ['b-alert', 'b-row', 'b-col', 'b-btn', 'b-form-select', 'b-table', 'b-pagination', 'b-modal', 'b-form', 'b-form-group', 'b-form-input', 'b-form-checkbox']
    })

    wrapper.vm.form.title = 'Test'
    wrapper.vm.form.isDone = true
    wrapper.vm.editingTodo = { id: '1' }

    wrapper.vm.resetForm()

    expect(wrapper.vm.form.title).toBe('')
    expect(wrapper.vm.form.isDone).toBe(false)
    expect(wrapper.vm.editingTodo).toBeNull()
  })
})
