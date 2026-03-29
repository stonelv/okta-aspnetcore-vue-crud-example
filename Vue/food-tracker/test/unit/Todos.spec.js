import Vue from 'vue'
import Todos from '@/components/Todos'

describe('Todos.vue', () => {
  it('has a created hook', () => {
    expect(typeof Todos.created).toBe('function')
  })

  it('should set correct default data', () => {
    const defaultData = Todos.data()
    expect(defaultData.todos).toEqual([])
    expect(defaultData.pagination.page).toBe(1)
    expect(defaultData.pagination.pageSize).toBe(10)
    expect(defaultData.filter.isDone).toBeNull()
    expect(defaultData.sort.by).toBe('CreatedAt')
    expect(defaultData.sort.desc).toBe(true)
  })

  it('should have the correct name', () => {
    expect(Todos.name).toBe('Todos')
  })
})
